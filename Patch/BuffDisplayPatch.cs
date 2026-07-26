using System;
using HarmonyLib;
using System.Text;
using BuffEffect = GClass3019.GClass3044.GClass3045;


namespace EternalCycleClient.Patch
{
    [HarmonyPatch(typeof(BuffEffect), "GetStringValue")]
    public class BuffDisplayPatch
    {
        [HarmonyPrefix]
        public static bool GetStringValue_Prefix(BuffEffect __instance, ref string __result)
        {
            string str = __instance.BuffColoredStringValue();
            bool flag = __instance.Value.IsZero();
            StringBuilder stringBuilder = new StringBuilder();

            // 处理 Chance
            if (__instance.Chance < 1f)
            {
                stringBuilder.Append(string.Format("{0} {1}%",
                    "UI/ItemAttribute/Chance".Localized(null),
                    Math.Round((double)(__instance.Chance * 100f))));
            }

            // 处理 Delay
            if (__instance.Delay > 1f)
            {
                if (stringBuilder.Length > 0) stringBuilder.Append(" / ");
                stringBuilder.Append(string.Format("{0} {1}{2}",
                    "Del.".Localized(null),
                    __instance.Delay,
                    "sec".Localized(null)));
            }

            // 修改后的 Duration 逻辑
            if (__instance.Duration > 0f)
            {
                if (stringBuilder.Length > 0) stringBuilder.Append(" / ");

                // 核心修改点：判断是否超过7200秒
                if (__instance.Duration < 7200f)
                {
                    stringBuilder.Append(string.Format("{0} {1}{2}",
                        "Dur.".Localized(null),
                        __instance.Duration,
                        "sec".Localized(null)));
                }
                else
                {
                    // 显示 Infinity（需确保本地化支持）
                    stringBuilder.Append(string.Format("{0}",
                        "Infinity".Localized(null)));
                }

                // 附加效果值（如果非零）
                if (!flag)
                {
                    stringBuilder.Append(" (" + str + ")");
                }
            }
            else if (!flag) // 处理 Duration <=0 但有效果值的情况
            {
                if (stringBuilder.Length > 0) stringBuilder.Append(" / ");
                stringBuilder.Append(" " + str);
            }

            __result = stringBuilder.ToString();
            return false; // 跳过原始方法
        }


    }

    [HarmonyPatch(typeof(BuffEffect), "GetFullStringValue")]
    public class VulcanCore_BuffDisplayTooltipPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(BuffEffect __instance, string displayName, ref string __result)
        {
            bool flag = __instance.Value.IsZero();

            // 保持原有的空值检查逻辑
            if (__instance.Delay.IsZero() && (__instance.Duration.IsZero() || __instance.Duration >= 7200f) && __instance.Value.IsZero())
            {
                __result = null;
                return false;
            }

            StringBuilder stringBuilder = new StringBuilder();
            string text = __instance.BuffName.Localized(null);

            // 保持原有的名称处理逻辑
            if (flag && !BuffEffect.HashSet_0.Contains(__instance.BuffName))
            {
                stringBuilder.Append("Applies".Localized(null) + " ");
                text = text.ToLower();
            }
            stringBuilder.Append(text);

            // 保持原有的数值显示逻辑
            if (!flag)
            {
                stringBuilder.Append("\n");
                stringBuilder.Append((__instance.Value.Positive() ? "Increase".Localized(null) : "Decrease".Localized(null)) + " ");
                stringBuilder.Append(__instance.BuffAbsoluteStringValue());
            }

            // 概率显示保持不变
            if (__instance.Chance < 1f)
            {
                stringBuilder.Append(string.Format("\n{0} {1}%", "UI/ItemAttribute/Chance".Localized(null), Math.Round((double)(__instance.Chance * 100f))));
            }

            // 延迟显示保持不变
            if (__instance.Delay > 1f)
            {
                stringBuilder.Append(string.Format("\n{0} {1}{2}", "Delay".Localized(null), __instance.Delay, "sec".Localized(null)));
            }

            // 修改持续时间显示逻辑
            if (__instance.Duration > 0f)
            {
                string durationText = __instance.Duration < 7200f
                    ? string.Format("{0}{1}", __instance.Duration, "sec".Localized(null))
                    : "Infinity".Localized(null);  // 只替换数值部分为Infinity

                stringBuilder.AppendFormat("\n{0} {1}", "Duration".Localized(null), durationText);
            }

            __result = stringBuilder.ToString();
            return false; // 跳过原始方法
        }


    }
}
