using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Violoncello.Routines.Tests
{
    internal class HeapContainer<T> {
        public T Value { get; set; }

        public HeapContainer() {
            Value = default;
        }

        public HeapContainer(T value) {
            Value = value;
        }

        public static implicit operator T(HeapContainer<T> b) => b.Value;
    }
}
