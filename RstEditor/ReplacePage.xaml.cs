using System;
using System.Collections.Generic;
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
        RstFileModel rstFileModel;
        public RstFileModel RstFileModel
        {
            get { return rstFileModel; }
        }

        //定义依赖属性
        static readonly DependencyProperty originalTextProperty = DependencyProperty.Register("OriginalText", typeof(string), typeof(EditPage), new PropertyMetadata("", OnPropertyChanged));
        static readonly DependencyProperty replaceWithProperty = DependencyProperty.Register("ReplaceWith", typeof(string), typeof(EditPage), new PropertyMetadata("", OnPropertyChanged));
        public static DependencyProperty OriginalTextProperty { get { return originalTextProperty; } }
        public static DependencyProperty ReplaceWithProperty { get { return replaceWithProperty; } }

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

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ReplacePage).OnPropertyChanged(e);
        }
        private void OnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
        }
        public ReplacePage(RstFileModel rstFileModel)
        {
            this.InitializeComponent();
            this.rstFileModel = rstFileModel;
        }

        private async Task<ReplacementCount> GetMatchedTextCount()
        {
            int count_item = 0;
            int count_text = 0;
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

            return new ReplacementCount(count_text, count_item);
        }

        
        public class ReplacementCount
        {
            public int countOfText;
            public int countOfItem;
            public ReplacementCount(int count_text,int count_item)
            {
                countOfText = count_text;
                countOfItem = count_item;
            }
        }
    }
}
