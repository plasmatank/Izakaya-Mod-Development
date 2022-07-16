using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;
using System.IO;
using Il2CppSystem.Collections.Generic;

// Made by Plasmatank. For Mystia's Izakaya.

namespace CustomRecipe
{
    [BepInPlugin("Plasmatank.CustomRecipe", "CustomRecipe", "2.1.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource MyLogger;

        static public ConfigEntry<int> amount;

        static public List<int> Total;

        static public List<int> Installed;

        public static void Print(object msg)
        {
            MyLogger.Log(BepInEx.Logging.LogLevel.Message, msg);
        }
        public override void Load()
        {
            MyLogger = Log;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Print("Loading custom recipes...");
            Total = new List<int>{ }; Installed = new List<int>{ }; 
            Harmony.PatchAll();

            Directory.CreateDirectory(Path.Combine(System.Environment.CurrentDirectory, @"BepInEx\plugins\CustomRecipe"));

        }    

        [HarmonyPatch(typeof(DayScene.SceneManager), nameof(DayScene.SceneManager.Awake))]
        public static class AwakeLoadHook
        {
            public static bool Prefix()
            {
                Print("Good Morning! :D");              
                Total.Clear();
                Add_Recipe();
                Alert();
                return true;
            }

            public static void Alert()
            {
                Common.UI.ReceivedObjectDisplayerController controller = Common.UI.ReceivedObjectDisplayerController.Instance;
                foreach (int i in Installed)
                {
                    Total.Remove(i);
                }
                if (Total.Count > 0)
                {
                    foreach (int i in Total)
                    {
                        controller.NotifyTextMessage("发现ID:" + i.ToString() + "的食谱存在冲突。");
                    }
                }
            }
            public static void Add_Recipe()
            {
                var Origin_Recipes = new List<int>();
                foreach (var keypair in GameData.Core.Collections.DataBaseCore.RecipesMapping)
                {
                    if (!keypair.value.ToString().StartsWith("PLASMATANK"))
                    {
                        Origin_Recipes.Add(keypair.Key);
                    }
                }

                foreach (FileInfo file in new DirectoryInfo(Path.Combine(System.Environment.CurrentDirectory, @"BepInEx\plugins\CustomRecipe")).GetFiles())
                {
                    if (file.Name.EndsWith(".json"))
                    {
                        Print("Loading " + file.Name);
                        bool Passed = false;
                        Recipe recipe = Utility.LoadJson(file.FullName);
                        Total.Add(recipe.ID);
                        if (!recipe.ReplaceMode)
                        {
                            if (!GameData.Core.Collections.DataBaseCore.Recipes.ContainsKey(recipe.ID))
                            {
                                var LanguageObject = new GameData.CoreLanguage.ObjectLanguageBase(recipe.Name, recipe.Description, Utility.LoadByIo(recipe.FoodSpritePath));
                                var SellableObject = new GameData.Core.Collections.Sellable(recipe.ID, recipe.Price, recipe.Level, recipe.Tags.ToArray(), recipe.BanTags.ToArray(), GameData.Core.Collections.Sellable.SellableType.Food, new Il2CppSystem.Collections.Generic.List<int>(), false);
                                var RecipeObejct = new GameData.Core.Collections.Recipe(recipe.ID, recipe.ID, Utility.ToEnum<GameData.Core.Collections.Cooker.CookerType>(recipe.CookerType), recipe.CookTime, recipe.Ingredients.ToArray());

                                GameData.CoreLanguage.Collections.DataBaseLanguage.Foods.Add(recipe.ID, LanguageObject);
                                GameData.CoreLanguage.Collections.DataBaseLanguage.FoodPlates.Add(recipe.ID, Utility.LoadByIo(recipe.SignboardPath));

                                GameData.Core.Collections.DataBaseCore.Foods.Add(recipe.ID, SellableObject);
                                GameData.Core.Collections.DataBaseCore.Recipes.Add(recipe.ID, RecipeObejct);

                                GameData.Core.Collections.DataBaseCore.FoodsMapping.Add(recipe.ID, "PLASMATANK_RECIPE");
                                GameData.Core.Collections.DataBaseCore.RecipesMapping.Add(recipe.ID, "PLASMATANK_RECIPE");
                                Passed = true;
                            }
                        }
                        else
                        {
                            if (Origin_Recipes.Contains(recipe.ID))
                            {
                                var text = GameData.Core.Collections.DataBaseCore.Recipes[recipe.ID].GetText(recipe.ID);
                                var OR = GameData.Core.Collections.DataBaseCore.Recipes[recipe.ID];
                                var OS = GameData.Core.Collections.DataBaseCore.Foods[OR.FoodID];
                                var OP = OR.FoodID != 66 ? GameData.CoreLanguage.Collections.DataBaseLanguage.FoodPlates[OR.FoodID] : GameData.CoreLanguage.Collections.DataBaseLanguage.FallbackFoodPlate;

                                var LanguageObject = new GameData.CoreLanguage.ObjectLanguageBase(recipe.Name is null ? text.Name : recipe.Name, recipe.Description is null ? text.Description : recipe.Description, recipe.FoodSpritePath is null ? text.Visual : Utility.LoadByIo(recipe.FoodSpritePath));
                                var SellableObject = new GameData.Core.Collections.Sellable(OR.foodID, recipe.Price == 0 ? OS.baseValue : recipe.Price, recipe.Level == 0 ? OS.Level : recipe.Level, recipe.Tags is null ? OS.tags : recipe.Tags.ToArray(), recipe.BanTags is null ? OS.banTags : recipe.BanTags.ToArray(), GameData.Core.Collections.Sellable.SellableType.Food, new Il2CppSystem.Collections.Generic.List<int>(), false);
                                var RecipeObejct = new GameData.Core.Collections.Recipe(recipe.ID, OR.foodID, recipe.CookerType is null ? OR.cookerType : Utility.ToEnum<GameData.Core.Collections.Cooker.CookerType>(recipe.CookerType), recipe.CookTime == 0 ? OR.cookTime : recipe.CookTime, recipe.Ingredients is null ? OR.Ingredients : recipe.Ingredients.ToArray());

                                GameData.CoreLanguage.Collections.DataBaseLanguage.Foods[OR.foodID] = LanguageObject;
                                GameData.CoreLanguage.Collections.DataBaseLanguage.FoodPlates[OR.foodID] = recipe.SignboardPath is null ? OP : Utility.LoadByIo(recipe.SignboardPath);
                                GameData.Core.Collections.DataBaseCore.Recipes[recipe.ID] = RecipeObejct;
                                GameData.Core.Collections.DataBaseCore.Foods[OR.foodID] = SellableObject;
                                Passed = true;
                            }
                            else
                            {
                                Print("ID:" + recipe.ID + "是非原版食谱，不支持替换。");
                            }
                        }
                        if (Passed || Installed.Contains(recipe.ID))
                        {
                            if (!Installed.Contains(recipe.ID))
                            {
                                Installed.Add(recipe.ID);
                                Print(file.Name + " is loaded!");
                            }                               
                            if (!GameData.RunTime.Common.RunTimeStorage.Recipes.Contains(recipe.ID))
                            {
                                GameData.RunTime.Common.RunTimeStorage.Recipes.Add(recipe.ID);
                            }
                            if (!GameData.RunTime.Common.RunTimeAlbum.Foods.Contains(recipe.ID))
                            {
                                GameData.RunTime.Common.RunTimeAlbum.Foods.Add(recipe.ID);
                            }
                        }                        
                    }
                }              
            }
        }    
        [HarmonyPatch(typeof(GameData.Profile.GameDataProfile), nameof(GameData.Profile.GameDataProfile.ActiveDLCLabel), MethodType.Getter)]
        public static class CheckHook
        {
            public static Common.LoadingSceneManager Loader = GameObject.FindObjectOfType<Common.LoadingSceneManager>();
            public static void Postfix(ref List<string> __result)
            {
                var Mod_Label = "PLASMATANK_RECIPE";               
                Loader.ShowLoadingMessage($"\nExtra Mod Loaded:{Mod_Label}");
                var New_List = new List<string>();
                New_List.AddRange(__result.Cast<IEnumerable<string>>());
                New_List.Add(Mod_Label);
                __result = New_List;                          
            }
        }
    }
}
