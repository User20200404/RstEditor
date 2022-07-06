using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Noisrev.League.IO.RST;
using System.ComponentModel;
// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace RstEditor
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RstFilePropertyPage : Page
    {
        private RSTFile rstFile;

        public RstFilePropertyPage(RSTFile rstFile)
        {
            this.InitializeComponent();
            this.rstFile = rstFile;
            InitializePropertyModel();
        }

        private void InitializePropertyModel()
        {
            propertyModel.Config = rstFile.Config;
            propertyModel.DataOffset = rstFile.DataOffset.ToString();
            propertyModel.Mode = rstFile.Mode.ToString();
            propertyModel.Type = rstFile.Type.ToString();
            propertyModel.Version = rstFile.Version.ToString();

            if(propertyModel.Config is null)
            {
                propertyModel.Config = "(null)";
            }
        }
    }
    public class RstFilePropertyModel : INotifyPropertyChanged
    {
        private string config, dataOffset, mode, type, version;
        public event PropertyChangedEventHandler PropertyChanged;
        public string Config
        {
            get { return config; }
            set
            {
                if (config != value)
                {
                    config = value;
                    OnPropertyChanged("Config");
                }
            }
        }
        public string DataOffset
        {
            get { return dataOffset; }
            set
            {
                if (dataOffset != value)
                {
                    dataOffset = value;
                    OnPropertyChanged("DataOffset");
                }
            }
        }
        public string Mode
        {
            get { return mode; }
            set
            {
                if (mode != value)
                {
                    mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }
        public string Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public string Version
        {
            get { return version; }
            set
            {
                if (version != value)
                {
                    version = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
