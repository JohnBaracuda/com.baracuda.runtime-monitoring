// Copyright (c) 2022 Jonathan Lang

using UnityEngine;

namespace Baracuda.Monitoring.UIToolkit
{
    internal interface IStyleProvider
    {
        string[] InstanceUnitStyles { get; }
        string[] InstanceGroupStyles { get; }
        string[] InstanceLabelStyles { get; }
        string[] StaticUnitStyles { get; }
        string[] StaticGroupStyles { get; }
        string[] StaticLabelStyles { get; }

        Font GetFont(int fontHash);
        Font DefaultFont { get; }
    }
}