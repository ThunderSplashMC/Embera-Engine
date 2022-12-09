using DevoidEngine.Engine.Core;


namespace DevoidEngine
{
    class Program
    {

        static void Main(string[] args)
        {
#if EDITORMODE
            Elemental.ElementalApp elemental = new Elemental.ElementalApp();
#else
            ApplicationSpecification applicationSpecification = new ApplicationSpecification()
            {
                AntiAliasingSamples = 4,
                WindowHeight = 1080,
                WindowWidth = 1920,
                FramesPerSecond = 60,
                Vsync = true,
                WindowTitle = "My Game",
                WindowFullscreen = false,
                workingDir = System.Reflection.Assembly.GetExecutingAssembly().Location
            };
            Application application = new Application();
            application.Create(ref applicationSpecification);
            application.AddLayer(new Game());
            application.Run();
#endif
        }
    }
}
