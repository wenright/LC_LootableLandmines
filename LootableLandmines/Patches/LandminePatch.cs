using HarmonyLib;
using LootableLandmines.Behavoiurs;
using Unity.Netcode;
using UnityEngine;

namespace LootableLandmines.Patches;

[HarmonyPatch(typeof(Landmine))]
public class LandminePatch
{
    private const bool Skip = false;
    private const bool Continue = true;
    
    [HarmonyPatch(typeof(Landmine), "Start")]
    [HarmonyPostfix]
    static void Start(Landmine __instance)
    {
        if (__instance == null) return;
        if (!NetworkManager.Singleton.IsServer) return;

        // Destroy existing landmines and replace with ones that are lootable
        if (__instance.GetComponentInParent<CustomMine>() == null)
        {
            Debug.Log($"{Plugin.GUID} - Replacing existing landmine with a custom one");
            
            var customMineInstance = GameObject.Instantiate(Plugin.customMinePrefab, __instance.transform.position, Quaternion.identity);
            var instanceNetworkComponent = customMineInstance.GetComponent<NetworkObject>();
            instanceNetworkComponent.Spawn();

            var networkObject = __instance.gameObject.GetComponentInParent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Despawn();
            }
        }
    }

    [HarmonyPatch(typeof(Landmine), "OnTriggerEnter")]
    [HarmonyPatch(typeof(Landmine), "OnTriggerExit")]
    [HarmonyPrefix]
    static bool PreTriggerExit(Collider other, Landmine __instance)
    {
        if (__instance == null) return Continue;
        
        // Skip collisions if falling or held by a player
        CustomMine customMine = __instance.GetComponentInParent<CustomMine>();
        if (customMine != null)
        {
            if (!customMine.hasHitGround)
            {
                return Skip;
            }
            
            if (customMine.playerHeldBy != null)
            {
                return Skip;
            }
        }
        
        // Skip collisions with itself or other mines
        CustomMine otherMine = other.GetComponent<CustomMine>();
        if (otherMine != null)
        {
            return Skip;
        }

        return Continue;
    }

    [HarmonyPatch(typeof(Landmine), "OnTriggerEnter")]
    [HarmonyPostfix]
    static void PostTriggerExit(Collider other, Landmine __instance, ref bool ___localPlayerOnMine)
    {
        if (__instance == null) return;

        // Prevents mine from detonating when teleporting (using doors)
        if (__instance.GetComponentInParent<CustomMine>() != null && __instance.GetComponentInParent<CustomMine>().isHeld)
        {
            ___localPlayerOnMine = false;
        }
    }

    [HarmonyPatch(typeof(Landmine), "Detonate")]
    [HarmonyPostfix]
    public static void PostDetonate(Landmine __instance)
    {
        if (__instance == null) return;
        if (!NetworkManager.Singleton.IsServer) return;
        
        Debug.Log(Time.time + "s: LANDMINE detonating");

        // Clean up destroyed mines
        var customMine = __instance.GetComponentInParent<CustomMine>();
        if (customMine != null)
        {
            customMine.DestroyObjectInHand(customMine.playerHeldBy);
            
            // Give a little time for the audio to play
            customMine.Cleanup();
        }
    }
    
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Landmine), "TriggerMineOnLocalClientByExiting")]
    public static void TriggerMineOnLocalClientByExiting(Landmine instance) { }
}
