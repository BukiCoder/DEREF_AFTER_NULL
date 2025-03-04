using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deref_after_null
{
    internal class LocalContext
    {
        public Dictionary<string, nameInfo> variables;

        public LocalContext()
        {
            variables = new Dictionary<string, nameInfo>();
        }
        public void SetToLock()
        {
            
                SetToLock_(variables);
            
        }

        void SetToLock_(Dictionary<string, nameInfo> members)
        {
            foreach (var item in members)
            {
                item.Value.val.n = false;
                item.Value.val.nn = false;
                SetToLock_(item.Value.members);
            }
        }
        public List<string> AllNames(Dictionary<string, nameInfo> nms)
        {
            List<string> res = new List<string>();
            foreach (var name in nms)
            {
                if (name.Value.members != null && name.Value.members.Count > 0)
                {
                    res.AddRange(AllNames(name.Value.members).Select(x => name.Key + "." + x));
                }

            }
            res.AddRange(nms.Keys);
            return res;
        }

        public void SetNameInfo(string name, nameInfo inf)
        {
            var spl = name.Split('.');
            Dictionary<string, nameInfo> vars = variables;
            for (int i = 0; i < spl.Length - 1; i++)
            {
                vars = variables[spl[i]].members;
            }
            vars[spl.Last()] = inf;
        }
        public void Add(string name, ValueSuggestion sugg)
        {
            var spl = name.Split('.');
            Dictionary<string, nameInfo> vars = variables;
            for (int i = 0; i < spl.Length; i++)
            {
                if (vars.ContainsKey(spl[i]))
                {
                    vars = variables[spl[i]].members;
                }
                else
                {
                    for (int j = i; j < spl.Length; j++)
                    {
                        var inf = new nameInfo();
                        vars.Add(spl[i], inf);
                        vars = inf.members;
                        if (j == spl.Length - 1)
                        {
                            inf.val = sugg;
                        }
                    }
                }
            }
        }
        public nameInfo GetInfo(string name)
        {
            var spl = name.Split('.');
            Dictionary<string, nameInfo> vars = variables;
            for (int i = 0; i < spl.Length - 1; i++)
            {
                if (vars.ContainsKey(spl[i]))
                {
                    vars = vars[spl[i]].members;
                }
                else return null;

            }
            if (vars.ContainsKey(spl.Last()))
            {
                return vars[spl.Last()];
            }
            return null;


        }
        public LocalContext(LocalContext prev)

        {
            variables = new Dictionary<string, nameInfo>();
            foreach (var item in prev.variables)
            {
                variables.Add(item.Key, new nameInfo(item.Value));
            }
        }
    }
}
