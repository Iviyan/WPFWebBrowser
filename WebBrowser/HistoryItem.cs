using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowser
{
    public class HistoryItem
    {
        public DateTime DateTime { get; set; }
        public string Address { get; set; }

        public HistoryItem(string address) => (DateTime, Address) = (DateTime.Now, address);
    }
}
