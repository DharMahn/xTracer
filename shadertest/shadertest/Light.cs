using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using SQLite;

namespace shadertest
{
    public class Light
    {
        public float positionX { get; set; }
        public float positionY { get; set; }
        public float positionZ { get; set; }
        public float colourX { get; set; }
        public float colourY { get; set; }
        public float colourZ { get; set; }
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
        protected string name;
        public int sceneID { get; set; }
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

        public Light(Vector3 position, Vector3 colour, string name)
        {
            this.positionX = position.X;
            this.positionY = position.Y;
            this.positionZ = position.Z;

            this.colourX = colour.X;
            this.colourY = colour.Y;
            this.colourZ = colour.Z;

            this.name = name;
        }
        public Light(Vector3 position, Vector3 colour)
        {
            this.positionX = position.X;
            this.positionY = position.Y;
            this.positionZ = position.Z;

            this.colourX = colour.X;
            this.colourY = colour.Y;
            this.colourZ = colour.Z;

            this.name = "Light";
        }
        public Light()
        {

        }
    }
}
