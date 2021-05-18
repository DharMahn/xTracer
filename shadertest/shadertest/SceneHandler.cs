using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using SQLite;
namespace shadertest
{
    class SceneHandler
    {
        SceneDescription SceneDescription;
        public List<Shape> shapes = new List<Shape>();
        public List<Light> lights = new List<Light>();
        public Camera mainCamera;
        public bool softShadows = true;
        Random r = new Random();
        SQLiteConnection db;
        public List<Shape> GetShapes()
        {
            return shapes;
        }
        public List<Light> GetLights()
        {
            return lights;
        }
        public SceneHandler(Camera c, SQLiteConnection connection)
        {
            SetMainCamera(c);
            mainCamera.LoadSceneHandler(this);
            this.db = connection;
        }
        public SceneHandler()
        {
            mainCamera = new Camera(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
            mainCamera.LoadSceneHandler(this);
        }
        public void SingleSphereInfrontOfCamera()
        {
            shapes = new List<Shape>();
            lights = new List<Light>();
            shapes.Add(new Sphere(new Vector3(3, 0, 0), Vector3.One, Vector3.One));
            lights.Add(new Light(new Vector3(0, 5, 0), new Vector3(1, 1, 1)));
        }
        public void SetMainCamera(Camera camera)
        {
            mainCamera = camera;
            mainCamera.LoadSceneHandler(this);
        }

        public string[] GetShapeNames()
        {
            string[] shapeNames = new string[shapes.Count];
            for (int i = 0; i < shapes.Count; i++)
            {
                shapeNames[i] = shapes[i].Name;
            }
            return shapeNames;
        }
        public string[] GetLightNames()
        {
            string[] lightNames = new string[lights.Count];
            for (int i = 0; i < lights.Count; i++)
            {
                lightNames[i] = lights[i].Name;
            }
            return lightNames;
        }
        public void LoadRandomScene()
        {
            shapes = new List<Shape>();
            lights = new List<Light>();
            int count = 0;
            for (double i = 0; i < Math.PI * 2; i += Math.PI / 6)
            {
                count++;
                double random = r.NextDouble();
                if (random > 0.66)
                {
                    shapes.Add(new Sphere(new Vector3((float)Math.Sin(i) * 20, 2, (float)Math.Cos(i) * 20), Vector3.One * 4, new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()), "Object#" + count));
                }
                else if (random > 0.33)
                {
                    shapes.Add(new Box(new Vector3((float)Math.Sin(i) * 20, 2, (float)Math.Cos(i) * 20), Vector3.One * 4, new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()), "Object#" + count));
                }
                else
                {
                    shapes.Add(new Torus(new Vector3((float)Math.Sin(i) * 20, 2, (float)Math.Cos(i) * 20), Vector3.One * 4, new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()), "Object#" + count));
                }
            }
            shapes.Add(new Plane(new Vector3(0, -4, 0), new Vector3(100, 0, 100), new Vector3((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble()), "Floor"));
            lights.Add(new Light(new Vector3(0, 15, 0), Vector3.One));
        }

        internal void SetShapes(List<Shape> shapes)
        {
            this.shapes = shapes;
        }
        internal void SetLights(List<Light> lights)
        {
            this.lights = lights;
        }
        public void LoadScene(string sceneName)
        {

            SceneDescription scene = db.Table<SceneDescription>().Where(x => x.sceneName.Equals(sceneName)).FirstOrDefault();

            List<Shape> dbshapes = new List<Shape>();
            dbshapes.AddRange(db.Table<Sphere>().Where(x => x.sceneID.Equals(scene.sceneID)).ToList());
            dbshapes.AddRange(db.Table<Box>().Where(x => x.sceneID.Equals(scene.sceneID)).ToList());
            dbshapes.AddRange(db.Table<Torus>().Where(x => x.sceneID.Equals(scene.sceneID)).ToList());
            dbshapes.AddRange(db.Table<Plane>().Where(x => x.sceneID.Equals(scene.sceneID)).ToList());
            SetShapes(dbshapes);

            var dblights = db.Table<Light>().Where(x => x.sceneID.Equals(scene.sceneID)).ToList();
            SetLights(dblights);
        }
        public void SaveScene(string sceneName)
        {
            SceneDescription = new SceneDescription
            {
                sceneName = sceneName
            };
            SceneDescription scene = db.Table<SceneDescription>().Where(x => x.sceneName.Equals(sceneName)).FirstOrDefault();
            if (scene != null)
            {
                DeleteScene(scene);
            }

            db.Insert(SceneDescription);
            foreach (var item in shapes)
            {
                item.sceneID = SceneDescription.sceneID;
                db.Insert(item, item.GetType());
            }
            foreach (var item in lights)
            {
                item.sceneID = SceneDescription.sceneID;
                db.Insert(item, item.GetType());
            }

        }

        public void DeleteScene(SceneDescription scene)
        {
            if (scene != null)
            {
                db.Execute("DELETE FROM Light WHERE sceneID = ?", scene.sceneID);
                db.Execute("DELETE FROM Box WHERE sceneID = ?", scene.sceneID);
                db.Execute("DELETE FROM Sphere WHERE sceneID = ?", scene.sceneID);
                db.Execute("DELETE FROM Plane WHERE sceneID = ?", scene.sceneID);
                db.Execute("DELETE FROM Torus WHERE sceneID = ?", scene.sceneID);
                db.Execute("DELETE FROM SceneDescription WHERE sceneID = ?", scene.sceneID);
            }
        }

        public void DeleteScene(string sceneName)
        {
            SceneDescription scene = db.Table<SceneDescription>().Where(x => x.sceneName.Equals(sceneName)).FirstOrDefault();
            DeleteScene(scene);

        }

        public float signedDstToScene(Vector3 p)
        {
            float dstToScene = mainCamera.maxDst;
            foreach (var item in shapes)
            {
                dstToScene = Math.Min(item.Distance(p), dstToScene);
            }
            return dstToScene;
        }
        public float signedDstToScene(Vector3 p, ref Shape shape)
        {
            float dstToScene = mainCamera.maxDst;
            float dst;
            foreach (var item in shapes)
            {
                dst = item.Distance(p);
                if (dst < dstToScene)
                {
                    shape = item;
                    dstToScene = dst;
                }
            }
            return dstToScene;
        }
    }
}
