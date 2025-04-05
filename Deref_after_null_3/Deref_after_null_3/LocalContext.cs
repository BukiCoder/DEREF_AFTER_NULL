using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deref_after_null_3
{
    internal class LocalContext
    {
        public SymbolTree<StatesContainer> Symbols;
        public List<SymbolTree<StatesContainer>> outContexts;
        public LocalContext()
        {
            Symbols = new SymbolTree<StatesContainer>();
            outContexts = new List<SymbolTree<StatesContainer>>();
        }

        public SymbolTree<StatesContainer> GetAdd(SyntaxNode name, SemanticModel semantic, bool add)
        {
            return Symbols.GetAdd(Condition.DecomposeName(name, semantic), 0, add);
        }

        public void Reset()
        {
            Symbols.All(new StatesContainer() { states = new HashSet<Guid>[2, 2] { { new HashSet<Guid>(), new HashSet<Guid>() }, { new HashSet<Guid>(), new HashSet<Guid>() } } });
        }
    }
}
