using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
//using HarmonyLib;

namespace GalacticScale
{
    [BepInPlugin("dsp.galactic-scale.loader", "Galactic Scale 2 Loader", "0.0.0.1")]
    [BepInDependency("dsp.galactic-scale.2")]
    public class Bootstrap : BaseUnityPlugin
    {
        public new static ManualLogSource Logger;

        // Internal Variables
        public static Queue buffer = new Queue();

        internal void Awake()
        {
            //var v = Assembly.GetExecutingAssembly().GetName().Version;
            //var _ = new Harmony("dsp.galactic-scale.loader");
            Logger = new ManualLogSource("GS2Loader");
            BepInEx.Logging.Logger.Sources.Add(Logger);
            var workingDir = Assembly.GetExecutingAssembly().Location;
            Debug(workingDir);
            var configDir = Path.Combine(BepInEx.Paths.ConfigPath, "GalacticScale2");
            var genDir = Path.Combine(configDir, "Generators");
            var themeDir = Path.Combine(configDir, "CustomThemes");

            foreach (var file in Directory.GetFiles(workingDir, "*.dll")) {
                Debug($"Found {file}");
                if (new FileInfo(file).Name != "GS2Loader.dll")
                {
                    var destination = Path.Combine(genDir, new FileInfo(file).Name);
                    if (File.Exists(destination)) File.Delete(destination);
                    File.Move(file, destination);
                    Debug($"{destination} relocated");
                }
            }
            foreach (var file in Directory.GetFiles(workingDir, "*.json"))
            {
                Debug($"Found {file}");
                if (new FileInfo(file).Name != "mainfest.json")
                {
                    Debug($"{file} processing");
                    var destination = Path.Combine(themeDir, new FileInfo(file).Name);
                    if (File.Exists(destination)) File.Delete(destination);
                    File.Move(file, destination);
                    Debug($"{destination} relocated");
                }
            }
            foreach (var dir in Directory.GetDirectories(workingDir))
            {
                Debug($"Found {dir}");
                var destination = Path.Combine(themeDir, new DirectoryInfo(dir).Name);
                MoveDirectory(dir, destination);
                Debug($"{destination} relocated");
                
            }
            
        }
        public static void MoveDirectory(string source, string target)
        {
            var sourcePath = source.TrimEnd('\\', ' ');
            var targetPath = target.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }
            Directory.Delete(source, true);
        }


        public static void Debug(object data, LogLevel logLevel, bool isActive)
        {
            if (isActive)
            {
                if (Logger != null)
                {
                    while (buffer.Count > 0)
                    {
                        var o = buffer.Dequeue();
                        var l = ((object data, LogLevel loglevel, bool isActive)) o;
                        if (l.isActive) Logger.Log(l.loglevel, "Q:" + l.data);
                    }

                    Logger.Log(logLevel, data);
                }
                else
                {
                    buffer.Enqueue((data, logLevel, true));
                }
            }
        }

        public static void Debug(object data)
        {
            Debug(data, LogLevel.Message, true);
        }

  
    }
}