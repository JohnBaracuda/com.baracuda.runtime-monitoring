// Copyright (c) 2022 Jonathan Lang

namespace Baracuda.Reflection
{
    public class VoidRef
    {
        public readonly string Data;

        public VoidRef(object[] data)
        {
            var dataString = "";
            for (int i = 0; i < data.Length; i++)
            {
                dataString += "\n";
                dataString += data[i]?.ToString() ?? "null";
            }

            Data = dataString;
        }
        
        public override string ToString()
        {
            return Data;
        }
    }
}