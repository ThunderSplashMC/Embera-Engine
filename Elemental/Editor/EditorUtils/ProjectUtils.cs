using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Elemental.Editor.EditorUtils
{
    public class ProjectUtils
    {
        string path;
        string fileContentCache;

        JsonObject jsonContentCache;

        public void LoadFile(string ProjectFilePath)
        {
            this.path = ProjectFilePath;

            using (StreamReader reader = new StreamReader(ProjectFilePath, Encoding.UTF8))
            {
                fileContentCache = reader.ReadToEnd();
            }

            jsonContentCache = JsonObject.Parse(fileContentCache).AsObject();
        }

        public void Reload()
        {
            LoadFile(path);
        }

        public string GetProjectBasePath()
        {
            return (string)jsonContentCache["Project Directory"];
        }

        public string GetProjectName()
        {
            return (string)jsonContentCache["Project Name"];
        }

        public static string CreateProject(string basePath, string name)
        {
            using (FileStream fs = File.Create(basePath + "/Project.dprj"))
            {
                JsonObject ProjectJson = new JsonObject();

                ProjectJson.Add("Project Name", name);
                ProjectJson.Add("Project Directory", basePath);

                string dataasstring = ProjectJson.ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }); //your data
                byte[] info = new UTF8Encoding(true).GetBytes(dataasstring);
                fs.Write(info, 0, info.Length);
            }

            Directory.CreateDirectory(basePath + "/Assets");
            Directory.CreateDirectory(basePath + "/Builds");
            Directory.CreateDirectory(basePath + "/Tool");

            //CopyFilesRecursively(Path.Combine(AppContext.BaseDirectory, "DevoidPlayer"), Path.Combine(basePath, "Tool"));

            CreateVSProject(basePath);

            return basePath + "/Project.dprj";
        }

        public static void CreateVSProject(string directory)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine($"dotnet new classlib -o {directory} --name MyProject");
            cmd.StandardInput.Flush();

            cmd.StandardInput.WriteLine($"dotnet new sln -o {directory} --name MyProject");
            cmd.StandardInput.Flush();

            cmd.StandardInput.WriteLine($"dotnet sln {directory}\\MyProject.sln add {directory}\\MyProject.csproj");
            cmd.StandardInput.Flush();

            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        public static void BuildVSProject(string directory)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine($"dotnet build {directory}/MyProject.csproj --output {directory}//Builds");
            cmd.StandardInput.Flush();

            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        public static void AddEngineRef(string userlib)
        {
            
        }

        public static void BuildRunnerProject(string runnerProj, string outputDir)
        {
            AddPackageProject(runnerProj, "OpenTK");
            AddPackageProject(runnerProj, "ImGui.NET", "1.87.3");

            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine($"dotnet build {runnerProj} --output {outputDir}");
            cmd.StandardInput.Flush();

            cmd.StandardInput.Close();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        public static void AddPackageProject(string project, string package,string version = null)
        {
            // Set up the process start info
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = version == null ? $"add {project} package {package}" : $"add {project} package {package} -v {version}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Start the process
            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                // Capture the output and error messages
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                // Check for any errors
                if (process.ExitCode != 0)
                {
                    Console.WriteLine($"Error running command: {error}");
                }
                else
                {
                    Console.WriteLine($"Command output: {output}");
                }
            }
        }

        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
    }
}
