// Copyright (c) 2022 Jonathan Lang

using System;

namespace Baracuda.Monitoring.Types
{
    internal class LazySingleton<T> where T : LazySingleton<T>, new()
    {
        internal static T Singleton => lazySingleton.Value;

        private static readonly Lazy<T> lazySingleton = new Lazy<T>(() => new T());
    }
}
