using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Deref_aftet_null_2
{
    internal class LocalContext
    {
        public Dictionary<ISymbol, SymbolInfo> Symbols;

        public ValueSuggestion Default = ValueSuggestion.Unknown;
        public LocalContext()
        {
            Symbols = new Dictionary<ISymbol, SymbolInfo>(SymbolEqualityComparer.Default);
        }
        public LocalContext(LocalContext other)
        {
            Default = other.Default;
            Symbols = new Dictionary<ISymbol, SymbolInfo>(SymbolEqualityComparer.Default);
            foreach (var key in other.Symbols.Keys)
            {
                Symbols.Add(key, new SymbolInfo(other.Symbols[key]));
            }
        }

        public void Merge(LocalContext other, bool hasntIntersection = true)
        {
            var allKeys = other.Symbols.Keys.Union(Symbols.Keys, SymbolEqualityComparer.Default);
            foreach (var key in allKeys)
            {
                if (!Symbols.ContainsKey(key))
                {
                    Symbols.Add(key, new SymbolInfo(Default));
                }
                if (!other.Symbols.ContainsKey(key))
                {
                    Symbols[key].Merge(new SymbolInfo(other.Default), hasntIntersection);
                }
                else
                {
                    Symbols[key].Merge(other.Symbols[key], hasntIntersection);
                }
            }
            Default = (ValueSuggestion)Math.Max((int)Default, (int)other.Default);
        }

        public SymbolInfo TryGetAdd(SyntaxNode name, SemanticModel semantic, bool add)
        {
            if (name.IsKind(SyntaxKind.IdentifierName))
            {
                var symbol = semantic.GetSymbolInfo(name).Symbol;
                if (symbol == null)
                {
                    return null;
                }
                if (!Symbols.ContainsKey(symbol))
                {
                    if (!add)
                    {
                        return null;
                    }
                    Symbols.Add(symbol, new SymbolInfo(Default));
                }
                return Symbols[symbol];

            }
            else if (name.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                if (name.DescendantNodes().All(x => x.IsKind(SyntaxKind.SimpleMemberAccessExpression) || x.IsKind(SyntaxKind.IdentifierName)))
                {
                    Stack<ISymbol> symbols = new Stack<ISymbol>();
                    while (name.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        var symbol = semantic.GetSymbolInfo(((MemberAccessExpressionSyntax)name).Name).Symbol;
                        if (symbol == null)
                        {
                            return null;
                        }
                        symbols.Push(symbol);
                        name = ((MemberAccessExpressionSyntax)name).Expression;
                    }
                    if (!name.IsKind(SyntaxKind.IdentifierName))
                    {
                        return null;
                    }
                    symbols.Push(semantic.GetSymbolInfo(name).Symbol);
                    Dictionary<ISymbol, SymbolInfo> curr = this.Symbols;
                    ValueSuggestion def = this.Default;
                    ISymbol next = null;
                    while (symbols.Count > 0)
                    {
                        next = symbols.Pop();
                        if (!curr.ContainsKey(next))
                        {
                            if (!add)
                            {
                                return null;
                            }
                            curr.Add(next, new SymbolInfo(def));
                        }
                        if (symbols.Count == 0)
                        {
                            break;
                        }
                        def = curr[next].Default;
                        curr = curr[next].Members;
                    }
                    return curr[next];
                }
            }
            return null;
        }
    }
}
