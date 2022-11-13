using System;
using System.Collections.Generic;

namespace DevoidEngine.Engine.Core
{
    class LayerManager
    {
        public List<Layer> Layers = new List<Layer>();

        public LayerManager()
        {
            
        }

        public void AddLayer(Layer layer)
        {
            Layers.Add(layer);
            layer.OnAttach();
        }

        public T GetLayer<T>() where T : Layer, new()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                if (Layers[i].GetType() == typeof(T))
                {
                    return (T)Layers[i];
                }
            }
            return new T();
        }

        public void KeyDownLayers(OpenTK.Windowing.Common.KeyboardKeyEventArgs eventArgs)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].KeyDown(eventArgs);
            }
        }

        public void RemoveLayer(Layer layer)
        {
            Layers.Remove(layer);
            layer.OnDetach();
        }

        public void UpdateLayers(float deltaTime)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnUpdate(deltaTime);
            }
        }
        
        public void RenderLayers()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnRender();
            }
        }

        public void ResizeLayers(int width, int height)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].OnResize(width, height);
            }
        }
    }
}
