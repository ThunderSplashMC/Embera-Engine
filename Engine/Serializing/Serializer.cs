using System;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Components;
using System.Collections.Generic;
using OpenTK.Mathematics;
using YamlDotNet.Serialization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using DevoidEngine.Engine.Serializing.Converters;

namespace DevoidEngine.Engine.Serializing
{
    class Serializer
    {

        public struct SceneSerialized
        {
            public List<GameObjectSerialized> GameObjects;
        }

        public struct GameObjectSerialized
        {
            public string name;
            public List<ComponentSerialized> Components;
        }

        public struct ComponentSerialized
        {
            public Dictionary<string, int> IntFields;
            public Dictionary<string, float> FloatFields;
            public Dictionary<string, Vector3> Vector3Fields;
        }

        //public static SceneSerialized SerializeScene(Scene scene)
        //{
        //    SceneSerialized Scene = new SceneSerialized();
        //    Scene.GameObjects = new List<GameObjectSerialized>();
        //    SceneRegistry sceneRegistry = scene.GetSceneRegistry();
        //    GameObject[] gameObjects = sceneRegistry.GetAllGameObjects();

        //    for (int i = 0; i < gameObjects.Length; i++)
        //    {
        //        Scene.GameObjects.Add(SerializeGameObject(gameObjects[i]));
        //    }
        //    return Scene;
        //}

        public static void SerializeToJson(Scene scene)
        {
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(scene, typeof(Scene)));
        }

        public static GameObjectSerialized SerializeGameObject(GameObject gameObject)
        {
            GameObjectSerialized GameObject = new();
            GameObject.Components = new();
            GameObject.name = gameObject.name;

            for (int i = 0; i < gameObject.components.Count; i++)
            {
                GameObject.Components.Add(SerializeComponent(gameObject.components[i]));
            }
            return GameObject;
        }

        public static ComponentSerialized SerializeComponent(Component component)
        {
            return new ComponentSerialized();
        }

        public static string ConvertToYaml(SceneSerialized scene)
        {
            ISerializer serializerBuilder = new SerializerBuilder().Build();


            return serializerBuilder.Serialize(scene);
        }

        public static string ConvertYamlToScene(string yaml)
        {
            return "";
        }


        public static string SerializeScene(Scene scene)
        {
            scene.PhysicsSystem = null;
            string serScene = JsonConvert.SerializeObject(scene, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            });
            return serScene;
        }

        public static Scene DeserializeScene(string scene)
        {
            Scene sc = JsonConvert.DeserializeObject<Scene>(scene, new Vector3CreateConverter(), new ComponentConverter());

            SceneRegistry sr = sc.GetSceneRegistry();

            for (int i = 0; i < sr.GameObjects.Count; i++)
            {
                Component[] components = sr.GameObjects[i].GetAllComponents();
                for (int y = 0; y < components.Length; y++)
                {
                    components[y].gameObject = sr.GameObjects[i];
                }
            }

            return sc;
        }
    }
}
