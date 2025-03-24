using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Deref_aftet_null_2
{
    enum ValueSuggestion
    {
        ComparedWithNull = 2,
        AssignedWithNull = 1,
        Unknown = 3,
        AssignedUnknown = 4
    }

    internal class SymbolInfo
    {
        public ValueSuggestion Default = ValueSuggestion.Unknown;
        public Dictionary<ISymbol, SymbolInfo> Members;
        public ValueSuggestion minValue;
        public ValueSuggestion maxValue;

        public SymbolInfo(ValueSuggestion def)
        {
            Members = new Dictionary<ISymbol, SymbolInfo>(SymbolEqualityComparer.Default);
            minValue = def;
            maxValue = def;
            Default = def;
        }

        public SymbolInfo(SymbolInfo other)
        {
            //minValue = other.minValue;
            minValue = other.minValue == ValueSuggestion.AssignedUnknown ? ValueSuggestion.AssignedUnknown : ValueSuggestion.Unknown;
            maxValue = other.maxValue;
            Default = other.Default;
            Members = new Dictionary<ISymbol, SymbolInfo>(SymbolEqualityComparer.Default);
            foreach (var key in other.Members.Keys)
            {
                Members.Add(key, new SymbolInfo(other.Members[key]));
            }
        }

        //if() else - parallel
        //if() { if() } - parent-child
        public void Merge(SymbolInfo other, bool parallel = true)
        {

            if (parallel)
            {
                if (other.minValue == ValueSuggestion.Unknown && minValue < ValueSuggestion.Unknown)
                {

                }
                else if (minValue == ValueSuggestion.Unknown && other.minValue < ValueSuggestion.Unknown)
                {
                    minValue = other.minValue;
                }
                else
                {
                    minValue = (other.maxValue == ValueSuggestion.AssignedUnknown && other.minValue == ValueSuggestion.AssignedUnknown) ? ValueSuggestion.Unknown : (ValueSuggestion)Math.Max((int)other.minValue, (int)minValue);
                }
                //minValue = (ValueSuggestion)Math.Min((int)other.minValue, (int)minValue);
            }
            else
            {
                minValue = (ValueSuggestion)Math.Min((int)other.minValue, (int)minValue);
            }

            maxValue = (ValueSuggestion)Math.Max((int)other.maxValue, (int)maxValue);          
            var allKeys = other.Members.Keys.Union(Members.Keys.Where(x => !Members[x].Equals(this)), SymbolEqualityComparer.Default);
            foreach (var key in allKeys)
            {
                if (!Members.ContainsKey(key))
                {
                    Members.Add(key, new SymbolInfo(Default));
                }
                if (!other.Members.ContainsKey(key))
                {
                    Members[key].Merge(new SymbolInfo(other.Default), parallel);
                }
                else
                {
                    Members[key].Merge(other.Members[key], parallel);
                }
            }
            Default = (ValueSuggestion)Math.Max((int)Default, (int)other.Default);
        }
    }
}
