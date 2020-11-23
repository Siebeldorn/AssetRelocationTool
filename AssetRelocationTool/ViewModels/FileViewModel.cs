using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace AdvancedFileMover.ViewModels
{
    public class FileViewModel : INotifyPropertyChanged
    {
        public static FileViewModel FromPath(string filePath)
        {
            var fi = new FileInfo(filePath);
            return new FileViewModel()
            {
                Icon = CreateImage(filePath),
                Name = fi.Name,
                Extension = fi.Extension.ToLowerInvariant(),
                Path = fi.FullName,
                SizeString = BuildSizeString(fi.Length)
            };
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if ( _isSelected != value )
                {
                    _isSelected = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _isSelected;

        public ImageSource Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                if ( _icon != value )
                {
                    _icon = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ImageSource _icon;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if ( _name != value )
                {
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _name;

        public string Extension
        {
            get
            {
                return _extension;
            }
            set
            {
                if ( _extension != value )
                {
                    _extension = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _extension;

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                if ( _path != value )
                {
                    _path = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _path;

        public string SizeString
        {
            get
            {
                return _sizeString;
            }
            set
            {
                if ( _sizeString != value )
                {
                    _sizeString = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _sizeString;

        private static ImageSource CreateImage(string filePath)
        {
            ImageSource imageSource = null;
            var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
            if ( icon != null )
            {
                imageSource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                if ( imageSource?.CanFreeze == true )
                    imageSource.Freeze();
            }
            return imageSource;
        }

        private static string BuildSizeString(long bytes)
        {
            if ( bytes < 1024 )
                return string.Format("{0:0.00} KB", Math.Round(bytes / 1024.0f, 2));
            else
                return string.Format("{0:0.00} MB", Math.Round(bytes / 1024.0f / 1024.0f, 2));
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged
    }
}
