using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using SQLite;
namespace shadertest
{
    public class Shape
    {
        public float positionX { get; set; }
        public float positionY { get; set; }
        public float positionZ { get; set; }

        public float sizeX { get; set; }
        public float sizeY { get; set; }
        public float sizeZ { get; set; }

        public float colourX { get; set; }
        public float colourY { get; set; }
        public float colourZ { get; set; }
        public int sceneID { get; set; }
        protected string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        [Ignore]
        public Vector3 position
        {
            get
            {
                return new Vector3(positionX, positionY, positionZ);
            }
            set
            {
                positionX = value.X;
                positionY = value.Y;
                positionZ = value.Z;
            }
        }
        [Ignore]
        public Vector3 size
        {
            get
            {
                return new Vector3(sizeX, sizeY, sizeZ);
            }
            set
            {
                sizeX = value.X;
                sizeY = value.Y;
                sizeZ = value.Z;
            }
        }
        [Ignore] 
        public Vector3 colour
        {
            get
            {
                return new Vector3(colourX, colourY, colourZ);
            }
            set
            {
                colourX = value.X;
                colourY = value.Y;
                colourZ = value.Z;
            }
        }
        
        public bool reflective = true;
        public Shape(Vector3 position, Vector3 size, Vector3 colour, string name)
        {
            positionX = position.X;
            positionY = position.Y;
            positionZ = position.Z;
            sizeX = size.X;
            sizeY = size.Y;
            sizeZ = size.Z;
            colourX = colour.X;
            colourY = colour.Y;
            colourZ = colour.Z;
            this.name = name;
        }
        public Shape(Vector3 position, Vector3 size, Vector3 colour)
        {
            positionX = position.X;
            positionY = position.Y;
            positionZ = position.Z;
            sizeX = size.X;
            sizeY = size.Y;
            sizeZ = size.Z;
            colourX = colour.X;
            colourY = colour.Y;
            colourZ = colour.Z;
            this.name = "Anonymous";
        }
        public Shape()
        {
            positionX = 0;
            positionY = 0;
            positionZ = 0;
            sizeX = 0;
            sizeY = 0;
            sizeZ = 0;
            colourX = 0;
            colourY = 0;
            colourZ = 0;
            name = "GenericInvalidShape";
        }
        public virtual float Distance(Vector3 p)
        {
            return -1;
        }
    }
}
