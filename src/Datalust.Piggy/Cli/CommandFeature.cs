using System;
using System.Collections.Generic;

namespace Datalust.Piggy.Cli
{
    abstract class CommandFeature
    {
        public abstract void Enable(OptionSet options);

        public virtual IEnumerable<string> GetUsageErrors() => Array.Empty<string>();
    }
}
