using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;

// Made by Plasmatank. For Mystia's Izakaya.

namespace BalancedSakuyaMission
{
    [BepInPlugin("Plasmatank.BalancedSakuyaMission", "BalancedSakuyaMission", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static ConfigEntry<int> amount;

        public static BepInEx.Logging.ManualLogSource Logger;

        static GameData.Profile.SchedulerNode.Reward news_temp;

        static GameData.Profile.SchedulerNode.Reward bevs_temp;


        public static void Debug(string msg)
        {
            Logger?.Log(BepInEx.Logging.LogLevel.Message, msg);
        }
        public override void Load()
        {
            Logger = Log;
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Debug("Plugin is now working.");
            amount = Config.Bind<int>("Config", "amount", 8, "完成咲夜任务后获得的三幻神数量，默认为8(重启游戏生效)");
            Harmony.PatchAll();
        }
        [HarmonyPatch(typeof(Common.UI.ReceivedObjectDisplayerController), nameof(Common.UI.ReceivedObjectDisplayerController.NotifyMissionStart))]
        public static class MessageHook
        {
            public static bool Prefix(string missionName)
            {
                Debug("Hooked!");
                if (missionName == "女仆长的采购委托")
                {
                    ModifyAmount();
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(GameData.RunTime.Common.RunTimePlayerData), nameof(GameData.RunTime.Common.RunTimePlayerData.GetFund))]
        public static class ExamplePatch
        {
            public static void Postfix(ref int __result)
            {
                Debug("Sakuya:"+__result.ToString());
            }
        }

        public static void ModifyAmount()
        {
            var Emo = GameData.RunTime.Common.RunTimeScheduler.trackingMissions;
            int count = 0;
            foreach (var emo in Emo)
            {
                Plugin.Debug(emo.Key.ToString());
                var list = emo.Value;
                foreach (var item in list)
                {
                    Plugin.Debug(item.missionLabel);
                    if (item.missionLabel == "Main_4_ScarletMansion_Loop-Mission_A" || item.missionLabel == "Main_4_ScarletMansion_Loop-Mission_B" || item.missionLabel == "Main_4_ScarletMansion_Loop-Mission_C")
                    {
                        Plugin.Debug("Found!");
                        GameData.Profile.SchedulerNodeCollection.MissionNode sakuyamission = item.GetMissionReference();
                        Plugin.Debug("//////");

                        foreach (var reward in sakuyamission.postRewards)
                        {
                            Plugin.Debug(reward.ToString());
                            if (reward.rewardType == GameData.Profile.SchedulerNode.Reward.RewardType.GiveItem)
                            {
                                Plugin.Debug("Item is delivered!");
                                foreach (var id in reward.rewardIntArray)
                                {
                                    Plugin.Debug(id.ToString());
                                }
                                int beverage = reward.rewardIntArray[0];
                                int[] new_array = new int[amount.Value];
                                for (int i = 0; i < amount.Value; i++)
                                {
                                    new_array[i] = beverage;
                                }
                                reward.rewardIntArray = new_array;
                                bevs_temp = reward;
                            }
                            if (reward.rewardType == GameData.Profile.SchedulerNode.Reward.RewardType.ScheduleNews)
                            {
                                news_temp = reward;
                            }
                        }
                        sakuyamission.postRewards[0] = news_temp;
                        sakuyamission.postRewards[1] = bevs_temp;                     
                        Plugin.Debug(sakuyamission.postRewards[0].ToString() + sakuyamission.postRewards[1].ToString());
                        Plugin.Debug("//////");
                    }
                }
                count++;
                Plugin.Debug("轮询次数:" + count.ToString());
            }

        }
    }
}
