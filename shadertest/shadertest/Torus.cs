using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
namespace shadertest
{
    class Torus : Shape
    {
        public Torus(Vector3 pos, Vector3 size, Vector3 colour) : base(pos, size, colour)
        {
            
        }
        public Torus(Vector3 pos, Vector3 size, Vector3 colour, string name) : base(pos, size, colour,name)
        {
            
        }
        public override float Distance(Vector3 p)
        {
            Vector2 q = new Vector2(new Vector2(p.X - position.X, p.Z - position.Z).Length()-size.X, p.Y - position.Y);
            return q.Length() - size.Y;
        }

        public Torus() : base()
        {

        }
    }
}
