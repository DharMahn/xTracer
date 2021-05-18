using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
namespace shadertest
{
    class Box : Shape
    {
        public Box(Vector3 pos, Vector3 size, Vector3 colour) : base(pos, size, colour)
        {
            
        }
        public Box(Vector3 pos, Vector3 size, Vector3 colour, string name) : base(pos, size, colour,name)
        {
            
        }
        public override float Distance(Vector3 p)
        {
            
            Vector3 q = Vector3.Abs(position - p) - size;
            return Vector3.Max(q, Vector3.Zero).Length() + Math.Min(Math.Max(q.X, Math.Max(q.Y, q.Z)), 0.0f);
        }
        public Box() : base()
        {

        }
    }
}
