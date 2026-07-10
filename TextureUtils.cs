using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.SinglePlayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


namespace EternalCycleClient
{
    public class TextureUtils
    {
        public static Sprite SimpleCreateSprite(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
        }
        public static Sprite SimpleCreateSprite(Texture2D tex, float pixelsPerUnit)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
        public static Texture2D LoadFromFile(string path, int width = 2, int height = 2)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"File not found: {path}");
                return null;
            }
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            //tex.globalMipMapLimit = 0;
            tex.LoadImage(bytes);
            return tex;
        }
    }
}
