using DevoidEngine.Elemental;


namespace DevoidEngine
{
    class Program
    {
        static void Main(string[] args)
        {

            new DevoidLauncher.Launcher().Init();
            return;

#if DEBUG
            ElementalApp Application = new ElementalApp();
#endif
#if RELEASE
            DevoidEngine.EngineSandbox.SandboxApp app = new EngineSandbox.SandboxApp();
#endif
        }
    }
}
