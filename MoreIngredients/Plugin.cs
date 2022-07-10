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
using BepInEx.IL2CPP.Utils.Collections; //WrapIEnumerableToManaged

// Made by Plasmatank. For Mystia's Izakaya.

namespace MoreIngredients
{
    [BepInPlugin("Plasmatank.MoreIngredients", "MoreIngredients", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource MyLogger;

        public static List<int> Custom_Ingredients;

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

            ClassInjector.RegisterTypeInIl2Cpp<RuntimeListener>();            
            var Modifier = new GameObject("ModifierInstance");
            Modifier.AddComponent<RuntimeListener>();
            GameObject.DontDestroyOnLoad(Modifier);
            Modifier.hideFlags |= HideFlags.HideAndDontSave;

            Custom_Ingredients = new List<int>();
            Directory.CreateDirectory(Path.Combine(System.Environment.CurrentDirectory, @"BepInEx\plugins\MoreIngredients"));

        }
    }  

    [HarmonyPatch(typeof(DayScene.SceneManager), nameof(DayScene.SceneManager.Awake))]
    public static class AwakeLoadHook
    {
        public static bool Prefix()
        {
            var Final_List = new List<GameData.Core.Collections.DaySceneUtility.Collections.Product>();
            var Origin_Product = GameData.RunTime.DaySceneUtility.RunTimeDayScene.trackedMerchants["Koakuma"].products;
            foreach (var product in Origin_Product)
            {
                Final_List.Add(product);
            }

            foreach (FileInfo file in new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, @"BepInEx\plugins\MoreIngredients")).GetFiles())
            {
                if (file.Name.EndsWith(".json"))
                {
                    Plugin.Print("Loading " + file.Name);
                    Ingredient ingredient = Utility.LoadJson(file.FullName);
                    if (!GameData.Core.Collections.DataBaseCore.Ingredients.ContainsKey(ingredient.ID))
                    {
                        var Ingredient_Object = new GameData.Core.Collections.Ingredient(ingredient.ID, ingredient.Price, ingredient.Level, 0, ingredient.Tags.ToArray());
                        var Ingredient_Pics = new GameData.CoreLanguage.ObjectLanguageBase(ingredient.Name, ingredient.Description, Utility.LoadByIo(ingredient.SpritePath));

                        GameData.Core.Collections.DataBaseCore.Ingredients.Add(ingredient.ID, Ingredient_Object);
                        GameData.CoreLanguage.Collections.DataBaseLanguage.Ingredients.Add(ingredient.ID, Ingredient_Pics);
                        GameData.Core.Collections.DataBaseCore.IngredientsMapping.Add(ingredient.ID, ingredient.ID + "PLASMATANK");
                        Plugin.Custom_Ingredients.Add(ingredient.ID);
                    }
                    if (!GameData.RunTime.Common.RunTimeAlbum.Ingredients.Contains(ingredient.ID))
                    {
                        GameData.RunTime.Common.RunTimeAlbum.Ingredients.Add(ingredient.ID);
                    }

                    //Add ingredients for trader
                    var example = Origin_Product[0];
                    example.productAmount = ingredient.TradeAmount;
                    example.productType = GameData.Core.Collections.DaySceneUtility.Collections.Product.ProductType.Ingredient;
                    example.productId = ingredient.ID;
                    Final_List.Add(example);
                }
            }           
            GameData.RunTime.DaySceneUtility.RunTimeDayScene.trackedMerchants["Koakuma"].products = Final_List.ToArray().TryCast<UnhollowerBaseLib.Il2CppReferenceArray<GameData.Core.Collections.DaySceneUtility.Collections.Product>>();
            return true;
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
            
        }
    }
    
}
