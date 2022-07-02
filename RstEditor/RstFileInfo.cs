using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace RstEditor
{
   public class RstFileInfo
    {
        public RstFileInfo()
        {

        }
        private StorageFile storageFile;
        private byte[] dataSource;
        private int filterComboSelectedIndex;
        private string filterText;

        public StorageFile StorageFile
        {
            get { return storageFile; }
            set { storageFile = value; }
        }
        public byte[] DataSource
        {
            get { return dataSource; }
            set { dataSource = value; }
        }

        public int FilterComboSelectedIndex
        {
            get { return filterComboSelectedIndex; }
            set { filterComboSelectedIndex = value; }
        }

        public string FilterText
        {
            get { return filterText; }
            set { filterText = value; }
        }

    }
}
