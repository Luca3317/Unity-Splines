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
        public void Add(int i) => _curveCaches.Insert(i, new CurveCacher());
        public void Add(int i, int amount) { for (int j = 0; j < amount; j++) _curveCaches.Insert(i, new CurveCacher()); }

        public void Remove() => _curveCaches.RemoveAt(_curveCaches.Count - 1);
        public void Remove(int i) => _curveCaches.RemoveAt(i);
        public void Remove(int i, int amount) { for (int j = 0; j < amount; j++) _curveCaches.RemoveAt(i); }

        private IList<CurveCacher> _curveCaches;
    }
}