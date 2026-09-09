using System;
using System.Collections.Generic;
using UnityEngine;

namespace ReleaseTheArrow.Utils
{
    /// Simple get/release pool for MonoBehaviours that are spawned repeatedly (arrows, in this
    /// game's case) — avoids per-level Instantiate/Destroy churn on boards with up to ~200 arrows.
    public class ObjectPool<T> where T : Component
    {
        private readonly Stack<T> _free = new Stack<T>();
        private readonly Func<T> _factory;

        public ObjectPool(Func<T> factory, int prewarm = 0)
        {
            _factory = factory;
            for (int i = 0; i < prewarm; i++)
            {
                var item = _factory();
                item.gameObject.SetActive(false);
                _free.Push(item);
            }
        }

        public T Get()
        {
            var item = _free.Count > 0 ? _free.Pop() : _factory();
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
            item.gameObject.SetActive(false);
            _free.Push(item);
        }
    }
}
