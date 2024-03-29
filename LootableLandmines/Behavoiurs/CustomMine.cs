using LootableLandmines.Patches;
using UnityEngine;

namespace LootableLandmines.Behavoiurs;

internal class CustomMine : PhysicsProp
{
    private Landmine _landmine;
    private const float armDelay = 0.75f;
    
    public override void Start()
    {
        base.Start();

        _landmine = GetComponentInChildren<Landmine>();
    }
    
    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        LandminePatch.TriggerMineOnLocalClientByExiting(_landmine);
    }

    public override void OnHitGround()
    {
        Invoke(nameof(DelayedEnable), armDelay);
    }

    public override void EquipItem()
    {
        Debug.Log(Time.time + " - LANDMINE EquipItem");
    }

    // The default GrabItem gets called with a 0.1s delay :(
    public void EarlyGrabItem()
    {
        Debug.Log("LANDMINE GrabItem");

        Disable();
    }

    public void Disable()
    {
        _landmine.ToggleMineEnabledLocalClient(false);
        // _landmine.ToggleMine(false);
    }

    private void DelayedEnable()
    {
        _landmine.ToggleMineEnabledLocalClient(true);
        // _landmine.ToggleMine(true);
    }
}