using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;

// Made by Plasmatank. For Mystia's Izakaya.

namespace EnhancedScone
{
    [BepInPlugin("Plasmatank.EnhancedScone", "EnhancedScone", "1.1.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource MyLogger;

        static public ConfigEntry<int> amount;

        public static void Print(object msg)
        {
            MyLogger.Log(BepInEx.Logging.LogLevel.Message, msg);
        }
        public override void Load()
        {
            MyLogger = Log;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Print("Scones are now enhanced.");
            amount = Config.Bind<int>("Config", "amount", 3, "这是我烤的<amount>个甜司康饼。您可以坐下来，喝点茶，与我说说您的烦心事。");
            Harmony.PatchAll();  

        }
        [HarmonyPatch(typeof(NightScene.CookingUtility.CookController), nameof(NightScene.CookingUtility.CookController.Extract))]
        public static class ExtractPatch
        {
            public static bool Prefix(ref NightScene.CookingUtility.CookController __instance, out GameData.Core.Collections.Sellable __state)
            {
                Plugin.Print("Extract hooked!");
                __state = __instance.Result;
                return true;
            }
            public static void Postfix(GameData.Core.Collections.Sellable __state)
            {
                int amount = 3;
                Plugin.Print(__state.ToString());
                if (__state.id == 46)
                {
                    GameData.RunTime.NightSceneUtility.IzakayaTray tray = GameData.RunTime.NightSceneUtility.IzakayaTray.instance;
                    GameData.RunTime.NightSceneUtility.IzakayaConfigure cabinet = GameData.RunTime.NightSceneUtility.IzakayaConfigure.instance;
                    for (int i = 0; i < amount - 1; i++)
                    {
                        if (!tray.IsTrayFull)
                        {
                            tray.Receive(__state);
                        }
                        else
                        {
                            cabinet.StoreFood(__state, 1);
                        }
                    }
                }
            }
            static Il2CppSystem.Exception Finalizer()
            {
                Print("收拾厨具中...");
                return null;
            }
        }
    }
}
