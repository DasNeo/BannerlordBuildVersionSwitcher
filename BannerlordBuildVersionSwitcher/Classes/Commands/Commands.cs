using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;

namespace BannerlordBuildVersionSwitcher.Classes.Commands
{
    public class OpenFolderBrowserCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }


        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is null) return;

            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog diag = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if(diag.ShowDialog() ?? false)
            {
                foreach (var prop in MainWindow.vm.GetType().GetProperties())
                {
                    if (prop.Name.ToString() == parameter.ToString())
                    {
                        prop.SetValue(MainWindow.vm, diag.SelectedPath);
                        break;
                    }
                }
            }
        }
    }

    public class BuildCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute(object? parameter) => (!String.IsNullOrEmpty(MainWindow.vm.SelectedProject)
            && !String.IsNullOrEmpty(MainWindow.vm.SelectedVersion));

        public void Execute(object? parameter)
        {
            MainWindow.Build();
        }
    }

    public class ZipCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool CanExecute(object? parameter) => (!String.IsNullOrEmpty(MainWindow.vm.SelectedProject)
            && !String.IsNullOrEmpty(MainWindow.vm.SelectedVersion)
            && !String.IsNullOrEmpty(MainWindow.ZipInPath));

        public void Execute(object? parameter)
        {
            MainWindow.Zip();
        }
    }
}
