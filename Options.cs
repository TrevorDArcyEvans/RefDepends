namespace RefDepends;

using CommandLine;

internal sealed class Options
{
  [Value(index: 0, Required = false, HelpText = "Path to directory to analyse")]
  public string Directory { get; set; } = Environment.CurrentDirectory;

  [Option('d', "dot", Required = false, HelpText = "Path to output output dot file")]
  public string DotFilePath { get; set; }
}
