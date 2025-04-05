using System;
using System.Collections.Generic;
using System.Text;

namespace Deref_after_null_3
{
    public struct NullAndNotNullContainer
    {
        public bool HasNull;
        public bool HasNotNull;
        public static NullAndNotNullContainer operator |(NullAndNotNullContainer a, NullAndNotNullContainer b)
        {
            return new NullAndNotNullContainer() { HasNotNull = a.HasNotNull | b.HasNotNull, HasNull = a.HasNull | b.HasNull };
        }

        public static NullAndNotNullContainer operator &(NullAndNotNullContainer a, NullAndNotNullContainer b)
        {
            return new NullAndNotNullContainer() { HasNotNull = a.HasNotNull & b.HasNotNull, HasNull = a.HasNull & b.HasNull };
        }
    }
}
