using HarmonyLib;
using System.Text;
using SkillEffect = GClass1443;


namespace EternalCycleClient.Patch
{
    [HarmonyPatch(typeof(SkillEffect), "GetStringValue")]
    public class AttitudeDisplayPatch
    {
        [HarmonyPrefix]
        public static bool GetStringValue_Prefix(SkillEffect __instance, string postfix, ref string __result)
        {
            StringBuilder stringBuilder = new StringBuilder();

            // 1. 处理 Delay
            if (__instance.Delay > 1f)
            {
                stringBuilder.Append(string.Format("{0} {1}{2}",
                    "Del.".Localized(null),
                    __instance.Delay,
                    "sec".Localized(null)));
            }

            // 2. 修改后的 Duration 逻辑（核心修改点）
            if (__instance.Duration > 0f)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(" / ");
                }

                // 判断是否超过7200秒
                if (__instance.Duration >= 7200f)
                {
                    stringBuilder.Append("Infinity".Localized(null)); // 直接显示 Infinity，不附加 "Dur." 和 "sec"
                }
                else
                {
                    stringBuilder.Append(string.Format("{0} {1}{2}",
                        "Dur.".Localized(null),
                        __instance.Duration,
                        "sec".Localized(null)));
                }
            }

            // 3. 处理 Cost
            if (__instance.Cost > 0)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(" / ");
                }
                stringBuilder.Append(__instance.Cost.ToString() + " HP");
            }

            // 4. 附加 postfix（如果存在）
            if (!string.IsNullOrEmpty(postfix))
            {
                stringBuilder.Append(postfix);
            }

            __result = stringBuilder.ToString();
            return false; // 跳过原始方法
        }


    }

    [HarmonyPatch(typeof(SkillEffect), "GetFullStringValue")]
    public class VulcanCore_AttitudeDisplayTooltipPatch
    {
        [HarmonyPrefix]
        public static bool GetFullStringValue_Prefix(SkillEffect __instance, string displayName, ref string __result)
        {
            // 保持原有的空值检查逻辑
            if (__instance.Delay.IsZero() && __instance.Duration.IsZero() && __instance.Cost == 0)
            {
                __result = string.Empty;
                return false;
            }

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(displayName.Localized(null));

            // 延迟显示保持不变
            if (__instance.Delay > 1f)
            {
                stringBuilder.Append(string.Format("\n{0} {1}{2}",
                    "Delay".Localized(null),
                    __instance.Delay,
                    "sec".Localized(null)));
            }

            // 修改后的持续时间显示逻辑
            if (__instance.Duration > 0f)
            {
                string durationValue = __instance.Duration >= 7200f
                    ? "Infinity".Localized(null)
                    : __instance.Duration.ToString() + "sec".Localized(null);

                stringBuilder.Append(string.Format("\n{0} {1}",
                    "Duration".Localized(null),
                    durationValue));
            }

            // 消耗显示保持不变
            if (__instance.Cost > 0)
            {
                stringBuilder.Append("\n" + __instance.Cost.ToString() + " HP");
            }

            __result = stringBuilder.ToString();
            return false; // 跳过原始方法
        }


    }
}
