using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using Newtonsoft.Json;
using Oracle.Utils;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using static EFT.HealthSystem.ActiveHealthController;
using static EFT.InventoryLogic.InventoryEquipment;

namespace EternalCycleClient
{

    public static class EternalCycleItem
    {
        //按钮文本
        public static string EXACT_CLONE = "<color=#00D0FF><b>永恒之环 : 复制</b></color>";

        public static string PERFECT_CLONE = "<color=#00D0FF><b>永恒之环 : 完美复制</b></color>";

        public static string ITEMID = "94fabbbc70e5e0418be0efbc";

        public static class StashFeature
        {
            public static class EternalCycleItemContext
            {

                public static Item generatedItem;

                //物品实例
                public static class ContextMenuMemory
                {
                    public static Item CurrentItem;
                }


                //捕获实例
                [HarmonyPatch(typeof(ItemUiContext), nameof(ItemUiContext.ShowContextMenu))]
                public class ItemUiContext_ShowContextMenu_Patch
                {
                    [HarmonyPrefix]
                    public static void Prefix(ItemContextAbstractClass itemContext)
                    {
                        //Console.WriteLine(123);
                        ContextMenuMemory.CurrentItem = itemContext?.Item;
                    }
                }


                //显示按钮
                [IgnoreAutoPatch]
                public class SimpleContextMenu_Show_Patch : ModulePatch
                {
                    protected override MethodBase GetTargetMethod()
                    {
                        return typeof(SimpleContextMenu)
                            .GetMethod(nameof(SimpleContextMenu.method_0))
                            .MakeGenericMethod(typeof(EItemInfoButton));
                    }
                    [PatchPrefix]
                    private static void Prefix(ItemInfoInteractionsAbstractClass<EItemInfoButton> contextInteractions, Item item)
                    {
                        //hyw呢, 我的Patch呢???
                        //Console.WriteLine(456);
                        if (!(contextInteractions is ContextInteractionsAbstractClass gclass)) return;

                        var itemContext = gclass.ItemContextAbstractClass;
                        if (itemContext.ViewType == EItemViewType.Inventory)
                        {
                            if (GClass2340.InRaid)
                            {
                                return;
                            }

                            // Save as variable in case we need to add more checks later...
                            var menuUI = Singleton<MenuUI>.Instance;

                            if (menuUI.HideoutAreaTransferItemsScreen.isActiveAndEnabled
                                || menuUI.HideoutMannequinEquipmentScreen.isActiveAndEnabled
                                || menuUI.HideoutCircleOfCultistsScreen.isActiveAndEnabled)
                            {
                                return;
                            }

                            var parentItems = item.GetAllParentItems();
                            if (parentItems.Any(x => x is InventoryEquipment))
                            {
                                return;
                            }

                            if (item.Parent.Container.ParentItem.TemplateId == "55d7217a4bdc2d86028b456d") // Fix for UI Fixes
                            {
                                return;
                            }
                            //物品检测
                            //明天在永恒时序直接测试物品文件编码
                            //Console.WriteLine(789);
                            if (!PluginsCore.StashController.Profile.InventoryInfo.GetPlayerItems().Any(x => x.StringTemplateId == ITEMID)) return;
                            //Console.WriteLine(000);
                            // 1. 保留所有原有的条件过滤（局内、藏身处、装备、锁定等）
                            // ...（你的原有代码，包括 InRaid、Hideout、Parent 等判断，此处省略）

                            // 2. 获取动态菜单字典（如果为 null 则初始化）
                            var dynamicInteractions = gclass.Dictionary_0 ?? new System.Collections.Generic.Dictionary<string, DynamicInteractionClass>();

                            var wishIcon = CacheResourcesPopAbstractClass.Pop<Sprite>("Characteristics/Icons/Disassemble");// + EItemInfoButton.AddToWishlist);


                            dynamicInteractions["EXACT_CLONE"] = new DynamicInteractionClass(
                                "完全复制",                           // 菜单显示的文本
                                EXACT_CLONE,                     // 第二个参数（通常也是标识，可重复显示文本）
                                new Action(() =>                     // 点击后的回调
                                {
                                    var cloneItem = item.CloneItem().ReassignAllIds();
                                    generatedItem = cloneItem;
                                    ItemSpawner.CloneAndSpawnItemIntoStash(cloneItem);
                                    NotificationManagerClass.DisplayMessageNotification($"{ContextMenuMemory.CurrentItem.StringTemplateId} Name".Localized());
                                }),
                                wishIcon                                 // 图标可为 null，也可加载
                            );

                            dynamicInteractions["PERFECT_CLONE"] = new DynamicInteractionClass(
                                "完美复制",
                                PERFECT_CLONE,                     // 第二个参数（通常也是标识，可重复显示文本）
                                new Action(() =>                     // 点击后的回调
                                {
                                    var cloneItem = item.CloneItem().ReassignAllIds().CleanAndResetItem(true);
                                    generatedItem = cloneItem;
                                    ItemSpawner.CloneAndSpawnItemIntoStash(cloneItem);
                                    // 你的完美复制逻辑
                                    NotificationManagerClass.DisplayMessageNotification($"{ContextMenuMemory.CurrentItem.StringTemplateId} Name".Localized());
                                }),
                                wishIcon                                 // 图标可为 null，也可加载
                            );
                        }
                    }
                }

                //改变按钮渲染
                [HarmonyPatch(typeof(InteractionButtonsContainer), nameof(InteractionButtonsContainer.method_3))]
                public class DynamicInteractionWishStylePatch
                {
                    [HarmonyPrefix]
                    public static bool Prefix(InteractionButtonsContainer __instance, DynamicInteractionClass interaction)
                    {
                        if (interaction.Key != EXACT_CLONE && interaction.Key != PERFECT_CLONE) return true;

                        var traverse = Traverse.Create(__instance);

                        var defaultButton = traverse.Field<SimpleContextMenuButton>("_buttonTemplate").Value;

                        var specifiedButtons = traverse.Field<SimpleSpecifiedContextButtons>("_specifiedButtons").Value;

                        var wishTemplate = specifiedButtons?.GetSpecifiedButton(EItemInfoButton.AddToWishlist, defaultButton) ?? defaultButton;

                        var buttonsContainer = traverse.Field<RectTransform>("_buttonsContainer").Value;

                        var method = AccessTools.Method(typeof(InteractionButtonsContainer), "method_1");

                        var button = method.Invoke(__instance, new object[] { interaction.Key, interaction.Key, wishTemplate, buttonsContainer, interaction.Icon, new Action(interaction.Execute), null, false, true });

                        AccessTools.Method(typeof(InteractionButtonsContainer), "method_5").Invoke(__instance, new object[] { button });

                        return false;
                    }
                }

                //捕获InvCtrler实例, 物品生成完整部分
                //直接从Oracle抄就行了
                //顺带补完前面的检测
            }


            public class ItemSpawner
            {
                public static async void CloneAndSpawnItemIntoStash(Item item)
                {
                    var controller = PluginsCore.StashController;
                    if (controller == null || item == null) return;

                    try
                    {
                        ItemAddress targetLocation = FindEmptyLocationInStash(controller, item);
                        if (targetLocation == null)
                        {
                            Notify.Warning("仓库已满！");
                            return;
                        }
                        var addResult = InteractionsHandlerClass.Add(
                            item,
                            targetLocation,
                            controller,
                            true //模拟， 防止冲突
                        );

                        if (addResult.Succeeded)
                        {
                            controller.TryRunNetworkTransaction(addResult);

                            Notify.Message($"{item.Name.Localized()}发送成功");
                        }
                    }
                    catch (Exception ex)
                    {
                        //OracleCommon.ShowError(ex);
                    }
                }

                //物品栏寻址算法
                public static ItemAddress FindEmptyLocation(Player player, Item newItem)
                {
                    //划定物品栏有效区域(胸挂, 口袋, 背包)
                    var equipment = player.Inventory.Equipment;
                    EquipmentSlot[] slotsToCheck = {
                EquipmentSlot.Pockets,
                EquipmentSlot.TacticalVest,
                EquipmentSlot.Backpack
            };
                    //遍历寻址
                    foreach (var slotType in slotsToCheck)
                    {
                        var slot = equipment.GetSlot(slotType);
                        if (slot.ContainedItem is CompoundItem containerItem)
                        {
                            foreach (var grid in containerItem.Grids)
                            {
                                //原版判断方法
                                var addressInGrid = grid.FindLocationForItem(newItem);
                                if (addressInGrid != null)
                                {
                                    return addressInGrid;
                                }
                            }
                        }
                    }
                    return null;
                }

                /// <summary>
                /// 战局外寻址：只找大仓库(Stash)，无视身上装备，绝对安全
                /// </summary>
                public static ItemAddress FindEmptyLocationInStash(InventoryController controller, Item newItem)
                {
                    if (controller?.Inventory?.Stash is CompoundItem stash)
                    {
                        foreach (var grid in stash.Grids)
                        {
                            var location = grid.FindLocationForItem(newItem);
                            if (location != null)
                            {
                                return (ItemAddress)location;
                            }
                        }
                    }
                    return null;
                }
            }

            public static class ItemSpawnStashPatch
            {
                //捕获invctrler
                [HarmonyPatch(typeof(InventoryScreen), "Show", new Type[]
                {
        typeof(IHealthController),
        typeof(InventoryController),
        typeof(AbstractQuestControllerClass),
        typeof(AbstractAchievementControllerClass),
        typeof(AbstractPrestigeControllerClass),
        typeof(CompoundItem),
        typeof(EInventoryTab),
        typeof(ISession),
        typeof(ItemContextAbstractClass),
        typeof(bool)
                })]
                public class InventoryScreen_Show_Patch
                {
                    [HarmonyPostfix]
                    public static void Postfix(InventoryController controller)
                    {
                        if (controller != null)
                        {
                            PluginsCore.StashController = controller;
                        }
                        else
                        {
                            Console.WriteLine("[EternalCycle]由于未知原因，InventoryController为空！");
                        }
                    }
                }

                //桥接请求
                [HarmonyPatch(typeof(TraderControllerClass), "ConvertOperationResultToOperation")]
                public class Patch_ConvertOperation
                {
                    [HarmonyPrefix]
                    public static bool Prefix(TraderControllerClass __instance, IRaiseEvents operationResult, ref BaseInventoryOperationClass __result)
                    {
                        try
                        {
                            //没有物品直接跳过
                            if (EternalCycleItemContext.generatedItem == null) return true;

                            //确认物品
                            Item targetItem = EternalCycleItemContext.generatedItem;

                            //类名检查
                            //3405是ADD
                            //你妈的这段4.1是不是得改
                            string operationTypeName = operationResult.GetType().Name;
                            if (targetItem != null && operationTypeName == "GClass3405")
                            {
                                var method12 = AccessTools.Method(operationResult.GetType().BaseType, "method_12")
                                            ?? AccessTools.Method(__instance.GetType(), "method_12");

                                if (method12 != null)
                                {
                                    ushort txId = (ushort)method12.Invoke(__instance, null);

                                    //桥接到自定义路由
                                    __result = new AddItemRouter.EternalCycleCloneOperationClass(txId, __instance, targetItem);
                                    Console.WriteLine($"[EternalCycle] {operationTypeName}桥接成功");

                                    //清空缓存

                                    //不再执行
                                    return false;
                                }
                                else
                                {
                                    Console.WriteLine("[EternalCycle] 警告： method_12 获取失败！");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //OracleCommon.ShowError(ex);
                        }

                        //正常路由
                        return true;
                    }
                }
            }

            public static class AddItemRouter
            {

                //路由层客户端通信协议
                [Serializable]
                public class EternalCycleCloneCommand : GClass3473
                {
                    //路由请求类型
                    [JsonProperty("Action")]
                    public string Action = "SyncStashExtend";

                    //物品数据
                    [JsonProperty("stashData")]
                    public FlatItemsDataClass[] ItemData;
                }

                //行为描述
                public class EternalCycleCloneDescriptor : BaseDescriptorClass
                {
                    public Item ItemData;

                    public override GStruct152<BaseInventoryOperationClass> ToInventoryOperation(IPlayer player)
                    {
                        var operation = new EternalCycleCloneOperationClass(
                            OperationId,
                            player.InventoryController,
                            ItemData
                        );
                        return operation;
                    }
                }

                //行为执行体
                public class EternalCycleCloneOperationClass : BaseInventoryOperationClass
                {
                    private Item _itemToSpawn;

                    //构造函数
                    public EternalCycleCloneOperationClass(
                    ushort id,
                    TraderControllerClass controller,
                    Item item)
                    : base(id, controller)
                    {
                        _itemToSpawn = item;
                    }

                    public override void ExecuteInternal(Callback callback)
                    {
                        callback?.Invoke(SuccessfulResult.New);
                    }

                    //描述
                    public override BaseDescriptorClass ToDescriptor()
                    {
                        return new EternalCycleCloneDescriptor
                        {
                            Operation = this,
                            ItemData = _itemToSpawn
                        };
                    }

                    //传递数据
                    public override GClass3471 ToBaseInventoryCommand(string ownerId)
                    {
                        var itemFactory = Singleton<ItemFactoryClass>.Instance;
                        return new EternalCycleCloneCommand
                        {
                            ItemData = itemFactory.TreeToFlatItems(new Item[] { _itemToSpawn })
                        };
                    }

                    //回收
                    public override void Dispose()
                    {
                    }
                }
            }
        }

        public static class InRaidFeature
        {
            public static class GodMode
            {
                //无敌Patch
                [HarmonyPatch(typeof(Player), "ApplyDamageInfo")]
                public class GodMode_ApplyDamageInfoPatch
                {
                    public static bool Prefix(Player __instance)
                    {
                        if (!__instance.IsYourPlayer) return true;

                        if (__instance?.Inventory?.CheckItem(ITEMID) ?? false)
                        {
                            return false;
                        }

                        return true;
                    }
                }

                //阻止死亡Patch
                [HarmonyPatch(typeof(ActiveHealthController), "Kill")]
                public static class GodMode_AHCKillPatch
                {
                    public static bool Prefix(ActiveHealthController __instance)
                    {
                        if (!__instance.Player.IsYourPlayer) return true;

                        if (__instance?.Player?.Inventory?.CheckItem(ITEMID) ?? false)
                        {
                            return false;
                        }

                        return true;
                    }
                }

                //阻止部位损毁
                [HarmonyPatch(typeof(ActiveHealthController), "DestroyBodyPart")]
                public static class GodMode_AHCDestroyBodyPartPatch
                {
                    public static bool Prefix(ActiveHealthController __instance)
                    {
                        if (!__instance.Player.IsYourPlayer) return true;

                        if (__instance?.Player?.Inventory?.CheckItem(ITEMID) ?? false)
                        {
                            return false;
                        }

                        return true;
                    }
                }

                [HarmonyPatch(typeof(ActiveHealthController), nameof(ActiveHealthController.ChangeEnergy))]
                public static class LockEnergyPatch
                {
                    public static bool Prefix(ActiveHealthController __instance, ref float value)
                    {
                        if (!__instance.Player.IsYourPlayer) return true;

                        if (__instance?.Player?.Inventory?.CheckItem(ITEMID) ?? false)
                        {
                            value = 0f;
                            return true;
                        }
                        return true;
                    }
                }

                [HarmonyPatch(typeof(ActiveHealthController), nameof(ActiveHealthController.ChangeHydration))]
                public static class LockHydrationPatch
                {
                    public static bool Prefix(ActiveHealthController __instance, ref float value)
                    {
                        if (!__instance.Player.IsYourPlayer) return true;

                        if (__instance?.Player?.Inventory?.CheckItem(ITEMID) ?? false)
                        {
                            value = 0f;
                            return true;
                        }
                        return true;
                    }
                }

                /*

                [HarmonyPatch(typeof(GClass3008), nameof(GClass3008.method_0))]
                class EffectStartPatch
                {
                    static bool Prefix(GClass3008 __instance)
                    {
                        if (__instance.State != EEffectState.None)
                            return true;

                        var player = __instance.HealthController?.Player;
                        if (player == null || !player.IsYourPlayer)
                            return true;

                        //尼基塔死妈妈了, 这里Inv初始化有问题
                        //我修尼玛
                        //if (!player?.Inventory?.CheckItem(ITEMID) ?? false)
                            return true;

                        // 如果只想免疫负面效果，可以在这里判断类型

                        //return false;
                    }
                }
                */
            }

            public static class InfiniteAmmo
            {
                //Patch
                [HarmonyPatch(typeof(BallisticsCalculator), "Shoot", new Type[] { typeof(EftBulletClass) })]
                public class ShootPatch
                {
                    [HarmonyPostfix]
                    public static void Postfix(EftBulletClass shot)
                    {
                        if (!PluginsCore.CorrectGameWorld || !Singleton<ItemFactoryClass>.Instantiated)
                        {
                            return;
                        }

                        if (shot?.Player?.iPlayer?.IsYourPlayer != true)
                        {
                            return;
                        }

                        if (!shot?.Player?.iPlayer?.InventoryController?.Inventory?.CheckItem(ITEMID) ?? false)
                        {
                            return;
                        }

                        if (shot.Ammo == null || !(shot.Weapon is Weapon weapon))
                        {
                            return;
                        }

                        MagazineItemClass currentMagazine = weapon.GetCurrentMagazine();

                        //提取武器弹药
                        if (currentMagazine != null)
                        {
                            //转轮
                            if (currentMagazine is CylinderMagazineItemClass cylinderMag)
                            {
                                foreach (Slot camora in cylinderMag.Camoras)
                                {
                                    if (camora.ContainedItem == null) camora.Add(CreateAmmo(shot.Ammo), false, true);
                                }
                            }
                            //弹匣
                            else if (currentMagazine.Cartridges != null)
                            {
                                if (currentMagazine.Cartridges.Count < currentMagazine.Cartridges.MaxCount) currentMagazine.Cartridges.Add(CreateAmmo(shot.Ammo), false);
                            }
                        }
                        else
                        {
                            //枪膛
                            foreach (Slot chamber in weapon.Chambers)
                            {
                                if (chamber.ContainedItem == null) chamber.Add(CreateAmmo(shot.Ammo), false, true);
                            }
                        }
                    }

                    /// <summary>
                    /// 复制子弹
                    /// </summary>
                    /// <param name="ammo">子弹实例</param>
                    /// <returns>子弹实例</returns>
                    private static Item CreateAmmo(Item ammo)
                    {
                        //重新生成ID
                        string fakeId = ItemInstanceHelper.GenerateSafeHexId(ammo.Template.StringId, $"{DateTime.Now.Ticks}_{Guid.NewGuid()}");// new MongoID();
                        return Singleton<ItemFactoryClass>.Instance.CreateItem(fakeId, ammo.TemplateId, null);
                    }
                }
            }
        }

        public class InfinityStamina
        {
            /// <summary>
            /// 耐力锁定脚本
            /// </summary>
            public class InfinityStaminaComponent : MonoBehaviour
            {
                private Player localPlayer;
                private void Awake()
                {
                    //查找玩家组件
                    localPlayer = gameObject.GetComponent<Player>();
                }
                private void Update()
                {
                    //防御
                    if (localPlayer == null) return;
                    bool isInfinite = localPlayer.Inventory.CheckItem(ITEMID);
                    if (localPlayer.Physical != null)
                    {
                        //定义开关
                        //赋值
                        if (localPlayer.Physical.Stamina != null)
                        {
                            localPlayer.Physical.Stamina.ForceMode = isInfinite;
                        }

                        if (localPlayer.Physical.HandsStamina != null)
                        {
                            localPlayer.Physical.HandsStamina.ForceMode = isInfinite;
                        }

                        if (localPlayer.Physical.Oxygen != null)
                        {
                            localPlayer.Physical.Oxygen.ForceMode = isInfinite;
                        }
                    }
                    if (isInfinite)
                    {
                        var hc = localPlayer.ActiveHealthController;
                        if (hc != null)
                        {
                            hc.RestoreFullHealth();
                            hc.FullRestoreBodyPart(EBodyPart.Head);
                            hc.FullRestoreBodyPart(EBodyPart.Chest);
                            hc.FullRestoreBodyPart(EBodyPart.Stomach);
                            hc.FullRestoreBodyPart(EBodyPart.LeftArm);
                            hc.FullRestoreBodyPart(EBodyPart.RightArm);
                            hc.FullRestoreBodyPart(EBodyPart.LeftLeg);
                            hc.FullRestoreBodyPart(EBodyPart.RightLeg);
                            hc.RemoveNegativeEffects(EBodyPart.Head);
                            hc.RemoveNegativeEffects(EBodyPart.Chest);
                            hc.RemoveNegativeEffects(EBodyPart.Stomach);
                            hc.RemoveNegativeEffects(EBodyPart.LeftArm);
                            hc.RemoveNegativeEffects(EBodyPart.RightArm);
                            hc.RemoveNegativeEffects(EBodyPart.LeftLeg);
                            hc.RemoveNegativeEffects(EBodyPart.RightLeg);
                            hc.RemoveMisfireEffect();
                        }
                    }
                }
            }

            [HarmonyPatch(typeof(GameWorld), "OnGameStarted")]
            public class GameStartPatch
            {
                [HarmonyPostfix]
                public static void Postfix(GameWorld __instance)
                {
                    //挂载脚本
                    __instance.MainPlayer.gameObject.AddComponent<InfinityStaminaComponent>();
                }
            }
        }

        public class InfinityWeight
        {
            //无限负重Patch
            [HarmonyPatch(typeof(InventoryEquipment), "smethod_1")]
            public class InfinityWeightPatch
            {
                public static bool Prefix(InventoryEquipment __instance, IEnumerable<Slot> slots, ref float __result)
                {
                    if ((PluginsCore.StashController != null && PluginsCore.StashController.Inventory.CheckItem(ITEMID)) || (PluginsCore.CorrectGameWorld != null && PluginsCore.CorrectPlayer != null && PluginsCore.CorrectPlayer.Inventory.CheckItem(ITEMID)))
                    {
                        //直接不计重量
                        __result = 0f;
                        return false;
                    }
                    return true;
                }
            }
            [HarmonyPatch(typeof(Class2408), "method_1")]
            public class InfinityWeightPatch2
            {
                public static bool Prefix(Class2408 __instance, Slot slot, ref float __result)
                {
                    if ((PluginsCore.StashController != null && PluginsCore.StashController.Inventory.CheckItem(ITEMID)) || (PluginsCore.CorrectGameWorld != null && PluginsCore.CorrectPlayer != null && PluginsCore.CorrectPlayer.Inventory.CheckItem(ITEMID)))
                    {
                        //直接不计重量
                        __result = 0f;
                        return false;
                    }
                    return true;
                }
            }
        }
        public static class NoMalfunction
        {
            //Patch
            [HarmonyPatch(typeof(Player.FirearmController), nameof(Player.FirearmController.GetMalfunctionState))]
            public class PlayerWeaponNeverJamPatch
            {
                static bool Prefix(Player.FirearmController __instance, ref Weapon.EMalfunctionState __result, ref Weapon.EMalfunctionSource malfunctionSource)
                {
                    //判断是否为自己
                    if (__instance != null && PluginsCore.CorrectPlayer != null && __instance == PluginsCore.CorrectPlayer.HandsController && PluginsCore.CorrectPlayer.Inventory.CheckItem(ITEMID))
                    {
                        //ref结果直接改为无故障
                        __result = Weapon.EMalfunctionState.None;

                        //ref故障来源为调试命令
                        malfunctionSource = Weapon.EMalfunctionSource.ConsoleCommand;

                        //阻止原方法执行
                        return false;
                    }
                    return true;
                }
            }
        }

        public static class NoWeaponDurabilityCost
        {
            //Patch
            [HarmonyPatch(typeof(Weapon), nameof(Weapon.GetDurabilityLossOnShot))]
            public class PlayerWeaponNeverJamPatch
            {
                static bool Prefix(Weapon __instance, float ammoBurnRatio, float overheatFactor, float skillWeaponTreatmentFactor, out float modsBurnRatio, ref float __result)
                {
                    //正常发热动画
                    modsBurnRatio = 1f;
                    if (PluginsCore.CorrectPlayer != null && PluginsCore.CorrectPlayer.Inventory.CheckItem(ITEMID))
                    {
                        //不掉耐久
                        __result = 0f;
                        return false;
                    }
                    return true;
                }
            }
        }

        public static class TelekinisisUnlock
        {
            //拦截互动菜单
            [HarmonyPatch(typeof(GetActionsClass), "GetAvailableActions", new Type[] { typeof(GamePlayerOwner), typeof(GInterface177) })]
            public class GetActionsClassPatch
            {
                public static void Postfix(GamePlayerOwner owner, GInterface177 interactive, ref ActionsReturnClass __result)
                {
                    if (interactive == null || __result == null || PluginsCore.CorrectPlayer == null || !PluginsCore.CorrectPlayer.Inventory.CheckItem(ITEMID)) return;

                    //查找Component
                    Component comp = interactive as Component;
                    if (comp == null) return;

                    //可交互物体
                    WorldInteractiveObject wio = comp.GetComponent<WorldInteractiveObject>() ?? comp.GetComponentInParent<WorldInteractiveObject>();
                    if (wio == null) return;

                    //门(只针对门)
                    if (wio.DoorState == EDoorState.Locked)
                    {
                        //如果没有解锁选项, 就加一个
                        bool hasUnlock = __result.Actions.Any(x => x.Name == "Unlock" || x.Name.Contains("Unlock"));
                        if (!hasUnlock)
                        {
                            //通过Insert让它变为首选项
                            __result.Actions.Insert(0, new ActionsTypesClass
                            {
                                Name = "Unlock",
                                Action = new Action(() =>
                                {
                                    //防止Sain空指针
                                    wio.SetUser(owner.Player);
                                    wio.DoorState = EDoorState.Shut;
                                }),
                                Disabled = false
                            });
                        }
                    }
                }
            }
        }
    }
    public static class ItemInstanceHelper
    {
        //快速哈希预缓存
        [ThreadStatic]
        private static SHA256 _sha256;
        private static readonly char[] HexLookup = "0123456789abcdef".ToCharArray();

        public static bool CheckItem(this Inventory inventory, string itemid)
        {
            return inventory.GetPlayerItems().Any(x => x.StringTemplateId == itemid);
        }

        /// <summary>
        /// 拓展方法, 对物品树进行清洗, 将其变为独立的实例
        /// </summary>
        public static Item ReassignAllIds(this Item clonedItem)
        {
            //生成salt
            var operationSalt = $"{Guid.NewGuid():N}-{DateTime.Now.Ticks}";
            //遍历整个物品树
            foreach (var item in clonedItem.GetAllItems())
            {
                //自带防御的ID读取
                string originalId = string.IsNullOrEmpty(item.Id) ? Guid.NewGuid().ToString("N") : item.Id;
                //加盐生成MongoId, 每一次使用统一的盐, 从而做到从单一实例复制无数个独立实例
                string newSafeId = GenerateSafeHexId(originalId, operationSalt);
                //通过回调设置Id
                ForceSetId(item, newSafeId);
            }
            return clonedItem;
        }

        /// <summary>
        /// 使用sha256生成符合MongoId规范的HEX字符串
        /// </summary>
        public static string GenerateSafeHexId(string originalId, string salt)
        {
            //没什么好注释的, 这种东西在新时代可以直接丢给AI解释了
            if (_sha256 == null) _sha256 = SHA256.Create();
            string input = originalId + salt;
            byte[] hashBytes = _sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            char[] hexBuffer = new char[24];
            for (int i = 0; i < 12; i++)
            {
                byte b = hashBytes[i];
                hexBuffer[i * 2] = HexLookup[b >> 4];
                hexBuffer[i * 2 + 1] = HexLookup[b & 0x0F];
            }
            return new string(hexBuffer);
        }

        /// <summary>
        /// 通过反射回调字段修改Id
        /// </summary>
        private static void ForceSetId(Item item, string newId)
        {
            if (item == null) return;
            var itemType = typeof(Item);
            //直接反射底层回调字段写Id
            FieldInfo backingField = itemType.GetField("<Id>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                                  ?? itemType.GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
            if (backingField != null)
            {
                backingField.SetValue(item, newId);
            }
            else
            {
                //回退(其实完全没必要)
                PropertyInfo idProp = itemType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                if (idProp != null && idProp.CanWrite)
                {
                    idProp.SetValue(item, newId);
                }
            }
        }

        /// <summary>
        /// 拓展方法, 清洗物品状态, 耐久度, 带勾....
        /// </summary>
        public static Item CleanAndResetItem(this Item clonedItem, bool fir)
        {
            //遍历物品树, 对整个树进行操作
            foreach (var item in clonedItem.GetAllItems())
            {
                //每个节点都要带勾
                //子弹会在游戏内部处理, 因此无需特殊处理
                //同步带勾状态而不是只有fir带勾非fir不同步
                item.SpawnedInSession = fir;
                //武器, 护甲....(可维修物品)
                if (item.TryGetItemComponent<RepairableComponent>(out var repairable))
                {
                    //恢复耐久上限和当前耐久
                    repairable.MaxDurability = repairable.TemplateDurability;
                    if (repairable.Durability < repairable.TemplateDurability)
                    {
                        repairable.Durability = repairable.TemplateDurability;
                    }
                }
                //刷新钥匙和钥匙卡的使用次数记录
                if (item.TryGetItemComponent<KeyComponent>(out var key))
                {
                    if (key.NumberOfUsages > 0)
                    {
                        key.NumberOfUsages = 0;
                    }
                }
                //恢复医疗物品的耐久度
                if (item.TryGetItemComponent<MedKitComponent>(out var medkit))
                {
                    if (medkit.HpResource < medkit.MaxHpResource)
                    {
                        medkit.HpResource = medkit.MaxHpResource;
                    }
                }
                //恢复食物和饮料的耐久度
                if (item.TryGetItemComponent<FoodDrinkComponent>(out var food))
                {
                    if (
                    food.HpPercent < food.MaxResource)
                    {
                        food.HpPercent = food.MaxResource;
                    }
                }
                //恢复过滤器, 燃料桶的耐久度
                if (item.TryGetItemComponent<ResourceComponent>(out var resource))
                {
                    if (resource.Value < resource.MaxResource)
                    {

                        resource.Value = resource.MaxResource;
                    }
                }
                //修复面罩的弹孔和裂痕
                if (item.TryGetItemComponent<FaceShieldComponent>(out var faceShield))
                {
                    faceShield.Hits = 0;
                    faceShield.HitSeed = 0;
                }
                //清除武器的故障状态
                if (item is Weapon weapon)
                {
                    weapon.MalfState.State = Weapon.EMalfunctionState.None;
                }
                //重新为维修包充能
                if (item.TryGetItemComponent<RepairKitComponent>(out var repairKit))
                {
                    if (repairKit.Resource < repairKit.RepairKitsTemplateClass.MaxRepairResource)
                    {
                        repairKit.Resource = repairKit.RepairKitsTemplateClass.MaxRepairResource;
                    }
                }
            }
            return clonedItem;
        }
    }
}