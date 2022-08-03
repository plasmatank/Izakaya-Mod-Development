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

namespace EarthElevator
{
    [BepInPlugin("Plasmatank.EarthElevator", "EarthElevator", "1.0.0")]
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
            Print("You can use the elevator of EarthSpiritsPalace now!");
            Harmony.PatchAll();
        }

        [HarmonyPatch(typeof(DayScene.DaySceneMap), nameof(DayScene.DaySceneMap.GenerateSpawnMarkerData))]
        public static class TransHook
        {
            public static void Postfix(DayScene.DaySceneMap __instance, Dictionary<string, DayScene.Interactables.SpawnMarker> __result)
            {               
                if (__result.ContainsKey("Sanae"))
                {
                    Print("GoToYoukaiMountain!");
                    if (!__result.ContainsKey("DLC1_YoukaiMountain_Elevator"))
                    {
                        var Origin = GameObject.Find("ToScarletMansion");

                        var elevator = GameObject.Instantiate(Origin);
                        elevator.name = "ToEarthSpiritsPalace";
                        elevator.transform.parent = Origin.transform.parent;
                        elevator.transform.position = new Vector3(17.4f, 19.6f, 0f);
                        elevator.transform.localScale = new Vector3(0.35f, 1f, 1f);
                        var data = elevator.GetComponent<DayScene.Interactables.MapTransitionData>();
                        data.targetSceneLabel = "DLC2_EarthSpiritsPalace";
                        data.targetSceneSpawnMarker = "DLC2_EarthSpiritsPalace_Elevator";

                        var spawn = GameObject.Instantiate(__result["Sanae"]);
                        spawn.name = "FromEarthSpiritsPalace";
                        spawn.transform.parent = Origin.transform.parent;
                        spawn.transform.position = new Vector3(12f, 19.6f, 0f);
                        spawn.spawnMarkerName = "DLC1_YoukaiMountain_Elevator";
                        spawn.targetRotation = DayScene.Input.DayScenePlayerInputGenerator.CharacterRotation.Left;
                        __result["DLC1_YoukaiMountain_Elevator"] = spawn;
                    }                    
                }

                if (__result.ContainsKey("Satori"))
                {
                    Print("GoToEarthSpiritsPalace!");
                    if (!__result.ContainsKey("DLC2_EarthSpiritsPalace_Elevator"))
                    {
                        var Origin = GameObject.Find("ToFormerHell");

                        var elevator = GameObject.Instantiate(Origin);
                        elevator.name = "ToYoukaiMountain";
                        elevator.transform.parent = Origin.transform.parent;
                        elevator.transform.position = new Vector3(-0.7f, 24f, 0f);
                        var data = elevator.GetComponent<DayScene.Interactables.MapTransitionData>();
                        data.targetSceneLabel = "DLC1_YoukaiMountain";
                        data.targetSceneSpawnMarker = "DLC1_YoukaiMountain_Elevator";

                        var spawn = GameObject.Instantiate(__result["Satori"]);
                        spawn.name = "FromYoukaiMountain";
                        spawn.transform.parent = Origin.transform.parent;
                        spawn.transform.position = new Vector3(-0.7f, 20f, 0f);
                        spawn.spawnMarkerName = "DLC2_EarthSpiritsPalace_Elevator";
                        spawn.targetRotation = DayScene.Input.DayScenePlayerInputGenerator.CharacterRotation.Down;
                        __result["DLC2_EarthSpiritsPalace_Elevator"] = spawn;
                    }                    
                }                
            }
        }
    }
}
