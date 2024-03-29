using LootableLandmines.Patches;

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

        DestroyObjectInHand(playerHeldBy);
        LandminePatch.Detonate(_landmine);
    }

    public override void EquipItem()
    {
        base.EquipItem();

        // TODO unsure if both calls are needed, but worried delay will cause non-server players to still trigger mine
        _landmine.ToggleMineServerRpc(false);
        _landmine.ToggleMineEnabledLocalClient(false);
    }

    public override void GrabItemFromEnemy(EnemyAI enemyAI)
    {
        base.GrabItemFromEnemy(enemyAI);

        _landmine.ToggleMineServerRpc(false);
        _landmine.ToggleMineEnabledLocalClient(false);
    }

    public override void DiscardItem()
    {
        base.DiscardItem();
        
        Invoke(nameof(DelayedEnable), armDelay);
        
        // TODO collision with ship on takeoff
        // if (!isInShipRoom)
        // {
        //     Invoke(nameof(DelayedEnable), armDelay);
        // }
    }

    public override void DiscardItemFromEnemy()
    {
        base.DiscardItem();
        
        Invoke(nameof(DelayedEnable), armDelay);
    }

    private void DelayedEnable()
    {
        _landmine.ToggleMineServerRpc(true);
        _landmine.ToggleMineEnabledLocalClient(true);
    }
}