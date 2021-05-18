using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace shadertest
{
    class Ray
    {
        public Vector3 origin;
        public Vector3 direction;

        public Ray(Vector3 o, Vector3 dir)
        {
            origin = o;
            direction = dir;
        }
    }
}
