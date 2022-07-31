// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring
{
    [Flags]
    public enum EventDisplay
    {
        None = 0,
        SubCount = 1,
        InvokeCount = 2,
        TrueCount = 4,
        SubInfo = 8,
        Signature = 16
    }
}