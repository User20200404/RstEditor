using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
    public sealed partial class ReplacePage : Page
    {
        private RstFileModel rstFileModel;
        private ContentDialog contentDialog;
        private bool aborted = false;
        private ObservableCollection<int> counts = new ObservableCollection<int>();
        private List<int> itemsToReplace;
        private DateTime startTime;
        /// <summary>
        /// 提供给替换操作的<see cref="RstFileModel"/>实例。
        /// </summary>
        public RstFileModel RstFileModel
        {
            get { return rstFileModel; }
        }

        /// <summary>
        /// 指示了是否正在进行替换操作。
        /// </summary>
        public bool IsWorking;

        //定义依赖属性
        static readonly DependencyProperty originalTextProperty = DependencyProperty.Register("OriginalText", typeof(string), typeof(EditPage), new PropertyMetadata("", OnPropertyChanged));
        static readonly DependencyProperty replaceWithProperty = DependencyProperty.Register("ReplaceWith", typeof(string), typeof(EditPage), new PropertyMetadata("", OnPropertyChanged));
        static readonly DependencyProperty progressProperty = DependencyProperty.Register("Progress", typeof(string), typeof(EditPage), new PropertyMetadata("", OnPropertyChanged));
        public static DependencyProperty OriginalTextProperty { get { return originalTextProperty; } }
        public static DependencyProperty ReplaceWithProperty { get { return replaceWithProperty; } }
        public static DependencyProperty ProgressProperty { get { return progressProperty; } }
        public string OriginalText
        {
            set
            {
                SetValue(OriginalTextProperty, value);
                Task.Run(async () =>
                {
                    ReplacementCount count = await GetMatchedTextCount();
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => { textBlock_Source.Text = "原始文本 - 找到 " + count.countOfItem.ToString() + " 个项目中的 " + count.countOfText.ToString() + " 个"; });

                });
            }
            get { return GetValue(OriginalTextProperty) as string; }
        }
        public string ReplaceWith
        {
            set { SetValue(ReplaceWithProperty, value); }
            get { return GetValue(ReplaceWithProperty) as string; }
        }

        public string Progress
        {
            set { SetValue(ProgressProperty, value); }
            get { return GetValue(ProgressProperty) as string; }
        }

        /// <summary>
        /// 对某页的项目文本进行快速替换。
        /// </summary>
        /// <param name="rstFileModel">该页的RstFileModel实例。</param>
        /// <param name="contentDialog">弹出的替换对话框，其Content属性就是<see cref="ReplacePage"/>的实例。</param>
        public ReplacePage(RstFileModel rstFileModel, ContentDialog contentDialog)
        {
            this.InitializeComponent();
            this.rstFileModel = rstFileModel;
            this.contentDialog = contentDialog;

            contentDialog.PrimaryButtonClick += ContentDialog_PrimaryButtonClick;
            contentDialog.CloseButtonClick += ContentDialog_CloseButtonClick;
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = IsWorking;
            if (IsWorking)
                Abort();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            Task.Run(() => { Work(); });
            args.Cancel = true; //阻止对话框关闭
        }

        /// <summary>
        /// 进行替换操作。
        /// </summary>
        private async void Work()
        {
            await OnWorkInitializing();

            counts.Clear();
            for (int i = 0; i < 8; i++)
                counts.Add(0);

            itemsToReplace = (await GetMatchedTextCount()).itemsToReplace;
            int count = itemsToReplace.Count;

            int div = count / 8;

            Task t1 = BeginReplace(itemsToReplace, 0, div, 0, counts);
            Task t2 = BeginReplace(itemsToReplace, div + 1, div * 2, 1, counts);
            Task t3 = BeginReplace(itemsToReplace, div * 2 + 1, div * 3, 2, counts);
            Task t4 = BeginReplace(itemsToReplace, div * 3 + 1, div * 4, 3, counts);
            Task t5 = BeginReplace(itemsToReplace, div * 4 + 1, div * 5, 4, counts);
            Task t6 = BeginReplace(itemsToReplace, div * 5 + 1, div * 6, 5, counts);
            Task t7 = BeginReplace(itemsToReplace, div * 6 + 1, div * 7, 6, counts);
            Task t8 = BeginReplace(itemsToReplace, div * 7 + 1, count - 1, 7, counts);
            Task.WaitAll(t1, t2, t3, t4, t5, t6, t7, t8);


            OnWorkEnding();
        }

        private async Task DisplayReplacedCount()
        {
            int count = 0;
            for (int i = 0; i < counts.Count; i++)
                count += counts[i];
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => { textBlock_Progress.Text = "已替换 " + count.ToString() + " 个项目。"; });
        }


        private async Task OnWorkInitializing()
        {
           await Task.Run(async () =>
           {
              await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
               {
                   aborted = false;
                   IsWorking = true;
                   startTime = DateTime.Now;

                   contentDialog.PrimaryButtonText = "";
                   contentDialog.CloseButtonText = "终止";
                   progressRing.IsActive = true;
                   textBox_Source.IsEnabled = false;
                   textBox_ReplaceWith.IsEnabled = false;
                   RstFilePage.UpdateDataSourceWhenModelChanged = false;
               });
           });
        }
        /// <summary>
        /// 在替换结束后调用此方法，更新数据 并 使控件状态归位。
        /// </summary>
        /// <param name="originalText"></param>
        private async void OnWorkEnding()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                await DisplayReplacedCount();

                contentDialog.PrimaryButtonText = "替换";
                contentDialog.CloseButtonText = "取消";
                progressRing.IsActive = false;
                textBox_Source.IsEnabled = true;
                textBox_ReplaceWith.IsEnabled = true;


                RstFilePage.UpdateDataSourceWhenModelChanged = true;
                var items = rstFileModel.RstFileItems;
                RstFileModel.RstFileItem temp_item = items[items.Count - 1];
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => { items[items.Count - 1] = new RstFileModel.RstFileItem(temp_item.ItemKey, temp_item.ItemValue); });

                OriginalText = OriginalText;//设置一次OriginalText属性，使找到的项目个数显示刷新。
                IsWorking = false;//指示工作结束。
            });
        }

        /// <summary>
        /// 替换的具体实现过程。
        /// </summary>
        private async void BeginReplace()
        {
            int count = 0;
            string originalText = "", replaceText = "";
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { originalText = this.OriginalText; replaceText = this.ReplaceWith; });
            if (!string.IsNullOrEmpty(originalText))
            {
                var items = rstFileModel.RstFileItems;
                //禁用数据更新，提高替换效率。
                RstFilePage.UpdateDataSourceWhenModelChanged = false;
                for (int i = 0; i < items.Count; i++)
                {
                    string itemValue = items[i].ItemValue;
                    string itemKey = items[i].ItemKey;
                    string newItemValue = itemValue.Replace(originalText, replaceText);
                    if (newItemValue != itemValue)
                    {
                        count++;
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => { items[i] = new RstFileModel.RstFileItem(itemKey, newItemValue); });
                        if (count % 15 == 0) //每替换15个项目更新一次进度回显。
                        {
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => { textBlock_Progress.Text = "已替换 " + count.ToString() + " 个项目"; });

                            //如果要求终止替换操作，则结束并返回。
                            bool abort = false;
                            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => { abort = aborted; }); //忽略此警告，如await会使性能大幅降低。
                            if (abort)
                                break;
                        }
                    }

                }
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => { textBlock_Progress.Text = "已替换 " + count.ToString() + " 个项目"; });
                //启用数据更新，并通过替换最后一项来触发数据更新事件。
                RstFilePage.UpdateDataSourceWhenModelChanged = true;
             
            }
            OnWorkEnding();
        }

        private async Task BeginReplace(List<int> itemsToReplace,int startIndex,int endIndex,int taskID,ObservableCollection<int> counts)
        {
            int count = itemsToReplace.Count;
            if (endIndex >= count)
                throw new Exception("Invalid Data");

            string originalText = "", replaceText = "";
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { originalText = this.OriginalText; replaceText = this.ReplaceWith; });

            var items = rstFileModel.RstFileItems;
            for (int i = startIndex;i<=endIndex;i++)
            {
                int index = itemsToReplace[i];
                string itemValue = items[index].ItemValue;
                string itemKey = items[index].ItemKey;
                string newItemValue = itemValue.Replace(originalText, replaceText);

                //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => { items[index] = new RstFileModel.RstFileItem(itemKey, newItemValue); });
                var newItem = new RstFileModel.RstFileItem(itemKey, newItemValue);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,()=> { items[index] = newItem; });
                int sub = i - startIndex + 1;
                counts[taskID] = sub;
                if (sub % 100 == 0)
                {
                    if (DateTime.Now - startTime >= TimeSpan.FromMilliseconds(250))
                    {
                        await DisplayReplacedCount();
                        startTime = DateTime.Now;
                    }
                }

                bool abort = false;
                Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => { abort = aborted; }); //忽略此警告，如await会使性能大幅降低。
                if (abort)
                    break;
            }
        }


        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ReplacePage).OnPropertyChanged(e);
        }
        private void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
        }

        /// <summary>
        /// 获取在当前替换参数下 将要 替换的项目数和文本数。
        /// </summary>
        /// <returns></returns>
        private async Task<ReplacementCount> GetMatchedTextCount()
        {
            int count_item = 0;
            int count_text = 0;
            List<int> itemsToReplace = new List<int>(1000);
            string originalText = null;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low,() => {originalText = OriginalText; });

            if (!string.IsNullOrEmpty(originalText))
            {
                var items = rstFileModel.RstFileItems;
                for (int i = 0; i < items.Count; i++)
                {
                    int indexOfText = items[i].ItemValue.IndexOf(originalText);
                    if (indexOfText != -1)
                    {
                        itemsToReplace.Add(i);
                        count_item++;
                        count_text++;
                        for (indexOfText = items[i].ItemValue.IndexOf(originalText, indexOfText + originalText.Length);
                        indexOfText != -1;
                        indexOfText = items[i].ItemValue.IndexOf(originalText, indexOfText + originalText.Length))
                        {
                            count_text++;
                        }
                    }
                }
            }

            return new ReplacementCount(count_text, count_item,itemsToReplace);
        }

        /// <summary>
        /// 终止正在进行的替换操作。
        /// </summary>
        public void Abort()
        {
            if (IsWorking)
                aborted = true;
            else throw new Exception("替换还未开始。");
        }



        /// <summary>
        /// 包含将要替换的项目数和文本数。
        /// </summary>
        public class ReplacementCount
        {
            public int countOfText;
            public int countOfItem;
            public List<int> itemsToReplace;
            public ReplacementCount(int count_text,int count_item,List<int> itemsToReplace)
            {
                countOfText = count_text;
                countOfItem = count_item;
                this.itemsToReplace = itemsToReplace; 
            }
        }
    }
}
