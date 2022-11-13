using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Utilities;
using ImGuiNET;
using OpenTK.Mathematics;

namespace DevoidEngine.EngineSandbox
{
    class SandboxLayer : Layer
    {

        Scene EditorScene;
        GameObject h;

        public override void OnAttach()
        {
            EditorScene = new Scene();


            PostEffectSettings.BloomSettings bloomSettings =  Renderer3D.GetBloomSettings();
            bloomSettings.enabled = true;
            bloomSettings.BloomIntensity = 0f;
            Renderer3D.SetBloomSettings(bloomSettings);


            h = EditorScene.NewGameObject("h");
            CameraComponent h1 = h.AddComponent<CameraComponent>();
            h1.CameraProjection = CameraComponent.CameraProjectionMode.Perspective;
            h.AddComponent<SphereMover>();

            h.transform.position = new OpenTK.Mathematics.Vector3(4.4f,0,0);
            h.transform.rotation = new OpenTK.Mathematics.Vector3(0, 0, 0);

            GameObject Demo = EditorScene.NewGameObject("Demo");
            Demo.AddComponent<MeshHolder>().AddMeshes(ModelImporter.LoadModel("Engine/EngineContent/models/demo.fbx"));
            Demo.AddComponent<MeshRenderer>();

            Demo.transform.rotation = new Vector3(-90, 0, 0);


            EditorScene.SetSceneState(Scene.SceneState.Play);
            EditorScene.Init();
            base.OnAttach();
        }

        public override void OnUpdate(float deltaTime)
        {
            PostEffectSettings.BloomSettings bloomSettings = Renderer3D.GetBloomSettings();
            bloomSettings.BloomIntensity += 0.0001f;
            Renderer3D.SetBloomSettings(bloomSettings);

            EditorScene.OnUpdate(deltaTime);
            base.OnUpdate(deltaTime);
        }

        public override void GUIRender()
        {
            ImGui.Begin("");
            DevoidEngine.Engine.GUI.DevoidGUI.DrawVector3Control("h", ref h.transform.position, 0f, 0.1f);
            ImGui.End();

            base.GUIRender();
        }

        public override void OnResize(int width, int height)
        {
            EditorScene.OnResize(width, height);
            base.OnResize(width, height);
        }
    }
}
