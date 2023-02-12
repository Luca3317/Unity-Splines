using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySplines
{
    public class SplineGeneratorEditorAttribute : System.Attribute
    {
        public string GeneratorType;
        public SplineGeneratorEditorAttribute(string generatorType)
        {
            GeneratorType = generatorType;
        }
    }
}
