using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Rendering;

using System.Collections.Generic;

namespace DevoidEngine.Engine.Core
{
    class Scene
    {

        public enum SceneState {
            EditorPlay,
            Play,
            Paused
        }


        private SceneRegistry sceneRegistry;
        private SceneState sceneState = SceneState.Paused;

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

        void UpdateRigidBodies()
        {
            List<Rigidbody> rigidbodies = sceneRegistry.GetRigidBodies();
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                for (int x = 0; x < PhysicsSystem.PhysicsWorld.CollisionObjectArray.Count; x++)
                {

                    BulletSharp.RigidBody rigidBody = rigidbodies[i].body;

                    BulletSharp.Math.Matrix transform = rigidBody.WorldTransform;
                    System.Console.WriteLine(transform);
                    rigidbodies[i].gameObject.transform.position = new OpenTK.Mathematics.Vector3(transform.M41, transform.M42, transform.M43);
                }
            }
        }

        public void OnRenderEditor()
        {
            Renderer3D.BeginScene(ref sceneRegistry.GetLights());
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
                UpdateRigidBodies();
            }

                
        }
        public void OnRender()
        {
            Renderer3D.BeginScene(ref sceneRegistry.GetLights());
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
                    Renderer3D.SetCamera(cameraComponents[0].Camera);
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
                    Renderer3D.SetCamera(((CameraComponent)component).Camera);
                }
            }
            if (component.GetType() == typeof(LightComponent))
            {
                sceneRegistry.AddLight((LightComponent)component);
            }
            if (component.GetType() == typeof(Rigidbody))
            {
                Rigidbody rigidbody = (Rigidbody)component;
                sceneRegistry.AddRigidBody(rigidbody);
            }
            if (component.GetType() == typeof(Skylight))
            {
                if ((sceneRegistry.GetComponentsOfType<Skylight>()).Length > 1)
                {

                }
            }


            sceneRegistry.GetAllEditorRuntimeComponents();
        }

        public void OnComponentRemoved(Component component)
        {
            if (component.GetType() == typeof(LightComponent))
            {
                sceneRegistry.RemoveLight((LightComponent)component);
            }
            if (component.GetType() == typeof(Rigidbody))
            {
                Rigidbody rigidbody = (Rigidbody)component;
                PhysicsSystem.PhysicsWorld.RemoveRigidBody(rigidbody.body);
            }
        }
    }
}
