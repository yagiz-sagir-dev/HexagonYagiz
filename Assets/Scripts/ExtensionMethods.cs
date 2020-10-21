using System.Linq;
using UnityEngine;

namespace ExtensionMethods
{
    public static class NecessaryExtensionMethods
    {
        public static Vector2 FindCenterOfMass(this Vector2[] vectors)
        {
            Vector2 center = Vector2.zero;
            for(int index = 0; index < vectors.Length; index++)
            {
                center += vectors[index];
            }

            return center / vectors.Length;
        }
    }
}

