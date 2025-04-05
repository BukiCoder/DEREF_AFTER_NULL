using System;
using System.Collections.Generic;
using System.Text;

namespace Deref_after_null_3
{
    internal class StatesContainer : ICloneable
    {
        //[IsNull, compared]
        public HashSet<Guid>[,] states;
        public object Clone()
        {
            var clone = new StatesContainer();
            clone.states = new HashSet<Guid>[2, 2];
            clone.states[1, 1] = new HashSet<Guid>(states[1, 1]);
            clone.states[1, 0] = new HashSet<Guid>(states[1, 0]);
            clone.states[0, 0] = new HashSet<Guid>(states[0, 0]);
            clone.states[0, 1] = new HashSet<Guid>(states[0, 1]);
            return clone;

        }
        public StatesContainer(StatesContainer oth)
        {
            states = new HashSet<Guid>[2, 2];
            states[1, 1] = new HashSet<Guid>(oth.states[1, 1]);
            states[1, 0] = new HashSet<Guid>(oth.states[1, 0]);
            states[0, 0] = new HashSet<Guid>(oth.states[0, 0]);
            states[0, 1] = new HashSet<Guid>(oth.states[0, 1]);
        }

        public StatesContainer()
        {
            states = new HashSet<Guid>[2, 2];
            states[1, 1] = new HashSet<Guid>();
            states[1, 0] = new HashSet<Guid>() { Guid.NewGuid() };
            states[0, 0] = new HashSet<Guid>() { Guid.NewGuid() };
            states[0, 1] = new HashSet<Guid>();
        }
        public StatesContainer(bool empty)
        {

            states = new HashSet<Guid>[2, 2];
            states[1, 1] = new HashSet<Guid>();
            states[1, 0] = new HashSet<Guid>();
            states[0, 0] = new HashSet<Guid>();
            states[0, 1] = new HashSet<Guid>();

            if (!empty)
            {

                states[1, 0].Add(Guid.NewGuid());
                states[0, 0].Add(Guid.NewGuid());

            }

        }
    }
}
