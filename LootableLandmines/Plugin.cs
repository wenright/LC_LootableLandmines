using System;
using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using LethalLib.Modules;
using LootableLandmines.Behavoiurs;
using LootableLandmines.Patches;
using UnityEngine;
using Utilities = LethalLib.Modules.Utilities;

namespace LootableLandmines
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public Item customMineData;
        public static GameObject customMinePrefab;

        public const string GUID = "procyon.lootablelandmines";
        public const string NAME = "Lootable Landmines";
        public const string VERSION = "1.3.1";

        private readonly Harmony harmony = new Harmony(GUID);

        private void Awake()
        {
            Instance = this;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "minemod");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            customMineData = bundle.LoadAsset<Item>("Assets/CustomLandmine.asset");

            // Custom behaviour
            CustomMine customMineComponent = customMineData.spawnPrefab.AddComponent<CustomMine>();
            customMineComponent.grabbable = true;
            customMineComponent.grabbableToEnemies = true;
            customMineComponent.itemProperties = customMineData;
            customMineComponent.SetScrapValue(57);

            Utilities.FixMixerGroups(customMineData.spawnPrefab);
            NetworkPrefabs.RegisterNetworkPrefab(customMineData.spawnPrefab);
            customMinePrefab = customMineData.spawnPrefab;

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "An explosive land mine";
            Items.RegisterShopItem(customMineData, null, null, node, 135);
            harmony.PatchAll(typeof(LandminePatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));

            Logger.LogInfo($"Plugin {GUID} is loaded!");
        }
    }
}