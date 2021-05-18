using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
namespace shadertest
{
    class Plane : Shape
    {
        public Plane(Vector3 pos, Vector3 size, Vector3 colour) : base(pos, size, colour)
        {
            
        }
        public Plane(Vector3 pos, Vector3 size, Vector3 colour, string name) : base(pos, size, colour,name)
        {
            
        }
        public override float Distance(Vector3 p)
        {
            return p.Y - position.Y;
        }
        public Plane() : base()
        {

        }
    }
}
