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

namespace MystiaBar
{
    [BepInPlugin("Plasmatank.MystiaBar", "MystiaBar", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource MyLogger;

        public static ConfigEntry<int> Start_Index;

        public static List<int> Custom_Beverage;

        public static void Print(object msg)
        {
            MyLogger.Log(BepInEx.Logging.LogLevel.Message, msg);
        }
        public override void Load()
        {
            MyLogger = Log;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Print("Now you can make your own drinks!");
            Harmony.PatchAll();

            ClassInjector.RegisterTypeInIl2Cpp<RuntimeListener>();            
            var Modifier = new GameObject("ModifierInstance");
            Modifier.AddComponent<RuntimeListener>();
            GameObject.DontDestroyOnLoad(Modifier);
            Modifier.hideFlags |= HideFlags.HideAndDontSave;

            Custom_Beverage = new List<int>();
            Start_Index = Config.Bind<int>("Config", "Start_Index", 8500, "基酒起始序号。");
            Directory.CreateDirectory(Path.Combine(System.Environment.CurrentDirectory, @"BepInEx\plugins\MystiaBar"));

        }
    }
    
    [HarmonyPatch(typeof(GameData.RunTime.Common.RunTimeStorage), nameof(GameData.RunTime.Common.RunTimeStorage.FoodInRange))]      
    public static class DayCookingHook
    {
        public static bool Prefix(ref IEnumerable<int> foodIds)
        {
            Plugin.Print("Food is packing!");
            var ManagedID = foodIds.TryCast<Il2CppSystem.Collections.IEnumerable>();
            var To_Convert = new List<int>();
            var Remaining = new List<int>();
            foreach (Il2CppSystem.Object i in ManagedID)
            {
                Plugin.Print("Inside.");
                int index = Il2CppSystem.Convert.ToInt32(i);
                Plugin.Print(index);
                if (Plugin.Custom_Beverage.Contains(index))
                {
                    To_Convert.Add(index);
                }
                else
                {
                    Remaining.Add(index);
                }
            }
            foodIds = Remaining.Cast<IEnumerable<int>>();
            GameData.RunTime.Common.RunTimeStorage.BeverageInRange(To_Convert.Cast<IEnumerable<int>>());
            return true;
        }
    }

    [HarmonyPatch(typeof(DayScene.SceneManager), nameof(DayScene.SceneManager.Awake))]
    public static class AwakeLoadHook
    {
        public static bool Prefix()
        {
            //Loading origin base wine
            if (!GameData.Core.Collections.DataBaseCore.Ingredients.ContainsKey(Plugin.Start_Index.Value))
            {              
                foreach (var keypair in GameData.Core.Collections.DataBaseCore.Beverages)
                {
                    var ID = keypair.Key + Plugin.Start_Index.Value;
                    Loading_Base_Wine(ID, keypair.Value);
                }
            }
            //Loading wine recipe
            foreach (FileInfo file in new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, @"BepInEx\plugins\MystiaBar")).GetFiles())
            {
                if (file.Name.EndsWith(".json"))
                {
                    Plugin.Print("Loading " + file.Name);
                    Beverage beverage = Utility.LoadJson(file.FullName);

                    if (!GameData.Core.Collections.DataBaseCore.Recipes.ContainsKey(beverage.ID))
                    {
                        var Pre_LanguageObject = new GameData.CoreLanguage.ObjectLanguageBase(beverage.Name + " 原酒", beverage.Description, Utility.LoadByIo(beverage.SpritePath, beverage.Name + "_" + beverage.ID));
                        var LanguageObject = new GameData.CoreLanguage.ObjectLanguageBase(beverage.Name, beverage.Description, Utility.LoadByIo(beverage.SpritePath, beverage.Name + "_" + beverage.ID));
                        var Pre_SellableObject = new GameData.Core.Collections.Sellable(beverage.ID, beverage.Price, beverage.Level, new UnhollowerBaseLib.Il2CppStructArray<int>(0), new UnhollowerBaseLib.Il2CppStructArray<int>(0), GameData.Core.Collections.Sellable.SellableType.Food, new List<int>(), false);
                        var SellableObject = new GameData.Core.Collections.Sellable(beverage.ID, beverage.Price, beverage.Level, beverage.Tags.ToArray(), new UnhollowerBaseLib.Il2CppStructArray<int>(0), GameData.Core.Collections.Sellable.SellableType.Beverage, new List<int>(), false);
                        var RecipeObejct = new GameData.Core.Collections.Recipe(beverage.ID, beverage.ID, Utility.ToEnum<GameData.Core.Collections.Cooker.CookerType>(beverage.CookerType), beverage.CookTime, beverage.Ingredients.ToArray());

                        GameData.CoreLanguage.Collections.DataBaseLanguage.Foods.Add(beverage.ID, Pre_LanguageObject);
                        GameData.Core.Collections.DataBaseCore.Foods.Add(beverage.ID, Pre_SellableObject);

                        GameData.CoreLanguage.Collections.DataBaseLanguage.Beverages.Add(beverage.ID, LanguageObject);
                        GameData.Core.Collections.DataBaseCore.Beverages.Add(beverage.ID, SellableObject);
                        GameData.CoreLanguage.Collections.DataBaseLanguage.BeveragePlates.Add(beverage.ID, Utility.LoadByIo(beverage.SignboardPath, beverage.Name + "_" + beverage.ID));

                        GameData.Core.Collections.DataBaseCore.Recipes.Add(beverage.ID, RecipeObejct);
                        GameData.Core.Collections.DataBaseCore.RecipesMapping.Add(beverage.ID, beverage.ID.ToString() + "PLASMATANK");
                        
                        Plugin.Custom_Beverage.Add(beverage.ID);
                    }                   
                }
            }
            //Adding custom beverage to RunTimeStorage
            foreach (int i in Plugin.Custom_Beverage)
            {
                if (!GameData.RunTime.Common.RunTimeStorage.Recipes.Contains(i))
                {
                    GameData.RunTime.Common.RunTimeStorage.Recipes.Add(i);
                    if (!GameData.Core.Collections.DataBaseCore.Ingredients.ContainsKey(i))
                    {
                        Loading_Base_Wine(i, GameData.Core.Collections.DataBaseCore.Beverages[i]);
                    }
                    GameData.RunTime.Common.RunTimeAlbum.Ingredients.Add(i);
                }
            }
            return true;
        }
        public static void Loading_Base_Wine(int ID, GameData.Core.Collections.Sellable wine)
        {
            var Base_Wine = new GameData.Core.Collections.Ingredient(ID, wine.baseValue, wine.Level, 0, Utility.Refer_Tags(wine.tags));
            var Wine_Pics = new GameData.CoreLanguage.ObjectLanguageBase(wine.Text.Name + " 基酒", wine.Text.Description, wine.Text.Visual);
            GameData.Core.Collections.DataBaseCore.Ingredients.Add(ID, Base_Wine);
            GameData.CoreLanguage.Collections.DataBaseLanguage.Ingredients.Add(ID, Wine_Pics);
            GameData.Core.Collections.DataBaseCore.IngredientsMapping.Add(ID, ID + "PLASMATANK");          
        }
    }

    [HarmonyPatch(typeof(GameData.RunTime.NightSceneUtility.IzakayaTray), nameof(GameData.RunTime.NightSceneUtility.IzakayaTray.Receive))]
    public static class ReceiveBeverageHook
    {
        public static bool Prefix(ref GameData.Core.Collections.Sellable value)
        {
            if (value.type == GameData.Core.Collections.Sellable.SellableType.Food && Plugin.Custom_Beverage.Contains(value.id))
            {
                value = GameData.Core.Collections.DataBaseCore.Beverages[value.id];
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Common.UI.StorageUtility.StorageOtherUI), nameof(Common.UI.StorageUtility.StorageOtherUI.Open))]
    public static class StorageLoadHook
    {
        public static void Postfix(Common.UI.StorageUtility.StorageOtherUI __instance)
        {
            var View = GameObject.Find("OtherViewPort");
            var Content = View.transform.GetChild(0);
            var Shine = GameObject.Find("TabsBlocker").transform.GetChild(2).gameObject.GetComponent<UnityEngine.UI.Image>();
            var childs = Enumerable.Range(0, Content.childCount);
            foreach (int i in childs)
            {
                var image = Content.GetChild(i).GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>();
                //Plugin.Print(image.activeSprite.ToString());
                var strings = image.activeSprite.name.Split("_", StringSplitOptions.RemoveEmptyEntries);

                Action action = () =>
                {
                    if (Shine.IsActive())
                    {
                        if (GameData.RunTime.Common.RunTimeStorage.Beverages[Convert.ToInt32(strings[^1])] != 0)
                        {
                            Plugin.Print("Converted: " + strings[^1]);
                            GameData.RunTime.Common.RunTimeStorage.BeverageOut(Convert.ToInt32(strings[^1]));
                            __instance.UpdatePannelElement();
                            var list = new List<int>();
                            list.Add((Plugin.Custom_Beverage.Contains(Convert.ToInt32(strings[^1])) ? 0 : Plugin.Start_Index.Value) + Convert.ToInt32(strings[^1]));
                            GameData.RunTime.Common.RunTimeStorage.IngredientInRange(list.Cast<IEnumerable<int>>());
                        }                       
                    }
                };
                Il2CppSystem.Action final_action = action;

                if (Content.GetChild(i).gameObject.GetComponent<DEYU.UniversalUISystem.InteractableBase>().onSubmitAction is null)
                {                  
                    Content.GetChild(i).gameObject.GetComponent<DEYU.UniversalUISystem.InteractableBase>().onSubmitAction = final_action;                
                }
            }
        }
    }
    //Refresh Element Action
    [HarmonyPatch(typeof(Common.UI.StorageUtility.StorageOtherUI), nameof(Common.UI.StorageUtility.StorageOtherUI.UpdatePannelElement))]
    public static class RefreshLoadHook
    {
        public static void Postfix(Common.UI.StorageUtility.StorageOtherUI __instance)
        {
            var View = GameObject.Find("OtherViewPort");
            var Content = View.transform.GetChild(0);
            var Shine = GameObject.Find("TabsBlocker").transform.GetChild(2).gameObject.GetComponent<UnityEngine.UI.Image>();
            var childs = Enumerable.Range(0, Content.childCount);
            foreach (int i in childs)
            {
                var image = Content.GetChild(i).GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>();
                var strings = image.activeSprite.name.Split("_", StringSplitOptions.RemoveEmptyEntries);
                Action action = () =>
                {
                    if (Shine.IsActive())
                    {
                        if (GameData.RunTime.Common.RunTimeStorage.Beverages[Convert.ToInt32(strings[^1])] != 0)
                        {
                            Plugin.Print("Converted: " + strings[^1]);
                            GameData.RunTime.Common.RunTimeStorage.BeverageOut(Convert.ToInt32(strings[^1]));
                            __instance.UpdatePannelElement();
                            var list = new List<int>();
                            list.Add((Plugin.Custom_Beverage.Contains(Convert.ToInt32(strings[^1])) ? 0 : Plugin.Start_Index.Value) + Convert.ToInt32(strings[^1]));
                            GameData.RunTime.Common.RunTimeStorage.IngredientInRange(list.Cast<IEnumerable<int>>());
                        }
                    }
                };
                Il2CppSystem.Action final_action = action;
                Content.GetChild(i).gameObject.GetComponent<DEYU.UniversalUISystem.InteractableBase>().onSubmitAction = final_action;
            }
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
