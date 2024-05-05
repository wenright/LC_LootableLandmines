using System.Reflection;
using LootableLandmines.Patches;
using Unity.Netcode;

namespace LootableLandmines.Behavoiurs;

internal class CustomMine : PhysicsProp
{
    private Landmine _landmine;
    private const float armDelay = 1.75f;
    
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
    
    // The default GrabItem gets called with a 0.1s delay :(
    public void EarlyGrabItem()
    {
        _landmine.ToggleMineEnabledLocalClient(false);
        
        typeof(Landmine)
            .GetField("localPlayerOnMine", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(_landmine, false);
    }

    private void DelayedEnable()
    {
        _landmine.ToggleMineEnabledLocalClient(true);
    }

    public void Cleanup()
    {
        Invoke(nameof(DelayedCleanup), 1.0f);
    }

    private void DelayedCleanup()
    {
        var networkObject = GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Despawn();
        }
    }
}