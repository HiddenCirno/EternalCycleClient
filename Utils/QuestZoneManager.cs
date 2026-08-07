using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EternalCycleClient.Class;
using Newtonsoft.Json;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EternalCycleClient.Utils
{
    /// <summary>
    /// 此部分参考了WTTCL的代码，基于MIT协议，保留原作者和声明
    /// </summary>
    public static class QuestZoneManager
    {
        private static List<QuestZoneData> _cachedZones;

        /// <summary>
        /// 从服务端拉取全部 Zone，并缓存在内存中
        /// </summary>
        public static void FetchAndCacheZones()
        {
            try
            {
                string json = RequestHandler.GetJson("/eternalcycle/loadquestzone");
                if (!string.IsNullOrWhiteSpace(json))
                {
                    _cachedZones = JsonConvert.DeserializeObject<List<QuestZoneData>>(json);
                    Console.WriteLine($"[EternalCycle]: 成功拉取 {_cachedZones?.Count ?? 0} 个自定义区域");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EternalCycle]: 拉取失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 在进入战局时调用，根据当前地图筛选并创建 Zone
        /// </summary>
        public static void CreateZonesForCurrentMap(GameWorld gameWorld)
        {
            if (_cachedZones == null || _cachedZones.Count == 0)
                return;

            string currentMap = gameWorld.LocationId?.ToLower();
            if (string.IsNullOrEmpty(currentMap))
                return;

            var matchingZones = _cachedZones.Where(z =>
                z.ZoneLocation?.ToLower() == currentMap
            ).ToList();

            foreach (var zone in matchingZones)
            {
                CreateZone(zone);

                if (zone.GroupPosition != null && zone.GroupPosition.Count > 0)
                {
                    foreach (var subTrans in zone.GroupPosition)
                    {
                        // 如果子空间坐标与主空间完全相同，跳过以避免完全重叠的触发器
                        if (IsSamePosition(zone.Position, subTrans.Position))
                            continue;
                        // 构造临时 Zone 对象，共享主 Zone 的核心属性
                        var subZone = new QuestZoneData
                        {
                            ZoneId = zone.ZoneId,
                            ZoneName = zone.ZoneName,
                            ZoneLocation = zone.ZoneLocation,
                            ZoneType = zone.ZoneType,
                            FlareType = zone.FlareType,
                            Position = subTrans.Position,
                            Rotation = subTrans.Rotation,
                            Scale = subTrans.Scale,
                            GroupPosition = null  // 防止无限递归
                        };
                        CreateZone(subZone);
                    }
                }
            }
        }
        private static bool IsSamePosition(ZoneTransform a, ZoneTransform b)
        {
            if (a == null || b == null)
                return false;
            return Mathf.Approximately(a.X, b.X) &&
                   Mathf.Approximately(a.Y, b.Y) &&
                   Mathf.Approximately(a.Z, b.Z);
        }

        private static void CreateZone(QuestZoneData zone)
        {
            try
            {
                Vector3 pos = ParseVector3(zone.Position);
                Vector3 scale = ParseVector3(zone.Scale);
                Quaternion rot = ParseQuaternion(zone.Rotation);

                switch (zone.ZoneType?.ToLower())
                {
                    case "visit":
                        CreateVisitZone(zone.ZoneId, pos, scale, rot);
                        break;
                    case "placeitem":
                        CreatePlaceItemZone(zone.ZoneId, pos, scale, rot);
                        break;
                    case "killbot":
                        CreateKillBotZone(zone.ZoneId, pos, scale, rot);
                        break;
                    case "flarezone":
                        CreateFlareZone(zone.ZoneId, pos, scale, rot, zone.FlareType);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EternalCycle]: 创建 {zone.ZoneId} 失败：{ex.Message}");
            }
        }

        private static void CreateVisitZone(string zoneId, Vector3 pos, Vector3 scale, Quaternion rot)
        {
            var go = CreateBaseGameObject(zoneId, pos, scale, rot);
            var trigger = go.AddComponent<ExperienceTrigger>();
            trigger.SetId(zoneId);
        }

        private static void CreatePlaceItemZone(string zoneId, Vector3 pos, Vector3 scale, Quaternion rot)
        {
            var go = CreateBaseGameObject(zoneId, pos, scale, rot);
            var trigger = go.AddComponent<PlaceItemTrigger>();
            trigger.SetId(zoneId);
        }

        private static void CreateKillBotZone(string zoneId, Vector3 pos, Vector3 scale, Quaternion rot)
        {
            var go = CreateBaseGameObject(zoneId, pos, scale, rot);
            var trigger = go.AddComponent<TriggerWithId>();
            trigger.SetId(zoneId);
        }

        private static void CreateFlareZone(string zoneId, Vector3 pos, Vector3 scale, Quaternion rot, string flareType)
        {
            var go = CreateBaseGameObject(zoneId, pos, scale, rot);
            go.AddComponent<MoveObjectsToAdditionalPhysSceneMarker>();

            // 添加 FlareShootDetectorZone 并设置私有字段
            var flareDetector = go.AddComponent<FlareShootDetectorZone>();
            SetPrivateField(flareDetector, "zoneID", zoneId);
            if (!string.IsNullOrEmpty(flareType) && Enum.TryParse<FlareEventType>(flareType, out var ft))
            {
                SetPrivateField(flareDetector, "flareTypeForHandle", ft);
            }

            // 添加 PhysicsTriggerHandler
            var boxCollider = go.GetComponent<BoxCollider>();
            var triggerHandler = go.AddComponent<PhysicsTriggerHandler>();
            triggerHandler.trigger = boxCollider;

            // 将 handler 注册到 FlareShootDetectorZone 内部的 _triggerHandlers 列表
            var handlersField = typeof(FlareShootDetectorZone).GetField("_triggerHandlers",
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (handlersField != null)
            {
                var handlers = handlersField.GetValue(flareDetector) as List<PhysicsTriggerHandler>;
                handlers?.Add(triggerHandler);
            }
        }

        // ===== 工具方法 =====

        private static GameObject CreateBaseGameObject(string zoneId, Vector3 pos, Vector3 scale, Quaternion rot)
        {
            var go = new GameObject(zoneId);
            go.layer = LayerMask.NameToLayer("Triggers");
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.transform.rotation = rot;
            return go;
        }

        private static Vector3 ParseVector3(ZoneTransform t)
        {
            return new Vector3(
                t.X,
                t.Y,
                t.Z
            );
        }

        private static Quaternion ParseQuaternion(ZoneTransform t)
        {
            return new Quaternion(
                t.X,
                t.Y,
                t.Z,
                t.W
            );
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}