using System;
using System.Collections.Generic;

using OpenTK.Mathematics;

namespace DevoidEngine.Engine.Core
{
    public class Material
    {
        public int materialIndex;
        private Shader shader;

        private List<TextureAttribute> textureAttributes = new();
        private Dictionary<string, int> uniformInts = new Dictionary<string, int>();
        private Dictionary<string, float> uniformFloats = new Dictionary<string, float>();
        private Dictionary<string, bool> uniformBools = new Dictionary<string, bool>();
        private Dictionary<string, Vector3> uniformVec3 = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector4> uniformVec4 = new Dictionary<string, Vector4>();
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
        public void Set(string name, bool value)
        {
            uniformBools[name] = value;
        }
        public void Set(string name, Vector3 value)
        {
            uniformVec3[name] = value;
        }
        public void Set(string name, Vector4 value)
        {
            uniformVec4[name] = value;
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
        public bool? GetBool(string name)
        {
            foreach (string key in uniformBools.Keys)
            {

                if (key == name)
                {
                    return uniformBools[key];
                }
            }
            return null;
        }
        public Matrix4 GetMatrix4(string name)
        {
            foreach (string key in uniformMat4.Keys)
            {

                if (key == name)
                {
                    return uniformMat4[key];
                }
            }
            return Matrix4.Zero;
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
        public Vector4 GetVec4(string name)
        {
            foreach (string key in uniformVec4.Keys)
            {

                if (key == name)
                {
                    return uniformVec4[key];
                }
            }
            return Vector4.Zero;
        }

        public void Apply()
        {
            Texture.UnbindTexture();
            foreach (string key in uniformInts.Keys)
            {
                shader.SetInt(key, uniformInts[key]);
            }
            foreach (string key in uniformFloats.Keys)
            {
                shader.SetFloat(key, uniformFloats[key]);
            }
            foreach (string key in uniformBools.Keys)
            {
                shader.SetBool(key, uniformBools[key]);
            }
            foreach (string key in uniformVec3.Keys)
            {
                shader.SetVector3(key, uniformVec3[key]);
            }
            foreach (string key in uniformVec4.Keys)
            {
                shader.SetVector4(key, uniformVec4[key]);
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
                    textureAttributes[i].Tex.BindUnit(textureAttributes.Count + textureAttributes[i].TextureIndex);
                    continue;
                }
                shader.SetInt("USE_TEX_" + textureAttributes[i].TextureIndex, 1);
                shader.SetInt(textureAttributes[i].AttrName, textureAttributes[i].TextureIndex);
                textureAttributes[i].Tex.BindUnit(textureAttributes[i].TextureIndex);
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

        public void SetPropertyFloat(Shader shader, string name)
        {
            float value = GetFloat(name);
            shader.SetFloat(name, value);
        }

        public void SetPropertyVector3(Shader shader, string name)
        {
            Vector3 value = GetVec3(name);
            shader.SetVector3(name, value);
        }

        public void SetPropertyInt(Shader shader, string name)
        {
            int value = GetInt(name);
            shader.SetInt(name, value);
        }

        public void SetPropertyTexture(Shader shader, string name, int unit)
        {
            Texture value = GetTexture(name);
            value?.BindUnit(unit);
        }

        public TextureAttribute[] GetAllTexAttributes()
        {
            return textureAttributes.ToArray();
        }
    }
}
