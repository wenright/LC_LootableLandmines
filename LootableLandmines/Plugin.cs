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

        const string GUID = "procyon.lootablelandmines";
        const string NAME = "Lootable Landmines";
        const string VERSION = "1.1.0";

        private const int minMines = 1;
        private const int maxMines = 7;

        private readonly Harmony harmony = new Harmony(GUID);

        private void Awake()
        {
            Instance = this;

            InitializeNetworkBehaviours();

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

            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.clearPreviousText = true;
            node.displayText = "An explosive land mine";
            Items.RegisterShopItem(customMineData, null, null, node, 135);

            AnimationCurve curve = new AnimationCurve(new Keyframe(0, minMines), new Keyframe(1, maxMines));

            var mineMapObj = new SpawnableMapObject()
            {
                prefabToSpawn = customMineData.spawnPrefab,
                numberToSpawn = curve
            };
            MapObjects.RegisterMapObject(mineMapObj, Levels.LevelTypes.All, _ => curve);

            harmony.PatchAll(typeof(LandminePatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));

            Logger.LogInfo($"Plugin {GUID} is loaded!");
        }

        private static void InitializeNetworkBehaviours()
        {
            // See https://github.com/EvaisaDev/UnityNetcodePatcher?tab=readme-ov-file#preparing-mods-for-patching
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (MethodInfo method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}