// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring.Example.Scripts
{
    public enum InputMode
    {
        Character = 0,
        UserInterface = 1
    }
    
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
        event Action<InputMode> InputModeChanged;
    }
}