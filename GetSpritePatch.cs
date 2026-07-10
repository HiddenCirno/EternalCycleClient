
using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using Newtonsoft.Json;
using SPT.Common.Http;
using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UI.Hideout;
using UnityEngine;
using UnityEngine.Windows;
using ShootingRangeTargetResourceManager = GClass2421;

namespace EternalCycleClient
{
    [HarmonyPatch(typeof(HideoutCustomizationIcons), nameof(HideoutCustomizationIcons.GetSprite))]
    public class VulcanCore_GetSprite_Patch
    {
        public static void Postfix(string id, ref Sprite __result)
        {
            ClientResourceManager.DecoIconDict.TryGetValue(id, out Sprite sprite);
            if (sprite != null)
            {
                __result = sprite;
            }
        }
    }
    [HarmonyPatch(typeof(ShootingRangeTargetResourceManager), nameof(ShootingRangeTargetResourceManager.method_2))]
    public class VulcanCore_ShootingRangeTarget_Patch
    {
        public static bool Prefix(ShootingRangeTargetResourceManager __instance, ResourceKey resourceKey, EHideoutCustomizationType customizationType)
        {
            if (customizationType != EHideoutCustomizationType.ShootingRangeMark)
            {
                return true;
            }
            string text = resourceKey.ToAssetName();
            if (text == null) return true;
            if (ClientResourceManager.TargetDict.TryGetValue(text, out Texture2D customTargetTex))
            {
                // 3. 是我们自己的靶纸！我们自己贴图，然后 return false 阻止原版报错
                __instance.HideoutCustomizationItemsInstaller_0.SetPaperTargetTexture(customTargetTex);

                return false; // 只有处理【我们自己的】资源时，才拦截！
            }
            return true;
        }
    }
}