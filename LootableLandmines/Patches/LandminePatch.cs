using HarmonyLib;
using LootableLandmines.Behavoiurs;
using Unity.Netcode;
using UnityEngine;

namespace LootableLandmines.Patches;

[HarmonyPatch(typeof(Landmine))]
public class LandminePatch
{
    private const bool SKIP = false;
    private const bool DONT_SKIP = true;
    
    [HarmonyPatch(typeof(Landmine), "Start")]
    [HarmonyPostfix]
    static void Start(Landmine __instance)
    {
        if (__instance == null) return;
        if (!NetworkManager.Singleton.IsServer) return;

        // Destroy existing landmines, since they aren't pickupable
        if (__instance.GetComponentInParent<CustomMine>() == null)
        {
            NetworkManager.Destroy(__instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(Landmine), "OnTriggerEnter")]
    [HarmonyPatch(typeof(Landmine), "OnTriggerExit")]
    [HarmonyPrefix]
    static bool PreTriggerExit(Collider other, Landmine __instance)
    {
        if (__instance == null) return DONT_SKIP;
        
        // Skip collisions if falling or held by a player
        CustomMine customMine = __instance.GetComponentInParent<CustomMine>();
        if (customMine != null)
        {
            if (!customMine.hasHitGround)
            {
                return SKIP;
            }
            if (customMine.playerHeldBy != null)
            {
                return SKIP;
            }
        }
        
        // Skip collisions if held, and collision with itself or other mines
        CustomMine otherMine = other.GetComponent<CustomMine>();
        if (otherMine != null)
        {
            return SKIP;
        }

        return DONT_SKIP;
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
        
        Debug.Log(Time.time + " - LANDMINE detonating");

        // Clean up destroyed mines
        var customMine = __instance.GetComponentInParent<CustomMine>();
        if (customMine != null)
        {
            customMine.DestroyObjectInHand(customMine.playerHeldBy);
            
            // Give a little time for the audio to play
            NetworkManager.Destroy(__instance.transform.parent.gameObject, 1.0f);
        }
    }
    
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Landmine), "TriggerMineOnLocalClientByExiting")]
    public static void TriggerMineOnLocalClientByExiting(Landmine instance)
    {
        Debug.Log("Reverse Patching Landmine -> TriggerMineOnLocalClientByExiting");
    }
}
