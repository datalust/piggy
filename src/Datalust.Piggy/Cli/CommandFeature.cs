using System.Collections.Generic;

namespace Datalust.Piggy.Cli
{
    public abstract class CommandFeature
    {
        protected CommandFeature()
        {
            Errors = new List<string>();
        }

        public abstract void Enable(OptionSet options);

        public IList<string> Errors  { get; private set; }
    }
}
