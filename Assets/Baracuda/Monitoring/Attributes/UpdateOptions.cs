// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring
{
    [Obsolete("Setting custom update intervals is no longer supported!")] 
    public enum UpdateOptions
    {
        [Obsolete("Setting custom update intervals is no longer supported!")] 
        Auto = -1,
        [Obsolete("Setting custom update intervals is no longer supported!")] 
        DontUpdate = -1,
        [Obsolete("Setting custom update intervals is no longer supported!")] 
        FrameUpdate = -1,
        [Obsolete("Setting custom update intervals is no longer supported!")] 
        TickUpdate = -1,
    }
}