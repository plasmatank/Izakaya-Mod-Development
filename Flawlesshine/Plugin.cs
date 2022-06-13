using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;

// Made by Plasmatank. For Mystia's Izakaya.

namespace Flawlesshine
{
    [BepInPlugin("Plasmatank.Flawlesshine", "Flawlesshine", "1.1.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource MyLogger;


        public static void Print(object msg)
        {
            MyLogger.Log(BepInEx.Logging.LogLevel.Message, msg);
        }
        public override void Load()
        {
            MyLogger = Log;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Print("Highlight is now flawless!");           
            Harmony.PatchAll();
        }
        [HarmonyPatch(typeof(NightScene.SceneManager), nameof(PrepNightScene.SceneManager.Start))]
        public static class HighlightPatch
        {            
            public static void Postfix()
            {
                var Shine = GameData.RunTime.Common.RunTimePlayerData.DefaultPropSelection[GameData.RunTime.Common.PlayerSaveFile.DefaultProp.Recipes];
                var Repeated = new List<int>(); var ToBeRemoved = new List<int>(); var Array = new List<int>();
                GameData.RunTime.NightSceneUtility.IzakayaConfigure menu = GameData.RunTime.NightSceneUtility.IzakayaConfigure.instance;
                foreach (var i in menu.DailyRecipes)
                {
                    if (Shine.Contains(i.id))
                    {
                        Repeated.Add(i.id);
                    }
                    Array.Add(i.id);
                }
                foreach (int i in Shine)
                {
                    if (!Repeated.Contains(i))
                    {
                        Array.Add(i);
                        ToBeRemoved.Add(i);
                    }
                }
                Print(string.Join(",", Array.ToArray()));
                menu.OverrideRecipes(Array.ToArray().TryCast<UnhollowerBaseLib.Il2CppStructArray<int>>());
                foreach (int i in ToBeRemoved)
                {
                    menu.LogoffFromDailyRecipes(i);
                }                
            }            
        }
    }
}
