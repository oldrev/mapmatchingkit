using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Utility
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> self, in TKey key) where TValue : class
            => self.TryGetValue(key, out var value) ? value : null;
    }
}
