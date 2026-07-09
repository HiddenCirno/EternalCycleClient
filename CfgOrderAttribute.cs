using System;

namespace EternalCycleClient
{
    /// <summary>
    /// 配置项排序标签
    /// </summary>
    public class CfgOrderAttribute : Attribute
    {
        public int Order { get; }

        public CfgOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
