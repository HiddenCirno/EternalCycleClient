using Diz.LanguageExtensions;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using ItemTransactionManagerResult = Diz.LanguageExtensions.OperationResult<EFT.InventoryLogic.DiscardResult>;

namespace EternalCycleClient.Patch
{
    [HarmonyPatch(typeof(WorldInteractiveObject), nameof(WorldInteractiveObject.UnlockOperation))]
    public class DoorUnlockPatch
    {
        [HarmonyPostfix]
        public static void Postfix(WorldInteractiveObject __instance, KeyComponent key, Player player, WorldInteractiveObject wio, ref Option<UnlockResult> __result)
        {
            //sbBSG
            if (__result.Failed && __result.Error is StringError)
            {
                key.NumberOfUsages++;
                ItemTransactionManagerResult gstruct = default;

                if (key.NumberOfUsages >= key.Template.MaximumNumberOfUsage && key.Template.MaximumNumberOfUsage > 0)
                {
                    gstruct = ItemManipulator.Discard(
                        key.Item,
                        (ItemController)key.Item.Parent.GetOwner(),
                        false
                    );

                    if (gstruct.Failed)
                    {
                        __result = gstruct.Error;
                        return;
                    }
                }

                // 4. 强行篡改结果：成功开门！
                __result = new UnlockResult(key, gstruct.Value, true);
            }
        }
    }
}