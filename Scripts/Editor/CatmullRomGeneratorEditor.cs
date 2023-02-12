using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    [SplineGeneratorEditor("CatmullRom")]
    public class CatmullRomGeneratorEditor : ISplineGeneratorEditor
    {
        public ISplineGenerator Generator => CatmullRom.CatmullRomGenerator.Instance;

    }
}
