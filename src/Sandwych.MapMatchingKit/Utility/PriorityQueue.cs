// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information. 

// Copy from https://github.com/Reactive-Extensions/Rx.NET/blob/master/Rx.NET/Source/src/System.Reactive/Internal/PriorityQueue.cs

using System;
using System.Collections;
using System.Collections.Generic;

namespace Sandwych.MapMatchingKit.Utility
{
    public sealed class PriorityQueue<T> where T : IComparable<T>
    {
        private readonly struct IndexedItem
        {
            public T Value { get; }
            public int Id { get; }

            public IndexedItem(in T value, int id)
            {
                this.Value = value;
                this.Id = id;
            }

            public int CompareTo(in IndexedItem other)
            {
                var c = Value.CompareTo(other.Value);
                if (c == 0)
                {
                    c = Id.CompareTo(other.Id);
                }

                return c;
            }

        }

        private static int _count = int.MinValue;
        private IndexedItem[] _items;
        private int _size;

        public PriorityQueue()
            : this(16)
        {
        }

        public PriorityQueue(int capacity)
        {
            _items = new IndexedItem[capacity];
            _size = 0;
        }

        private bool IsHigherPriority(int left, int right)
        {
            ref var leftItem = ref _items[left];
            ref var rightItem = ref _items[right];
            return leftItem.CompareTo(rightItem) < 0;
        }

        private void Percolate(int index)
        {
            if (index >= _size || index < 0)
            {
                return;
            }

            var parent = (index - 1) / 2;
            if (parent < 0 || parent == index)
            {
                return;
            }

            if (IsHigherPriority(index, parent))
            {
                var temp = _items[index];
                _items[index] = _items[parent];
                _items[parent] = temp;
                this.Percolate(parent);
            }
        }

        private void Heapify() => this.Heapify(0);

        private void Heapify(int index)
        {
            if (index >= _size || index < 0)
            {
                return;
            }

            var left = 2 * index + 1;
            var right = 2 * index + 2;
            var first = index;

            if (left < _size && this.IsHigherPriority(left, first))
            {
                first = left;
            }

            if (right < _size && this.IsHigherPriority(right, first))
            {
                first = right;
            }

            if (first != index)
            {
                var temp = _items[index];
                _items[index] = _items[first];
                _items[first] = temp;
                this.Heapify(first);
            }
        }

        public int Count => _size;

        public T Peek()
        {
            if (_size == 0)
            {
                throw new InvalidOperationException("Heap is empty");
            }

            return _items[0].Value;
        }

        private void RemoveAt(int index)
        {
            _items[index] = _items[--_size];

            this.Heapify();
            this.ShrinkWhenRequired();
        }

        private void ShrinkWhenRequired()
        {
            if (_size < _items.Length / 4)
            {
                var temp = _items;
                _items = new IndexedItem[_items.Length / 2];
                Array.Copy(temp, 0, _items, 0, _size);
            }
        }

        public T Dequeue()
        {
            var result = this.Peek();
            this.RemoveAt(0);
            return result;
        }

        public void Enqueue(T item)
        {
            this.GrowWhenRequired();

            var index = _size++;
            _count++;
            _items[index] = new IndexedItem(item, _count);
            this.Percolate(index);
        }

        private void GrowWhenRequired()
        {
            if (_size >= _items.Length)
            {
                var temp = _items;
                _items = new IndexedItem[_items.Length * 2];
                Array.Copy(temp, _items, temp.Length);
            }
        }

        public bool Remove(in T item)
        {
            for (var i = 0; i < _size; ++i)
            {
                if (_items[i].Value.CompareTo(item) == 0)
                {
                    RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

    } //class PriorityQueue

}
