using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LentoCore.Atoms;

namespace LentoCore.Evaluator
{
    public class Scope
    {
        private static int _scopeCount = 0;

        protected int ID;
        protected string Name;
        protected Scope Parent;
        protected Dictionary<string, Atomic> Environment;
        public TypeTable TypeTable;

        protected Scope(string name, Scope parent, TypeTable typeTable)
        {
            ID = _scopeCount++;
            Name = name;
            Parent = parent;
            Environment = new Dictionary<string, Atomic>();
            TypeTable = typeTable;
        }

        /// <summary>
        /// Create a child scope derived from the current scope.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The new child scope</returns>
        public Scope Derive(string name) => new Scope(name, this, TypeTable);

        /// <summary>
        /// Get a variable from the current scope or its parent scopes.
        /// </summary>
        /// <param name="name">The name of the variable to find.</param>
        /// <returns>The variable if found, otherwise null.</returns>
        public Atomic Get(string name)
        {
            if (Environment.ContainsKey(name)) return Environment[name];
            return Parent?.Get(name);
        }

        /// <summary>
        /// Set a variable in the current scope.
        /// </summary>
        /// <param name="name">Name of the variable, function, etc.</param>
        /// <param name="value">Value of the variable</param>
        /// <returns></returns>
        public Atomic Set(string name, Atomic value)
        {
            Environment.Add(name, value);
            return value;
        }

        public bool Contains(string name) => Environment.ContainsKey(name) || (Parent != null && Parent.Contains(name));

        public override string ToString() => $"Scope[{Name}]<ID: {ID}, Size: {Environment.Count}>";
    }
}
