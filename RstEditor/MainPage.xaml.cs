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
        /// 格式化RST文件数据。
        /// </summary>
        /// <param name="dataSource">byte[]形式的RST文件数据。</param>
        /// <returns>对RST数据操作类的实例。</returns>
        private Task<RSTFile> FormatRstData(byte[] dataSource)
        {
            return Task.Run(() => { return new RSTFile(dataSource); });
        }

        /// <summary>
        /// 为已读取的RST文件数据分配RstFilePage页面。
        /// </summary>
        private void AllocRstPageForRstData(StorageFile file, byte[] dataSource)
        {
            NavigationViewItem navigationViewItem = new NavigationViewItem()
            {
                Content = file.Name,
                Tag = new RstFileInfo() { DataSource = dataSource, StorageFile = file, FilterComboSelectedIndex = 1, FilterText = "" }
            };

            topNavigationView.MenuItems.Add(navigationViewItem);
            contentFrame.Navigate(typeof(RstFilePage), navigationViewItem.Tag, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight});
            topNavigationView.SelectedItem = navigationViewItem;
        }

        public async void barButton_Open_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StorageFile storageFile = await OpenRstFile();
            byte[] rstData = await ReadRstFile(storageFile);
            if (rstData != null)
            {
                AllocRstPageForRstData(storageFile, rstData);
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
                RstFileInfo info = (topNavigationView.SelectedItem as NavigationViewItem).Tag as RstFileInfo;
                FileSavePicker picker = new FileSavePicker();
                picker.DefaultFileExtension = ".txt";
                picker.FileTypeChoices.Add("RSTFile", new List<string> { ".txt" });
                StorageFile storageFile = await picker.PickSaveFileAsync();

                if (storageFile != null)
                {
                    Stream stream = await storageFile.OpenStreamForWriteAsync();
                    new RSTFile(info.DataSource).Write(stream, false);
                }
            }
        }

        private async void barButton_Replace_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsSelectedItemARstFilePage())
            {
                RstFileInfo info = (topNavigationView.SelectedItem as NavigationViewItem).Tag as RstFileInfo;
                ContentDialog contentDialog = new ContentDialog()
                {
                    Content = new ReplacePage(RstFilePage.RstFileModel),
                    Title = "值(Value)替换",
                    PrimaryButtonText = "替换",
                    CloseButtonText = "取消",
                    DefaultButton = ContentDialogButton.Primary
                };
                ContentDialogResult result = await contentDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    int count = 0;
                    ReplacePage page = contentDialog.Content as ReplacePage;
                    string originalText = page.OriginalText;
                    if (!string.IsNullOrEmpty(originalText))
                    {
                        string replaceText = page.ReplaceWith;
                        RstFileModel targetModel = page.RstFileModel;
                        var items = targetModel.RstFileItems;
                        //禁用数据更新，提高替换效率。
                        RstFilePage.UpdateDataSourceWhenModelChanged = false;
                        for (int i = 0; i < items.Count; i++)
                        {
                            string itemValue = items[i].ItemValue;
                            string itemKey = items[i].ItemKey;
                            string newItemValue = itemValue.Replace(originalText, replaceText);
                            if (newItemValue != itemValue)
                            {
                                items[i] = new RstFileModel.RstFileItem(itemKey, newItemValue);
                                count++;
                            }
                        }
                        //启用数据更新，并通过替换最后一项来触发数据更新事件。
                        RstFilePage.UpdateDataSourceWhenModelChanged = true;
                        RstFileModel.RstFileItem temp_item = items[items.Count - 1];
                        items[items.Count - 1] = new RstFileModel.RstFileItem(temp_item.ItemKey, temp_item.ItemValue);
                    }
                        contentFrame.Navigate(typeof(RstFilePage), info, new SuppressNavigationTransitionInfo());
                    ContentDialog completedDialog = new ContentDialog()
                    {
                        Content = "共替换了 " + count.ToString() + " 个数据。",
                        Title = "操作成功完成",
                        PrimaryButtonText = "返回",
                        DefaultButton = ContentDialogButton.Primary
                    };
                    await completedDialog.ShowAsync();
                }
            }
        }
    }
}
