using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Rendering;

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Assimp;
using System.Numerics;

namespace DevoidEngine.Engine.Core
{
    public class Scene
    {

        public enum SceneState {
            EditorPlay,
            Play,
            Paused
        }

        public string SceneName = "Scene";
        public SceneRegistry sceneRegistry;
        private SceneState sceneState = SceneState.Paused;
        private Vector2 currentViewportSize;

        public Physics PhysicsSystem;
        
        public Scene()
        {
            this.sceneRegistry = new SceneRegistry();
        }

        public void Init()
        {
            PhysicsSystem = new Physics();
            PhysicsSystem.Init();

            sceneRegistry.GetAllEditorRuntimeComponents();
            sceneRegistry.StartAllGameObjects();
        }

        public void OnRenderEditor()
        {

            sceneRegistry.RenderCallAllGameObjects();
            Renderer3D.BeginScene(sceneRegistry.GetLights());
        }

        public void OnUpdate(float deltaTime)
        {
            if (sceneState == SceneState.EditorPlay)
            {
                sceneRegistry.UpdateAllComponentsEditor(deltaTime);
            }
            if (sceneState == SceneState.Play)
            {
                sceneRegistry.UpdateAllGameObjects(deltaTime);
                PhysicsSystem.Update(deltaTime);
            }

                
        }
        public void OnRender()
        {
            sceneRegistry.RenderCallAllGameObjects();
            Renderer3D.BeginScene(sceneRegistry.GetLights());
        }

        public SceneRegistry GetSceneRegistry()
        {
            return sceneRegistry;
        }

        public void OnEditorGUIRender()
        {
            sceneRegistry.GUIRenderCall();
        }

        public void OnResize(int width, int height)
        {
            currentViewportSize = new Vector2(width, height);
            CameraComponent[] cameras = sceneRegistry.GetComponentsOfType<CameraComponent>();
            for (int i = 0; i < cameras.Length; i++)
            {
                CameraComponent Camera = cameras[i];
                Camera.SetViewportSize(width, height);
            }
        }

        public GameObject NewGameObject(string name = "GameObject")
        {
            GameObject gameObject = new GameObject(name);
            gameObject.scene = this;
            this.sceneRegistry.AddGameObject(gameObject);
            return gameObject;
        }

        public void RemoveGameObject(GameObject reference)
        {
            sceneRegistry.RemoveGameObject(reference);
        }

        public SceneState GetSceneState()
        {
            return sceneState;
        }

        public void SetSceneState(SceneState state)
        {
            sceneState = state;

            if (sceneState == SceneState.Play)
            {
                CameraComponent[] cameraComponents = sceneRegistry.GetComponentsOfType<CameraComponent>();

                if (cameraComponents.Length > 0)
                {
                    Renderer.SetCamera(cameraComponents[0].Camera);
                }

                sceneRegistry.StartAllGameObjects();
            }
        }

        public void OnComponentAdded(Component component)
        {

            if (component.GetType() == typeof(CameraComponent))
            {
                CameraComponent[] cameraComponents = sceneRegistry.GetComponentsOfType<CameraComponent>();
                if (cameraComponents.Length == 0)
                {
                    ((CameraComponent)component).SetViewportSize((int)currentViewportSize.X, (int)currentViewportSize.Y);
                    Renderer.SetCamera(((CameraComponent)component).Camera);
                }
            }
            if (component.GetType() == typeof(LightComponent))
            {
                ((LightComponent)component).OnStart();
                sceneRegistry.AddLight((LightComponent)component);
            }
            if (component.GetType() == typeof(Skylight))
            {
                component.OnStart();
            }


            sceneRegistry.GetAllEditorRuntimeComponents();

            if (sceneState == SceneState.Play)
            {
                component.OnStart();
            }

        }

        public void OnComponentRemoved(Component component)
        {
            if (component.GetType() == typeof(LightComponent))
            {
                System.Console.WriteLine("LIGHT REMOVED");
                sceneRegistry.RemoveLight((LightComponent)component);
            }
        }
    }
}
