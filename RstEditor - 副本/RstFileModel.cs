using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Noisrev.League.IO.RST;
namespace RstEditor
{
    public class RstFileModel : INotifyPropertyChanged
    {
        private ObservableCollection<RstFileItem> rstFileItems;
        private RSTFile rstFile;
        private RstFileInfo rstFileInfo;
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 用于构建<see cref="RstFileModel"/>的<see cref="RSTFile"/>，每当修改该值时， <see cref="RstFileItems"/>就会更新。
        /// </summary>
        public RSTFile RstFile
        {
            get { return rstFile; }
            set { rstFile = value; FillRstFileItems(); }
        }

        /// <summary>
        /// 如果在初始化<see cref="RstFileModel"/>时提供了<see cref="RstEditor.RstFileInfo"/>，本值就为<see cref="RstEditor.RstFileInfo"/>的实例；否则为null。
        /// </summary>
        public RstFileInfo RstFileInfo
        {
            get { return rstFileInfo; }
        }
        /// <summary>
        /// 从<see cref="RstFile"/>中整理的数据，每项包含Key和Value两个值。
        /// </summary>
        public ObservableCollection<RstFileItem> RstFileItems
        {
            get { return rstFileItems; }
            set { SetProperty(ref rstFileItems, value, "RstFileItems"); }
        }

        public RstFileModel()
        {
            rstFileItems = new ObservableCollection<RstFileItem>();
            rstFileItems.CollectionChanged += RstFileItems_CollectionChanged;
        }

        /// <summary>
        /// 从rstFile数据填充<see cref="RstFileItems"/>。
        /// </summary>
        private void FillRstFileItems()
        {
            if (rstFile != null)
            {
                Dictionary<ulong, string>.Enumerator enumerator = rstFile.Entries.GetEnumerator();
                for (int i = 0; i < rstFile.Entries.Count; i++)
                {
                    enumerator.MoveNext();
                    this.RstFileItems.Add(new RstFileModel.RstFileItem(enumerator.Current.Key.ToString("x"), enumerator.Current.Value));
                }
            }
        }

        public RstFileModel(RstFileInfo rstFileInfo) : this()
        {
            //从byte[]数据创建新RSTFile实例
             rstFile = new RSTFile(rstFileInfo.DataSource);
            this.rstFileInfo = rstFileInfo;
            FillRstFileItems();
        }

        /// <summary>
        /// 在<see cref="RstFileItems"/>的项改变时通知其值发生变化。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RstFileItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("RstFileItems");
        }

        protected bool SetProperty<T>(ref T storage,T value,[CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value))
                return false; //数据未发生变化，返回false。

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public class RstFileItem
        {
            public string ItemKey { get; set; }
            public string ItemValue { get; set; }
            public RstFileItem(string itemKey, string itemValue)
            {
                ItemKey = itemKey;
                ItemValue = itemValue;
            }

            
        }
    }
}
