using HarmonyLib;
using LootableLandmines.Behavoiurs;
using UnityEngine;

namespace LootableLandmines.Patches;

[HarmonyPatch(typeof(Landmine))]
public class LandminePatch
{
    [HarmonyPatch(typeof(Landmine), "Start")]
    [HarmonyPostfix]
    static void Start(Landmine __instance)
    {
        if (__instance == null) return;

        // Destroy existing landmines, since they aren't pickupable
        if (__instance.GetComponentInParent<CustomMine>() == null)
        {
            Object.Destroy(__instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(Landmine), "OnTriggerEnter")]
    [HarmonyPatch(typeof(Landmine), "OnTriggerExit")]
    [HarmonyPrefix]
    static bool PreTriggerExit(Collider other, Landmine __instance)
    {
        // Skip collisions with self and other landmines
        return other.GetComponent<CustomMine>() == null;
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

    [HarmonyPatch(typeof(Landmine), "OnTriggerExit")]
    [HarmonyPostfix]
    public static void PostDetonate(Collider other, Landmine __instance)
    {
        
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Landmine), "TriggerMineOnLocalClientByExiting")]
    public static void Detonate(Landmine instance)
    {
        Debug.Log("Reverse Patching Landmine -> TriggerMineOnLocalClientByExiting");
        
    }
}
