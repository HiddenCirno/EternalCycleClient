using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EternalCycleClient;
using HarmonyLib;
using ItemTransactionManagerResult = GStruct154<GClass3408>;

namespace EternalCycle
{
    [HarmonyPatch(typeof(WorldInteractiveObject), "UnlockOperation")]
    public class DoorUnlockPatch
    {
        [HarmonyPostfix]
        public static void Postfix(WorldInteractiveObject __instance, KeyComponent key, ref GStruct156<KeyInteractionResultClass> __result)
        {
            // 1. 如果原版已经成功（真钥匙），或者因为技能不够（原版返回错误），直接跳过
            if (__result.Failed || __result.Value == null || __result.Value.Succeed)
            {
                return;
            }

            // 2. 原版因为钥匙 ID 不匹配拒绝了，我们来查验是不是我们的“假钥匙”
            try
            {
                string desc = LocaleManagerClass.LocaleManagerClass.method_4(key.Template.KeyId + " Description");
                bool isFakeKey = FakeKeyPatch.ExtractFirstObjectId(desc) == "A0A0A0A0FDFFFF000A0A0A0A" ||
                                 FakeKeyPatch.ContainsWrappedObjectId(desc, __instance.KeyId);

                // 不是我们的假钥匙，确实是错的钥匙，直接返回，门不开
                if (!isFakeKey)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            // 3. 确认为假钥匙，开始手动扣除耐久
            key.NumberOfUsages++;
            ItemTransactionManagerResult gstruct = default;

            if (key.NumberOfUsages >= key.Template.MaximumNumberOfUsage && key.Template.MaximumNumberOfUsage > 0)
            {
                gstruct = InteractionsHandlerClass.Discard(
                    key.Item,
                    (TraderControllerClass)key.Item.Parent.GetOwner(),
                    false
                );

                if (gstruct.Failed)
                {
                    __result = gstruct.Error;
                    return;
                }
            }

            // 4. 强行篡改结果：成功开门！
            __result = new KeyInteractionResultClass(key, gstruct.Value, true);
        }
    }
}