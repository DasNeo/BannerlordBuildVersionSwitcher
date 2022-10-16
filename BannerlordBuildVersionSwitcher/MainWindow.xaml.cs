using BannerlordBuildVersionSwitcher.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace BannerlordBuildVersionSwitcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ViewModelBase vm = new ViewModelBase();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = vm;


            vm.ProjectPath = Properties.Settings.Default.PathProject;
            vm.VersionPath = Properties.Settings.Default.PathVersion;
            vm.MsBuildPath = Properties.Settings.Default.MSBuildPath;
            vm.ZipOutPath = Properties.Settings.Default.ZipOutPath;
        }

        public static void SearchAllProjects()
        {
            vm.Projects.Clear();
            vm.ProjectNames.Clear();
            vm.FoundProjects = "";
            if (string.IsNullOrEmpty(vm.ProjectPath))
                return;
            if (!Directory.Exists(vm.ProjectPath))
            {
                vm.FoundProjects = "ProjectPath does not Exist!";
                return;
            }
            vm.FoundProjects += $"Searching in {vm.ProjectPath}\r";
            string[] projectFiles = Directory.GetFiles(vm.ProjectPath, "*.csproj", SearchOption.AllDirectories);
            foreach(string project in projectFiles)
            {
                
                var split = string.Join("\\", project.Split('\\').Reverse().Take(2).Reverse());
                vm.Projects.Add(project, split);
                vm.ProjectNames.Add(split);
                vm.FoundProjects += $"Found {project}\r";
            }
        }

        public static void SearchVersions()
        {
            vm.VersionNames.Clear();
            vm.Versions.Clear();
            if (string.IsNullOrEmpty(vm.VersionPath))
                return;
            if (!Directory.Exists(vm.VersionPath))
            {
                vm.FoundProjects = "VersionPath does not Exist!";
                return;
            }

            string[] dirs = Directory.GetDirectories(vm.VersionPath);
            foreach(var dir in dirs)
            {
                var split = dir.Split('\\').Last();
                vm.VersionNames.Add(split);
                vm.Versions.Add(split, dir);
            }
            
        }

        public static string ZipInPath { get; set; } = "";

        public static void Build()
        {
            Dictionary<string, string> dlls = new Dictionary<string, string>();
            var versionPath = vm.Versions[vm.SelectedVersion];

            foreach(var file in Directory.GetFiles(versionPath))
            {
                dlls.Add(file.Split('\\').Last(), file);
            }

            var backupDir = AppDomain.CurrentDomain.BaseDirectory + "\\Backup";
            var csproj = vm.Projects.FirstOrDefault(r => r.Value == vm.SelectedProject).Key;
            if(!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);

            var newFileName = vm.SelectedProject.Replace("\\", "_");
            int counter = 0;
            while(File.Exists(System.IO.Path.Combine(backupDir, newFileName)))
            {
                newFileName = vm.SelectedProject.Replace("\\", "_").Replace(".csproj", "") + "_" + counter + ".csproj";
                counter++;
            }
            string newCsProj = System.IO.Path.Combine(backupDir, newFileName);
            File.Copy(csproj, newCsProj);

            XmlDocument doc = new XmlDocument();
            doc.Load(csproj);

            XmlNodeList outPath = doc.GetElementsByTagName("OutputPath");
            if (outPath.Count == 0)
                outPath = doc.GetElementsByTagName("BaseOutPath");

            if (outPath.Count == 0)
                outPath = doc.GetElementsByTagName("BaseOutputPath");

            if (outPath.Count != 0)
            {
                foreach(XmlNode node in outPath)
                {
                    if (node.InnerText.Contains("Bannerlord"))
                        ZipInPath = node.InnerText;
                }
            }

            XmlNodeList references = doc.GetElementsByTagName("Reference");
            foreach(XmlNode reference in references)
            {
                bool skipNode = false;
                bool hasPrivateNode = false;
                foreach(XmlNode node in reference.ChildNodes)
                {
                    if (skipNode)
                        break;

                    if (node.Name == "HintPath")
                    {
                        var dllName = node.InnerText.Split('\\').Last();
                        if (dlls.ContainsKey(dllName))
                        {
                            node.InnerText = dlls[dllName];
                        }
                        else
                            skipNode = true; // Break out if the dll is not in our dlls list
                    } else if(node.Name == "Private")
                    {
                        node.InnerText = "False";
                        hasPrivateNode = true;
                    }
                }
                if(!hasPrivateNode && !skipNode)
                {
                    var node = reference.AppendChild(doc.CreateElement("Private"));
                    node.InnerText = "False";
                }
            }
            
            doc.Save(csproj);
            Task.Factory.StartNew(() =>
            {
                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = System.IO.Path.Combine(vm.MsBuildPath, "msbuild.exe"),
                        WorkingDirectory = System.IO.Path.GetDirectoryName(csproj),
                        Arguments = $"\"{System.IO.Path.GetFileName(csproj)}\" /t:Rebuild /property:Configuration=Debug /property:Platform=x64",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                vm.FoundProjects = "";
                proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine() ?? "";
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        vm.FoundProjects += line + "\r";
                    });
                }
                App.Current.Dispatcher.Invoke(() =>
                {
                    vm.IsBuildStatusVisible = true;
                    if (proc.ExitCode == 1)
                    {
                        vm.BuildStatus = "Build failed";
                        vm.BuildStatusBackground = new SolidColorBrush(Color.FromRgb(201, 58, 62));
                        ZipInPath = "";
                    } else
                    {
                        vm.BuildStatus = "Build successful";
                        vm.BuildStatusBackground = new SolidColorBrush(Color.FromRgb(124, 209, 111));
                    }
                });
            }).ContinueWith((a) =>
            {
                File.Move(newCsProj, csproj, true);
            });
        }

        public static void Zip()
        {
            var csproj = vm.Projects.FirstOrDefault(r => r.Value == vm.SelectedProject).Key;
            var csprojFolder = System.IO.Path.GetDirectoryName(csproj);
            var file = System.IO.Path.GetFullPath(MainWindow.ZipInPath, csprojFolder);
            var split = file.Split('\\');

            bool breakAfterNext = false;
            string newPath = "";
            foreach(var s in split)
            {
                if(breakAfterNext)
                {
                    newPath += s;
                    break;
                }
                if(s == "Modules")
                {
                    breakAfterNext = true;
                }
                newPath += $"{s}\\";
            }
            newPath = HttpUtility.UrlDecode(newPath);
            var name = $"{vm.SelectedProject.Split('\\').Last().Replace(".csproj", "")}_{vm.SelectedVersion}.zip";
            var zipOut = System.IO.Path.Combine(vm.ZipOutPath, name);
            int counter = 0;
            string zipOutTemp = zipOut;
            while(File.Exists(zipOutTemp))
            {
                zipOutTemp = zipOut.Replace(".zip", "") + counter + ".zip";
                counter++;
            }
            ZipFile.CreateFromDirectory(newPath, zipOutTemp);

            var zipPath = System.IO.Path.GetDirectoryName(zipOut);
            if(zipPath != null)
                Process.Start("explorer.exe", zipPath);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
