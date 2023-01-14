using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    public class SplineCacher : CurveCacher
    {
        public IList<CurveCacher> CurveCaches => _curveCaches;

        public SplineCacher() : base() => _curveCaches = new List<CurveCacher>();

        public CurveCacher this[int i] => _curveCaches[i];

        public void Add() => _curveCaches.Insert(_curveCaches.Count, new CurveCacher());
        public void Insert(int i) => _curveCaches.Insert(i, new CurveCacher());
        public void InsertRange(int i, int amount) { for (int j = 0; j < amount; j++) _curveCaches.Insert(i, new CurveCacher()); }

        public void Remove() => _curveCaches.RemoveAt(_curveCaches.Count - 1);
        public void RemoveAt(int i) => _curveCaches.RemoveAt(i);
        public void RemoveRange(int i, int amount) { for (int j = 0; j < amount; j++) _curveCaches.RemoveAt(i); }

        public void SetSize(int size)
        {
            if (size > _curveCaches.Count) while (size > _curveCaches.Count) _curveCaches.Add(new CurveCacher());
            else while (size < _curveCaches.Count) _curveCaches.RemoveAt(0);
        }

        private List<CurveCacher> _curveCaches;
    }
}