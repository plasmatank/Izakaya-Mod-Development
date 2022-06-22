using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using UnhollowerRuntimeLib;
using System;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

// Made by Plasmatank. For Mystia's Izakaya.

namespace FoodSlice
{
    [BepInPlugin("Plasmatank.FoodSlice", "FoodSlice", "1.1.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource MyLogger;

        public static ConfigEntry<KeyCode> Slice_Key;

        public static ConfigEntry<double> Price_Ratio;

        public static void Print(object msg)
        {
            MyLogger.Log(BepInEx.Logging.LogLevel.Message, msg);
        }
        public override void Load()
        {
            MyLogger = Log;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Print("Foods now can be sliced.");
            Harmony.PatchAll();

            ClassInjector.RegisterTypeInIl2Cpp<RuntimeListener>();
            var Modifier = new GameObject("ModifierInstance");
            Modifier.AddComponent<RuntimeListener>();
            GameObject.DontDestroyOnLoad(Modifier);
            Modifier.hideFlags |= HideFlags.HideAndDontSave;

            Slice_Key = Config.Bind<KeyCode>("Config", "Slice_Key", KeyCode.L, "切分当前托盘的菜品，优先切分所有大份的食物，再次按下则会将普通菜品分成带有小巧Tag的小份。");
            Price_Ratio = Config.Bind<double>("Config", "Price_Ratio", 0.75, "切分后菜品价格所乘倍率，该值小于1菜品就会越切越便宜。");

        }
    }
    public class RuntimeListener : MonoBehaviour
    {
        void Start()
        {
            Plugin.Print("Listener is loaded!");
        }

        void Update()
        {
            if (UniverseLib.Input.InputManager.GetKeyDown(Plugin.Slice_Key.Value))
            {
                GameData.RunTime.NightSceneUtility.IzakayaTray tray = GameData.RunTime.NightSceneUtility.IzakayaTray.instance;
                List<int> index = new() { };
                var tray_index = Enumerable.Range(0, tray.Tray.Elements.Count);

                foreach (int i in tray_index)
                {
                    if (!tray.Tray.GetAvailableIndex().Contains(i))
                    {
                        index.Add(i);
                    }                   
                }

                foreach (int i in index)
                {
                    var Food = tray.Tray.Elements[i];
                    if (Food.additiveTags.Contains(-1))
                    {
                        Food.additiveTags.Remove(-1);
                        Plugin.Print(string.Join(",", Food.Tags));
                        var Convert_List = new List<int>{ };
                        foreach (int x in Food.Tags)
                        {
                            if (x != -1)
                            {
                                Convert_List.Add(x);
                            }                          
                        }
                        Convert_List.Remove(-20); Convert_List.Remove(-21);
                        Food.baseValue = Convert.ToInt32(Math.Round(Food.baseValue*Plugin.Price_Ratio.Value, 0, MidpointRounding.AwayFromZero));
                        var ReforgedFood = new GameData.Core.Collections.Sellable(Food.id, Food.baseValue, Food.level, Food.tags, Food.banTags, GameData.Core.Collections.Sellable.SellableType.Food, Convert_List, false);
                        Utility.Add_Food(ReforgedFood);
                        Plugin.Print("Processed!");
                        Utility.Processed = true;
                    }                                  
                }

                if (!Utility.Processed)
                {
                    foreach (int i in index)
                    {
                        var Food = tray.Tray.Elements[i];
                        if (!Food.tags.Contains(28) && !Food.AdditiveTags.Contains(28))
                        {
                            Food.additiveTags.Add(28);
                            Plugin.Print("///" + string.Join(",", Food.tags) + "///");
                            var Convert_List = new List<int> { };
                            foreach (int x in Food.Tags)
                            {
                                Convert_List.Add(x);                           
                            }
                            Convert_List.Remove(-20); Convert_List.Remove(-21);
                            Food.baseValue = Convert.ToInt32(Math.Round(Food.baseValue * Plugin.Price_Ratio.Value, 0, MidpointRounding.AwayFromZero));
                            var ReforgedFood = new GameData.Core.Collections.Sellable(Food.id, Food.baseValue, Food.level, Food.tags, Food.banTags, GameData.Core.Collections.Sellable.SellableType.Food, Convert_List, false);
                            Utility.Add_Food(ReforgedFood);
                        }
                        
                    }                                                    
                }
                Utility.Processed = false;
            }
        }
    }
    public static class Utility
    {
        public static bool Processed = false;
        public static void Add_Food(GameData.Core.Collections.Sellable food)
        {
            GameData.RunTime.NightSceneUtility.IzakayaTray tray = GameData.RunTime.NightSceneUtility.IzakayaTray.instance;
            GameData.RunTime.NightSceneUtility.IzakayaConfigure cabinet = GameData.RunTime.NightSceneUtility.IzakayaConfigure.instance;
            if (!tray.IsTrayFull)
            {
                tray.Receive(food);
            }
            else
            {
                cabinet.StoreFood(food, 1);
            }
        }
    }
}
