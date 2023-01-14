using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    public static class SplineHelper
    {
        #region Converters
        /// <summary>
        /// Converts a point index to the corresponding segment indeces.
        /// </summary>
        /// <param name="pointIndex">The point index that will be converted.</param>
        /// <returns>The indeces of all segments that contain the point at i.</returns>
        public static IList<int> PointToSegmentIndeces(int pointIndex, int segmentSize, int slideSize)
        {
            List<int> indeces = new List<int>();

            int firstIndex = pointIndex - pointIndex % slideSize;
            int lastIndex = firstIndex + slideSize;

            while (pointIndex <= lastIndex)
            {
                indeces.Add(PointToFirstSegmentIndex(pointIndex, segmentSize, slideSize));
                pointIndex += slideSize;
            }

            return indeces;
        }
        public static int PointToFirstSegmentIndex(int pointIndex, int segmentSize, int slideSize)
            => pointIndex < segmentSize ? 0 : (pointIndex - segmentSize) / slideSize + 1;
        /// <summary>
        /// Converts a segment index to the corresponding point index.
        /// </summary>
        /// <param name="segmentIndex">The segment index to convert.</param>
        /// <returns>The index of the first point contained in this segment.</returns>
        public static int SegmentToPointIndex(int segmentIndex, int segmentSize, int slideSize) => slideSize * segmentIndex;

        public static Vector3 ConvertToSpace(Vector3 vec, SplineSpace currentSpace, SplineSpace newSpace)
        {
            if (newSpace == currentSpace) return vec;

            Vector3 newPosition = vec;
            if (newSpace == SplineSpace.XY)
            {
                if (currentSpace == SplineSpace.XYZ)
                {
                    newPosition.z = 0f;
                }
                else
                {
                    newPosition.y = vec.z;
                    newPosition.z = 0f;
                }
            }
            else if (newSpace == SplineSpace.XZ)
            {
                if (currentSpace == SplineSpace.XYZ)
                {
                    newPosition.y = 0f;
                }
                else
                {
                    newPosition.z = vec.y;
                    newPosition.y = 0f;
                }
            }

            Debug.Log("Setting " + vec + " to " + newPosition + " ( " + currentSpace + " to " + newSpace + " )");

            return newPosition; 
        }

        public static IList<Vector3> SplinePointsToVector(IList<SplinePoint> points)
        {
            List<Vector3> vectors = new List<Vector3>();
            foreach (var item in points) vectors.Add(item.Position);
            return vectors;
        }

        public static (int, float) PercentageToSegmentPercentage(float t)
        {
            int segmentIndex = (int)t;
            if (t % 1 == 0 && t > 0)
            {
                segmentIndex--;
                t = 1f;
            }
            else t %= 1;

            return (segmentIndex, t);
        }

        public static Matrix4x4 CreateTMatrix(float t, int order)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            Matrix4x4 tMatrix = new Matrix4x4();
            switch (order)
            {
                case 0:
                    tMatrix[0, 0] = 1;
                    tMatrix[0, 1] = t;
                    tMatrix[0, 2] = t2;
                    tMatrix[0, 3] = t3;
                    break;

                case 1:
                    tMatrix[0, 0] = 0;
                    tMatrix[0, 1] = 1;
                    tMatrix[0, 2] = 2 * t;
                    tMatrix[0, 3] = 3 * t2;
                    break;

                case 2:
                    tMatrix[0, 0] = 0;
                    tMatrix[0, 1] = 0;
                    tMatrix[0, 2] = 2;
                    tMatrix[0, 3] = 6 * t;
                    break;

                case 3:
                    tMatrix[0, 0] = 0;
                    tMatrix[0, 1] = 0;
                    tMatrix[0, 2] = 0;
                    tMatrix[0, 3] = 6;
                    break;

                default: throw new System.NotImplementedException("");
            }

            return tMatrix;
        }

        public static Matrix4x4 CreatePointMatrix(params Vector3[] points) => CreatePointMatrix((IList<Vector3>)points);
        public static Matrix4x4 CreatePointMatrix(IList<Vector3> points)
        {
            if (points.Count > 4) throw new System.ArgumentException();

            Matrix4x4 pMatrix = new Matrix4x4();
            for (int i = 0; i < points.Count; i++)
                pMatrix.SetRow(i, points[i]);

            return pMatrix;
        }

        public static Matrix4x4 CreateNormalAngleOffsetMatrix(params float[] normalAngles) => CreateNormalAngleOffsetMatrix((IList<float>)normalAngles); 
        public static Matrix4x4 CreateNormalAngleOffsetMatrix(IList<float> normalAngles)
        {
            if (normalAngles.Count > 4) throw new System.ArgumentException();

            Matrix4x4 naMatrix = new Matrix4x4();
            for (int i = 0; i < normalAngles.Count; i++)
                naMatrix[i, 0] = normalAngles[i];

            return naMatrix;
        }

        public static Vector3 Evaluate(float t, int order, Matrix4x4 characteristicMatrix, Matrix4x4 pointMatrix) => (CreateTMatrix(t, order) * characteristicMatrix * pointMatrix).GetRow(0);
        public static Vector3 Evaluate(float t, int order, Matrix4x4 characteristicMatrix, params Vector3[] points) => (CreateTMatrix(t, order) * characteristicMatrix * CreatePointMatrix(points)).GetRow(0);
        public static Vector3 Evaluate(float t, int order, Matrix4x4 characteristicMatrix, IList<Vector3> points) => (CreateTMatrix(t, order) * characteristicMatrix * CreatePointMatrix(points)).GetRow(0);

        public static float GetNormalsModifier(float t, Matrix4x4 characteristicMatrix, params float[] normalAngles) => (CreateTMatrix(t, 0) * characteristicMatrix * CreateNormalAngleOffsetMatrix(normalAngles))[0,0];
        public static float GetNormalsModifier(float t, Matrix4x4 characteristicMatrix, IList<float> normalAngles) => (CreateTMatrix(t, 0) * characteristicMatrix * CreateNormalAngleOffsetMatrix(normalAngles))[0,0];
        #endregion

        public static Bounds GetBounds(ISplineGenerator generator, IList<Vector3> points)
        {
            SplineExtrema extrema = GetExtrema(generator, points);
            return new Bounds((extrema.Maxima + extrema.Minima) / 2, extrema.Maxima - extrema.Minima);
        }

        public static SplineExtrema GetExtrema(ISplineGenerator generator, IList<Vector3> points)
        {
            SplineExtrema extrema = new SplineExtrema();
            foreach (var extremaT in generator.GetExtremaTs(points))
                extrema.InsertValueT(extremaT, generator, points);

            extrema.InsertValueT(0, generator, points);
            extrema.InsertValueT(1, generator, points);
            return extrema;
        }

        public static IReadOnlyList<Vector3> GetFlattened(int accuracy, ISplineGenerator generator, IList<Vector3> points)
        {
            if (accuracy < 1) throw new System.ArgumentOutOfRangeException();

            List<Vector3> flattened = new List<Vector3>();
            //flattened.Add(points[0]);
            for (int i = 0; i < accuracy; i++)
                flattened.Add(generator.Evaluate((float)i / (accuracy - 1), points));

            return flattened.AsReadOnly();
        }

        public static float GetLength(int accuracy, ISplineGenerator generator, IList<Vector3> points)
        {
            if (accuracy < 1) throw new System.ArgumentOutOfRangeException();

            IReadOnlyList<Vector3> flattened = GetFlattened(accuracy, generator, points);
            float length = 0f;
            for (int i = 1; i < flattened.Count; i++)
                length += (flattened[i - 1] - flattened[i]).magnitude;

            return length;
        }

        public static IReadOnlyList<float> GetDistanceLUT(int accuracy, ISplineGenerator generator, IList<Vector3> points, float startingDistance = 0)
        {
            if (accuracy < 1) throw new System.ArgumentOutOfRangeException();

            IReadOnlyList<Vector3> flattened = GetFlattened(accuracy, generator, points);
            List<float> distances = new List<float>();

            Vector3 prevPos = points[0];
            float cumulativeDistance = startingDistance;
            for (int i = 0; i < flattened.Count; i++)
            {
                cumulativeDistance += (flattened[i] - prevPos).magnitude;
                prevPos = flattened[i];
                distances.Add(cumulativeDistance);
            }

            return distances.AsReadOnly();
        }

        public static IReadOnlyList<FrenetFrame> GenerateFrenetFrames(int accuracy, ISplineGenerator generator, IList<Vector3> points, FrenetFrame? initialOrientation = null)
        {
            float step = 1f / accuracy;
            List<FrenetFrame> frames = new List<FrenetFrame>();

            FrenetFrame firstFrame;
            if (initialOrientation == null) firstFrame = GenerateFrenetFrameAt(0, generator, points);
            else firstFrame = initialOrientation.Value;

            frames.Add(firstFrame);
            for (int i = 1; i < accuracy; i++)
            {
                var x0 = frames[frames.Count - 1];
                var t0 = (float)i / accuracy;
                var t1 = t0 + step;

                var x1 = GenerateFrenetFrameAt(t1, generator, points);

                var v1 = x1.origin - x0.origin;
                var c1 = Vector3.Dot(v1, v1);
                var riL = x0.rotationalAxis - v1 * 2 / c1 * Vector3.Dot(v1, x0.rotationalAxis);
                var tiL = x0.rotationalAxis - v1 * 2 / c1 * Vector3.Dot(v1, x0.tangent);

                var v2 = x1.tangent - tiL;
                var c2 = Vector3.Dot(v2, v2);

                x1.rotationalAxis = riL - v2 * 2 / c2 * Vector3.Dot(v1, riL);
                x1.normal = Vector3.Cross(x1.rotationalAxis, x1.tangent);
                frames.Add(x1);
            }

            return frames.AsReadOnly();
        }

        static FrenetFrame GenerateFrenetFrameAt(float t, ISplineGenerator generator, IList<Vector3> points)
        {
            FrenetFrame frame = new FrenetFrame();
            frame.origin = generator.Evaluate(t, points);
            frame.tangent = generator.EvaluateDerivative(t, 1, points);

            Vector3 a = frame.tangent.normalized;
            Vector3 b = generator.EvaluateDerivative(t, 2, points).normalized;

            frame.rotationalAxis = Vector3.Cross(b, a).normalized;
            frame.normal = Vector3.Cross(frame.rotationalAxis, a).normalized;

            return frame;
        }
    }
}