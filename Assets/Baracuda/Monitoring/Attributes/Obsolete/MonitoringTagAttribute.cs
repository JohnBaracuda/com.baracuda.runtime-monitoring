// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring.Obsolete
{
    [Obsolete("use MTagAttribute instead!")]
    public class MonitoringTagAttribute : MTagAttribute
    {
        public MonitoringTagAttribute(string tag) : base(tag)
        {
        }
        public MonitoringTagAttribute(params string[] tags) : base(tags)
        {
        }
    }
}