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
        public static void Postfix(WorldInteractiveObject __instance, KeyComponent key, Player player, WorldInteractiveObject wio, ref GStruct156<KeyInteractionResultClass> __result)
        {
            //sbBSG
            if (__result.Failed && __result.Error is GClass1522)
            {
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
}