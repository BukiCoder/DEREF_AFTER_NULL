using System;
using System.Collections.Generic;
using System.Text;

namespace Deref_after_null
{
    internal class ValueSuggestion
    {
        public bool n { get; set; }
        public bool nn { get; set; }
        public bool beyond { get; set; }
        public ValueSuggestion(ValueSuggestion oth)
        {
            n = oth.n;
            nn = oth.nn;
            beyond = oth.beyond;
        }
        public ValueSuggestion()
        {
            n = false;
            nn = true;
            beyond = true;
        }

    }
}
