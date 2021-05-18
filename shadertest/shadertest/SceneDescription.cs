using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
namespace shadertest
{
    class SceneDescription
    {
        [PrimaryKey, AutoIncrement]
        public int sceneID { get; set; }
        public string sceneName { get; set; }
    }
}
