using System;

namespace Datalust.Piggy.History
{
    class AppliedChangeScript
    {
        public string ScriptFile { get; set; }
        public DateTimeOffset AppliedAt { get; set; }
        public string AppliedBy { get; set; }
        public int AppliedOrder { get; set; }
    }
}
