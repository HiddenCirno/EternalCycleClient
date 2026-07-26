using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using ItemTransactionManagerResult = GStruct154<GClass3408>;

namespace EternalCycleClient.Patch
{
    [HarmonyPatch(typeof(KeycardDoor), "UnlockOperation")]
    public class KeyCardDoorUnlockPatch
    {
        [HarmonyPostfix]
        public static void Postfix(KeycardDoor __instance, KeyComponent key, ref GStruct156<KeyInteractionResultClass> __result)
        {
            // 1. 如果原版方法报错了，或者原版已经判断成功了（钥匙匹配），我们直接退出，不插手。
            if (__result.Failed || __result.Value == null || __result.Value.Succeed)
            {
                return;
            }

            // 2. 运行到这里，说明原版方法拒绝了这把钥匙（__result.Value.Succeed == false）。
            // 此时我们来检测它是不是我们的“万能假钥匙”。
            try
            {
                string desc = LocaleManagerClass.LocaleManagerClass.method_4(key.Template.KeyId + " Description");
                bool isFakeKey = FakeKeyPatch.ExtractFirstObjectId(desc) == "A0A0A0A0FDFFFF000A0A0A0A" ||
                                 FakeKeyPatch.ContainsWrappedObjectId(desc, __instance.KeyId);

                // 如果连我们的假钥匙判断也没通过，说明它就是一把错的钥匙，直接退出。
                if (!isFakeKey)
                {
                    return;
                }
            }
            catch
            {
                // 防御性编程：如果解析字符串报错，当作不匹配处理
                return;
            }

            // 3. 核心：它是我们的假钥匙！
            // 因为原版方法提前退出了，没有扣耐久，所以我们必须在这里手动扣除耐久并处理销毁逻辑。
            key.NumberOfUsages++;
            ItemTransactionManagerResult discardResult = default;

            if (key.NumberOfUsages >= key.Template.MaximumNumberOfUsage && key.Template.MaximumNumberOfUsage > 0)
            {
                discardResult = InteractionsHandlerClass.Discard(
                    key.Item,
                    (TraderControllerClass)key.Item.Parent.GetOwner(),
                    false
                );

                if (discardResult.Failed)
                {
                    // 如果销毁钥匙的操作失败了，返回错误信息
                    __result = discardResult.Error;
                    return;
                }
            }

            // 4. 大功告成！强行把结果扭转为成功，并把交易结果塞进去。
            __result = new KeyInteractionResultClass(key, discardResult.Value, succeed: true);
        }
    }
}