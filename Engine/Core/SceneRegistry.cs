using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Components;
using System.Reflection;

namespace DevoidEngine.Engine.Core
{
    class SceneRegistry
    {

        public List<GameObject> GameObjects = new List<GameObject>();
        public List<Camera> Cameras = new List<Camera>();
        public List<LightComponent> Lights = new List<LightComponent>();
        public List<Component> EditorRunnableComponents = new List<Component>();
        public List<Rigidbody> RigidBodies = new List<Rigidbody>();

        public void AddGameObject(GameObject gameObject)
        {
            gameObject.ID = GameObjects.Count + new Random().Next(10000);
            GameObjects.Add(gameObject);
        }

        public void GetGameObjectByID()
        {

        }

        public GameObject GetGameObjectByName(string name)
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                if (GameObjects[i].name == name)
                {
                    return GameObjects[i];
                }
            }
            return null;
        }

        public void RemoveGameObject(string name)
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                if (GameObjects[i].name == name)
                {
                    GameObjects.Remove(GameObjects[i]);
                }
            }
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            GameObjects.Remove(gameObject);
        }

        public GameObject[] GetAllGameObjects() {
            return GameObjects.ToArray();
        }

        public int GetGameObjectCount()
        {
            return GameObjects.Count;
        }

        public List<Rigidbody> GetRigidBodies()
        {
            return RigidBodies;
        }

        public void AddRigidBody(Rigidbody rigidbody)
        {
            rigidbody.RigidbodyID = RigidBodies.Count;
            RigidBodies.Add(rigidbody);
        }

        public void AddLight(LightComponent light)
        {
            Lights.Add(light);
        }

        public void RemoveLight(LightComponent light)
        {
            Lights.Remove(light);
        }

        public ref List<LightComponent> GetLights()
        {
            return ref Lights;
        }


        public T[] GetComponentsOfType<T>() where T : Component
        {
            List<T> ComponentsOfType = new List<T>();
            for (int i = 0; i < GameObjects.Count; i++)
            {
                T Component = GameObjects[i].GetComponent<T>();
                if (Component != null)
                {
                    ComponentsOfType.Add(Component);
                }
            }
            return ComponentsOfType.ToArray();
        }

        public Component[] GetAllComponentsInRegistry()
        {
            List<Component> components = new List<Component>();
            for (int i = 0; i < GameObjects.Count; i++)
            {
                components.AddRange(GameObjects[i].GetAllComponents());
            }
            return components.ToArray();
        }

        public void UpdateAllGameObjects(float deltaTime)
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnUpdate(deltaTime);
            }
        }

        public void UpdateAllComponentsEditor(float deltaTime)
        {
            for (int i = 0; i < EditorRunnableComponents.Count; i++)
            {
                EditorRunnableComponents[i].OnUpdate(deltaTime);
            }
        }

        public void RenderCallAllComponentsEditor()
        {
            for (int i = 0; i < EditorRunnableComponents.Count; i++)
            {
                EditorRunnableComponents[i].OnRender();
            }
        }

        public void RenderCallAllGameObjects()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnRender();
            }
        }

        public void GetAllEditorRuntimeComponents()
        {
            Type[] types = Assembly.GetAssembly(typeof(Component)).GetTypes();
            
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];


                if (type.GetCustomAttribute(typeof(RunInEditMode)) != null)
                {
                    if (type.IsSubclassOf(typeof(Component)))
                    {

                        
                        Component[] components = GetAllComponentsInRegistry();
                        for (int x = 0; x < components.Length; x++)
                        {
                            if (components[x].GetType() == type)
                            {
                                //EditorRunnableComponents.Add(components[x]);
                            }
                        }
                    }
                }
            }

        }

        public void StartAllGameObjects()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnStart();
            }
        }

        public void GUIRenderCall()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnGUIRender();
            }
        }
    }
}
