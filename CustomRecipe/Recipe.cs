using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Configuration;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnhollowerBaseLib;
using System.Text.Json;

namespace CustomRecipe
{
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
                Loaded_sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            else
            {
                Loaded_sprite = GameData.CoreLanguage.ObjectLanguageBase.defaultNull;
            }

            return Loaded_sprite;
        }
        
        public static Recipe LoadJson(string path)
        {
            return JsonSerializer.Deserialize<Recipe>(File.ReadAllText(path));        
        }

        public static T ToEnum<T>(this string str)
        {
            return (T)Enum.Parse(typeof(T), str);
        }
    }

    [Serializable]
    public class Recipe
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int Level { get; set; }
        public List<int> Ingredients { get; set; }
        public List<int> Tags { get; set; }
        public List<int> BanTags { get; set; }
        public float CookTime { get; set; }
        public string CookerType { get; set; }
        public string FoodSpritePath { get; set; }
        public string SignboardPath { get; set; }
        public bool ReplaceMode { get; set; }
    }
}