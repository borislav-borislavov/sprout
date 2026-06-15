using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Sprout.Core.Services.CPL
{
    /// <summary>
    /// Needed for things to work, but not much to see here. Just a custom AssemblyLoadContext with some extra metadata for unloading old versions of the assembly.
    /// </summary>
    public sealed class PageLogicLoadContext : AssemblyLoadContext
    {
        // isCollectible = true → critical for hot reload. Without it, the old assembly can never be unloaded and you'll leak memory on every recompile.
        public PageLogicLoadContext(string pageId)
            : base(name: $"PageLogic_{pageId}", isCollectible: true) { }

        protected override Assembly? Load(AssemblyName assemblyName)
            => null; // Fall back to the shared default context for everything else
    }
}
