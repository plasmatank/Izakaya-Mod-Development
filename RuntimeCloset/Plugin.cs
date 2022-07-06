using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnhollowerRuntimeLib;
using UnityEngine.UI;
using System.Collections.Generic;
using UniverseLib;
using System.Reflection;
using System;
using System.Linq;

// Made by Plasmatank. For Mystia's Izakaya.

namespace RuntimeCloset
{
    [BepInPlugin("Plasmatank.RuntimeCloset", "RuntimeCloset", "2.0.0")]
    public class Plugin : BasePlugin
    {
        public Harmony Harmony { get; } = new("VeryHarmonious");
        public ConfigEntry<string> ConfigName { get; private set; }

        public static ConfigEntry<KeyCode> WindowKey;

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

            WindowKey = Config.Bind<KeyCode>("Config", "WindowKey", KeyCode.F1, "切换衣柜窗口的开/关");                  

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
        public bool DisplayingWindow = false;
        internal static RuntimeListener Instance { get; private set; }

        Rect windowRect = new Rect(500, 200, 500, 400);

        public static Coroutine Current_Coroutine;
        public static Sprite Current_Sprite;       

        static Sprite Newyear;
        static Sprite Halloween;
        static Sprite Christmas;
        static Sprite Butler;
        static Sprite Routine;
        static Sprite Hostess;
        static Sprite Sparrow;
        static Sprite Sailor;
        static Sprite Miko;
        static Sprite Oldschool;
        static Sprite Idol;
        static Sprite Kimono;

        public static Sprite s = Sprite.Create(new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        public static List<Sprite> Black = new List<Sprite>() {s,s,s,s,s,s};
        public static List<Texture2D> Origin_Black = new List<Texture2D>() {s.texture, s.texture, s.texture, s.texture, s.texture, s.texture};


        public static void search()
        {
            UnityEngine.Object[] Sprites = GameObject.FindObjectsOfTypeIncludingAssets(UnhollowerRuntimeLib.Il2CppType.Of<Sprite>());
            foreach (UnityEngine.Object i in Sprites)
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
                    case "MystiaSparrow":
                        Sparrow = sprite; break;
                    case "MystiaSailor":
                        Sailor = sprite; break;
                    case "MystiaMiko":
                        Miko = sprite; break;
                    case "米斯蒂娅中华校服":
                        Oldschool = sprite; break;
                    case "米斯蒂娅偶像":
                        Idol = sprite; break;
                    case "米斯蒂娅访问和服":
                        Kimono = sprite; break;

                    case "MystiaBlack_1":
                        Black[0] = sprite; Origin_Black[0] = sprite.texture; 
                        break;
                    case "MystiaBlack_2":
                        Black[1] = sprite; Origin_Black[1] = sprite.texture; 
                        break;
                    case "MystiaBlack_3":
                        Black[2] = sprite; Origin_Black[2] = sprite.texture;
                        break;
                    case "MystiaBlack_4":
                        Black[3] = sprite; Origin_Black[3] = sprite.texture;
                        break;
                    case "MystiaBlack_5":
                        Black[4] = sprite; Origin_Black[4] = sprite.texture;
                        break;
                    case "MystiaBlack_6":
                        Black[5] = sprite; Origin_Black[5] = sprite.texture;
                        break;
                }
            }
            Current_Sprite = Butler;
        }

        public static void ImageSetter(Sprite this_sprite, bool animation = false)
        {             
            var portrayal = GameObject.Find("PlayerPortrayal");
            var pre_image = portrayal.GetComponent<Image>();
            var image = pre_image.TryCast<Image>();      
            
            if (Current_Sprite == Black[0])
            {
                RuntimeHelper.StopCoroutine(Current_Coroutine);
            }

            if (!animation)
            {
                if (GameData.RunTime.Common.RunTimeAlbum.GetPlayerClothes().index != 23)
                {
                    image.sprite = this_sprite;                                                           
                }
                else
                {
                    Common.UI.ReceivedObjectDisplayerController controller = Common.UI.ReceivedObjectDisplayerController.Instance;
                    controller.NotifyTextMessage("当穿着黑色套装时，不支持运行时更衣。");                    
                    //Traverse.Create(Black[i]).Property("texture").SetValue(this_sprite.texture);
                    //Black[i].GetType().GetFields().FirstOrDefault(f => f.Name.Contains($"<texture>") && f.Name.Contains("BackingField")).SetValue(Black[i], this_sprite.texture);
                    //尝试反射修改，但属性被IL2CPP包装了，无功而返
                }                   
                
            }
            else
            {
                Current_Coroutine = RuntimeHelper.StartCoroutine(Utility.BlackAnimation(image));               
                if (GameData.RunTime.Common.RunTimeAlbum.GetPlayerClothes().index == 23)
                {
                    Common.UI.ReceivedObjectDisplayerController controller = Common.UI.ReceivedObjectDisplayerController.Instance;
                    controller.NotifyTextMessage("老板娘智商-1!");
                }
            }
            Current_Sprite = this_sprite;
        }

        void Start()
        {
            Plugin.Print("Listener is loaded!");
        }    

        void Update()
        {
            if (UniverseLib.Input.InputManager.GetKeyDown(Plugin.WindowKey.Value))
            {
                Plugin.Print(Plugin.WindowKey.Value.ToString() + " is down!");
                DisplayingWindow = !DisplayingWindow;
            }
        }

        public void OnGUI()
        {
            if (this.DisplayingWindow)
            {
                windowRect = GUI.Window(3090, windowRect, (GUI.WindowFunction)MainWindow, "衣柜");
            }
        }
        public void MainWindow(int id)
        {
            if (GUILayout.Button("切换到新年服装"))
            {
                ImageSetter(Newyear);
            }
            if (GUILayout.Button("切换到万圣节服装"))
            {
                ImageSetter(Halloween);
            }
            if (GUILayout.Button("切换到圣诞节服装"))
            {
                ImageSetter(Christmas);
            }
            if (GUILayout.Button("切换到执事服"))
            {
                ImageSetter(Butler);
            }
            if (GUILayout.Button("切换到日常服装"))
            {
                ImageSetter(Routine);
            }
            if (GUILayout.Button("切换到老板娘营业服"))
            {
                ImageSetter(Hostess);
            }
            if (GUILayout.Button("切换到睡衣"))
            {
                ImageSetter(Sparrow);
            }
            if (GUILayout.Button("切换到水手服"))
            {
                ImageSetter(Sailor);
            }
            if (GUILayout.Button("切换到巫女服"))
            {
                ImageSetter(Miko);
            }
            if (GUILayout.Button("切换到中华校服"))
            {
                ImageSetter(Oldschool);
            }
            if (GUILayout.Button("切换到偶像服"))
            {
                ImageSetter(Idol);
            }
            if (GUILayout.Button("切换到访问和服"))
            {
                ImageSetter(Kimono);
            }
            if (GUILayout.Button("切换到黑色套装(动态)"))
            {
                ImageSetter(Black[0], true);
            }

            GUI.DragWindow();
        }
    }
    public class Utility
    {
        public static System.Collections.IEnumerator BlackAnimation(Image image)
        {
            while (true)
            {
                foreach (Sprite sprite in RuntimeListener.Black)
                {
                    image.sprite = sprite;
                    yield return new WaitForSeconds(0.1f);
                }
            }                       
        }
    }
}
