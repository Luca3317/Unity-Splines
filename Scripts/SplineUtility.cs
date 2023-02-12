using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    public static class SplineUtility
    {
        public static Vector3 ConvertToSpace(Vector3 vec, SplineSpace currentSpace, SplineSpace newSpace) => ConvertToSpace(vec, Vector3.zero, currentSpace, newSpace);
        public static Vector3 ConvertToSpace(Vector3 vec, Vector3 splinePosition, SplineSpace currentSpace, SplineSpace newSpace)
        {
            if (newSpace == currentSpace) return vec;

            Vector3 newPosition = vec;
            if (newSpace == SplineSpace.XY)
            {
                if (currentSpace == SplineSpace.XYZ)
                {
                    newPosition.z = splinePosition.z;
                }
                else
                {
                    newPosition.y = vec.z;
                    newPosition.z = splinePosition.z;
                }
            }
            else if (newSpace == SplineSpace.XZ)
            {
                if (currentSpace == SplineSpace.XYZ)
                {
                    newPosition.y = splinePosition.y;
                }
                else
                {
                    newPosition.z = vec.y;
                    newPosition.y = splinePosition.y;
                }
            }

            return newPosition;
        }

        public static SplineSpace GetSpaceOf(params Vector3[] vectors)
        {
            byte space = 0b100;
            for (int i = 0; i < vectors.Length; i++)
            {
                if (vectors[i].y != 0)
                {
                    space += 0b010;
                    break;
                }
            }
            for (int i = 0; i < vectors.Length; i++)
            {
                if (vectors[i].z != 0)
                {
                    space += 0b001;
                    break;
                }
            }

            return (SplineSpace)space;
        }

        public static IList<Vector3> SplinePointsToVector(IEnumerable<SplinePoint> points)
        {
            List<Vector3> vectors = new List<Vector3>();
            foreach (var item in points) vectors.Add(item.Position);
            return vectors;
        }

        public static IList<SplinePoint> VectorsToSplinePoints(IEnumerable<Vector3> vectors)
        {
            List<SplinePoint> points = new List<SplinePoint>();
            foreach (var item in vectors) points.Add(new SplinePoint(item));
            return points;
        }

        public static int ToSegmentPercentage(ref float t)
        {
            int segmentIndex = (int)t;
            if (t % 1 == 0 && t > 0)
            {
                segmentIndex--;
                t = 1f;
            }
            else t %= 1;

            return segmentIndex;
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

        public static float GetNormalsModifier(float t, Matrix4x4 characteristicMatrix, params float[] normalAngles) => (CreateTMatrix(t, 0) * characteristicMatrix * CreateNormalAngleOffsetMatrix(normalAngles))[0, 0];
        public static float GetNormalsModifier(float t, Matrix4x4 characteristicMatrix, IList<float> normalAngles) => (CreateTMatrix(t, 0) * characteristicMatrix * CreateNormalAngleOffsetMatrix(normalAngles))[0, 0];

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

            Vector3 prevPos = generator.Evaluate(0, points);
            float cumulativeDistance = startingDistance;
            for (int i = 0; i < flattened.Count; i++)
            {
                cumulativeDistance += (flattened[i] - prevPos).magnitude;
                prevPos = flattened[i];
                distances.Add(cumulativeDistance);
            }

            return distances.AsReadOnly();
        }

        public static IList<FrenetFrame> GenerateFrenetFrames(int accuracy, ISplineGenerator generator, IList<Vector3> points, FrenetFrame? initialOrientation = null)
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

            return frames;
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

        #region PosRotScale
        public static Vector3 ApplyPosRotScale(Vector3 pointPos, Vector3 pivot, PosRotScale posRotScale)
        {
            return ApplyScale(ApplyRotation(ApplyPosition(pointPos, posRotScale), pivot, posRotScale), pivot, posRotScale);
        }

        public static Vector3 ApplyPosition(Vector3 pointPos, Vector3 position)
        {
            return pointPos + position;
        }

        public static Vector3 ApplyPosition(Vector3 pointPos, PosRotScale posRotScale)
        {
            return pointPos + posRotScale.position;
        }

        public static Vector3 ApplyRotation(Vector3 pointPos, Vector3 pivot, Quaternion rotation)
        {
            return RotateAroundPivot(pointPos, pivot, rotation.eulerAngles);
        }

        public static Vector3 ApplyRotation(Vector3 pointPos, Vector3 pivot, PosRotScale posRotScale)
        {
            return RotateAroundPivot(pointPos, pivot, posRotScale.rotation.eulerAngles);
        }

        public static Vector3 ApplyScale(Vector3 pointPos, Vector3 pivot, Vector3 scale)
        {
            Vector3 relativeStartPos = pointPos - pivot;
            relativeStartPos = new Vector3(relativeStartPos.x * scale.x, relativeStartPos.y * scale.y, relativeStartPos.z * scale.z);
            return relativeStartPos + pivot;
        }

        public static Vector3 ApplyScale(Vector3 pointPos, Vector3 pivot, PosRotScale posRotScale)
        {
            return ApplyScale(pointPos, pivot, posRotScale.scale);
        }

        // Any built in way to rotate around a point?
        private static Vector3 RotateAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 direction = point - pivot;
            direction = Quaternion.Euler(angles) * direction;
            point = direction + pivot;
            return point;
        }

        public static Vector3 ApplyPosRotScale(Vector3 point, PosRotScale posRotScale)
        {
            return posRotScale.rotation * (point + posRotScale.position);
        }

        public static Vector3 UnapplyPosRotScale(Vector3 point, PosRotScale posRotScale)
        {
            return Quaternion.Inverse(posRotScale.rotation) * (point - posRotScale.position);
        }
        #endregion

        #region Intersections
        public static bool LinesIntersect(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, SplineSpace dimension)
            => LinesIntersect(start1, end1, start2, end2, dimension, MathUtility.defaultIntersectionEpsilon);
        public static bool LinesIntersect(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, float epsilon)
            => LinesIntersect(start1, end1, start2, end2, null, epsilon);

        public static bool LinesIntersect(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, SplineSpace? dimension = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            if (dimension == null) dimension = GetSpaceOf(start1, end1, start2, end2);

            switch (dimension)
            {
                case SplineSpace.XY:
                    return (start1.x - end1.x) * (start2.y - end2.y) - (start1.y - end1.y) * (start2.x - end2.x) != 0;
                case SplineSpace.XZ:
                    return (start1.x - end1.x) * (start2.z - end2.z) - (start1.z - end1.z) * (start2.z - end2.z) != 0;
                default:
                    var res = MathUtility.LinesIntersection3D(start1, end1, start2, end2);
                    return res.Item1;
            }
        }

        public static bool LineSegmentsIntersect(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, SplineSpace dimension)
            => LineSegmentsIntersect(start1, end1, start2, end2, dimension, MathUtility.defaultIntersectionEpsilon);
        public static bool LineSegmentsIntersect(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, float epsilon)
            => LineSegmentsIntersect(start1, end1, start2, end2, null, epsilon);

        public static bool LineSegmentsIntersect(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, SplineSpace? dimension = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            if (dimension == null) dimension = GetSpaceOf(start1, end1, start2, end2);

            switch (dimension)
            {
                case SplineSpace.XY:
                    float d = (end2.x - start2.x) * (start1.y - end1.y) - (start1.x - end1.x) * (end2.y - start2.y);
                    if (d == 0) return false;
                    float t = ((start2.y - end2.y) * (start1.x - start2.x) + (end2.x - start2.x) * (start1.y - start2.y)) / d;
                    float u = ((start1.y - end1.y) * (start1.x - start2.x) + (end1.x - start1.x) * (start1.y - start2.y)) / d;
                    return t >= 0 && t <= 1 && u >= 0 && u <= 1;
                case SplineSpace.XZ:
                    d = (end2.x - start2.x) * (start1.z - end1.z) - (start1.x - end1.x) * (end2.z - start2.z);
                    if (d == 0) return false;
                    t = ((start2.z - end2.z) * (start1.x - start2.x) + (end2.x - start2.x) * (start1.z - start2.z)) / d;
                    u = ((start1.z - end1.z) * (start1.x - start2.x) + (end1.x - start1.x) * (start1.z - start2.z)) / d;
                    return t >= 0 && t <= 1 && u >= 0 && u <= 1;
                default:
                    var res = MathUtility.LinesIntersection3D(start1, end1, start2, end2);

                    return res.Item1 && (res.Item3 - res.Item2).magnitude < epsilon && MathUtility.IsBetween(start1, end1, res.Item2, epsilon) && MathUtility.IsBetween(start2, end2, res.Item3, epsilon);
            }
        }

        public static (bool, Vector3) LinesIntersectionPoint(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, SplineSpace dimension)
            => LinesIntersectionPoint(start1, end1, start2, end2, dimension, MathUtility.defaultIntersectionEpsilon);
        public static (bool, Vector3) LinesIntersectionPoint(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, float epsilon)
            => LinesIntersectionPoint(start1, end1, start2, end2, null, epsilon);

        public static (bool, Vector3) LinesIntersectionPoint(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, SplineSpace? dimension = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            if (dimension == null) dimension = GetSpaceOf(start1, end1, start2, end2);

            float d, t, n;
            switch (dimension)
            {
                case SplineSpace.XY:
                    d = (start1.x - end1.x) * (start2.y - end2.y) - (start1.y - end1.y) * (start2.x - end2.x);
                    if (d == 0) return (false, Vector3.zero);
                    n = (start1.x - start2.x) * (start2.y - end2.y) - (start1.y - start2.y) * (start2.x - end2.x);
                    t = n / d;
                    return (true, start1 + (end1 - start1) * t);
                case SplineSpace.XZ:
                    d = (start1.x - end1.x) * (start2.z - end2.z) - (start1.z - end1.z) * (start2.x - end2.x);
                    if (d == 0) return (false, Vector3.zero);
                    n = (start1.x - start2.x) * (start2.z - end2.z) - (start1.z - start2.z) * (start2.x - end2.x);
                    t = n / d;
                    return (true, start1 + (end1 - start1) * t);
                default:
                    var res = MathUtility.LinesIntersection3D(start1, end1, start2, end2);
                    return (res.Item1, (res.Item2 + res.Item3) / 2);
            }
        }

        public static (bool, Vector3) LineSegmentsIntersectionPoint(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, SplineSpace dimension)
            => LineSegmentsIntersectionPoint(start1, end1, start2, end2, dimension, MathUtility.defaultIntersectionEpsilon);
        public static (bool, Vector3) LineSegmentsIntersectionPoint(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, float epsilon)
            => LineSegmentsIntersectionPoint(start1, end1, start2, end2, null, epsilon);

        public static (bool, Vector3) LineSegmentsIntersectionPoint(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, SplineSpace? dimension = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            if (dimension == null) dimension = GetSpaceOf(start1, end1, start2, end2);
            if (LineSegmentsIntersect(start1, end1, start2, end2, dimension, epsilon))
            {
                return LinesIntersectionPoint(start1, end1, start2, end2, dimension, epsilon);
            }

            return (false, Vector3.zero);
        }

        public static bool SplineLineIntersect(SplineBase spline, Vector3 start, Vector3 end, int accuracy, SplineSpace dimension)
            => SplineLineIntersect(spline, start, end, accuracy, dimension, MathUtility.defaultIntersectionEpsilon);
        public static bool SplineLineIntersect(SplineBase spline, Vector3 start, Vector3 end, int accuracy, float epsilon)
            => SplineLineIntersect(spline, start, end, accuracy, null, epsilon);

        public static bool SplineLineIntersect(SplineBase spline, Vector3 start, Vector3 end, int accuracy, SplineSpace? dimension = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            IReadOnlyList<Vector3> segments = spline.GetFlattened(accuracy);
            for (int i = 0; i < segments.Count - 1; i++)
            {
                if (LineSegmentsIntersect(segments[i], segments[i + 1], start, end, dimension, epsilon))
                    return true;
            }

            return false;
        }

        public static bool CurveLineIntersect(ISplineGenerator generator, IList<Vector3> points, Vector3 start, Vector3 end, int accuracy, SplineSpace space)
            => CurveLineIntersect(generator, points, start, end, accuracy, space, MathUtility.defaultIntersectionEpsilon);
        public static bool CurveLineIntersect(ISplineGenerator generator, IList<Vector3> points, Vector3 start, Vector3 end, int accuracy, float epsilon)
            => CurveLineIntersect(generator, points, start, end, accuracy, null, epsilon);

        public static bool CurveLineIntersect(ISplineGenerator generator, IList<Vector3> points, Vector3 start, Vector3 end, int accuracy, SplineSpace? space = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            Vector3 newPrev;
            Vector3 previous = points[0];
            for (int i = 1; i <= accuracy; i++)
            {
                newPrev = generator.Evaluate((float)i / accuracy, points);
                if (LineSegmentsIntersect(previous, newPrev, start, end, space, epsilon)) return true;
                previous = newPrev;
            }
            return false;
        }

        public static IList<Vector3> SplineLineIntersectionPoints(SplineBase spline, Vector3 start, Vector3 end, int accuracy, SplineSpace dimension)
            => SplineLineIntersectionPoints(spline, start, end, accuracy, dimension, MathUtility.defaultIntersectionEpsilon);
        public static IList<Vector3> SplineLineIntersectionPoints(SplineBase spline, Vector3 start, Vector3 end, int accuracy, float epsilon)
            => SplineLineIntersectionPoints(spline, start, end, accuracy, null, epsilon);

        public static IList<Vector3> SplineLineIntersectionPoints(SplineBase spline, Vector3 start, Vector3 end, int accuracy, SplineSpace? dimension = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            List<Vector3> list = new List<Vector3>();
            IReadOnlyList<Vector3> segments = spline.GetFlattened(accuracy);
            Vector3 segStart, segEnd;
            for (int i = 0; i < segments.Count - 1; i++)
            {
                segStart = segments[i];
                segEnd = segments[i + 1];
                if (LineSegmentsIntersect(segStart, segEnd, start, end, dimension, epsilon))
                    list.Add(LinesIntersectionPoint(segStart, segEnd, start, end, dimension, epsilon).Item2);
            }

            return list;
        }

        public static IList<Vector3> CurveLineIntersectionPoints(ISplineGenerator generator, IList<Vector3> points, Vector3 start, Vector3 end, int accuracy, SplineSpace dimension)
            => CurveLineIntersectionPoints(generator, points, start, end, accuracy, dimension, MathUtility.defaultIntersectionEpsilon);
        public static IList<Vector3> CurveLineIntersectionPoints(ISplineGenerator generator, IList<Vector3> points, Vector3 start, Vector3 end, int accuracy, float epsilon)
            => CurveLineIntersectionPoints(generator, points, start, end, accuracy, null, epsilon);

        public static IList<Vector3> CurveLineIntersectionPoints(ISplineGenerator generator, IList<Vector3> points, Vector3 start, Vector3 end, int accuracy, SplineSpace? space = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            List<Vector3> list = new List<Vector3>();
            Vector3 newPrev;
            Vector3 previous = points[0];
            for (int i = 1; i <= accuracy; i++)
            {
                newPrev = generator.Evaluate((float)i / accuracy, points);
                if (LineSegmentsIntersect(previous, newPrev, start, end, space, epsilon))
                    list.Add(LinesIntersectionPoint(previous, newPrev, start, end, space, epsilon).Item2);
                previous = newPrev;
            }
            return list;
        }

        public static IList<float> SplineLineIntersectionTs(SplineBase spline, Vector3 start, Vector3 end, int accuracy, SplineSpace dimension)
            => SplineLineIntersectionTs(spline, start, end, accuracy, dimension, MathUtility.defaultIntersectionEpsilon);
        public static IList<float> SplineLineIntersectionTs(SplineBase spline, Vector3 start, Vector3 end, int accuracy, float epsilon)
            => SplineLineIntersectionTs(spline, start, end, accuracy, null, epsilon);

        public static IList<float> SplineLineIntersectionTs(SplineBase spline, Vector3 start, Vector3 end, int accuracy, SplineSpace? dimension = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            List<float> list = new List<float>();

            Vector3 segStart, segEnd;
            float step = 1f / accuracy;

            for (float i = 0; i < 1f; i += step)
            {
                segStart = spline.ValueAt(i);
                segEnd = spline.ValueAt(i + step);
                if (LineSegmentsIntersect(segStart, segEnd, start, end, dimension, epsilon))
                    list.Add(i + (step / 2f));
            }

            return list;
        }

        public static IList<float> CurveLineIntersectionTs(ISplineGenerator generator, IList<Vector3> points, Vector3 start, Vector3 end, int accuracy, SplineSpace dimension)
            => CurveLineIntersectionTs(generator, points, start, end, accuracy, dimension, MathUtility.defaultIntersectionEpsilon);
        public static IList<float> CurveLineIntersectionTs(ISplineGenerator generator, IList<Vector3> points, Vector3 start, Vector3 end, int accuracy, float epsilon)
            => CurveLineIntersectionTs(generator, points, start, end, accuracy, null, epsilon);

        public static IList<float> CurveLineIntersectionTs(ISplineGenerator generator, IList<Vector3> points, Vector3 start, Vector3 end, int accuracy, SplineSpace? space = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            List<float> list = new List<float>();

            Vector3 newPrev;
            Vector3 previous = points[0];
            for (int i = 1; i <= accuracy; i++)
            {
                newPrev = generator.Evaluate((float)i / accuracy, points);
                if (LineSegmentsIntersect(previous, newPrev, start, end, space, epsilon))
                    list.Add((float)i / accuracy);
                previous = newPrev;
            }
            return list;
        }

        public static IList<Vector3> SplineSplineIntersectionPoints(SplineBase spline1, SplineBase spline2, int accuracy, SplineSpace dimension)
            => SplineSplineIntersectionPoints(spline1, spline1, accuracy, dimension, MathUtility.defaultIntersectionEpsilon);
        public static IList<Vector3> SplineSplineIntersectionPoints(SplineBase spline1, SplineBase spline2, int accuracy, float epsilon)
            => SplineSplineIntersectionPoints(spline1, spline2, accuracy, null, epsilon);

        public static IList<Vector3> SplineSplineIntersectionPoints(SplineBase spline1, SplineBase spline2, int accuracy, SplineSpace? dimension = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            List<Vector3> list = new List<Vector3>();

            if (!spline1.GetBounds().Intersects(spline2.GetBounds())) return list;

            SplineSplineIntersections_Iterative(spline1, spline2, accuracy, list, dimension, epsilon);
            return list;
        }

        public static IList<Vector3> CurveSplineIntersectionPoints(ISplineGenerator generator, IList<Vector3> points, SplineBase spline, int accuracy, SplineSpace dimension)
            => CurveSplineIntersectionPoints(generator, points, spline, accuracy, dimension, MathUtility.defaultIntersectionEpsilon);
        public static IList<Vector3> CurveSplineIntersectionPoints(ISplineGenerator generator, IList<Vector3> points, SplineBase spline, int accuracy, float epsilon)
            => CurveSplineIntersectionPoints(generator, points, spline, accuracy, null, epsilon);

        public static IList<Vector3> CurveSplineIntersectionPoints(ISplineGenerator generator, IList<Vector3> points, SplineBase spline, int accuracy, SplineSpace? space = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            List<Vector3> list = new List<Vector3>();

            var ts = generator.GetExtremaTs(points);
            SplineExtrema extrema = new SplineExtrema();
            foreach (float t in ts) extrema.InsertValueT(t, generator, points);
            Bounds b = new Bounds((extrema.Maxima + extrema.Minima) / 2, extrema.Maxima - extrema.Minima);

            if (!b.Intersects(spline.GetBounds())) return list;

            CurveSplineIntersections_Iterative(generator, points, spline, accuracy, list, space, epsilon);

            return list;
        }

        public static bool SplineSplineIntersect(SplineBase spline1, SplineBase spline2, int accuracy, SplineSpace space)
            => SplineSplineIntersect(spline1, spline2, accuracy, space, MathUtility.defaultIntersectionEpsilon);
        public static bool SplineSplineIntersect(SplineBase spline1, SplineBase spline2, int accuracy, float epsilon)
            => SplineSplineIntersect(spline1, spline2, accuracy, null, epsilon);

        public static bool SplineSplineIntersect(SplineBase spline1, SplineBase spline2, int accuracy, SplineSpace? space = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            if (!spline1.GetBounds().Intersects(spline2.GetBounds())) return false;
            return SplineSplineIntersect_Iterative(spline1, spline2, accuracy, space, epsilon);
        }

        public static bool CurveSplineIntersect(ISplineGenerator generator, IList<Vector3> points, SplineBase spline, int accuracy, SplineSpace space)
            => CurveSplineIntersect(generator, points, spline, accuracy, space, MathUtility.defaultIntersectionEpsilon);
        public static bool CurveSplineIntersect(ISplineGenerator generator, IList<Vector3> points, SplineBase spline, int accuracy, float epsilon)
            => CurveSplineIntersect(generator, points, spline, accuracy, null, epsilon);

        public static bool CurveSplineIntersect(ISplineGenerator generator, IList<Vector3> points, SplineBase spline2, int accuracy, SplineSpace? space = null, float epsilon = MathUtility.defaultIntersectionEpsilon)
        {
            var ts = generator.GetExtremaTs(points);
            SplineExtrema extrema = new SplineExtrema();
            foreach (float t in ts) extrema.InsertValueT(t, generator, points);
            Bounds b = new Bounds((extrema.Maxima + extrema.Minima) / 2, extrema.Maxima - extrema.Minima);
            if (!b.Intersects(spline2.GetBounds())) return false;
            return CurveSplineIntersect_Iterative(generator, points, spline2, accuracy, space, epsilon);
        }

        private static bool SplineSplineIntersect_Iterative(SplineBase spline1, SplineBase spline2, int accuracy, SplineSpace? dimension, float epsilon)
        {
            IReadOnlyList<Vector3> segments1 = spline1.GetFlattened(accuracy);
            IReadOnlyList<Vector3> segments2 = spline2.GetFlattened(accuracy);

            Vector3 start1, end1, start2, end2;
            for (int i = 0; i < segments1.Count - 1; i++)
            {
                start1 = segments1[i];
                end1 = segments1[i + 1];
                for (int j = 0; j < segments2.Count - 1; j++)
                {
                    start2 = segments2[j];
                    end2 = segments2[j + 1];

                    if (LineSegmentsIntersect(start1, end1, start2, end2, dimension, epsilon))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CurveSplineIntersect_Iterative(ISplineGenerator generator, IList<Vector3> points, SplineBase spline, int accuracy, SplineSpace? space, float epsilon)
        {
            IReadOnlyList<Vector3> segments = spline.GetFlattened(accuracy);

            Vector3 start1, end1, start2, end2;

            start1 = points[0];
            for (int i = 1; i <= accuracy; i++)
            {
                end1 = generator.Evaluate((float)i / accuracy, points);
                for (int j = 0; j < segments.Count - 1; j++)
                {
                    start2 = segments[j];
                    end2 = segments[j + 1];

                    if (LineSegmentsIntersect(start1, end1, start2, end2, space, epsilon))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void SplineSplineIntersections_Iterative(SplineBase spline1, SplineBase spline2, int accuracy, List<Vector3> list, SplineSpace? dimension, float epsilon)
        {
            IReadOnlyList<Vector3> segments1 = spline1.GetFlattened(accuracy);
            IReadOnlyList<Vector3> segments2 = spline2.GetFlattened(accuracy);

            Vector3 start1, end1, start2, end2;
            for (int i = 0; i < segments1.Count - 1; i++)
            {
                start1 = segments1[i];
                end1 = segments1[i + 1];
                for (int j = 0; j < segments2.Count - 1; j++)
                {
                    start2 = segments2[j];
                    end2 = segments2[j + 1];

                    if (LineSegmentsIntersect(start1, end1, start2, end2, dimension, epsilon))
                    {
                        var res = LinesIntersectionPoint(start1, end1, start2, end2, dimension, epsilon);
                        if (res.Item1 && !list.Contains(res.Item2)) list.Add(res.Item2);
                    }
                }
            }
        }

        private static void CurveSplineIntersections_Iterative(ISplineGenerator generator, IList<Vector3> points, SplineBase spline, int accuracy, List<Vector3> list, SplineSpace? space, float epsilon)
        {
            IReadOnlyList<Vector3> segments = spline.GetFlattened(accuracy);

            Vector3 start1, end1, start2, end2;

            start1 = points[0];
            for (int i = 1; i <= accuracy; i++)
            {
                end1 = generator.Evaluate((float)i / accuracy, points);
                for (int j = 0; j < segments.Count - 1; j++)
                {
                    start2 = segments[j];
                    end2 = segments[j + 1];

                    if (LineSegmentsIntersect(start1, end1, start2, end2, space, epsilon))
                    {
                        var res = LinesIntersectionPoint(start1, end1, start2, end2, space, epsilon);
                        if (res.Item1 && !list.Contains(res.Item2)) list.Add(res.Item2);
                    }
                }
            }
        }
        #endregion
    }
}