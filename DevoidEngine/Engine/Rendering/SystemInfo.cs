using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Rendering
{
    public struct DeviceInformation
    {
        public string RENDERER_NAME;
        public string GL_VERSION;
        public string SHADER_LANG_VERSION;
        public string GRAPHICS_VENDOR;
        public string EXTENSIONS;
        public bool isComputeShaderSupported;
        public int Major, Minor;
        public override string ToString() 
        {
            string info = "";
            info += $"Renderer                  : {RENDERER_NAME}\n";
            info += $"Version                   : {GL_VERSION}\n";
            info += $"Version X.x               : {Major}.{Minor}\n";
            info += $"Shader Version            : {SHADER_LANG_VERSION}\n";
            info += $"Graphics Vendor           : {GRAPHICS_VENDOR}\n";
            info += $"Extensions                : {EXTENSIONS}\n";
            info += $"Compute Shader Supported  : {isComputeShaderSupported}\n";
            return info;
        }
    }

    public static class SystemInfo
    {
        static DeviceInformation DeviceInformation;

        public static void Validate()
        {
            DeviceInformation = new DeviceInformation()
            {
                RENDERER_NAME = GL.GetString(StringName.Renderer),
                GL_VERSION = GL.GetString(StringName.Version),
                SHADER_LANG_VERSION = GL.GetString(StringName.ShadingLanguageVersion),
                GRAPHICS_VENDOR = GL.GetString(StringName.Vendor),
                EXTENSIONS = (GL.GetString(StringName.Extensions) == string.Empty ? "None" : GL.GetString(StringName.Extensions))
            };

            try
            {
                string versionString = DeviceInformation.GL_VERSION;
                DeviceInformation.Major = int.Parse(versionString[0].ToString());
                DeviceInformation.Minor = int.Parse(versionString[2].ToString());

                if ((DeviceInformation.Major == 4 && DeviceInformation.Minor >= 3) || (DeviceInformation.Major > 4))
                {
                    DeviceInformation.isComputeShaderSupported = true;
                }
                else
                {
                    DeviceInformation.isComputeShaderSupported = false;
                }
            } catch (Exception e) { Console.WriteLine("There was an error while parsing the opengl version\n" + e.Message); }


            Console.WriteLine(DeviceInformation.ToString());
        }

        public static DeviceInformation GetDeviceInfo()
        {
            return DeviceInformation;
        }

    }
}
