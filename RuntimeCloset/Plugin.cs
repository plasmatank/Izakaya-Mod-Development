using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnhollowerRuntimeLib;
using UnityEngine.UI;

// Made by Plasmatank. For Mystia's Izakaya.

namespace RuntimeCloset
{
    [BepInPlugin("Plasmatank.RuntimeCloset", "RuntimeCloset", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static ConfigEntry<KeyCode> NewyearKey;
        public static ConfigEntry<KeyCode> HalloweenKey;
        public static ConfigEntry<KeyCode> ChristmasKey;
        public static ConfigEntry<KeyCode> ButlerKey;
        public static ConfigEntry<KeyCode> RoutineKey;
        public static ConfigEntry<KeyCode> HostessKey;

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

            NewyearKey = Config.Bind<KeyCode>("Config", "Newyear", KeyCode.F1, "切换到新年皮肤。");
            HalloweenKey = Config.Bind<KeyCode>("Config", "Halloween", KeyCode.F2, "切换到万圣节皮肤。");
            ChristmasKey = Config.Bind<KeyCode>("Config", "Christmas", KeyCode.F3, "切换到圣诞节皮肤。");
            ButlerKey = Config.Bind<KeyCode>("Config", "Butler", KeyCode.F4, "切换到执事皮肤。");
            RoutineKey = Config.Bind<KeyCode>("Config", "Routine", KeyCode.F5, "切换到普通皮肤。");
            HostessKey = Config.Bind<KeyCode>("Config", "Hostess", KeyCode.F6, "切换到工作服。");         

            ClassInjector.RegisterTypeInIl2Cpp<RuntimeListener>();
            var closet = new GameObject("ClosetInstance");
            closet.AddComponent<RuntimeListener>();
            GameObject.DontDestroyOnLoad(closet);
            closet.hideFlags |= HideFlags.HideAndDontSave;
        }
        [HarmonyPatch(typeof(DayScene.SceneManager), nameof(DayScene.SceneManager.OnDayOver))]
        public static class SceneLoadHook
        {
            public static bool Prefix()
            {
                Print("Scene switch is hooked!");
                RuntimeListener.search();
                return true;
            }
        }
    }
    
    public class RuntimeListener : MonoBehaviour
    {
        static Sprite Newyear;
        static Sprite Halloween;
        static Sprite Christmas;
        static Sprite Butler;
        static Sprite Routine;
        static Sprite Hostess;

        public static void search()
        {
            Object[] Sprites = GameObject.FindObjectsOfTypeIncludingAssets(UnhollowerRuntimeLib.Il2CppType.Of<Sprite>());
            foreach (Object i in Sprites)
            {
                var sprite = i.TryCast<Sprite>();
                switch (sprite.texture.name)
                {
                    case "MystiaNewYear":
                        Newyear = sprite; break;
                    case "MystiaHalloween":
                        Halloween = sprite; break;
                    case "MystiaChristmas":
                        Christmas = sprite; break;
                    case "MystiaButler":
                        Butler = sprite; break;
                    case "米斯蒂娅 普通（微笑）":
                        Routine = sprite; break;
                    case "老板娘 普通（微笑）翅膀垂下":
                        Hostess = sprite; break;
                }
            }
        }

        public static void ImageSetter(Sprite method_sprite)
        {
            var portrayal = GameObject.Find("PlayerPortrayal");
            var pre_image = portrayal.GetComponent<Image>();
            var image = pre_image.TryCast<Image>();
            Plugin.Print(image.ToString());
            image.sprite = method_sprite;
        }

        void Start()
        {
            Plugin.Print("Listener is loaded!");
        }    

        void Update()
        {
            if (UniverseLib.Input.InputManager.GetKeyDown(Plugin.NewyearKey.Value))
            {
                Plugin.Print(Plugin.NewyearKey.Value.ToString() + " is down!");
                ImageSetter(Newyear);
            }


            if (UniverseLib.Input.InputManager.GetKeyDown(Plugin.HalloweenKey.Value))
            {
                Plugin.Print(Plugin.HalloweenKey.Value.ToString() + " is down!");
                ImageSetter(Halloween);
            }


            if (UniverseLib.Input.InputManager.GetKeyDown(Plugin.ChristmasKey.Value))
            {
                Plugin.Print(Plugin.ChristmasKey.Value.ToString() + " is down!");
                ImageSetter(Christmas);
            }


            if (UniverseLib.Input.InputManager.GetKeyDown(Plugin.ButlerKey.Value))
            {
                Plugin.Print(Plugin.ButlerKey.Value.ToString() + " is down!");
                ImageSetter(Butler);
            }


            if (UniverseLib.Input.InputManager.GetKeyDown(Plugin.RoutineKey.Value))
            {
                Plugin.Print(Plugin.RoutineKey.Value.ToString() + " is down!");
                ImageSetter(Routine);
            }


            if (UniverseLib.Input.InputManager.GetKeyDown(Plugin.HostessKey.Value))
            {
                Plugin.Print(Plugin.HostessKey.Value.ToString() + " is down!");
                ImageSetter(Hostess);
            }
        }
    }
}
