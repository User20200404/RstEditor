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
using System.Collections.ObjectModel;
// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace RstEditor
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RstFilePage : Page
    {
        private RSTFile rstFile;
        private RstFileInfo rstFileInfo;
        private RstFileModel rstFileModel;
        public static RstFileModel RstFileModel;
        /// <summary>
        /// 指示在<see cref="RstFileModel.RstFileItems"/> 集合变更时，是否立即将新RST文件byte[]数据写入DataSource。设为false能显著提升大量更改数据时的效率。
        /// </summary>
        public static bool UpdateDataSourceWhenModelChanged = true;
        /// <summary>
        /// 被缓存的页面只会初始化一次。
        /// </summary>
        public RstFilePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //从byte[]数据创建新RSTFile实例
            rstFileInfo = e.Parameter as RstFileInfo;

            //创建新的RstFileModel模型实例，并从RSTFile中构造RstFileItems。
            rstFileModel = new RstFileModel(rstFileInfo);
            RstFileModel = rstFileModel;
            rstFile = rstFileModel.RstFile;

            //设置dataGrid的DataContext为RstFileItems，传递列表数据。
            //设置dataGrid的Tag为rstFileInfo，传递筛选数据。
            dataGrid.DataContext = rstFileModel.RstFileItems;
            dataGrid.Tag = rstFileInfo;
            rstFileModel.RstFileItems.CollectionChanged += RstFileItems_CollectionChanged;
            base.OnNavigatedTo(e);
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            rstFileModel.RstFileItems.CollectionChanged -= RstFileItems_CollectionChanged;
            base.OnNavigatedFrom(e);
        }


        private void RstFileItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //将更改应用到RSTFile的实例 rstFile中去。
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                var items = sender as ObservableCollection<RstFileModel.RstFileItem>;
                for (int i = 0; i < e.NewItems.Count; i++)
                {
                    var currentOldItem = e.OldItems[i] as RstFileModel.RstFileItem;
                    var currentNewItem = e.NewItems[i] as RstFileModel.RstFileItem;
                    //int index = items.IndexOf(currentOldItem);
                    //rstFileModel.RstFileItems[index].ItemValue = currentNewItem.ItemValue;
                    //rstFileModel.RstFileItems[index].ItemKey = currentNewItem.ItemKey;

                    ulong currentNewItemKey = ulong.Parse(currentNewItem.ItemKey,System.Globalization.NumberStyles.HexNumber);
                    rstFile.Entries.Remove(ulong.Parse(currentOldItem.ItemKey,System.Globalization.NumberStyles.HexNumber));
                    rstFile.Entries.Add(currentNewItemKey, currentNewItem.ItemValue);
                }
            }
            //将更改应用到MainPage的RstFileInfo中的byte[]数组中去。
            if(UpdateDataSourceWhenModelChanged)
            rstFileInfo.DataSource = rstFile.RSTDataInBytes;
        }
    }


}
