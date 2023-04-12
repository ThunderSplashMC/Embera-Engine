using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Components;

namespace DevoidPlayer.Player
{
    internal class GameLayer : Layer
    {
        Dictionary<string, string> resourceContent = new Dictionary<string, string>();
        Scene MainScene;

        public override void OnInitialize()
        {
            Assembly assembly = Assembly.LoadFrom(AppContext.BaseDirectory + "\\MyProject.dll");
            List<Type> types = assembly.GetTypes().Where(TheType => TheType.IsClass && TheType.IsSubclassOf(typeof(DevoidScript))).ToList();


            using (StreamReader sr = new StreamReader(AppContext.BaseDirectory + "\\" + "Content\\resourcetable.devoid", Encoding.UTF8))
            {
                string content = sr.ReadToEnd();

                resourceContent = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
            }

            List<string> keyList = new List<string>(resourceContent.Keys);
            List<string> sceneList = new List<string>();

            for (int i = 0; i < keyList.Count; i++)
            {
                Console.WriteLine(keyList[i]);
                Console.WriteLine(Path.GetExtension(resourceContent[keyList[i]]));
                Resources.AddResourceToPool(keyList[i], Path.Combine(AppContext.BaseDirectory, resourceContent[keyList[i]]), Path.GetExtension(keyList[i]));


                if (keyList[i].Split('.').Length > 1 && keyList[i].Split(".")[1] == "scene")
                {
                    Console.WriteLine(Path.Combine(AppContext.BaseDirectory, resourceContent[keyList[i]]));

                    sceneList.Add(resourceContent[keyList[i]]);

                }
            }

            List<Resource> res = Resources.GetPool();

            for (int i = 0; i < sceneList.Count; i++)
            {
                string path = Path.Combine(AppContext.BaseDirectory, sceneList[i]);
                string src = "";

                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                {
                    src = sr.ReadToEnd();
                }

                MainScene = DevoidEngine.Engine.Serializing.Deserializer.Deserialize(src);
            }

            GameObject a = MainScene.NewGameObject("a");

            Renderer.SetCamera(a.AddComponent<CameraComponent>().ca);

            MainScene.SetSceneState(Scene.SceneState.Play);
            
            MainScene.Init();
        }

        public override void OnUpdate(float deltaTime)
        {
            GameObject[] g = MainScene.GetSceneRegistry().GetAllGameObjects();

            for (int i = 0; i < g.Length; i++)
            {
                Console.WriteLine(g[i].name);
            }

            MainScene.OnUpdate(deltaTime);
        }

        public override void OnRender()
        {

        }

        public override void OnLateRender()
        {
            Renderer.BlitToScreen();
            MainScene.OnRender();
        }

        public override void OnResize(int width, int height)
        {
            MainScene.OnResize(width, height);
        }
    }
}
