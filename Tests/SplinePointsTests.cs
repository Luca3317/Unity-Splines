using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnitySplines;

public class SplinePointsTests
{
    /*
    #region Creation
    [Test]
    public void SplinePointsContructor1Test()
    {
        List<Vector3> list = new List<Vector3>() { Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one };
        SplinePoints tmp;

        // Constructor 1
        Assert.NotNull(tmp = new SplinePoints(2, 1, list));
        Check(tmp, 2, 1, 5, Vector3.one);

        Assert.NotNull(tmp = new SplinePoints(4, 1, list));
        Check(tmp, 4, 1, 5, Vector3.one);

        Assert.NotNull(tmp = new SplinePoints(5, 2, list));
        Check(tmp, 5, 2, 5, Vector3.one);

        Assert.That(() => new SplinePoints(2, 2, list), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(6, 2, list), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(0, 2, list), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(2, 0, list), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(2, 2, (List<Vector3>)null), Throws.TypeOf<System.ArgumentNullException>());
    }

    [Test]
    public void SplinePointsContructor2Test()
    {
        SplinePoints tmp;

        // Constructor 2
        Vector3[] vecs = null;
        Assert.NotNull(tmp = new SplinePoints(1, 1, Vector3.one));
        Check(tmp, 1, 1, 1, Vector3.one);

        Assert.NotNull(tmp = new SplinePoints(2, 1, Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one));
        Check(tmp, 2, 1, 5, Vector3.one);

        Assert.That(() => new SplinePoints(2, 1, Vector3.zero), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(2, 2, Vector3.zero, Vector3.zero, Vector3.zero), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(0, 1, Vector3.zero), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(2, -1, Vector3.zero, Vector3.zero), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(2, -1, vecs), Throws.TypeOf<System.ArgumentNullException>());
    }

    [Test]
    public void SplinePointsContructor3Test()
    {
        List<Vector3> list = new List<Vector3>() { Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one };
        SplinePoints sp = new SplinePoints(2, 1, list);
        SplinePoints tmp;

        // Constructor 3
        Assert.NotNull(tmp = new SplinePoints(2, 2, sp));
        Check(tmp, 2, 2, 4, Vector3.one);

        Assert.NotNull(tmp = new SplinePoints(4, 1, sp));
        Check(tmp, 4, 1, 5, Vector3.one);

        Assert.That(() => new SplinePoints(6, 2, sp), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(0, 1, sp), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(5, -1, sp), Throws.TypeOf<System.ArgumentException>());
        Assert.That(() => new SplinePoints(2, 2, (SplinePoints)null), Throws.TypeOf<System.NullReferenceException>());
    }
    #endregion

    [Test]
    public void SplinePointsInsertionTests()
    {
        SplinePoints sp;
        Assert.NotNull(sp = new SplinePoints(2, 1, Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one));
        Check(sp, 2, 1, 5, Vector3.one);

        sp.AddSegment(Vector3.zero);
        sp.AddSegment(Vector3.zero);
        Check(sp, 2, 1, 7);
        for (int i = 5; i < 7; i++)
            Assert.AreEqual(sp.Point(i), Vector3.zero);

        sp.InsertSegment(2, Vector3.right);
        Check(sp, 2, 1, 8);
        Assert.AreEqual(sp.Point(2), Vector3.right);
        Assert.AreEqual(Vector3.one, sp.Segment(2)[1]);
    }

    [Test]
    public void SplinePointsDeletionTests()
    {
        SplinePoints sp;
        Assert.NotNull(sp = new SplinePoints(3, 2, 
            Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.zero));

        sp.DeleteLastSegment();
        Check(sp, 3, 2, 9, Vector3.one);

        sp.InsertSegment(3, new Vector3(0, 2, 2), new Vector3(0, 2, 2));
        Assert.AreEqual(sp.Segment(3)[0], new Vector3(0, 2, 2));
        Assert.AreEqual(sp.Segment(3)[1], new Vector3(0, 2, 2));
        Assert.AreEqual(sp.Segment(3)[2], Vector3.one);
        
        sp.DeleteSegment(3);
        Check(sp, 3, 2, 9);
        Assert.AreEqual(sp.Segment(3)[0], Vector3.one);
        Assert.AreEqual(sp.Segment(3)[1], Vector3.one);
        Assert.AreEqual(sp.Segment(3)[2], Vector3.one);

        Assert.That(() => sp.DeleteSegment(5), Throws.TypeOf<System.ArgumentException>());
        sp.DeleteLastSegment();
        sp.DeleteLastSegment();
        sp.DeleteLastSegment();
        Assert.That(() => sp.DeleteLastSegment(), Throws.TypeOf<System.InvalidOperationException>());
    }

    void Check(SplinePoints sp, int segmentSize, int slideSize, int count)
    {
        Assert.AreEqual(segmentSize, sp.SegmentSize);
        Assert.AreEqual(slideSize, sp.SlideSize);
        Assert.AreEqual(count, sp.PointCount);
        Assert.AreEqual((count - segmentSize) / slideSize + 1, sp.SegmentCount);
    }

    void Check(SplinePoints sp, int segmentSize, int slideSize, int count, Vector3 vec)
    {
        Check(sp, segmentSize, slideSize, count);
        foreach (var currVec in sp.Points) Assert.AreEqual(vec, currVec);
    }
    */
}
