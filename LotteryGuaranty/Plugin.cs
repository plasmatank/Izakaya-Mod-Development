using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnhollowerRuntimeLib;
using UnityEngine.UI;
using Il2CppSystem.Collections.Generic;

// Made by Plasmatank. For Mystia's Izakaya.

namespace LotteryGuaranty
{
    [BepInPlugin("Plasmatank.LotteryGuaranty", "LotteryGuaranty", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static ConfigEntry<int> Lottery_Limit;

        public static ConfigEntry<int> Doll_Limit;

        public static ConfigEntry<bool> Is_Guaranteed;

        public static int Doll_Count;

        public static BepInEx.Logging.ManualLogSource MyLogger;

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

            Lottery_Limit = Config.Bind<int>("Config", "Lottery_Limit", 4, "守矢神龛每日抽奖最大次数。");
            Doll_Limit = Config.Bind<int>("Config", "Doll_Limit", 1, "成美人偶每日抽奖最大次数。");
            Is_Guaranteed = Config.Bind<bool>("Config", "Is_Guaranteed", true, "是否启用守矢神龛抽奖保底(即最后一发必出风祝)?");

        }
        [HarmonyPatch(typeof(DayScene.Interactables.Collections.BehaviourComponents.MoriyaShrineBehaviourComponent), "OnInitialize")]
        public static class SanaeLoadHook
        {
            public static bool Prefix(DayScene.Interactables.Collections.BehaviourComponents.MoriyaShrineBehaviourComponent __instance)
            {               
                __instance.interactCount = Lottery_Limit.Value;
                return true;
            }
        }
        [HarmonyPatch(typeof(DayScene.Interactables.Collections.BehaviourComponents.MoriyaShrineBehaviourComponent), "OnInteract")]
        public static class LotteryLoadHook
        {
            public static bool Prefix(DayScene.Interactables.Collections.BehaviourComponents.MoriyaShrineBehaviourComponent __instance)
            {
                var tracker = GameData.RunTime.Common.StatusTracker.instance;
                if (tracker.GetComponentNum("MoriyaShrine") == Lottery_Limit.Value - 1 && Is_Guaranteed.Value)
                {
                    tracker.IncreaseComponentNum("MoriyaShrine");
                    var Sanae_Wine = new List<int>(); var Sanae_Coin = new List<int>();
                    for (int i = 0; i < Lottery_Limit.Value; i++)
                    {
                        Sanae_Wine.Add(18);
                    }
                    Sanae_Coin.Add(29);
                    GameData.RunTime.Common.RunTimeStorage.BeverageInRange(Sanae_Wine.Cast<IEnumerable<int>>());
                    GameData.RunTime.Common.RunTimeStorage.ItemInRange(Sanae_Coin.Cast<IEnumerable<int>>());
                    GameData.RunTime.DaySceneUtility.RunTimeDayScene.SetActions(GameData.RunTime.DaySceneUtility.RunTimeDayScene.RemainActions - 1);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        [HarmonyPatch(typeof(DayScene.SceneManager), nameof(DayScene.SceneManager.Awake))]
        public static class AwakeHook
        {
            public static void Postfix()
            {
                Doll_Count = 0;
            }
        }

        [HarmonyPatch(typeof(DayScene.Interactables.Collections.BehaviourComponents.NarumiJizoDollComponent), "OnInteract")]
        public static class NarumiLotteryHook
        {
            public static bool Prefix(DayScene.Interactables.Collections.BehaviourComponents.NarumiJizoDollComponent __instance)
            {
                var tracker = GameData.RunTime.Common.StatusTracker.instance;
                if (Doll_Limit.Value > 1)
                {
                    //tracker.hasInitializedComponent["NarumiJizoDoll"] Useless,0=Interactable,1=Uninteractable
                    if (Doll_Count < Doll_Limit.Value)
                    {
                        __instance.GiveItem();
                        Doll_Count++;
                        if (Doll_Count == Doll_Limit.Value)
                        {
                            tracker.IncreaseComponentNum("NarumiJizoDoll");
                        }
                    }                  
                    return false;
                }
                else
                {
                    return true;
                }               
            }
        }
    }     
}
