using System;
using System.Collections.Generic;
using System.Reflection;
using DevoidEngine.Engine.Utilities;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Rendering;

namespace DevoidEngine.Engine.Core
{
    class GameObject
    {
        public bool enabled = true;

        public string name;
        public GameObject parent;
        public Transform transform;
        public Scene scene;

        public List<GameObject> children;
        public List<Component> components;

        public int ID;

        public GameObject()
        {
            children = new List<GameObject>();
            components = new List<Component>();
            transform = AddComponent<Transform>();
        }

        public GameObject(string name)
        {
            children = new List<GameObject>();
            components = new List<Component>();
            transform = AddComponent<Transform>();
            this.name = name;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public void OnStart()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnStart();
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].OnStart();
            }
        }

        public void OnUpdate(float deltaTime)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnUpdate(deltaTime);
            }

            // Updating All Children after components
            for (int i = 0; i < children.Count; i++)
            {
                children[i].OnUpdate(deltaTime);
            }
        }

        public void OnUpdateEditor(float deltaTime)
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnUpdate(deltaTime);
            }

            // Updating All Children after components
            for (int i = 0; i < children.Count; i++)
            {
                children[i].OnUpdate(deltaTime);
            }
        }

        public void OnRender()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnRender();
            }

            // Updating All Children after components
            for (int i = 0; i < children.Count; i++)
            {
                children[i].OnRender();
            }
        }

        public void OnGUIRender()
        {
            for (int i = 0; i < components.Count; i++)
            {
                components[i].OnGUIRender();
            }

            // Updating All Children after components
            for (int i = 0; i < children.Count; i++)
            {
                children[i].OnGUIRender();
            }
        }

        public void AddChild(GameObject gameObject)
        {
            gameObject.SetParent(this);
            children.Add(gameObject);
        }

        public void SetParent(GameObject gameObject)
        {
            this.parent = gameObject;
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T _component = new();
            _component.gameObject = this;
            components.Add(_component);
            if (scene != null) 
            { 
                scene.OnComponentAdded(_component);
                if (scene.GetSceneState() == Scene.SceneState.Play) { _component.OnStart(); }
            }

            return _component;
        }

        public Component AddComponent(Component component)
        {
            Component _component = component;
            _component.gameObject = this;
            components.Add(_component);
            if (scene != null)
            {
                scene.OnComponentAdded(_component);
                if (scene.GetSceneState() == Scene.SceneState.Play) { _component.OnStart(); }
            }

            return _component;
        }

        public void RemoveComponent(Component component)
        {
            scene.OnComponentRemoved(component);
            if (!components.Remove(component))
            {
                Console.WriteLine("Unable to remove Component");
            }
        }

        public T GetComponent<T>() where T : Component
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].GetType() == typeof(T))
                {
                    return (T)components[i];
                }
            }

            return null;
        }

        public Component[] GetAllComponents()
        {
            return components.ToArray();
        }
    }
}
