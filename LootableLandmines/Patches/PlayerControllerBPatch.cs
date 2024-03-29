using GameNetcodeStuff;
using HarmonyLib;
using LootableLandmines.Behavoiurs;

namespace LootableLandmines.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public class PlayerControllerBPatch
{
    [HarmonyPatch(typeof(PlayerControllerB), "BeginGrabObject")]
    [HarmonyPostfix]
    static void GrabObject(PlayerControllerB __instance, GrabbableObject ___currentlyGrabbingObject)
    {
        if (___currentlyGrabbingObject == null) return;

        if (___currentlyGrabbingObject is CustomMine)
        {
            CustomMine customMine = (CustomMine) ___currentlyGrabbingObject;
            customMine.EarlyGrabItem();
        }
    }
}