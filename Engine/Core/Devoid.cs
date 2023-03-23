using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace DevoidEngine.Engine.Core
{
    public static class Devoid
    {
        public static void LOGMESSAGE(string text)
        {
        #if DEBUG
            Console.WriteLine("[DEBUG] " + text);
        #elif RELEASE
            Console.WriteLine("[RELEASE] " + text);
        #endif
        }


        public static void SCOPE_START(string scope_nm)
        {

        }

        public static void SCOPE_END()
        {

        }

        


        public struct ScopeData
        {
            public string name;
            public DateTime time;
        }
    }
}
