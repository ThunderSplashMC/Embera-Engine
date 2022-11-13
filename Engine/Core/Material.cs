using System;
using System.Collections.Generic;

using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Core
{
    class Material
    {
        public int materialIndex;
        private Shader shader;

        private List<TextureAttribute> textureAttributes = new();
        private Dictionary<string, int> uniformInts = new Dictionary<string, int>();
        private Dictionary<string, float> uniformFloats = new Dictionary<string, float>();
        private Dictionary<string, Vector3> uniformVec3 = new Dictionary<string, Vector3>();
        private Dictionary<string, Matrix3> uniformMat3 = new Dictionary<string, Matrix3>();
        private Dictionary<string, Matrix4> uniformMat4 = new Dictionary<string, Matrix4>();

        public Material(Shader shader)
        {
            this.shader = shader;
        }

        public Material()
        {

        }

        public void SetShader(Shader shader)
        {
            this.shader = shader;
        }

        public Shader GetShader()
        {
            return shader;
        }

        public void Set(string name, int value)
        {
            uniformInts[name] = value;
        }
        public void Set(string name, float value)
        {
            uniformFloats[name] = value;
        }
        public void Set(string name, Vector3 value)
        {
            uniformVec3[name] = value;
        }
        public void Set(string name, Matrix3 value)
        {
            uniformMat3[name] = value;
        }
        public void Set(string name, Matrix4 value)
        {
            uniformMat4[name] = value;
        }

        public int GetInt(string name)
        {
            foreach(string key in uniformInts.Keys)
            {
                if (key == name)
                {
                    return uniformInts[key]; 
                }
            }
            return 0;
        }

        public float GetFloat(string name)
        {
            foreach (string key in uniformFloats.Keys)
            {
                
                if (key == name)
                {
                    return uniformFloats[key];
                }
            }
            return 0f;
        }
        public Vector3 GetVec3(string name)
        {
            foreach (string key in uniformVec3.Keys)
            {
                
                if (key == name)
                {
                    return uniformVec3[key];
                }
            }
            return Vector3.Zero;
        }

        public void UpdateUniforms()
        {
            shader.Use();
            foreach (string key in uniformInts.Keys)
            {
                shader.SetInt(key, uniformInts[key]);
            }
            foreach (string key in uniformFloats.Keys)
            {
                shader.SetFloat(key, uniformFloats[key]);
            }
            foreach (string key in uniformVec3.Keys)
            {
                shader.SetVector3(key, uniformVec3[key]);
            }
            foreach (string key in uniformMat4.Keys)
            {
                shader.SetMatrix4(key, uniformMat4[key]);
            }

            for (int i = 0; i < textureAttributes.Count; i++)
            {
                if (textureAttributes[i].AttrName == "W_SKYBOX")
                {
                    shader.SetInt(textureAttributes[i].AttrName, textureAttributes.Count + textureAttributes[i].TextureIndex);
                    textureAttributes[i].Tex.SetActiveUnit(TextureActiveUnit.UNIT0 + textureAttributes.Count + textureAttributes[i].TextureIndex, OpenTK.Graphics.OpenGL.TextureTarget.TextureCubeMap);
                    continue;
                }

                shader.SetInt("USE_TEX_" + textureAttributes[i].TextureIndex, 1);
                shader.SetInt(textureAttributes[i].AttrName, textureAttributes[i].TextureIndex);
                textureAttributes[i].Tex.SetActiveUnit(TextureActiveUnit.UNIT0 + textureAttributes[i].TextureIndex);
            }
        }

        public void SetTexture(string name, Texture texture, int TextureIndex = 0)
        {
            for (int i = 0; i < textureAttributes.Count; i++)
            {
                if (textureAttributes[i].AttrName == name)
                {
                    return;
                }
            }

            textureAttributes.Add(new TextureAttribute(name, texture, TextureIndex));
        }

        public Texture GetTexture(string name)
        {
            for (int i = 0; i < textureAttributes.Count; i++)
            {
                if (textureAttributes[i].AttrName == name)
                {
                    return textureAttributes[i].Tex;
                }
            }
            return null;
        }

        public TextureAttribute[] GetAllTexAttributes()
        {
            return textureAttributes.ToArray();
        }
    }

    struct TextureAttribute {
        public string AttrName;
        public Texture Tex;
        public int TextureIndex;

        public TextureAttribute(string AttrName, Texture Tex, int TextureIndex)
        {
            this.AttrName = AttrName;
            this.Tex = Tex;
            this.TextureIndex = TextureIndex;
        }
    }
}
