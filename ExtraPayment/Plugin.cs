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
    [BepInPlugin("Plasmatank.ExtraPayment", "ExtraPayment", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource MyLogger;

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

            Price_Ratio = Config.Bind<double>("Config", "Price_Ratio", 0.75, "切分后菜品价格所乘倍率，该值小于1菜品就会越切越便宜。");

        }
    }
    [HarmonyPatch(typeof(NightScene.CookingUtility.CookController), nameof(NightScene.CookingUtility.CookController.SetCook))]
    public static class SelectionPatch
    {
        public static void Prefix(ref GameData.Core.Collections.Sellable result, ref GameData.Core.Collections.Recipe recipe)
        {
            Plugin.Print("Selection hooked！");
            var selection = GameObject.FindObjectOfType<NightScene.UI.CookingUtility.CookingSelectionModuleUI>();
            var extra_ingredients = new List<GameData.Core.Collections.Ingredient>();
            foreach (int i in selection.selectedIngredients)
            {
                if (!recipe.Ingredients.Contains(i))
                {
                    extra_ingredients.Add(GameData.Core.Collections.DataBaseCore.RefIngredient(i));
                    result.baseValue += Convert.ToInt32(Math.Round(GameData.Core.Collections.DataBaseCore.RefIngredient(i).baseValue * Plugin.Price_Ratio.Value, 0, MidpointRounding.AwayFromZero));
                }
            };
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
                    foreach (int i in selection.selectedIngredients)
                    {
                        if (!recipe.Ingredients.Contains(i))
                        {
                            detail.baseValue += Convert.ToInt32(Math.Round(GameData.Core.Collections.DataBaseCore.RefIngredient(i).baseValue * Plugin.Price_Ratio.Value, 0, MidpointRounding.AwayFromZero));
                        }
                    }
                }              
            }          
        }
        public static void Postfix(ref GameData.Core.Collections.Sellable detail, int __state)
        {
            detail.baseValue = __state;
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
