using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using UnhollowerBaseLib;
using System.Text.Json;
using System.Linq;

namespace MystiaBar
{
    [Serializable]
    public class Beverage
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int Level { get; set; }
        public System.Collections.Generic.List<int> Ingredients { get; set; }
        public System.Collections.Generic.List<int> Tags { get; set; }
        public float CookTime { get; set; }
        public string CookerType { get; set; }
        public string SpritePath { get; set; }
        public string SignboardPath { get; set; }
    }

    public static class Utility
    {
        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;
        
        public static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");

            var il2cppArray = (Il2CppStructArray<byte>)data;

            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }

        public static Sprite LoadByIo(string path, string processed_name)
        {
            Sprite Loaded_sprite;
            Texture2D tex = new Texture2D(1, 1);

            if (LoadImage(tex, File.ReadAllBytes(path), false))
            {
                Loaded_sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.height * 1.25f);
            }
            else
            {
                Loaded_sprite = GameData.CoreLanguage.ObjectLanguageBase.defaultNull;
            }
            Loaded_sprite.name = processed_name;

            return Loaded_sprite;
        }

        public static Beverage LoadJson(string path)
        {
            return JsonSerializer.Deserialize<Beverage>(File.ReadAllText(path));
        }

        public static T ToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        public static UnhollowerBaseLib.Il2CppStructArray<int> Refer_Tags(UnhollowerBaseLib.Il2CppStructArray<int> reference)
        {
            List<int> tags = new List<int>();
            if (reference.Contains(-1))
            {
                tags.Add(7);   //无酒精=>清淡
            }
            if (reference.Contains(14))
            {
                tags.Add(34);  //辛=>辣
            }
            if (reference.Contains(2) && reference.Contains(5))
            {
                tags.Add(35);  //高酒精的烧酒=>燃起来了
            }
            if (reference.Contains(12))
            {
                tags.Add(31);  //水果=>果味
            }
            if (reference.Contains(19))
            {
                tags.Add(23);  //提神=>力量涌现 ---觉大人是这么认为的
            }
            return tags.ToArray().TryCast<UnhollowerBaseLib.Il2CppStructArray<int>>();
        }

        public static void Add_Food(GameData.Core.Collections.Sellable food)
        {
            GameData.RunTime.NightSceneUtility.IzakayaTray tray = GameData.RunTime.NightSceneUtility.IzakayaTray.instance;
            GameData.RunTime.NightSceneUtility.IzakayaConfigure cabinet = GameData.RunTime.NightSceneUtility.IzakayaConfigure.instance;
            if (!tray.IsTrayFull)
            {
                tray.Receive(food);
            }
            else
            {
                cabinet.StoreFood(food, 1);
            }
        }
    }
    [HarmonyPatch(typeof(Common.UI.StorageUtility.StorageIngredientUI), nameof(Common.UI.StorageUtility.StorageIngredientUI.Open))]
    public static class IngredientLoadHook
    {
        public static void Postfix(Common.UI.StorageUtility.StorageIngredientUI __instance)
        {
            var View = GameObject.Find("IngredientPannel");
            var Content = View.transform.GetChild(0).GetChild(0).GetChild(3);
            var childs = Enumerable.Range(0, Content.childCount);
            foreach (int i in childs)
            {
                var image = Content.GetChild(i).GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>();
                if (!image.activeSprite.name.Contains("Ingredient"))
                {
                    var strings = image.activeSprite.name.Split("_", StringSplitOptions.RemoveEmptyEntries);
                    int Real_ID = (Plugin.Custom_Beverage.Contains(Convert.ToInt32(strings[^1])) ? 0 : Plugin.Start_Index.Value) + Convert.ToInt32(strings[^1]);

                    Action action = () =>
                    {
                        if (GameData.RunTime.Common.RunTimeStorage.Ingredients[Real_ID] > 0)
                        {
                            Plugin.Print("Converted: " + Real_ID.ToString());
                            GameData.RunTime.Common.RunTimeStorage.IngredientOut(Real_ID);
                            __instance.UpdatePannelElement();
                            var list = new List<int>();
                            list.Add((Plugin.Custom_Beverage.Contains(Real_ID) ? 0 : -Plugin.Start_Index.Value) + Real_ID);
                            GameData.RunTime.Common.RunTimeStorage.BeverageInRange(list.Cast<IEnumerable<int>>());
                        }
                    };
                    Il2CppSystem.Action final_action = action;

                    if (Content.GetChild(i).gameObject.GetComponent<DEYU.UniversalUISystem.InteractableBase>().onSubmitAction is null)
                    {
                        Content.GetChild(i).gameObject.GetComponent<DEYU.UniversalUISystem.InteractableBase>().onSubmitAction = final_action;
                    }
                }            
            }
        }
    }

    [HarmonyPatch(typeof(Common.UI.StorageUtility.StorageIngredientUI), nameof(Common.UI.StorageUtility.StorageIngredientUI.UpdatePannelElement))]
    public static class IngredientUpdateHook
    {
        public static void Postfix(Common.UI.StorageUtility.StorageIngredientUI __instance)
        {
            var View = GameObject.Find("IngredientPannel");
            var Content = View.transform.GetChild(0).GetChild(0).GetChild(3);
            var childs = Enumerable.Range(0, Content.childCount);
            foreach (int i in childs)
            {
                var image = Content.GetChild(i).GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>();
                if (!image.activeSprite.name.Contains("Ingredient"))
                {
                    var strings = image.activeSprite.name.Split("_", StringSplitOptions.RemoveEmptyEntries);
                    int Real_ID = (Plugin.Custom_Beverage.Contains(Convert.ToInt32(strings[^1])) ? 0 : Plugin.Start_Index.Value) + Convert.ToInt32(strings[^1]);

                    Action action = () =>
                    {
                        if (GameData.RunTime.Common.RunTimeStorage.Ingredients[Real_ID] > 0)
                        {
                            Plugin.Print("Converted: " + Real_ID.ToString());
                            GameData.RunTime.Common.RunTimeStorage.IngredientOut(Real_ID);
                            __instance.UpdatePannelElement();
                            var list = new List<int>();
                            list.Add((Plugin.Custom_Beverage.Contains(Real_ID) ? 0 : -Plugin.Start_Index.Value) + Real_ID);
                            GameData.RunTime.Common.RunTimeStorage.BeverageInRange(list.Cast<IEnumerable<int>>());
                        }
                    };
                    Il2CppSystem.Action final_action = action;
                    Content.GetChild(i).gameObject.GetComponent<DEYU.UniversalUISystem.InteractableBase>().onSubmitAction = final_action;
                }               
            }
        }
    }
}
