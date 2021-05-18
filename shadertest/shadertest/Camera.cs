using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using SkiaSharp;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace shadertest
{
    class Camera
    {
        public float maxDst = 100f;
        public float minDst = 0.0001f;
        private Vector3 Forward;
        private Vector3 Pos;
        private Vector3 Right;
        private Vector3 Up;
        private int width = 0;
        private int height = 0;
        private float alpha = 1.0f;
        Vector3 ambientLight = new Vector3(0.2f, 0.2f, 0.2f);
        Vector3 spec = new Vector3(1.0f);
        SceneHandler sceneHandler;
        public bool startedRendering = false;
        public bool finishedRendering = false;
        private Task renderTask;
        CancellationTokenSource ts = new CancellationTokenSource();
        CancellationToken ct;

        public Camera(Vector3 pos, Vector3 lookAt)
        {
            MoveCamera(pos, lookAt);
        }
        public void LoadSceneHandler(SceneHandler sh)
        {
            sceneHandler = sh;
        }
        public void MoveCamera(Vector3 pos, Vector3 lookAt)
        {
            Forward = Vector3.Normalize(lookAt - pos);
            this.Pos = pos;
            Vector3 Down = new Vector3(0f, -1f, 0f);
            Right = Vector3.Normalize(Vector3.Cross(Forward, Down)) * 1.5f;
            Up = Vector3.Normalize(Vector3.Cross(Forward, Right)) * 1.5f;
        }

        public void Render(SKBitmap bitmap, ProgressBar progressBar = null)
        {
            if (startedRendering)
            {
                ts.Cancel();
                ts = new CancellationTokenSource();
                ct = ts.Token;
                finishedRendering = false;
                startedRendering = true;
                renderTask = Task.Run(() => { RenderCode(bitmap, progressBar); }, ct).ContinueWith(t => Console.WriteLine("DONE RENDERING"));
                //RenderCode(bitmap,progressBar);
            }
            else
            {
                finishedRendering = false;
                startedRendering = true;
                renderTask = Task.Run(() => { RenderCode(bitmap, progressBar); }, ct).ContinueWith(t => Console.WriteLine("DONE RENDERING"));
            }
        }
        private void RenderCode(SKBitmap bitmap, ProgressBar progressBar = null)
        {
            SKColor[] colors = bitmap.Pixels;
            width = bitmap.Width;
            height = bitmap.Height;
            int count = 0;
            #region SINGLE THREAD | DEBUG PURPOSES ONLY
            /*for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    colors[y * (bitmap.Width) + x] = Raytrace(x, y);
                }
                count++;
                if (progressBar != null)
                {
                    progressBar.Progress = (float)count / bitmap.Height;
                    Console.WriteLine(count);
                }
            }*/
            #endregion
            #region MULTITHREAD | USE THIS
            Parallel.For(0, bitmap.Height, y =>
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    colors[y * (bitmap.Width) + x] = Raytrace(x, y);
                }
                count++;
                if (progressBar != null)
                {
                    progressBar.Progress = (float)count / bitmap.Height;
                }
            });
            #endregion
            bitmap.Pixels = colors;
            finishedRendering = true;
            startedRendering = false;
        }
        Vector3 ReflectionTrace(Ray r, out Vector3 end)
        {
            float rayDst = 0f;
            float circleSize = 0f;
            Shape hitEnd = new Shape();
            end = new Vector3(float.PositiveInfinity);
            while (rayDst < maxDst)
            {
                circleSize = sceneHandler.signedDstToScene(r.origin);
                if (circleSize < minDst*100f)
                {
                    circleSize = sceneHandler.signedDstToScene(r.origin, ref hitEnd);
                    end = r.origin;
                    return hitEnd.colour;
                }
                else
                {
                    r.origin += r.direction * circleSize;
                    rayDst += circleSize;
                }
            }
            return ambientLight;
        }
        private SKColor Raytrace(int xCoord, int yCoord)
        {
            float rayDst = 0f;
            Ray r = new Ray(Pos, GetRayDir(xCoord, yCoord));
            float circleSize = 0f;
            Shape hitEnd = new Shape();
            while (rayDst < maxDst)
            {
                circleSize = sceneHandler.signedDstToScene(r.origin);
                if (circleSize < minDst)
                {
                    circleSize = sceneHandler.signedDstToScene(r.origin, ref hitEnd);
                    Vector3 outcolor = PhongIllumination(ambientLight, hitEnd.colour, spec, alpha, r.origin, Pos);
                    if (hitEnd.reflective)
                    {
                        Vector3 endPos = r.origin;
                        Vector3 normal = estimateNormal(endPos);
                        Vector3 reflect = Vector3.Normalize(Reflect(r.direction, normal));
                        Ray reflectionRay = new Ray(endPos + (normal * 0.03f), reflect);
                        outcolor += (ReflectionTrace(reflectionRay, out endPos)) * 0.35f;
                    }
                    foreach (var item in sceneHandler.GetLights())
                    {
                        Ray shadowRay = new Ray(r.origin, Vector3.Normalize(item.position - r.origin));
                        if (sceneHandler.softShadows)
                        {
                            outcolor *= Softshadow(shadowRay.origin, shadowRay.direction, 1.0f, 100f, 16f);
                        }
                        else
                        {
                            outcolor *= Shadow(r.origin, shadowRay.direction);
                        }
                    }
                    rayDst += circleSize;
                    return ToSKColor(outcolor);
                }
                else
                {
                    r.origin += r.direction * circleSize;
                    rayDst += circleSize;
                }
            }
            return ToSKColor(ambientLight);
            
        }
        private SKColor ToSKColor(Vector3 col)
        {
            return new SKColor((byte)(Clamp(col.X, 0, 1) * 255f), (byte)(Clamp(col.Y, 0, 1) * 255f), (byte)(Clamp(col.Z, 0, 1) * 255f), 255);
        }
        private Vector3 estimateNormal(Vector3 pos)
        {
            return Vector3.Normalize(new Vector3(
                sceneHandler.signedDstToScene(new Vector3(pos.X + 0.0001f, pos.Y, pos.Z)) - sceneHandler.signedDstToScene(new Vector3(pos.X - 0.0001f, pos.Y, pos.Z)),
                sceneHandler.signedDstToScene(new Vector3(pos.X, pos.Y + 0.0001f, pos.Z)) - sceneHandler.signedDstToScene(new Vector3(pos.X, pos.Y - 0.0001f, pos.Z)),
                sceneHandler.signedDstToScene(new Vector3(pos.X, pos.Y, pos.Z + 0.0001f)) - sceneHandler.signedDstToScene(new Vector3(pos.X, pos.Y, pos.Z - 0.0001f))
            ));
        }

        private float Shadow(Vector3 ro, Vector3 rd)
        {
            ///based on https://www.iquilezles.org/www/articles/rmshadows/rmshadows.htm
            float h = 0f;
            for (float t = minDst; t < maxDst; t+=h)
            {
                h = sceneHandler.signedDstToScene(ro + rd * t);
                if (h < 0.001f) { 
                    return 0.0f;
                }

            }
            return 1.0f;
        }
        private float Softshadow(Vector3 ro, Vector3 rd, float mint, float maxt, float k)
        {
            ///based on https://www.iquilezles.org/www/articles/rmshadows/rmshadows.htm
            float res = 1.0f;
            float ph = 1e20f;
            float h = 0;
            for (float t = mint; t < maxt; t+=h)
            {
                h = sceneHandler.signedDstToScene(ro + rd * t);
                if (h < 0.0001f) 
                { 
                    return 0.0f;
                }
                float y = h * h / (2.0f * ph);
                float d = (float)Math.Sqrt(h * h - y * y);
                res = Math.Min(res, k * d / Math.Max(0.0f, t - y));
                ph = h;
            }
            return res;
        }
        private Vector3 Reflect(Vector3 incidentVec, Vector3 normal)
        {
            return incidentVec - 2.0f * Vector3.Dot(incidentVec, normal) * normal;
        }

        private float Clamp(float val, float min, float max)
        {
            if (val.CompareTo(min) < 0)
            {
                return min;
            }
            else if (val.CompareTo(max) > 0)
            {
                return max;
            }
            return val;
        }

        private Vector3 PhongContribForLight(Vector3 diffuse, Vector3 specular, float alpha, Vector3 p, Vector3 eye, Vector3 lightPos, Vector3 lightColour)
        {
            Vector3 n = estimateNormal(p);
            Vector3 l = Vector3.Normalize(lightPos - p);
            Vector3 v = Vector3.Normalize(eye - p);
            Vector3 r = Vector3.Normalize(Reflect(-l, n));
            float dotLN = Clamp(Vector3.Dot(l, n), 0.0f, 1.0f);
            float dotRV = Vector3.Dot(r, v);

            if (dotLN < 0.0)
            {
                return new Vector3(0.0f, 0.0f, 0.0f);
            }
            if (dotRV < 0.0)
            {
                return lightColour * (diffuse * dotLN);
            }
            return lightColour * (diffuse * dotLN + specular * (dotRV * alpha));
        }

        private Vector3 PhongIllumination(Vector3 ambient, Vector3 diffuse, Vector3 specular, float alpha, Vector3 p, Vector3 eye)
        {
            Vector3 color = ambient;

            foreach (var item in sceneHandler.GetLights())
            {
                color += PhongContribForLight(diffuse, specular, alpha, p, eye, item.position, item.colour) * 0.4545f;
            }
            return color;
        }
        private float CenterX(float x)
        {
            return (x - (width / 2.0f)) / (width);
        }

        private float CenterY(float y)
        {
            return -(y - (height / 2.0f)) / (height);
        }

        private Vector3 GetRayDir(float x, float y)
        {
            return Vector3.Normalize(Forward + ((CenterX(x) * Right) + (CenterY(y) * Up)));
        }
    }
}
