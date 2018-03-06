using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Index;

namespace Sandwych.MapMatchingKit.Spatial.Index
{
    public sealed class NtsIndexItemVisitor<T> : IItemVisitor<T>
    {
        public Action<T> Action { get; }

        public NtsIndexItemVisitor(Action<T> action)
        {
            this.Action = action;
        }

        public void VisitItem(T item)
        {
            this.Action(item);
        }
    }

}
