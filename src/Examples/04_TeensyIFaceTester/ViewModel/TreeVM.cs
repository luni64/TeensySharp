using lunOptics.LibUsbTree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.Linq;
using System.Collections;

namespace ViewModel
{
    public class TreeVM
    {
        public string name { get; set; }
        public ObservableCollection<TreeVM> children { get; set; }

        public TreeVM(UsbDevice r) : this(r.Description)
        {
            foreach(var rr in r.children)
            {
                children.Add(new TreeVM(rr));
            }
        }

        public TreeVM(string name)
        {
            children = new ObservableCollection<TreeVM>();
            this.name = name;

            
        }







        public List<UsbDevice> items { get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

       
    }
}
