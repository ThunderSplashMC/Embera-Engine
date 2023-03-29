using System;
using System.Collections.Generic;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;

using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Components
{
    [RunInEditMode]
    public class LightComponent : Component
    {
        public override string Type { get; } = nameof(LightComponent);

        [NonSerialized]
        public int Intensity = 1;
        public float Attenuation = 1f;
        public LightType Lighttype = LightType.PointLight;
        public Color4 color = new Color4(1f,1f,1f,1f);
        public Vector3 direction = new Vector3(0,-1,0);
        public float cutOff = (float)Math.Cos(MathHelper.DegreesToRadians(12.5f));
        public float OuterCutSoftness;
        public float OuterCutOff;

        public bool CanEmitShadows = true;

        private int LightID;

        // SHADOW -- POINTLIGHTS

        public FrameBuffer shadowBufferPointLight;
        public Texture depthTexture;

        public LightComponent()
        {

        }

        public override void OnStart()
        {
            direction = -Vector3.UnitY;
            shadowBufferPointLight = new FrameBuffer(new FrameBufferSpecification()
            {
                width = 1000,
                height = 1000,
                DepthAttachment = new DepthAttachment()
                {
                    width = 1000,
                    height = 1000,
                    textureType = FrameBufferTextureType.Cubemap
                }
            });
            depthTexture = new Texture(shadowBufferPointLight.GetDepthAttachment());
        }

        public void GenDepthBuffer()
        {
            shadowBufferPointLight = new FrameBuffer(new FrameBufferSpecification()
            {
                width = 1000,
                height = 1000,
                DepthAttachment = new DepthAttachment()
                {
                    width = 1000,
                    height = 1000,
                    textureType = FrameBufferTextureType.Cubemap
                }
            });
            depthTexture = new Texture(shadowBufferPointLight.GetDepthAttachment());
        }

        public void SetLightID(int id)
        {
            LightID = id;
        }

        public int GetLightID()
        {
            return LightID;
        }

        private Vector3 prevRot;
        private LightType prevType;

        public override void OnUpdate(float deltaTime)
        {
        //    OuterCutOff = cutOff - OuterCutSoftness;

            if (gameObject.transform.rotation != prevRot)
            {

                // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.

                //direction = gameObject.transform.position + (gameObject.transform.rotation * GetFrontVector());
                //direction.Normalize();


                Vector3.Transform(direction, Quaternion.FromEulerAngles(new Vector3(MathHelper.DegreesToRadians(gameObject.transform.rotation.X), MathHelper.DegreesToRadians(gameObject.transform.rotation.Y), MathHelper.DegreesToRadians(gameObject.transform.rotation.Z))));

                direction = Vector3.Transform(-Vector3.UnitY, Quaternion.FromEulerAngles(gameObject.transform.rotation));
                direction.Normalize();

                prevRot = gameObject.transform.rotation;
                
            }


            base.OnUpdate(deltaTime);
        }

        public Vector3 GetFrontVector()
        {
            float PITCH = MathHelper.DegreesToRadians(gameObject.transform.rotation.Y);
            float YAW = MathHelper.DegreesToRadians(gameObject.transform.rotation.X);

            Vector3 _front;

            _front.X = MathF.Cos(PITCH) * MathF.Cos(YAW);
            _front.Y = MathF.Sin(PITCH);
            _front.Z = MathF.Cos(PITCH) * MathF.Sin(YAW);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            _front = Vector3.Normalize(_front);

            return _front;
        }

        public Vector3 GetRightVector()
        {
            return Vector3.Normalize(Vector3.Cross(GetFrontVector(), Vector3.UnitY));
        }

        public Vector3 GetUpVector()
        {
            return Vector3.Normalize(Vector3.Cross(GetRightVector(), GetFrontVector()));
        }

        public enum LightType
        {
            PointLight,
            SpotLight,
            DirectionalLight
        }
    }
}
