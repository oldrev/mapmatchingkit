using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Index;

namespace Sandwych.MapMatchingKit.Spatial
{
    public readonly struct IndexItemVisitor<T> : IItemVisitor<T>
    {
        public Action<T> Action { get; }

        public IndexItemVisitor(Action<T> action)
        {
            this.Action = action;
        }

        public void VisitItem(T item)
        {
            this.Action(item);
        }
    }

}
