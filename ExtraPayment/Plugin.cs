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

namespace ExtraPayment
{
    [BepInPlugin("Plasmatank.ExtraPayment", "ExtraPayment", "1.1.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource MyLogger;

        public static ConfigEntry<double> Price_Ratio;

        public static ConfigEntry<bool> Is_Repeated_Extra;

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

            Price_Ratio = Config.Bind<double>("Config", "Price_Ratio", 0.75, "食材价格调整比率，该值小于1，则在加料时每个食材提供的加成价格越少。");
            Is_Repeated_Extra = Config.Bind<bool>("Config", "Is_Repeated_Extra", true, "启用后，新加入的重复食材也可获得加成，默认开启。");

        }
    }
    public static class Utility
    {
        public static void ModifyPrice(ref GameData.Core.Collections.Sellable food, ref GameData.Core.Collections.Recipe recipe)
        {
            var selection = GameObject.FindObjectOfType<NightScene.UI.CookingUtility.CookingSelectionModuleUI>();
            
            if (Plugin.Is_Repeated_Extra.Value)
            {
                var copied_selection = new List<int> { };
                foreach (int i in selection.selectedIngredients)
                {
                    copied_selection.Add(i);
                }
                foreach (int j in recipe.Ingredients)
                {
                    copied_selection.Remove(j);
                }
                foreach (int k in copied_selection)
                {
                    food.baseValue += Convert.ToInt32(Math.Round(GameData.Core.Collections.DataBaseCore.RefIngredient(k).baseValue * Plugin.Price_Ratio.Value, 0, MidpointRounding.AwayFromZero));
                }
            }
            else
            {
                foreach (int i in selection.selectedIngredients)
                {
                    if (!recipe.Ingredients.Contains(i))
                    {
                        food.baseValue += Convert.ToInt32(Math.Round(GameData.Core.Collections.DataBaseCore.RefIngredient(i).baseValue * Plugin.Price_Ratio.Value, 0, MidpointRounding.AwayFromZero));
                    }
                };
            }
        }
    }

    [HarmonyPatch(typeof(NightScene.CookingUtility.CookController), nameof(NightScene.CookingUtility.CookController.SetCook))]
    public static class SelectionPatch
    {
        public static void Prefix(ref GameData.Core.Collections.Sellable result, ref GameData.Core.Collections.Recipe recipe)
        {
            Plugin.Print("Selection hooked！");           
            Utility.ModifyPrice(ref result, ref recipe);     
        }
    }
    [HarmonyPatch(typeof(Common.UI.SellableDescriber), nameof(Common.UI.SellableDescriber.Describe))]
    public static class DescribePatch
    {
        public static void Prefix(ref GameData.Core.Collections.Sellable detail, out int __state)
        {
            __state = detail.baseValue;
            GameData.Core.Collections.Recipe recipe = GameData.Core.Collections.DataBaseCore.Recipes[0];
            foreach (var keypair in GameData.Core.Collections.DataBaseCore.Recipes)
            {
                if (detail.Id == keypair.Value.FoodID)
                {
                    recipe = GameData.Core.Collections.DataBaseCore.RefRecipe(keypair.Key);
                    break;
                }
            }           
            var selection = GameObject.FindObjectOfType<NightScene.UI.CookingUtility.CookingSelectionModuleUI>();
            if (selection is not null)
            {
                bool Recipe_Flag = true;
                foreach (int x in recipe.Ingredients)
                {
                    if (!selection.selectedIngredients.Contains(x))
                    {
                        Recipe_Flag = false;
                    }
                }
                if (Recipe_Flag)
                {
                    Utility.ModifyPrice(ref detail, ref recipe);
                }              
            }          
        }
        public static void Postfix(ref GameData.Core.Collections.Sellable detail, int __state)
        {
            detail.baseValue = __state;
        }
    }     
}
