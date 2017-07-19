namespace Datalust.Piggy.Cli
{
    interface ICommandMetadata
    {
        string Name { get; }
        string HelpText { get; }
    }
}
