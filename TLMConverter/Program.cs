using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TLMConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) {
                List<string> inputFiles = GetFilesInDirectory("./InputFiles/");
                List<string> fbxFiles = UpdateFiles(inputFiles);
                bool done = ConvertFiles(fbxFiles);
            }

        }

        private static List<string> UpdateFiles(List<string> inputFiles, string inputFolder = "./InputFiles/", string outputFolder = "./OutputFiles/")
        {
            ConcurrentBag<string> convertedFiles = new ConcurrentBag<string>();
            
            string fbxConverter = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Utils/FbxConverter.exe";

            if (!File.Exists(fbxConverter))
                return null;

            Parallel.ForEach(inputFiles,
                new Action<string>(
                    (x) =>
                    {
                        string fbxFile = x.Split('/').Last();
                        string outputFile = outputFolder + fbxFile;
                        string conversionArgs = inputFolder + fbxFile + " " + outputFile + " /tex /mat /anim /mesh /light /cam /filter /ambientlight";

                        ProcessStartInfo processInfo = new ProcessStartInfo()
                        {
                            FileName = fbxConverter,
                            Arguments = conversionArgs,
                            CreateNoWindow = true
                        };

                        var result = Process.Start(processInfo);
                        result.WaitForExit();
                        if (result.ExitCode == 0)
                        {
                            convertedFiles.Add(outputFile);
                        }
                        else
                        {
                            Console.WriteLine("error updating: " + fbxFile);
                        }
                    }
            ));

            return convertedFiles.ToList();
        }

        private static bool ConvertFiles(List<string> fbxFiles, string outputFolder = "./OgreFiles/")
        {
            string ogreConverter = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/Utils/OgreAssimpConverter.exe";

            if (!File.Exists(ogreConverter))
                return false;
            
            Parallel.ForEach(fbxFiles,
                new Action<string>(
                    (x) =>
                    {
                        string fileFolderName = x.Split('/').Last().Split('.')[0];

                        string fileFolder = outputFolder + "/" + fileFolderName;

                        Directory.CreateDirectory(fileFolder);

                        string conversionArgs = x + " " + fileFolder;

                        ProcessStartInfo processInfo = new ProcessStartInfo()
                        {
                            FileName = ogreConverter,
                            Arguments = conversionArgs,
                            CreateNoWindow = true
                        };

                        var result = Process.Start(processInfo);
                        result.WaitForExit();
                        if (result.ExitCode != 0)
                        { 
                            Console.WriteLine("error unpacking: " + x);
                        }
                    }
            ));
            return true;
        }


        private static List<string> GetFilesInDirectory(string directory)
        {
            List<string> files = Directory.GetFiles(directory).Where(x => x.EndsWith(".fbx")).ToList();
            foreach (var subdir in Directory.GetDirectories(directory))
            {
                files.AddRange(GetFilesInDirectory(subdir));
            }
            return files;
        }
    }
}
