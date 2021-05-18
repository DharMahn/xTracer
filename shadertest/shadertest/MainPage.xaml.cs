using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Numerics;
using System.Globalization;
using System.Timers;
using System.Collections.ObjectModel;
using SQLite;
using Xamarin.Essentials;
using System.IO;
using Android;
using Plugin.Permissions;
using Android.Widget;
using Android.App;
using Android.Content.PM;

namespace shadertest
{
    //[Activity(Label = "xTracer", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public partial class MainPage : ContentPage
    {
        SKBitmap renderedImage;
        SceneHandler sc;
        SKBitmap bigImage;
        Random r = new Random();
        Timer renderTimer;
        public ObservableCollection<Shape> scShapes;
        public ObservableCollection<Light> scLights;
        Dictionary<Type, string> types = new Dictionary<Type, string>();
        bool changing = false;
        SQLiteConnection db;
        bool shouldIRedraw = true;
        public MainPage()
        {
            InitializeComponent();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string databaseFilePath = Path.Combine(path, "Scene.db");
            db = new SQLiteConnection(databaseFilePath);
            renderedImage = new SKBitmap((int)DeviceDisplay.MainDisplayInfo.Width / 4, ((int)DeviceDisplay.MainDisplayInfo.Height - 125) / 4);
            sc = new SceneHandler(GetCameraFromEntry(), db);
            renderTimer = new Timer();
            renderTimer.Interval = 100;
            renderTimer.Elapsed += RenderTimer_Elapsed;
            renderTimer.Start();
            BindingContext = this;
            LoadShapesIntoOC();
            LoadLightsIntoOC();
            TypePicker.SelectedIndex = 0;
            shapeOptions.IsVisible = true;
            shapeOptions.IsEnabled = true;
            optionsView.Content = shapeOptions;
            #region Reset Table
            /*db.DropTable<SceneDescription>();
            db.DropTable<Shape>();
            db.DropTable<Light>();
            db.CreateTable<SceneDescription>();
            db.CreateTable<Shape>();
            db.CreateTable<Light>();
            sc.LoadRandomScene();
            sc.SaveScene("RandomShapes");
            sc.SingleSphereInfrontOfCamera();
            sc.SaveScene("OneSphere");
            sc.shapes = new List<Shape>();
            sc.lights = new List<Light>();*/
            //ha mindent shapeként mentesz akkor szar lesz a SDF mert nincs a shapenek SDF-je
            /*db.DropTable<SceneDescription>();
            db.DropTable<Shape>();
            db.DropTable<Light>();
            db.DropTable<Torus>();
            db.DropTable<Box>();
            db.DropTable<Sphere>();
            db.DropTable<Plane>();*/
            db.CreateTable<SceneDescription>();
            db.CreateTable<Light>();
            db.CreateTable<Torus>();
            db.CreateTable<Box>();
            db.CreateTable<Sphere>();
            db.CreateTable<Plane>();
            sc.LoadRandomScene();
            sc.SaveScene("RandomShapes");
            sc.SingleSphereInfrontOfCamera();
            sc.SaveScene("OneSphere");
            sc.shapes = new List<Shape>();
            sc.lights = new List<Light>();
            //sc.SingleSphereInfrontOfCamera();
            #endregion
        }

        private void UpdateLight_Clicked(object sender, EventArgs e)
        {
            Light selected = (Light)lightList.SelectedItem;
            selected.position = new Vector3(
                            float.Parse(lightX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(lightY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(lightZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                            );
            selected.colour = new Vector3(
                            float.Parse(lightColorX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(lightColorY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(lightColorZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                            );
            selected.Name = lightName.Text;
        }
        private void UpdateShape_Clicked(object sender, EventArgs e)
        {
            Shape selected = (Shape)shapeList.SelectedItem;
            selected.position = new Vector3(
                            float.Parse(shapeX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                            );
            selected.size = new Vector3(
                            float.Parse(shapeSizeX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeSizeY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeSizeZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                            );
            selected.colour = new Vector3(
                            float.Parse(shapeColorX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeColorY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeColorZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                            );
            selected.reflective = reflectiveCheckBox.IsChecked;
            selected.Name = lightName.Text;
        }

        private void LoadLightsIntoOC()
        {
            scLights = new ObservableCollection<Light>();
            foreach (Light item in sc.lights)
            {
                scLights.Add(item);
            }
            lightList.ItemsSource = scLights;
        }

        private void LoadShapesIntoOC()
        {
            scShapes = new ObservableCollection<Shape>();
            foreach (Shape item in sc.shapes)
            {
                scShapes.Add(item);
            }
            shapeList.ItemsSource = scShapes;
        }

        private void RenderTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (sc.mainCamera.finishedRendering)
            {
                //RenderButton.IsEnabled = true;
                if (shouldIRedraw)
                {
                    try
                    {
                        mySKCanvasView.InvalidateSurface();
                        //RenderButton.IsEnabled = true;
                        progressBar.Progress = 1;
                    }
                    catch (Exception)
                    {
                        return;
                    }
                    //RenderButton.IsEnabled = true;
                    progressBar.Progress = 1;
                    shouldIRedraw = false;
                }
            }
            else if (sc.mainCamera.startedRendering)
            {
                shouldIRedraw = true;
            }

        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Aqua);
            var resized = renderedImage.Resize(info, SKFilterQuality.High);
            canvas.DrawBitmap(resized, info.Width / 2 - resized.Width / 2, info.Height / 2 - resized.Height / 2);
        }

        async void OnCanvasViewTapped(object sender, EventArgs e)
        {
            List<string> scenes = new List<string>();
            string action = await DisplayActionSheet("What to do?", null, null, "Load Scene", "Save Scene","Delete Scene", "Render Scene");
            if (action == "Load Scene")
            {
                var sceneDescs = db.Table<SceneDescription>();
                foreach (var item in sceneDescs)
                {
                    scenes.Add(item.sceneName);
                }
                string whatToLoad = await DisplayActionSheet("Which scene to load?", null, null, scenes.ToArray());
                if (whatToLoad != null)
                {
                    sc.LoadScene(whatToLoad);
                    sc.SetMainCamera(GetCameraFromEntry());
                    LoadShapesIntoOC();
                    LoadLightsIntoOC();
                }
            }
            else if (action == "Save Scene")
            {
                string sceneName = await DisplayPromptAsync("Save Scene", "Scene name:");
                sc.SaveScene(sceneName);
            }
            else if(action=="Delete Scene")
            {
                var sceneDescs = db.Table<SceneDescription>();
                foreach (var item in sceneDescs)
                {
                    scenes.Add(item.sceneName);
                }
                string whatToDelete = await DisplayActionSheet("Which scene to load?", null, null, scenes.ToArray());
                if (whatToDelete != null)
                {
                    sc.DeleteScene(whatToDelete);
                }

            }
            else if (action == "Render Scene")
            {
                bigImage = new SKBitmap(1024, 1024);
                sc.mainCamera.finishedRendering = false;
                sc.mainCamera.Render(bigImage);
                Toast.MakeText(Android.App.Application.Context, "Rendering, please wait! (it may take a loooong time)", ToastLength.Long).Show();
                while (!sc.mainCamera.finishedRendering)
                {
                    Console.WriteLine("still busy");
                    await Task.Delay(25);
                }
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (SKManagedWStream wstream = new SKManagedWStream(ms))
                        {
                            bigImage.Encode(wstream, SKEncodedImageFormat.Png, 100);
                            byte[] data = ms.ToArray();
                            var status = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
                            if (status != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                            {
                                await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                                status = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                            }
                            if (status == Plugin.Permissions.Abstractions.PermissionStatus.Granted)
                            {
                                string filename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
                                if (!Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/DCIM/xTracer"))
                                {
                                    Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/DCIM/xTracer");
                                }
                                if (File.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/DCIM/xTracer/" + filename))
                                {
                                    File.Delete(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/DCIM/xTracer/" + filename);
                                }

                                using (var stream = File.Create(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/DCIM/xTracer/" + filename))
                                {
                                    stream.Write(data, 0, data.Length);
                                    Toast.MakeText(Android.App.Application.Context, "Finished rendering!, saved to "+ Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/DCIM/xTracer/" + filename, ToastLength.Long).Show();
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }

            }
        }



        private void RenderButton_Clicked(object sender, EventArgs e)
        {
            //RenderButton.IsEnabled = false;
            progressBar.Progress = 0;
            sc.SetMainCamera(GetCameraFromEntry());
            if (sc.shapes != null && sc.lights != null)
            {
                sc.mainCamera.Render(renderedImage, progressBar);
            }
        }

        private Camera GetCameraFromEntry()
        {
            return new Camera(new Vector3(
                float.Parse(cameraX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(cameraY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(cameraZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                ), new Vector3(
                float.Parse(cameraLookAtX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(cameraLookAtY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(cameraLookAtZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                ));
        }

        private void reflectionBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sc != null)
            {
                List<Shape> shapes = sc.GetShapes();
                foreach (var item in shapes)
                {
                    item.reflective = globalReflectionBox.IsChecked;
                }
                sc.SetShapes(shapes);
            }
        }
        private void shapeList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (!changing)
            {
                changing = true;
                lightList.SelectedItem = null;
                shapeOptions.IsVisible = true;
                shapeOptions.IsEnabled = true;
                lightOptions.IsVisible = false;
                lightOptions.IsEnabled = false;
                optionsView.Content = shapeOptions;
                Shape selected = (Shape)((Xamarin.Forms.ListView)sender).SelectedItem;
                string type = selected.GetType().ToString();
                switch (type)
                {
                    case "shadertest.Sphere":
                        TypePicker.SelectedIndex = 0;
                        break;
                    case "shadertest.Box":
                        TypePicker.SelectedIndex = 1;
                        break;
                    case "shadertest.Torus":
                        TypePicker.SelectedIndex = 2;
                        break;
                    case "shadertest.Plane":
                        TypePicker.SelectedIndex = 3;
                        break;
                    default:
                        break;
                }
                shapeX.Text = selected.position.X.ToString();
                shapeY.Text = selected.position.Y.ToString();
                shapeZ.Text = selected.position.Z.ToString();
                shapeSizeX.Text = selected.size.X.ToString();
                shapeSizeY.Text = selected.size.Y.ToString();
                shapeSizeZ.Text = selected.size.Z.ToString();
                shapeColorX.Text = selected.colour.X.ToString();
                shapeColorY.Text = selected.colour.Y.ToString();
                shapeColorZ.Text = selected.colour.Z.ToString();
                reflectiveCheckBox.IsChecked = selected.reflective;
                changing = false;
            }
        }

        private void lightList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (!changing)
            {
                changing = true;
                shapeList.SelectedItem = null;
                shapeOptions.IsVisible = false;
                shapeOptions.IsEnabled = false;
                lightOptions.IsVisible = true;
                lightOptions.IsEnabled = true;
                optionsView.Content = lightOptions;
                Light selected = ((Xamarin.Forms.ListView)sender).SelectedItem as Light;
                lightX.Text = selected.position.X.ToString();
                lightY.Text = selected.position.Y.ToString();
                lightZ.Text = selected.position.Z.ToString();
                lightColorX.Text = selected.colour.X.ToString();
                lightColorY.Text = selected.colour.Y.ToString();
                lightColorZ.Text = selected.colour.Z.ToString();
                lightName.Text = selected.Name;
                TypePicker.SelectedIndex = 4;
                changing = false;
            }
        }
        private void ColorEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue)) return;
            if (float.TryParse(e.NewTextValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
            {
                return;
            }
            else
            {
                ((Entry)sender).Text = e.OldTextValue;
            }
        }
        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                return;
            }
            if (e.NewTextValue.Equals("-"))
            {
                return;
            }
            if (!float.TryParse(e.NewTextValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
            {
                ((Entry)sender).Text = e.OldTextValue;
            }
        }

        private void AddShape_Clicked(object sender, EventArgs e)
        {
            switch (TypePicker.SelectedIndex)
            {
                case 0:
                    sc.shapes.Add(new Sphere(
                        new Vector3(
                            float.Parse(shapeX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), new Vector3(
                            float.Parse(shapeSizeX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeSizeY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeSizeZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), new Vector3(
                            float.Parse(shapeColorX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeColorY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeColorZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), shapeName.Text
                        ));
                    sc.shapes.Last().reflective = globalReflectionBox.IsChecked;
                    break;
                case 1:
                    sc.shapes.Add(new Box(
                        new Vector3(
                            float.Parse(shapeX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), new Vector3(
                            float.Parse(shapeSizeX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeSizeY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeSizeZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), new Vector3(
                            float.Parse(shapeColorX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeColorY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeColorZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), shapeName.Text
                        ));
                    sc.shapes.Last().reflective = globalReflectionBox.IsChecked;
                    break;
                case 2:
                    sc.shapes.Add(new Torus(
                        new Vector3(
                            float.Parse(shapeX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), new Vector3(
                            float.Parse(shapeSizeX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeSizeY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeSizeZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), new Vector3(
                            float.Parse(shapeColorX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeColorY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeColorZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), shapeName.Text
                        ));
                    sc.shapes.Last().reflective = globalReflectionBox.IsChecked;
                    break;
                case 3:
                    sc.shapes.Add(new Plane(
                        new Vector3(
                            float.Parse(shapeX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), new Vector3(
                            float.Parse(shapeSizeX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeSizeY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeSizeZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), new Vector3(
                            float.Parse(shapeColorX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeColorY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                            float.Parse(shapeColorZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                        ), shapeName.Text
                    ));
                    sc.shapes.Last().reflective = globalReflectionBox.IsChecked;
                    break;
                default:
                    break;

            }
            LoadShapesIntoOC();
        }

        private void RemoveShape_Clicked(object sender, EventArgs e)
        {
            sc.shapes.Remove(shapeList.SelectedItem as Shape);
            LoadShapesIntoOC();
        }
        private void AddLight_Clicked(object sender, EventArgs e)
        {
            sc.lights.Add(new Light(
                new Vector3(
                    float.Parse(lightX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    float.Parse(lightY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    float.Parse(lightZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                ), new Vector3(
                    float.Parse(lightColorX.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    float.Parse(lightColorY.Text, NumberStyles.Any, CultureInfo.InvariantCulture),
                    float.Parse(lightColorZ.Text, NumberStyles.Any, CultureInfo.InvariantCulture)
                ), lightName.Text
            ));
            LoadLightsIntoOC();
        }

        private void RemoveLight_Clicked(object sender, EventArgs e)
        {
            sc.lights.Remove(lightList.SelectedItem as Light);
            LoadLightsIntoOC();

        }

        private void TypePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker picker = sender as Picker;
            if (picker.SelectedIndex == 4)
            {
                shapeOptions.IsVisible = false;
                shapeOptions.IsEnabled = false;
                lightOptions.IsVisible = true;
                lightOptions.IsEnabled = true;
                optionsView.Content = lightOptions;
            }
            else
            {
                shapeOptions.IsVisible = true;
                shapeOptions.IsEnabled = true;
                lightOptions.IsVisible = false;
                lightOptions.IsEnabled = false;
                optionsView.Content = shapeOptions;
            }
        }
    }


}
