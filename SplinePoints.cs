using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    /*
     * Handles storage and structure of the point collection, primarily segment logic.
     * 
     * Invariant: always contains at least one segment / segmentSize-many points (called base segment).
     *      1. Create a list with either:
     *          exactly segmentSize many points (so: just the base segment) or
     *          segmentSize many points + slideSize * x many points (so: base segment + x following segments)
     *      2. After that you may
     *          2.1 Add segments
     *              This cant harm the integrity of the base segment; 
     *              however, if insertion happens at 0, it will be replaced by the new points (rather, the base segment will become the second segment)
     *          2.2 Remove segments
     *              Error if only base segment remains
     *              Otherwise, this cant harm the integrity of the base segment either;
     *          2.3 Change segment / slide sizes
     *              Should only happen if the generator is changes to another generator which requires different segment sizes
     *              TODO implement case for when new sizes incompatible with current points          
     *              
     *      4. You CANNOT add / remove indvidual points; would make it impossible to keep integrity of segment structure
    */
    [System.Serializable]
    public abstract class SplinePoints
    {
        public ICollection<Vector3> Segment(int i) => throw new System.NotImplementedException();
        public Vector3 Point(int i) => throw new System.NotImplementedException();

        public void AddSegment() => throw new System.NotImplementedException();
        public void DeleteSegment(int i) => throw new System.NotImplementedException();

        [SerializeField] protected List<Vector3> points;
        [SerializeField] protected int segmentSize;
        [SerializeField] protected int slideSize;
    }
}
