namespace Datalust.Piggy.Filesystem
{
    class ChangeScriptFile
    {
        public string RelativeName { get; set; }
        public string FullPath { get; set; }

        public ChangeScriptFile(string fullPath, string relativeName)
        {
            FullPath = fullPath;
            RelativeName = relativeName;
        }
    }
}
