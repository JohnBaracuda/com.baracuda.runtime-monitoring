// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring.Source.Types
{
    public class ValidationEvent
    {
        public readonly Action<Action<bool>> AddMethod;
        public readonly  Action<Action<bool>> RemoveMethod;

        public ValidationEvent(Action<Action<bool>> addMethod, Action<Action<bool>> removeMethod)
        {
            AddMethod = addMethod;
            RemoveMethod = removeMethod;
        }
    }
}