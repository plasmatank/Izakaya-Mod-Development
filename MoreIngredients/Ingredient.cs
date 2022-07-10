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

namespace MoreIngredients
{
    [Serializable]
    public class Ingredient
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int Level { get; set; }
        public System.Collections.Generic.List<int> Tags { get; set; }
        public string Trader { get; set; }
        public int TradeAmount { get; set; }

        public string SpritePath { get; set; }
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

        public static Sprite LoadByIo(string path)
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

            return Loaded_sprite;
        }

        public static Ingredient LoadJson(string path)
        {
            return JsonSerializer.Deserialize<Ingredient>(File.ReadAllText(path));
        }

        public static T ToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }
    }
}
