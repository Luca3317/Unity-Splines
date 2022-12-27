using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    /*
     * Representation of a full spline. Primarily consists of a segmented point-collection (SplinePoints) and a SplineGenerator.
     * 
     * Maybe inherit from SplinePoints instead of wrapper methods?
     */
    [System.Serializable]
    public class Spline
    {
        public Spline(ISplineGenerator generator, params Vector3[] points) => Init(generator, points);
        public Spline(ISplineGenerator generator, IEnumerable<Vector3> points) => Init(generator, points);
        public Spline(ISplineGenerator generator, SplinePoints points) => Init(generator, points.Points);

        public Vector3 ValueAt(float t)
        {
            (int segmentIndex, float segmentT) = PercentageToSegmentPercentage(t);
            return Generator.Evaluate(segmentT, Points.Segment(segmentIndex));
        }

        public void SetGenerator(ISplineGenerator generator)
        {
            if (generator == Generator) return;
            Generator = generator;
            Points.SetSegmentSizes(Generator.SegmentSize, Generator.SlideSize);
        }

        [SerializeField] protected SplinePoints Points;
        [SerializeField] protected ISplineGenerator Generator; 

        protected void Init(ISplineGenerator generator, IEnumerable<Vector3> points)
        {
            Generator = generator;
            Points = new SplinePoints(Generator.SegmentSize, Generator.SlideSize, points);
        }

        protected (int, float) PercentageToSegmentPercentage(float t)
        {
            int segmentIndex = (int)t;
            if (t % 1 == 0)
            {
                segmentIndex--;
                t = 1f;
            }
            else t %= 1;

            return (segmentIndex, t);          
        }
    }
}