using System.Text;

namespace RefDepends;

using System.Reflection;

public static class Program
{
  public static void Main(string[] args)
  {
    var targetFolder = args.Length == 1 ? args[0] : Environment.CurrentDirectory;

    // [dll] --> [where used]
    // NOTE:  have to use strings because AssemblyName equality is object equality
    var refDepsMap = new Dictionary<string, HashSet<string>>();

    var assyPaths = Directory.EnumerateFiles(targetFolder, "*.dll", SearchOption.TopDirectoryOnly).Where(IsAssembly);
    foreach (var assyPath in assyPaths)
    {
      try
      {
        var assy = Assembly.LoadFile(assyPath);
        var assyName = assy.GetName();
        var refs = assy.GetReferencedAssemblies();
        foreach (var thisRef in refs)
        {
          var thisRefName = thisRef.FullName;
          if (!refDepsMap.ContainsKey(thisRefName))
          {
            refDepsMap[thisRefName] = new();
          }

          refDepsMap[thisRefName].Add(assyName.FullName);
        }
      }
      catch
      {
        // can't load some .NET assys
      }
    }

    // reconstitute AssemblyNames
    var assyNameRefDependsMap = refDepsMap.ToDictionary(
      kvp => new AssemblyName(kvp.Key),
      kvp => kvp.Value.Select(val => new AssemblyName(val)));
    var assGrps = assyNameRefDependsMap.GroupBy(x => x.Key.Name);
    foreach (var assGrp in assGrps)
    {
      Console.WriteLine(assGrp.Key);
      foreach (var kvp in assGrp)
      {
        Console.WriteLine($"  {kvp.Key.FullName}");
        foreach (var assyName in kvp.Value)
        {
          Console.WriteLine($"    {assyName.FullName}");
        }
      }
    }

    var dot = ToDot(assGrps);
  }

  private static bool IsAssembly(string assyPath)
  {
    try
    {
      _ = AssemblyName.GetAssemblyName(assyPath);
    }
    catch
    {
      return false;
    }

    return true;
  }

  private static string ToDot(IEnumerable<IGrouping<string?, KeyValuePair<AssemblyName, IEnumerable<AssemblyName>>>> assGrps)
  {
    var sb = new StringBuilder();

    sb.AppendLine("digraph G");
    sb.AppendLine("{");

    foreach (var assGrp in assGrps)
    {
      var unVerAss = assGrp.Key;
      foreach (var kvp in assGrp)
      {
        var verAss = kvp.Key.FullName;
        sb.AppendLine($"  {kvp.Key.FullName} -> {verAss}");
        foreach (var assyName in kvp.Value)
        {
          var depAss = assyName.FullName;
          sb.AppendLine($"    {depAss}->{verAss};");
        }
      }
    }

    sb.AppendLine("}");

    return sb.ToString();
  }
}
