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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace RstEditor
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class EditPage : Page
    {
        private RstFileModel.RstFileItem currentItem;
        //定义依赖属性
        static readonly DependencyProperty valueProperty = DependencyProperty.Register("Value", typeof(string), typeof(EditPage), new PropertyMetadata("", OnPropertyChanged));
        static readonly DependencyProperty keyProperty = DependencyProperty.Register("Key", typeof(string), typeof(EditPage), new PropertyMetadata("", OnPropertyChanged));
        public static DependencyProperty ValueProperty { get { return valueProperty; } }
        public static DependencyProperty KeyProperty { get { return keyProperty; } }

        public string Key 
        {
            set { SetValue(KeyProperty, value); }
            get { return GetValue(KeyProperty) as  string; }
        }
        public string Value
        {
            set { SetValue(ValueProperty, value); }
            get { return GetValue(ValueProperty) as string; }
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EditPage).OnPropertyChanged(e);
        }
        private void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
        }

        public EditPage()
        {
            this.InitializeComponent();
        }

        public EditPage(RstFileModel.RstFileItem item):this()
        {
            this.currentItem = item;
            Key = item.ItemKey;
            Value = item.ItemValue;
        }

        private void thisPage_Loaded(object sender, RoutedEventArgs e)
        {
            textBox_Value.Focus(FocusState.Programmatic);
        }
    }
}
