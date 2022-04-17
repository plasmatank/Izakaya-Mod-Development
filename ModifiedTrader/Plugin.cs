using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UniverseLib;
using UnhollowerRuntimeLib;

// Made by Plasmatank. For Mystia's Izakaya.

namespace ModifiedTrader
{
    [BepInPlugin("Plasmatank.ModifiedTrader", "ModifiedTrader", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource MyLogger;

        public static ConfigEntry<int> Iberico_amount;

        public static ConfigEntry<KeyCode> Refresh_key;


        public static void Print(object msg)
        {
            MyLogger.Log(BepInEx.Logging.LogLevel.Message, msg);
        }
        public override void Load()
        {
            MyLogger = Log;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Print("The second plugin is now working.");
            Harmony.PatchAll();  

            ClassInjector.RegisterTypeInIl2Cpp<RuntimeListener>();
            var Modifier = new GameObject("ModifierInstance");
            Modifier.AddComponent<RuntimeListener>();
            GameObject.DontDestroyOnLoad(Modifier);
            Modifier.hideFlags |= HideFlags.HideAndDontSave;

            Iberico_amount = Config.Bind<int>("Config", "Iberico_amount", 10, "每日补货黑毛猪肉数量，默认为10(重启游戏生效)");
            Refresh_key = Config.Bind<KeyCode>("Config", "Refresh_key", KeyCode.Alpha0, "刷新女仆备货。");

        }
        [HarmonyPatch(typeof(DayScene.SceneManager), nameof(DayScene.SceneManager.Awake))]
        public static class AwakeLoadHook
        {
            public static bool Prefix()
            {
                Print("Good Morning! :D");
                Modify();
                return true;
            }
        }

        public static void Modify()
        {
            Plugin.Print("Target Located!");
            bool inside = false;
            int index = 0;
            var Product_Helper = GameData.RunTime.DaySceneUtility.RunTimeDayScene.trackedMerchants["Merchant_Maid"].products;
            //Iberico = new GameData.Core.Collections.DaySceneUtility.Collections.Product(GameData.Core.Collections.DaySceneUtility.Collections.Product.ProductType.Ingredient, 15, 100, "");
            //Useless. Fucking IL2CPP garbage collection.
            for (int j = 0; j < Product_Helper.Length; j++)
            {
                if (Product_Helper[j].GetText().Name == "黑毛猪肉")
                {
                    inside = true;
                    index = j;
                }
            }
            GameData.Core.Collections.DaySceneUtility.Collections.Product[] Custom_Array;
            var Iberico = Product_Helper[index];
            if (inside)
            {
                Iberico.productAmount = Iberico_amount.Value;
                Custom_Array = new GameData.Core.Collections.DaySceneUtility.Collections.Product[Product_Helper.Length];
                Custom_Array[index] = Iberico;
                for (int j = 0; j < Product_Helper.Length; j++)
                {
                    if (j != index)
                    {
                        Custom_Array[j] = Product_Helper[j];
                    }
                }
            }
            else
            {
                Custom_Array = new GameData.Core.Collections.DaySceneUtility.Collections.Product[Product_Helper.Length + 1];
                Iberico.productAmount = Iberico_amount.Value;
                Iberico.productType = GameData.Core.Collections.DaySceneUtility.Collections.Product.ProductType.Ingredient;
                Iberico.productId = 15;
                Custom_Array[Product_Helper.Length] = Iberico;
                for (int j = 0; j < Product_Helper.Length; j++)
                {
                    Custom_Array[j] = Product_Helper[j];
                }
            }           
            GameData.RunTime.DaySceneUtility.RunTimeDayScene.trackedMerchants["Merchant_Maid"].products = Custom_Array;
        }
    }
    
    public class RuntimeListener : MonoBehaviour
    {
        public int GoodsIndex;
        
        void Start()
        {
            Plugin.Print("Listener is loaded!");
        }    

        void Update()
        {
            if (UniverseLib.Input.InputManager.GetKeyDown(Plugin.Refresh_key.Value))
            {
                GameData.RunTime.DaySceneUtility.RunTimeDayScene.trackedMerchants["Merchant_Maid"].GenerateProduct();
            }           
        }
    }
}
