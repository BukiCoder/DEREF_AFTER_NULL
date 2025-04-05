using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Deref_after_null_3
{
    internal class SymbolTree<Value> : ICloneable where Value : ICloneable, new()
    {
        #region tree_generic_methods
        public static void Transform<T1>(SymbolTree<T1> s1, SymbolTree<T1> s2, Func<T1, T1, T1> transform) where T1 : ICloneable, new()
        {
            var allKeys = s1.Members.Keys.Union(s2.Members.Keys, SymbolEqualityComparer.Default);
            foreach (var key in allKeys)
            {
                if (!s1.Members.ContainsKey(key))
                {
                    s1.Members.Add(key, new SymbolTree<T1>() { });

                }
                if (!s2.Members.ContainsKey(key))
                {
                    s2.Members.Add(key, new SymbolTree<T1>() { });
                }
                Transform<T1>(s1.Members[key], s2.Members[key], transform);
            }
            s1.val = transform(s1.val, s2.val);
        }
        public static void Transform<T1>(SymbolTree<T1> s1, SymbolTree<T1> s2, SymbolTree<T1> s3, Func<T1, T1, T1, T1> transform) where T1 : ICloneable, new()
        {
            var allKeys = s1.Members.Keys.Union(s2.Members.Keys, SymbolEqualityComparer.Default);
            foreach (var key in allKeys)
            {
                if (!s1.Members.ContainsKey(key))
                {
                    s1.Members.Add(key, new SymbolTree<T1>() { });
                }
                if (!s2.Members.ContainsKey(key))
                {
                    s2.Members.Add(key, new SymbolTree<T1>() { });
                }
                if (!s3.Members.ContainsKey(key))
                {
                    s3.Members.Add(key, new SymbolTree<T1>() { });
                }
                Transform<T1>(s1.Members[key], s2.Members[key], s3.Members[key], transform);
            }
            s1.val = transform(s1.val, s2.val, s3.val);
        }
        public static void TransformBySelected<T1, T2>(SymbolTree<T1> s1, SymbolTree<T1> s2, SymbolTree<T2> sel, Func<T1, T1, T1> transform, Func<T1, T2, T1> selectThis) where T1 : ICloneable, new() where T2 : ICloneable, new()
        {
            var allKeys = s1.Members.Keys.Union(s2.Members.Keys, SymbolEqualityComparer.Default).Union(sel.Members.Keys, SymbolEqualityComparer.Default);
            foreach (var key in allKeys)
            {
                if (!s1.Members.ContainsKey(key))
                {
                    s1.Members.Add(key, new SymbolTree<T1>() { });
                }
                if (!s2.Members.ContainsKey(key))
                {
                    s2.Members.Add(key, new SymbolTree<T1>() { });
                }
                if (!sel.Members.ContainsKey(key))
                {
                    sel.Members.Add(key, new SymbolTree<T2>() { });
                }
                TransformBySelected<T1, T2>(s1.Members[key], s2.Members[key], sel.Members[key], transform, selectThis);

            }
            s1.val = transform(s1.val, selectThis(s2.val, sel.val));
        }
        public static SymbolTree<T1> Select<T1, T2>(SymbolTree<T1> s1, SymbolTree<T2> s2, Func<T1, T2, T1> selectThis) where T1 : ICloneable, new() where T2 : ICloneable, new()
        {
            var allKeys = s1.Members.Keys.Union(s2.Members.Keys, SymbolEqualityComparer.Default);
            SymbolTree<T1> res = new SymbolTree<T1>();
            foreach (var key in allKeys)
            {
                if (!s1.Members.ContainsKey(key))
                {
                    s1.Members.Add(key, new SymbolTree<T1>() { });
                }
                if (!s2.Members.ContainsKey(key))
                {
                    s2.Members.Add(key, new SymbolTree<T2>() { });
                }
                res.Members.Add(key, Select<T1, T2>(s1.Members[key], s2.Members[key], selectThis));
            }
            res.val = selectThis(s1.val, s2.val);
            return res;
        }
        #endregion


        public Dictionary<ISymbol, SymbolTree<Value>> Members;
        public Value val;

        public SymbolTree()
        {
            val = new Value();
            Members = new Dictionary<ISymbol, SymbolTree<Value>>(SymbolEqualityComparer.Default);
        }
        public SymbolTree<Value> GetAdd(List<(ISymbol, bool)> name, int i, bool add)
        {
            if (name == null)
            {
                return null;
            }
            else
            {
                if (i == name.Count)
                {
                    return this;
                }
                if (!Members.ContainsKey(name[i].Item1))
                {
                    if (add)
                    {
                        Members.Add(name[i].Item1, new SymbolTree<Value>());
                        return Members[name[i].Item1].GetAdd(name, i + 1, add);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return Members[name[i].Item1].GetAdd(name, i + 1, add);
                }
            }
        }
        public object Clone()
        {
            var clone = new SymbolTree<Value>();
            clone.val = (Value)val.Clone();
            clone.Members = new Dictionary<ISymbol, SymbolTree<Value>>(SymbolEqualityComparer.Default);
            foreach (var key in Members.Keys)
            {
                clone.Members.Add(key, (SymbolTree<Value>)Members[key].Clone());
            }
            return clone;
        }
        public void All(Value val)
        {
            foreach (var item in Members)
            {
                item.Value.All(val);
            }
            this.val = (Value)val.Clone();
        }
        public void All(Func<Value, Value> f)
        {
            foreach (var item in Members)
            {
                item.Value.All(f);
            }
            val = (Value)f(val);
        }

        public bool Any(Func<Value, bool> f)
        {
            if (f(val))
            {
                return true;
            }
            foreach (var item in Members)
            {
                if (item.Value.Any(f))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
