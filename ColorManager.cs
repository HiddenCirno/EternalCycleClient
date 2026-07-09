using UnityEngine;

namespace EternalCycleClient
{
    /// <summary>
    /// 自定义色板
    /// </summary>
    public static class ColorManager
    {
        public static readonly ECColor EnemySafe = new ECColor("#00FF00");
        public static readonly ECColor EnemySafeDestroy = new ECColor("#0000FF");
        public static readonly ECColor EnemyWarning = new ECColor("#FFFF00");
        public static readonly ECColor EnemyWarningDestroy = new ECColor("#FF0000");
        public static readonly ECColor EnemyDangerous = new ECColor("#FF0000");
        public static readonly ECColor EnemyDangerousDestroy = new ECColor("#590000");
        public static readonly ECColor EnemyPartDestroy = new ECColor("#FF00FF");

        public static readonly ECColor HealthBarBG = new ECColor("#333333");
        public static readonly ECColor HealthBarFull = new ECColor("#00FF00");
        public static readonly ECColor HealthBarHalf = new ECColor("#FFFF00");
        public static readonly ECColor HealthBarQuarter = new ECColor("#FF0000");

        public static readonly ECColor AimbotCircle = new ECColor("#FF0000");

        public static readonly ECColor Tripwire = new ECColor("#FF0000");

        public static readonly ECColor Corpse = new ECColor("#800000");

        public static readonly ECColor LootCircle = new ECColor("#FFFFFF");

        public static readonly ECColor Distance = new ECColor("#FFFF00");

        public static readonly ECColor LootTier0 = new ECColor("#FFFFFF");
        public static readonly ECColor LootTier1 = new ECColor("#00AA00");
        public static readonly ECColor LootTier2 = new ECColor("#00A0FF");
        public static readonly ECColor LootTier3 = new ECColor("#AA00AA");
        public static readonly ECColor LootTier4 = new ECColor("#FFAA00");
        public static readonly ECColor LootTier5 = new ECColor("#AA0000");
        public static readonly ECColor LootTier6 = new ECColor("#FF55FF");
        public static readonly ECColor LootTierX = new ECColor("#808080");
        public static readonly ECColor LootTierEX = new ECColor("#DC143C");

        public static readonly ECColor TextGray = new ECColor("#808080");

        public static readonly ECColor ManagerGUIBackground = new ECColor("#FFFFFF");

        public static readonly ECColor PlayerLevel = new ECColor("#7FFF00");
        public static readonly ECColor PMCUSEC = new ECColor("#007CFF");
        public static readonly ECColor PMCBEAR = new ECColor("#FF8C00");
        public static readonly ECColor AllyPlayer = new ECColor("#66CCFF");
        public static readonly ECColor Scav = new ECColor("#FFFF8B");
        public static readonly ECColor Boss = new ECColor("#CE0000");
        public static readonly ECColor Sniper = new ECColor("#00FA9A");
        public static readonly ECColor Raider = new ECColor("#7300A6");
        public static readonly ECColor Follower = new ECColor("#FF2DE9");
        public static readonly ECColor Sectant = new ECColor("#ADFF2F");
        public static readonly ECColor Santa = new ECColor("#00FFFF");
        public static readonly ECColor BTR = new ECColor("#228B22");
        public static readonly ECColor BlackDiv = new ECColor("#DC143C");//WTT compat
        public static readonly ECColor Event = new ECColor("#818ef2");//BloodHound, etc
    }

    /// <summary>
    /// 二次封装的自定义颜色结构, 同时具备字符串和UnityColor隐式转换
    /// </summary>
    public readonly struct ECColor
    {
        public readonly string HexColor;
        public readonly string HexColorNoHash;
        public readonly Color UnityColor;

        /// <summary>
        /// 从十六进制字符串构造颜色
        /// </summary>
        public ECColor(string hex)
        {
            //防御
            HexColor = hex.StartsWith("#") ? hex.ToUpper() : $"#{hex}".ToUpper();
            HexColorNoHash = HexColor.Substring(1);

            //Color
            if (ColorUtility.TryParseHtmlString(HexColor, out Color parsedColor))
            {
                UnityColor = parsedColor;
            }
            else
            {
                //解析失败
                Debug.LogError($"颜色解析失败，无效的代码: {hex}");
                UnityColor = Color.magenta;
            }
        }

        //隐式转换
        public static implicit operator Color(ECColor oc) => oc.UnityColor;

        public static implicit operator string(ECColor oc) => oc.HexColor;

        //覆盖ToString为文本拼接提供兼容
        public override string ToString() => HexColor;
    }
}
