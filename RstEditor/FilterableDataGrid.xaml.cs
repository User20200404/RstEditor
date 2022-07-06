using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Telerik.Data.Core;
using Telerik.UI.Xaml.Controls.Grid;
using Telerik.UI.Xaml.Controls.Grid.Commands;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace RstEditor
{
    public sealed partial class FilterableDataGrid : UserControl
    {
        private bool initializing;

        public FilterableDataGrid()
        {

            this.InitializeComponent();
            var warningSuppression = this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => this.InitializecolumnListForSelectedItemSelection());
        }

        private void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var warningSuppression = this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => this.InitializecolumnListForSelectedItemSelection());
        }


        /// <summary>
        /// 此事件在每次切换RstFilePage页面时会被触发。在这里更新筛选列表。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitializecolumnListForSelectedItemSelection()
        {
            initializing = true;
            if (columnListForSelectedItem.Items.Count > 0)
            {
                RstFileInfo info = Tag as RstFileInfo;
                columnListForSelectedItem.SelectedIndex = info.FilterComboSelectedIndex;
                FilterText.Text = info.FilterText;
            }
            initializing = false;
        }

        /// <summary>
        /// 此事件应在更改筛选信息时触发，用于将筛选信息记录至RstInfo。
        /// </summary>
        private void UpdateRstInfo()
        {
            if (!initializing)
            {
                RstFileInfo info = Tag as RstFileInfo;
                info.FilterComboSelectedIndex = columnListForSelectedItem.SelectedIndex;
                info.FilterText = FilterText.Text;
            }
        }

        private void columnListForSelectedItem_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.dataGrid.FilterDescriptors.Clear();
            string text = FilterText.Text;

            if (!string.IsNullOrEmpty(text))
            {
                this.dataGrid.FilterDescriptors.Add(new DelegateFilterDescriptor() { Filter = new EmployeeSearchFilter(text, columnListForSelectedItem.SelectedItem as DataGridTypedColumn) });
            }
            UpdateRstInfo();

        }

        private void FilterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.dataGrid.FilterDescriptors.Clear();
            string text = ((TextBox)sender).Text;

            if (!string.IsNullOrEmpty(text))
            {
                this.dataGrid.FilterDescriptors.Add(new DelegateFilterDescriptor() { Filter = new EmployeeSearchFilter(text, columnListForSelectedItem.SelectedItem as DataGridTypedColumn) });
            }
            UpdateRstInfo();
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            FilterText.Text = string.Empty;
        }
        private void thisControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.dataGrid.Columns.CollectionChanged -= this.Columns_CollectionChanged;
        }

        private void thisControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.dataGrid.Columns.CollectionChanged += this.Columns_CollectionChanged;
        }

        private class EmployeeSearchFilter : IFilter
        {
            private string matchString;

            private DataGridTypedColumn column;

            public EmployeeSearchFilter(string match, DataGridTypedColumn column)
            {
                this.matchString = match;
                this.column = column;
            }

            public bool PassesFilter(object item)
            {
                var model = item as RstFileModel.RstFileItem;

                if (column == null)
                {
                    return false;
                }

                switch (column.PropertyName)
                {
                    case "ItemKey":
                        return model.ItemKey.Contains(this.matchString, StringComparison.OrdinalIgnoreCase);
                    case "ItemValue":
                        return model.ItemValue.Contains(this.matchString, StringComparison.OrdinalIgnoreCase);
                    case "ItemName":
                        return model.ItemName.Contains(this.matchString, StringComparison.OrdinalIgnoreCase);
                    default:
                        break;
                }
                return false;
            }
        }

        private void dataGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            //由点定位项
            Point physicalPoint = e.GetPosition(dataGrid); //dataGrid即sender as RadDataGrid
            Point point = new Point() { X = physicalPoint.X, Y = physicalPoint.Y };
            object rowItem = dataGrid.HitTestService.RowItemFromPoint(point);
            DataGridCellInfo cellInfo = dataGrid.HitTestService.CellInfoFromPoint(point);
            if (cellInfo != null)
            {
                dataGrid.SelectedItem = rowItem;

                //构造菜单项
                MenuFlyout flyout = new MenuFlyout();
                MenuFlyoutItem flyoutItem = new MenuFlyoutItem() { Text = "编辑", Tag = cellInfo };
                flyoutItem.Click += Flyout_Edit_Click;
                flyout.Items.Add(flyoutItem);
                //弹出菜单
                flyout.ShowAt(dataGrid, point);
            }
        }

        /// <summary>
        /// 项目右键菜单中“编辑”被单击。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Flyout_Edit_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem item = sender as MenuFlyoutItem;
            DataGridCellInfo info = item.Tag as DataGridCellInfo;
            RstFileModel.RstFileItem rstFileItem = info.Item as RstFileModel.RstFileItem;
            ContentDialog contentDialog = new ContentDialog()
            {
                Content = new EditPage(rstFileItem),
                Title = "编辑项目",
                PrimaryButtonText = "保存",
                CloseButtonText = "取消",
                DefaultButton = ContentDialogButton.Primary

            };
            ContentDialogResult result = await contentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var rstFileItems = (DataContext as ObservableCollection<RstFileModel.RstFileItem>);
                int index = rstFileItems.IndexOf(rstFileItem);
                string key = (contentDialog.Content as EditPage).Key;
                string value = (contentDialog.Content as EditPage).Value;
                rstFileItems[index] = new RstFileModel.RstFileItem(key, value);
            }

        }

    }
}
