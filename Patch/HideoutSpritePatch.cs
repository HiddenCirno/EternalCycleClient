
using EFT;
using EFT.Hideout;
using HarmonyLib;
using UnityEngine;
using System;
using HideoutResourceManager = EFT.Hideout.HideoutCustomizationController;
using EternalCycleClient.Utils;

namespace EternalCycleClient.Patch
{
    [HarmonyPatch(typeof(HideoutCustomizationIcons), nameof(HideoutCustomizationIcons.GetSprite))]
    public class HideoutGetSpritePatch
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
    [HarmonyPatch(typeof(HideoutResourceManager), nameof(HideoutResourceManager.InstallCustomization), new Type[] { typeof(ResourceKey), typeof(EHideoutCustomizationType) })]
    public class ShootingRangeTargetPatch
    {
        public static bool Prefix(HideoutResourceManager __instance, ResourceKey resourceKey, EHideoutCustomizationType customizationType)
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
                __instance._customizationItemsInstaller.SetPaperTargetTexture(customTargetTex);

                return false; // 只有处理【我们自己的】资源时，才拦截！
            }
            return true;
        }
    }
}