using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnitySplines;

public class SplineTestes
{/*
    [Test]
    public void SplinePointsInsertionTests()
    {
        Spline sp;
        Assert.NotNull(sp = new Spline(2, 1, Vector3.one, Vector3.one, Vector3.one, Vector3.one, Vector3.one));
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
    */
}
