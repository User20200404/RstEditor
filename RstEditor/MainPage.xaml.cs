using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Noisrev.League.IO.RST;
using Windows.UI;
using Windows.UI.Xaml.Media.Animation;
using System.Collections;
using Windows.UI.Popups;
using System.Text.Json;
using System.Text.Encodings.Web;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace RstEditor
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static Frame ContentFrame;
        public static NavigationView TopNavigationView;
        private event TappedEventHandler openButtonTapped;
        private NavigationViewItem nav_LastSelectedItem;
        public MainPage()
        {
            this.InitializeComponent();
            //通过静态字段暴露contentFrame和TopNavigationView，以便在子页面中Navigate
            ContentFrame = this.contentFrame;
            TopNavigationView = this.topNavigationView;

            //
            openButtonTapped += barButton_Open_Tapped;

            //设置CommandBar的背景为半透明
            Color originalBrushColor =  (Resources["AppBarBackgroundThemeBrush"] as SolidColorBrush).Color;
            SolidColorBrush newBrush = new SolidColorBrush(Color.FromArgb(200, originalBrushColor.R, originalBrushColor.G, originalBrushColor.B));
            bottomCommandBar.Background = newBrush;
            //导航至起始页
            contentFrame.Navigate(typeof(StartPage),openButtonTapped);
            topNavigationView.SelectedItem = nav_StartPage;
        }

        /// <summary>
        /// 弹出文件选择器以打开RST文件。
        /// </summary>
        /// <returns>返回对该文件操作的类的实例。</returns>
        private async Task<StorageFile> OpenRstFile()
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".txt");
            return await picker.PickSingleFileAsync();
        }

        /// <summary>
        /// 以byte[]的形式读取RST文件的内容。
        /// </summary>
        /// <param name="storageFile">对该文件操作类的实例。</param>
        /// <returns>RST文件的byte数组。</returns>
        private async Task<byte[]> ReadRstFile(StorageFile storageFile)
        {
            if (storageFile != null)
            {
                using (IRandomAccessStream stream = await storageFile.OpenReadAsync())
                {
                    using (DataReader dataReader = new DataReader(stream))
                    {
                        uint length = (uint)stream.Size;
                        await dataReader.LoadAsync(length);
                        byte[] bytes = new byte[stream.Size];
                        dataReader.ReadBytes(bytes);
                        return bytes;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 为已读取的RST文件数据分配RstFilePage页面。
        /// </summary>
        private void AllocRstPageForRstData(StorageFile file, byte[] dataSource)
        {
            NavigationViewItem navigationViewItem = new NavigationViewItem()
            {
                Content = file.Name,
                Tag = new RstFileInfo() { DataSource = dataSource, StorageFile = file, FilterComboSelectedIndex = 2, FilterText = "" }
            };

            topNavigationView.MenuItems.Add(navigationViewItem);
            contentFrame.Navigate(typeof(RstFilePage), navigationViewItem.Tag, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight});
            topNavigationView.SelectedItem = navigationViewItem;
        }

        /// <summary>
        /// 通过<see cref="StorageFile"/>实例寻找已打开的RST文件。
        /// </summary>
        /// <param name="storageFile">已打开的RST文件的<see cref="StorageFile"/>实例。</param>
        /// <returns>找到返回该已打开文件在<see cref="topNavigationView"/>的<see cref="NavigationView.MenuItems"/>中的索引。未找到返回-1。</returns>
        private int FindOpenedFile(StorageFile storageFile)
        {
            var items = topNavigationView.MenuItems;
            for(int i = 0;i<items.Count;i++)
            {
                RstFileInfo info = (items[i] as NavigationViewItem)?.Tag as RstFileInfo;
                if (info!= null && storageFile!= null)
                {
                    if (storageFile.Path == info.StorageFile.Path)
                        return i;
               }
            }
            return -1;
        }

        public async void barButton_Open_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile storageFile = await OpenRstFile();
            int index = FindOpenedFile(storageFile);
            if (index == -1)
            {
                byte[] rstData = await ReadRstFile(storageFile);
                if (rstData != null)
                {
                    AllocRstPageForRstData(storageFile, rstData);
                }
            }
            else
            {
                NavigationViewItem item = topNavigationView.MenuItems[index] as NavigationViewItem;
                contentFrame.Navigate(typeof(RstFilePage), item.Tag, new SlideNavigationTransitionInfo() { Effect = (uint)topNavigationView.MenuItems.IndexOf(item) > (uint)topNavigationView.MenuItems.IndexOf(nav_LastSelectedItem) ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft });
                topNavigationView.SelectedItem = item;
            }
        }

        private void topNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var navItem = topNavigationView.SelectedItem as NavigationViewItem;
            var navItemTag = navItem.Tag;

            if (navItem == nav_LastSelectedItem)
                return;


            if (args.IsSettingsInvoked)
                contentFrame.Navigate(typeof(SettingsPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight});
            else if (navItemTag.ToString() == "StartPage")
                contentFrame.Navigate(typeof(StartPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft});
            else contentFrame.Navigate(typeof(RstFilePage), navItemTag, new SlideNavigationTransitionInfo() { Effect = (uint)topNavigationView.MenuItems.IndexOf(navItem) > (uint)topNavigationView.MenuItems.IndexOf(nav_LastSelectedItem)?SlideNavigationTransitionEffect.FromRight:SlideNavigationTransitionEffect.FromLeft });
        }

        private void barButton_Close_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int index = topNavigationView.MenuItems.IndexOf(topNavigationView.SelectedItem);
            if (index > 1)
            {
                topNavigationView.MenuItems.RemoveAt(index);
                if (topNavigationView.MenuItems.Count > 2)
                {
                    if (topNavigationView.MenuItems.Count > index)
                        topNavigationView.SelectedItem = topNavigationView.MenuItems[index];
                    else topNavigationView.SelectedItem = topNavigationView.MenuItems[index - 1];
                }
                else
                {
                    var item_temp = topNavigationView.SelectedItem;
                    topNavigationView.SelectedItem = topNavigationView.MenuItems[0];
                    nav_LastSelectedItem = item_temp as NavigationViewItem;
                }
                topNavigationView_ItemInvoked(topNavigationView, new NavigationViewItemInvokedEventArgs());
            }
        }

        /// <summary>
        /// 判断当前选中的导航页是否为关联到RSTFile的页面。
        /// </summary>
        /// <returns>是则返回true，否则返回false。</returns>
        private bool IsSelectedItemARstFilePage()
        {
            return topNavigationView.MenuItems.IndexOf(topNavigationView.SelectedItem) > 1;
        }
        private async void barButton_Save_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsSelectedItemARstFilePage())
            {
                RstFileInfo info = (topNavigationView.SelectedItem as NavigationViewItem).Tag as RstFileInfo;
                Stream stream = await info.StorageFile.OpenStreamForWriteAsync();
                new RSTFile(info.DataSource).Write(stream, false);
            }
        }

        private void topNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            nav_LastSelectedItem = args.SelectedItem as NavigationViewItem;
        }

        private async void barButton_SaveAs_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsSelectedItemARstFilePage())
            {
                MenuFlyout flyout = new MenuFlyout();
                MenuFlyoutItem item_rst = new MenuFlyoutItem() { Text = "另存为RST文件" };
                MenuFlyoutItem item_json = new MenuFlyoutItem() { Text = "格式化为JSON文件" };
                item_rst.Click += Item_rst_Click;
                item_json.Click += Item_json_Click;
                flyout.Items.Add(item_rst);
                flyout.Items.Add(item_json);
                flyout.ShowAt(barButton_SaveAs);
            }
        }

        private async Task<StorageFile> ShowSaveAsDialogAndPick(SupportedFileType type)
        {
            FileSavePicker picker = new FileSavePicker();
            switch(type)
            {
                case SupportedFileType.JSON:
                    picker.DefaultFileExtension =".json" ;
                    picker.FileTypeChoices.Add( "JSON文件", new List<string> {".json"});
                    break;
                case SupportedFileType.RST:
                    picker.DefaultFileExtension = ".txt";
                    picker.FileTypeChoices.Add("英雄联盟RST文件", new List<string> { ".txt" });
                    break;
                default: break;
            }
            return await picker.PickSaveFileAsync();
        }

        private async void Item_json_Click(object sender, RoutedEventArgs e)
        {
            StorageFile storageFile = await ShowSaveAsDialogAndPick(SupportedFileType.JSON);
            if (storageFile != null)
            {
                Stream stream = await storageFile.OpenStreamForWriteAsync();
                RstFileInfo info = (topNavigationView.SelectedItem as NavigationViewItem).Tag as RstFileInfo;
                var json = Noisrev.Rion.RSTConvert.RstToJson(new RSTFile(info.DataSource));
                Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true , Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                json.WriteTo(writer);
                writer.Flush();
            }
        }

        private async void Item_rst_Click(object sender, RoutedEventArgs e)
        {
            StorageFile storageFile = await ShowSaveAsDialogAndPick(SupportedFileType.RST);
            if (storageFile != null)
            {
                Stream stream = await storageFile.OpenStreamForWriteAsync();
                RstFileInfo info = (topNavigationView.SelectedItem as NavigationViewItem).Tag as RstFileInfo;
                new RSTFile(info.DataSource).Write(stream, false);
            }
        }

        private async void barButton_Replace_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsSelectedItemARstFilePage())
            {
                RstFileInfo info = (topNavigationView.SelectedItem as NavigationViewItem).Tag as RstFileInfo;
                ContentDialog contentDialog = new ContentDialog()
                {
                    Title = "值(Value)替换",
                    PrimaryButtonText = "替换",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                };
                contentDialog.Content = new ReplacePage(RstFilePage.RstFileModel, contentDialog);

                await contentDialog.ShowAsync();
            }
        }

        private async void barButton_ShowProperty_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsSelectedItemARstFilePage())
            {
                ContentDialog contentDialog = new ContentDialog()
                {
                    Content = new RstFilePropertyPage(RstFilePage.RstFileModel.RstFile),
                    Title = "RST文件信息",
                    PrimaryButtonText = "完成",
                    DefaultButton = ContentDialogButton.Primary
                };
                await contentDialog.ShowAsync();
            }
        }

        private enum SupportedFileType
        {
            JSON = 0,
            RST = 1
        }
    }
}
