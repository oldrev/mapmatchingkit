using Nito.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.MapMatchingKit.Utility
{
    public static class DequeExtensions
    {
        public static T PeekFirst<T>(this Deque<T> self) =>
            self[0];

        public static T PeekLast<T>(this Deque<T> self) =>
            self[self.Count - 1];
    }
}
