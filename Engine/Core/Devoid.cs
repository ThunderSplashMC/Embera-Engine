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

        public static void OnDebugMessage(
           DebugSource source,     // Source of the debugging message.
           DebugType type,         // Type of the debugging message.
           int id,                 // ID associated with the message.
           DebugSeverity severity, // Severity of the message.
           int length,             // Length of the string in pMessage.
           IntPtr pMessage,        // Pointer to message string.
           IntPtr pUserParam)      // The pointer you gave to OpenGL, explained later.
        {
            // In order to access the string pointed to by pMessage, you can use Marshal
            // class to copy its contents to a C# string without unsafe code. You can
            // also use the new function Marshal.PtrToStringUTF8 since .NET Core 1.1.
            string message = Marshal.PtrToStringAnsi(pMessage, length);

            // The rest of the function is up to you to implement, however a debug output
            // is always useful.

            // Potentially, you may want to throw from the function for certain severity
            // messages.
            if (type == DebugType.DebugTypeError)
            {
                Console.WriteLine(" ");
                Console.WriteLine("##################");
                Console.WriteLine("ERR: " + message);
                Console.WriteLine("SOURCE: " + source);
                Console.WriteLine("TYPE: " + type);
                Console.WriteLine("ID: " + id);
                Console.WriteLine("##################");
                Console.WriteLine(" ");
                //Console.WriteLine("[{0} source={1} type={2} id={3}] {4}", severity, source, type, id, message);
            }
            else if (type == DebugType.DebugTypeDeprecatedBehavior)
            {
                Console.WriteLine("DEPRECATED: " + message);
            }
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
