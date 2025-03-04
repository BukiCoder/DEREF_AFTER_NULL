using System;
using System.Collections.Generic;
using System.Text;

namespace Deref_after_null
{
    internal class nameInfo
    {

        public bool isLocal { get; set; }
        public ValueSuggestion val { get; set; }
        public Dictionary<string, nameInfo> members;
        public nameInfo()
        {
            isLocal = false;
            val = new ValueSuggestion();
            members = new Dictionary<string, nameInfo>();
        }
        public nameInfo(nameInfo oth)
        {
            isLocal = oth.isLocal;
            val = new ValueSuggestion(oth.val);
            members = new Dictionary<string, nameInfo>();
            foreach (var item in oth.members)
            {
                members.Add(item.Key, new nameInfo(item.Value));
            }
        }
    }
}
