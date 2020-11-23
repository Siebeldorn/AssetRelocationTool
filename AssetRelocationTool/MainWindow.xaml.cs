using AdvancedFileMover.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace AssetRelocationTool
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Init

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            PropertyChanged += MainWindow_PropertyChanged;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            Title = $"{fvi.FileDescription} v{fvi.ProductVersion}";

            FilesView = CollectionViewSource.GetDefaultView(Files);
            FilesView.Filter = FilesFilter;

            ExtensionsFilters.CollectionChanged += ExtensionFilters_CollectionChanged;
        }

        #endregion Init
        #region Properties

        #region FilesView

        public ICollectionView FilesView
        {
            get
            {
                return _filesView;
            }
            set
            {
                if ( _filesView != value )
                {
                    _filesView = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ICollectionView _filesView;

        #endregion FilesView
        #region FilenameFilter

        public string FilenameFilter
        {
            get
            {
                return _filenameFilter;
            }
            set
            {
                if ( _filenameFilter != value )
                {
                    _filenameFilter = value.ToLowerInvariant();
                    NotifyPropertyChanged();
                    FilesView.Refresh();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _filenameFilter = "";

        #endregion FilenameFilter
        #region SourcePath

        public string SourcePath
        {
            get
            {
                return _sourcePath;
            }
            set
            {
                if ( _sourcePath != value )
                {
                    _sourcePath = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _sourcePath;

        #endregion SourcePath
        #region TargetPath

        public string TargetPath
        {
            get
            {
                return _targetPath;
            }
            set
            {
                if ( _targetPath != value )
                {
                    _targetPath = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _targetPath;

        #endregion SourcePath
        #region FlattenFolders

        public bool FlattenFolders
        {
            get
            {
                return _flattenFolders;
            }
            set
            {
                if ( _flattenFolders != value )
                {
                    _flattenFolders = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _flattenFolders;

        #endregion SourcePath

        #endregion Properties
        #region Filter

        public bool FilesFilter(object item)
        {
            if ( item is FileViewModel fi )
            {
                var visible = ExtensionsFilters.Count == 0 || ExtensionsFilters.Contains(fi.Extension);
                visible &= string.IsNullOrEmpty(FilenameFilter) || fi.Name.ToLowerInvariant().Contains(FilenameFilter);
                return visible;
            }

            return false;
        }

        #endregion
        #region Events

        private void MainWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch ( e.PropertyName )
            {
                case "SourcePath":
                case "FlattenFolders":
                    UpdateFiles();
                    break;
            }
        }

        private void SourceFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog()
            {
                ShowNewFolderButton = false,
                SelectedPath = string.IsNullOrWhiteSpace(SourcePath) ? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) : SourcePath,
                Description = "Select the source folder",
            };

            if ( dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                SourcePath = dlg.SelectedPath;
            }
        }

        private void TargetFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog()
            {
                ShowNewFolderButton = false,
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                Description = "Select the target folder for the selected files."
            };

            if ( dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK )
            {
                TargetPath = dlg.SelectedPath;
            }

        }

        private void ExtensionFilters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FilesView.Refresh();
        }

        private void FilesListView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ( sender is System.Windows.Controls.ListViewItem lvi && lvi.Content is FileViewModel fvm )
            {
                fvm.IsSelected = !fvm.IsSelected;
            }
        }

        private void FileExtensionButton_Click(object sender, RoutedEventArgs e)
        {
            // hide others with unselected extension
            if ( sender is ToggleButton tb && tb.Content is string ext )
            {
                if ( tb.IsChecked == true )
                {
                    if ( !ExtensionsFilters.Contains(ext) )
                        ExtensionsFilters.Add(ext);
                }
                else
                {
                    ExtensionsFilters.Remove(ext);
                }

                FilesView.Refresh();
            }
        }

        private void FilenameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            FilesView.Refresh();
        }

        private void MoveFiles_Click(object sender, RoutedEventArgs e)
        {
            if ( !Directory.Exists(TargetPath) )
            {
                MessageBox.Show("The target directory does not exist.", "Move Files", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var total = 0;
            List<string> errors = new List<string>();
            foreach ( FileViewModel vfm in FilesView )
            {
                try
                {
                    if ( vfm.IsSelected )
                    {
                        total++;
                        var newPath = Path.Combine(TargetPath, vfm.Name);
                        File.Move(vfm.Path, newPath);
                    }
                }
                catch ( Exception ex )
                {
                    errors.Add($"{vfm.Name}: {ex.Message}");
                }
            }

            if ( errors.Count > 0 )
            {
                var messages = string.Join(Environment.NewLine, errors.Take(10));
                MessageBox.Show($"{errors.Count} of {total} files could not be moved:\n\r\n\r{messages}", "Move Files", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show($"{total} files have been successfully moved.", "Move Files", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            UpdateFiles();
        }

        #endregion Events
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
        #region Misc

        private void UpdateFiles()
        {
            if ( Directory.Exists(SourcePath) )
            {
                Files.Clear();
                var filePathes = Directory.GetFiles(SourcePath, "*", FlattenFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
                foreach ( var item in filePathes.Select(f => FileViewModel.FromPath(f)) )
                {
                    Files.Add(item);
                    if ( !Extensions.Contains(item.Extension) )
                        Extensions.Add(item.Extension);
                }

                for ( int i = Extensions.Count - 1; i >= 0; --i )
                {
                    if ( !Files.Any(f => f.Extension == Extensions[i]) )
                        Extensions.Remove(Extensions[i]);
                }
            }
        }

        #endregion Misc

        public ObservableCollection<FileViewModel> Files { get; set; } = new ObservableCollection<FileViewModel>();

        public ObservableCollection<string> Extensions { get; set; } = new ObservableCollection<string>();

        private ObservableCollection<string> ExtensionsFilters = new ObservableCollection<string>();
    }
}
