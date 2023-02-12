using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    public class EditorSpline : MonoBehaviour
    {
        public Spline Spline => _spline;

        public void Init()
        {
            _spline = new Spline(Hermite.HermiteGenerator.Instance, true, Vector3.one, -Vector3.one, Vector3.right, Vector3.left);
        }

        [SerializeField] private Spline _spline;
    }
}
 
 