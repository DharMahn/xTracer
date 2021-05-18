using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using SQLite;
namespace shadertest
{
    class Sphere : Shape
    {
        public Sphere(Vector3 pos, Vector3 size, Vector3 colour) : base(pos,size,colour)
        {
            
        }
        public Sphere(Vector3 pos, Vector3 size, Vector3 colour, string name) : base(pos, size, colour,name)
        {
            
        }
        public override float Distance(Vector3 p)
        {
            return Vector3.Distance(p,position) - size.X;
        }
        public Sphere() : base()
        {
            
        }
    }
}
