using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtility
{
    public static IList<int> PointToSegmentIndeces(int pointIndex, int segmentSize, int slideSize, int segmentCount = -1)
    {
        List<int> indeces = new List<int>();
        int firstSegmentIndex = PointToFirstSegmentIndex(pointIndex, segmentSize, slideSize);
        int firstIndex = firstSegmentIndex * slideSize;
        while (firstIndex <= pointIndex)
        {
            if (indeces.Contains(firstSegmentIndex) || (segmentCount >= 0 && firstSegmentIndex >= segmentCount)) return indeces;
            indeces.Add(firstSegmentIndex++);
            firstIndex += slideSize;
        }
        return indeces;
    }

    /// <summary>
    /// Converts a point index to the corresponding segment indeces.
    /// </summary>
    /// <param name="pointIndex">The point index that will be converted.</param>
    /// <returns>The indeces of all segments that contain the point at i.</returns>
    public static IList<int> PointToSegmentIndeces(int pointIndex, int segmentSize, int slideSize, int pointCount, int segmentCount, bool loops)
    {
        if (!loops) return PointToSegmentIndeces(pointIndex, segmentSize, slideSize, segmentCount);

        List<int> indeces = new List<int>();
        pointIndex = LoopedIndexToDirectIndex(pointIndex, pointCount);

        int firstSegmentIndex = PointToFirstSegmentIndex(pointIndex, segmentSize, slideSize, pointCount, segmentCount, loops);

        // TODO
        // This works for all tested cases, but might still not work universally
        int indecesAmount = segmentSize / slideSize;
        if (pointIndex % slideSize < segmentSize % slideSize) indecesAmount += (segmentSize % slideSize) - (pointIndex % slideSize);

        for (int i = 0; i < indecesAmount; i++)
        {
            indeces.Add(firstSegmentIndex++);
        }

        return indeces;
    }

    /// <summary>
    /// Get the index of the first segment containing a point.
    /// </summary>
    /// <param name="pointIndex"></param>
    /// <param name="segmentSize"></param>
    /// <param name="slideSize"></param>
    /// <returns></returns>
    public static int PointToFirstSegmentIndex(int pointIndex, int segmentSize, int slideSize) => pointIndex < segmentSize ? 0 : (pointIndex - segmentSize) / slideSize + 1;

    /// <summary>
    /// Get the index of the first segment containing a point.
    /// If looping, will not necessarily return the segment with the smallest index, but the first of the consecutive segments containing the point.
    /// </summary>
    /// <param name="pointIndex"></param>
    /// <param name="segmentSize"></param>
    /// <param name="slideSize"></param>
    /// <param name="pointCount"></param>
    /// <param name="segmentCount"></param>
    /// <param name="loops"></param>
    /// <returns></returns>
    public static int PointToFirstSegmentIndex(int pointIndex, int segmentSize, int slideSize, int pointCount, int segmentCount, bool loops)
    {
        if (!loops) return PointToFirstSegmentIndex(pointIndex, segmentSize, slideSize);

        pointIndex = LoopedIndexToDirectIndex(pointIndex, pointCount);
        int segmentIndex = pointIndex < segmentSize ? 0 : (pointIndex - segmentSize) / slideSize + 1;

        if (segmentIndex != 0)
        {
            return segmentIndex;
        }

        int start = 0;
        int end = segmentSize;

        while (start <= pointIndex && end > pointIndex)
        {
            segmentIndex--;
            segmentIndex = LoopedIndexToDirectIndex(segmentIndex, segmentCount);

            start -= slideSize;
            end = start + segmentSize;
        }
        segmentIndex++;
        segmentIndex = LoopedIndexToDirectIndex(segmentIndex, segmentCount);

        return segmentIndex;
    }

    /// <summary>
    /// Converts a segment index to the corresponding point index.
    /// </summary>
    /// <param name="segmentIndex">The segment index to convert.</param>
    /// <returns>The index of the first point contained in this segment.</returns>
    public static int SegmentToPointIndex(int segmentIndex, int segmentSize, int slideSize) => slideSize * segmentIndex;

    public static int LoopedIndexToDirectIndex(int loopedIndex, int itemCount)
    {
        while (loopedIndex < 0) loopedIndex += itemCount;
        loopedIndex %= itemCount;
        return loopedIndex;
    }



    public const float defaultIntersectionEpsilon = 0.000001f;

    public static bool IsBetween(Vector3 start, Vector3 end, Vector3 point, float epsilon = defaultIntersectionEpsilon)
    {
        float distance = (end - start).magnitude;
        float distanceWithPoint = (end - point).magnitude + (point - start).magnitude;
        return Mathf.Abs(distance - distanceWithPoint) < epsilon;
    }

    public static (bool, Vector3, Vector3) LinesIntersection3D(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
    {
        // Algorithm is ported from the C algorithm of 
        // Paul Bourke at http://local.wasp.uwa.edu.au/~pbourke/geometry/lineline3d/
        Vector3 resultSegmentPoint1 = Vector3.zero;
        Vector3 resultSegmentPoint2 = Vector3.zero;

        Vector3 p1 = start1;
        Vector3 p2 = end1;
        Vector3 p3 = start2;
        Vector3 p4 = end2;
        Vector3 p13 = p1 - p3;
        Vector3 p43 = p4 - p3;

        if (p43.sqrMagnitude < Mathf.Epsilon)
        {
            return (false, Vector3.zero, Vector3.zero);
        }
        Vector3 p21 = p2 - p1;
        if (p21.sqrMagnitude < Mathf.Epsilon)
        {
            return (false, Vector3.zero, Vector3.zero);
        }

        double d1343 = p13.x * (double)p43.x + (double)p13.y * p43.y + (double)p13.z * p43.z;
        double d4321 = p43.x * (double)p21.x + (double)p43.y * p21.y + (double)p43.z * p21.z;
        double d1321 = p13.x * (double)p21.x + (double)p13.y * p21.y + (double)p13.z * p21.z;
        double d4343 = p43.x * (double)p43.x + (double)p43.y * p43.y + (double)p43.z * p43.z;
        double d2121 = p21.x * (double)p21.x + (double)p21.y * p21.y + (double)p21.z * p21.z;

        double denom = d2121 * d4343 - d4321 * d4321;
        if (System.Math.Abs(denom) < Mathf.Epsilon)
        {
            return (false, Vector3.zero, Vector3.zero);
        }
        double numer = d1343 * d4321 - d1321 * d4343;

        double mua = numer / denom;
        double mub = (d1343 + d4321 * (mua)) / d4343;

        resultSegmentPoint1.x = (float)(p1.x + mua * p21.x);
        resultSegmentPoint1.y = (float)(p1.y + mua * p21.y);
        resultSegmentPoint1.z = (float)(p1.z + mua * p21.z);
        resultSegmentPoint2.x = (float)(p3.x + mub * p43.x);
        resultSegmentPoint2.y = (float)(p3.y + mub * p43.y);
        resultSegmentPoint2.z = (float)(p3.z + mub * p43.z);

        return (true, resultSegmentPoint1, resultSegmentPoint2);
    }

    public static Vector3 ClosestPointOnLineSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 aB = b - a;
        Vector3 aP = p - a;
        float sqrLenAB = aB.sqrMagnitude;

        if (sqrLenAB == 0)
            return a;

        float t = Mathf.Clamp01(Vector3.Dot(aP, aB) / sqrLenAB);
        return a + aB * t;
    }
}