using System;
using System.Collections.Generic;
using System.Text;

namespace Deref_after_null_3
{
    public class SimpleCondition : ICloneable
    {
        public NullAndNotNullContainer Always;
        public NullAndNotNullContainer Only;
        public bool Valid;
        public SimpleCondition()
        {
            Always = new NullAndNotNullContainer() { HasNotNull = false, HasNull = false };
            Only = new NullAndNotNullContainer() { HasNotNull = false, HasNull = false };
            Valid = false;
        }
        public object Clone()
        {
            return new SimpleCondition() { Always = Always, Only = Only, Valid = Valid };
        }

        public static SimpleCondition MergeAnd(SimpleCondition this_, SimpleCondition other)
        {
            (this_.Only, this_.Always) = (this_.Only | other.Only, this_.Always & other.Always);
            this_.Valid = this_.Valid | other.Valid;
            return this_;
        }
        public static SimpleCondition MergeOr(SimpleCondition this_, SimpleCondition other)
        {
            (this_.Only, this_.Always) = (this_.Only & other.Only, this_.Always | other.Always);
            this_.Valid = this_.Valid | other.Valid;
            return this_;
        }
        public static SimpleCondition MergeNot(SimpleCondition this_, SimpleCondition other)
        {
            var only_ = this_.Only;
            this_.Only.HasNull = this_.Always.HasNotNull;
            this_.Only.HasNotNull = this_.Always.HasNull;
            this_.Always.HasNull = only_.HasNotNull;
            this_.Always.HasNotNull = only_.HasNull;

            return this_;

        }
    }
}
