using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using UnhollowerRuntimeLib;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using Il2CppSystem.Collections.Generic;

// Made by Plasmatank. For Mystia's Izakaya.

namespace MoreIngredients
{
    [BepInPlugin("Plasmatank.MoreIngredients", "MoreIngredients", "2.0.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource MyLogger;

        public static List<int> Custom_Ingredients;

        public static System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Ingredient>> Trader_Goods;

        public static void Print(object msg)
        {
            MyLogger.Log(BepInEx.Logging.LogLevel.Message, msg);
        }
        public override void Load()
        {
            MyLogger = Log;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Print("Now you can buy the ingredients you like!");
            Harmony.PatchAll();

            Custom_Ingredients = new List<int>();
            Trader_Goods = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<Ingredient>>();
            Directory.CreateDirectory(Path.Combine(System.Environment.CurrentDirectory, @"BepInEx\plugins\MoreIngredients"));
        }
    }  

    [HarmonyPatch(typeof(DayScene.SceneManager), nameof(DayScene.SceneManager.Awake))]
    public static class AwakeLoadHook
    {
        public static bool Prefix()
        {
            var merchants = GameData.RunTime.DaySceneUtility.RunTimeDayScene.trackedMerchants.Keys;
            foreach (FileInfo file in new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, @"BepInEx\plugins\MoreIngredients")).GetFiles())
            {
                if (file.Name.EndsWith(".json"))
                {
                    Plugin.Print("Loading " + file.Name);
                    Ingredient ingredient = Utility.LoadJson(file.FullName);
                    if (!GameData.Core.Collections.DataBaseCore.Ingredients.ContainsKey(ingredient.ID))
                    {
                        var Ingredient_Object = new GameData.Core.Collections.Ingredient(ingredient.ID, ingredient.Price, ingredient.Level, 0, ingredient.Tags.ToArray());
                        var Ingredient_Pics = new GameData.CoreLanguage.ObjectLanguageBase(ingredient.Name, ingredient.Description, Utility.LoadByIo(ingredient.SpritePath, "Extra_Ingredient_"+ingredient.ID));

                        GameData.Core.Collections.DataBaseCore.Ingredients.Add(ingredient.ID, Ingredient_Object);
                        GameData.CoreLanguage.Collections.DataBaseLanguage.Ingredients.Add(ingredient.ID, Ingredient_Pics);
                        GameData.Core.Collections.DataBaseCore.IngredientsMapping.Add(ingredient.ID, "PLASMATANK_INGREDIENT");
                        Plugin.Custom_Ingredients.Add(ingredient.ID);
                        if (!Plugin.Trader_Goods.ContainsKey(ingredient.Trader))
                        {
                            var New_Value = new System.Collections.Generic.List<Ingredient>(); New_Value.Add(ingredient);
                            Plugin.Trader_Goods.Add(ingredient.Trader, New_Value);
                        }
                        else
                        {
                            Plugin.Trader_Goods[ingredient.Trader].Add(ingredient);
                        }
                    }
                    if (!GameData.RunTime.Common.RunTimeAlbum.Ingredients.Contains(ingredient.ID))
                    {
                        GameData.RunTime.Common.RunTimeAlbum.Ingredients.Add(ingredient.ID);
                    }
                }
            }

                    
            foreach (string merchant in Plugin.Trader_Goods.Keys)
            {
                var Final_List = new List<GameData.Core.Collections.DaySceneUtility.Collections.Product>();
                var Origin_Product = GameData.RunTime.DaySceneUtility.RunTimeDayScene.trackedMerchants[merchant].products;

                foreach (var product in Origin_Product)
                {
                    Final_List.Add(product);
                }

                if (Plugin.Trader_Goods.ContainsKey(merchant))
                {
                    foreach (Ingredient ingredient in Plugin.Trader_Goods[merchant])
                    {
                        var example = Origin_Product[0];
                        example.productAmount = ingredient.TradeAmount;
                        example.productType = GameData.Core.Collections.DaySceneUtility.Collections.Product.ProductType.Ingredient;
                        example.productId = ingredient.ID;
                        Final_List.Add(example);
                    }
                }                           

                GameData.RunTime.DaySceneUtility.RunTimeDayScene.trackedMerchants[merchant].products = Final_List.ToArray().TryCast<UnhollowerBaseLib.Il2CppReferenceArray<GameData.Core.Collections.DaySceneUtility.Collections.Product>>();
            }           
            return true;
        }
    }
    [HarmonyPatch(typeof(GameData.Profile.GameDataProfile), nameof(GameData.Profile.GameDataProfile.ActiveDLCLabel), MethodType.Getter)]
    public static class CheckHook
    {
        public static Common.LoadingSceneManager Loader = GameObject.FindObjectOfType<Common.LoadingSceneManager>();
        public static void Postfix(ref List<string> __result)
        {
            var Mod_Label = "PLASMATANK_INGREDIENT";
            Loader.ShowLoadingMessage($"\nExtra Mod Loaded:{Mod_Label}");
            var New_List = new List<string>();
            New_List.AddRange(__result.Cast<IEnumerable<string>>());
            New_List.Add(Mod_Label);
            __result = New_List;
        }
    }
}
