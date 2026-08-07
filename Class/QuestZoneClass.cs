using Newtonsoft.Json;
using System.Collections.Generic;

namespace EternalCycleClient.Class
{
    public class QuestZoneData
    {
        [JsonProperty("zoneId")]
        public string ZoneId { get; set; }

        [JsonProperty("zoneName")]
        public string ZoneName { get; set; }

        [JsonProperty("zoneLocation")]
        public string ZoneLocation { get; set; }

        [JsonProperty("zoneType")]
        public string ZoneType { get; set; }

        [JsonProperty("flareType")]
        public string FlareType { get; set; }

        [JsonProperty("position")]
        public ZoneTransform Position { get; set; }

        [JsonProperty("rotation")]
        public ZoneTransform Rotation { get; set; }

        [JsonProperty("scale")]
        public ZoneTransform Scale { get; set; }

        [JsonProperty("groupPosition")]
        public List<QuestZone> GroupPosition { get; set; }
    }

    public class ZoneTransform
    {
        [JsonProperty("x")] public float X { get; set; }
        [JsonProperty("y")] public float Y { get; set; }
        [JsonProperty("z")] public float Z { get; set; }
        [JsonProperty("w")] public float W { get; set; }
    }

    public class QuestZone
    {
        [JsonProperty("position")] public ZoneTransform Position { get; set; }
        [JsonProperty("rotation")] public ZoneTransform Rotation { get; set; }
        [JsonProperty("scale")] public ZoneTransform Scale { get; set; }
    }
}