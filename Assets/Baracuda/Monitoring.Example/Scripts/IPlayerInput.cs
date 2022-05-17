// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)

using System;

namespace Baracuda.Monitoring.Example.Scripts
{
    public interface IPlayerInput
    {
        float Vertical { get; }
        float Horizontal { get; }
        
        float MouseX { get; }
        float MouseY { get; }
        
        bool JumpPressed { get; }
        bool PrimaryFirePressed { get; }
        bool SecondaryFirePressed { get; }

        bool DashPressed { get; }

        event Action ToggleCursor;
    }
}