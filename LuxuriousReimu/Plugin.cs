using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

// Made by Plasmatank. For Mystia's Izakaya.

namespace LuxuriousReimu
{
    [BepInPlugin("Plasmatank.LuxuriousReimu", "LuxuriousReimu", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static BepInEx.Logging.ManualLogSource Logger;

        static public GameData.Core.Collections.NightSceneUtility.SpecialGuest ReimuInstance;

        static public Vector2Int ReimuBudget;

        public static void Print(string msg)
        {
            Logger?.Log(BepInEx.Logging.LogLevel.Message, msg);
        }
        public override void Load()
        {
            Logger = Log;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Print("Plugin is now working.");         
            Harmony.PatchAll();
        }
        [HarmonyPatch(typeof(GameData.RunTime.Common.RunTimePlayerData), nameof(GameData.RunTime.Common.RunTimePlayerData.AddHakureiMoneyBoxDonateNum))]
        public static class DonateHook
        {
            public static bool Prefix(int num)
            {
                Print("Hooked!");
                Print("Donated:" + num.ToString());
                Vector2Int vector = new Vector2Int(ReimuBudget.x + num, ReimuBudget.y + num);
                ReimuInstance.fundRange = vector;             
                return true;
            }
        }
        [HarmonyPatch(typeof(DayScene.SceneManager), nameof(DayScene.SceneManager.Awake))]
        public static class AwakeHook
        {
            public static bool Prefix()
            {
                Print("Good Morning! :D");
                ReimuInstance = GameData.Core.Collections.CharacterUtility.DataBaseCharacter.SpecialGuest[7];
                Vector2Int original_vector = new Vector2Int(150, 300);
                ReimuInstance.fundRange = original_vector;
                ReimuBudget = ReimuInstance.fundRange;
                return true;
            }
        }
    }
}
