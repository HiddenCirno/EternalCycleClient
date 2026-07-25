using ItemManager = GClass3380;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using System.Linq;
using System.Text.RegularExpressions;


namespace EternalCycleClient
{
    [HarmonyPatch(typeof(PlayerOwner), "GetKey")]
    public class FakeKeyPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(PlayerOwner __instance, WorldInteractiveObject worldInteractiveObject, ref KeyComponent __result)
        {
            // 获取所有符合条件的仿制钥匙
            var fakekeys = ItemManager.GetItemComponentsInChildren<KeyComponent>(
                __instance.Player.InventoryController.Inventory.Equipment,
                onlyMerged: false
            ).Where(x =>
            {
                string description = LocaleManagerClass.LocaleManagerClass.method_4(x.Template.KeyId + " Description");
                if (ContainsObjectId(description))
                {
                    string targetId = ExtractFirstObjectId(description);
                    if (targetId == "A0A0A0A0FDFFFF000A0A0A0A" ||
                        ContainsWrappedObjectId(description, worldInteractiveObject.KeyId))
                        return true;
                }
                return false;
            });

            // 选择最优仿制钥匙
            var bestFakeKey = fakekeys
                .OrderByDescending(x => x.Template.MaximumNumberOfUsage == 0) // 无限耐久排最前
                .ThenBy(x =>
                {
                    // 计算真实耐久
                    if (x.Template.MaximumNumberOfUsage == 0)
                        return int.MaxValue; // 无限耐久单独优先，不参与耐久比较
                    return x.Template.MaximumNumberOfUsage - x.NumberOfUsages;
                })
                .FirstOrDefault();

            // 普通钥匙匹配
            var normalkey = ItemManager.GetItemComponentsInChildren<KeyComponent>(
                __instance.Player.InventoryController.Inventory.Equipment,
                onlyMerged: false
                )
                .Where(x => x.Template.KeyId == worldInteractiveObject.KeyId) // 先筛选出正确的钥匙
                .OrderByDescending(x => x.Template.MaximumNumberOfUsage == 0) // 1. 无限耐久排最前
                .ThenBy(x =>
                    {
                        // 2. 剩余耐久少的排前面（优先用快坏掉的钥匙）
                        if (x.Template.MaximumNumberOfUsage == 0) return int.MaxValue;
                        return x.Template.MaximumNumberOfUsage - x.NumberOfUsages;
                    })
                    .ThenBy(x => x.Item.SpawnedInSession) // 3. 核心：优先使用不带勾的！
                    .FirstOrDefault();

            // 结果优先使用仿制钥匙
            __result = bestFakeKey ?? normalkey;

            return false; // 跳过原始方法
        }

        // 正则：匹配「」包裹的24位十六进制字符串
        private static readonly Regex ObjectIdRegex = new Regex("「([0-9a-fA-F]{24})」", RegexOptions.Compiled);

        /// <summary>
        /// 判断字符串是否包含至少一个 ObjectId
        /// </summary>
        public static bool ContainsObjectId(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return ObjectIdRegex.IsMatch(input);
        }

        /// <summary>
        /// 提取字符串中第一个 ObjectId，如果没有匹配返回 null
        /// </summary>
        public static string ExtractFirstObjectId(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;

            Match match = ObjectIdRegex.Match(input);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        /// <summary>
        /// 提取字符串中所有被「」包裹的 24 位哈希 ObjectId，并判断是否包含指定目标
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="target">要查找的目标 ObjectId</param>
        /// <returns>true 如果目标 ObjectId 在提取结果中，否则 false</returns>
        public static bool ContainsWrappedObjectId(string input, string target)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(target))
                return false;

            MatchCollection matches = ObjectIdRegex.Matches(input);
            foreach (Match match in matches)
            {
                if (match.Groups[1].Value == target)
                    return true;
            }

            return false;
        }
    }
}
