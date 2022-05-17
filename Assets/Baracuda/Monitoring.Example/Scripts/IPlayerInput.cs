// Copyright (c) 2022 Jonathan Lang (CC BY-NC-SA 4.0)
namespace Baracuda.Monitoring.Example.Scripts
{
    public interface IPlayerInput
    {
        public float Vertical { get; }
        public float Horizontal { get; }
        
        public float MouseX { get; }
        public float MouseY { get; }
        
        public bool JumpPressed { get; }
        public bool PrimaryFirePressed { get; }
        public bool SecondaryFirePressed { get; }

        public bool DashPressed { get; }
    }
}