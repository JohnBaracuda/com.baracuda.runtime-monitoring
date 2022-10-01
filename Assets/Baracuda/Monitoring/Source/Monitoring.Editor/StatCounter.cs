// Copyright (c) 2022 Jonathan Lang

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Baracuda.Monitoring.Editor
{
    internal class StatCounter
    {
        private readonly Dictionary<string, SortedDictionary<string, int>> _storage =
            new Dictionary<string, SortedDictionary<string, int>>();

        public void SetStat(string stat, int value, string category = "General")
        {
            EnsureCategoryExists(category);
            if (_storage[category].ContainsKey(stat))
            {
                _storage[category][stat] = value;
            }
            else
            {
                _storage[category].Add(stat, value);
            }
        }

        public void ResetStat(string stat, string category = "General")
        {
            EnsureCategoryExists(category);
            if (_storage[category].ContainsKey(stat))
            {
                _storage[category][stat] = 0;
            }
            else
            {
                _storage[category].Add(stat, 0);
            }
        }

        public void IncrementStat(string stat, string category = "General")
        {
            IncrementStat(stat, 1, category);
        }

        public void IncrementStat(string stat, int increment, string category = "General")
        {
            EnsureCategoryExists(category);
            if (_storage[category].ContainsKey(stat))
            {
                _storage[category][stat] += increment;
            }
            else
            {
                _storage[category].Add(stat, increment);
            }
        }

        private void EnsureCategoryExists(string category)
        {
            if (!_storage.ContainsKey(category))
            {
                _storage.Add(category, new SortedDictionary<string, int>());
            }
        }

        public string ToString(bool asComment)
        {
            var sb = new StringBuilder();
            var lineBreak = asComment ? "\n//" : "\n";

            var max = (from keyValuePair in _storage from valuePair in keyValuePair.Value select valuePair.Key.Length).Prepend(0).Max();

            sb.Append(asComment? "//--- Stats ---" : "--- Stats ---");
            sb.Append('\n');

            foreach (var categoryPair in _storage)
            {
                var category = categoryPair.Key;
                var items = categoryPair.Value;
                sb.Append(lineBreak);
                sb.Append(category);
                sb.Append('\n');

                foreach (var entryPair in items)
                {
                    var statName = entryPair.Key;
                    var statValue = entryPair.Value.ToString();

                    sb.Append(lineBreak);
                    sb.Append(statName);
                    sb.Append(':');
                    sb.Append(new string(' ', (max - statName.Length) + 4 - statValue.Length));
                    sb.Append(statValue);
                }

                sb.Append('\n');
            }

            return sb.ToString();
        }
    }
}