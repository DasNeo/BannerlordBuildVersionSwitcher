using BannerlordBuildVersionSwitcher.Classes.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace BannerlordBuildVersionSwitcher.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private string projectPath { get; set; } = Properties.Settings.Default.PathProject;
        public string ProjectPath
        {
            get => projectPath;
            set
            {
                projectPath = value;
                OnPropertyChanged();
                MainWindow.SearchAllProjects();
                Properties.Settings.Default.PathProject = value;
            }
        }

        private string msBuildPath { get; set; } = Properties.Settings.Default.MSBuildPath;
        public string MsBuildPath
        {
            get => msBuildPath;
            set
            {
                msBuildPath = value;
                OnPropertyChanged();
                Properties.Settings.Default.MSBuildPath = value;
            }
        }

        private string zipOutPath { get; set; } = Properties.Settings.Default.ZipOutPath;
        public string ZipOutPath
        {
            get => zipOutPath;
            set
            {
                zipOutPath = value;
                OnPropertyChanged();
                Properties.Settings.Default.ZipOutPath = value;
            }
        }

        private string versionPath { get; set; } = Properties.Settings.Default.PathVersion;
        public string VersionPath
        {
            get => versionPath;
            set
            {
                versionPath = value;
                OnPropertyChanged();
                MainWindow.SearchVersions();
                Properties.Settings.Default.PathVersion = value;
            }
        }

        private List<string> projectNames { get; set; } = new List<string>();
        public List<string> ProjectNames
        {
            get => projectNames;
            set
            {
                projectNames = value;
                OnPropertyChanged();
            }
        }

        private List<string> versionNames { get; set; } = new List<string>();
        public List<string> VersionNames
        {
            get => versionNames;
            set
            {
                versionNames = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, string> versions { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Versions
        {
            get => versions;
            set
            {
                versions = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, string> projects { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Projects 
        {
            get => projects;
            set
            {
                projects = value;
                OnPropertyChanged();
            }
        }

        private string selectedProject { get; set; } = "";
        public string SelectedProject 
        {
            get => selectedProject;
            set
            {
                selectedProject = value;
                OnPropertyChanged();
            }
        }
        private string selectedVersion { get; set; } = "";
        public string SelectedVersion
        {
            get => selectedVersion;
            set
            {
                selectedVersion = value;
                OnPropertyChanged();
            }
        }

        private string foundProjects { get; set; } = "";
        public string FoundProjects
        {
            get => foundProjects;
            set
            {
                foundProjects = value;
                OnPropertyChanged();
            }
        }

        private bool isBuildStatusVisible { get; set; } = false;
        public bool IsBuildStatusVisible 
        {
            get => isBuildStatusVisible; 
            set
            {
                isBuildStatusVisible = value;
                OnPropertyChanged();
            }
        }


        private string buildStatus { get; set; } = "";
        public string BuildStatus
        {
            get => buildStatus;
            set
            {
                buildStatus = value;
                OnPropertyChanged();
            }
        }

        private Brush buildStatusBackground { get; set; } = new SolidColorBrush(Colors.Red);
        public Brush BuildStatusBackground
        {
            get => buildStatusBackground;
            set
            {
                buildStatusBackground = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
