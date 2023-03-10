namespace RefDepends;

using System.Reflection;
using System.Text;
using CommandLine;

public static class Program
{
  public static void Main(string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
      .WithParsed<Options>(opts =>
      {
        var targetFolder = opts.Directory;

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

        if (!string.IsNullOrEmpty(opts.DotFilePath))
        {
          var dot = ToDot(assGrps);
          File.WriteAllText(opts.DotFilePath, dot);
        }
      });
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

  private static string ToDot(IEnumerable<IGrouping<string, KeyValuePair<AssemblyName, IEnumerable<AssemblyName>>>> assGrps)
  {
    var assyNodeNames = GetAssemblyNodeNames(assGrps).Distinct();
    var sb = new StringBuilder();

    sb.AppendLine("digraph G");
    sb.AppendLine("{");
    sb.AppendLine(" rankdir=\"BT\";");
    sb.AppendLine(" {");
    sb.AppendLine("    node [shape=box]");
    foreach (var assyNodeName in assyNodeNames)
    {
      sb.AppendLine($"    \"{assyNodeName}\";");
    }
    sb.AppendLine(" }");
    sb.AppendLine();

    foreach (var assGrp in assGrps)
    {
      var unVerAss = assGrp.Key;
      foreach (var kvp in assGrp)
      {
        var verAss = kvp.Key.GetNormalisedName();
        sb.AppendLine($"  \"{verAss}\" -> \"{unVerAss}\";");
        foreach (var assyName in kvp.Value)
        {
          var depAss = assyName.GetNormalisedName();
          sb.AppendLine($"    \"{depAss}\" -> \"{verAss}\";");
        }
      }
    }

    sb.AppendLine("}");

    return sb.ToString();
  }

  private static IEnumerable<string> GetAssemblyNodeNames(IEnumerable<IGrouping<string, KeyValuePair<AssemblyName, IEnumerable<AssemblyName>>>> assGrps)
  {
    foreach (var assGrp in assGrps)
    {
      var unVerAss = assGrp.Key;
      yield return unVerAss;
      
      foreach (var kvp in assGrp)
      {
        var verAss = kvp.Key.GetNormalisedName();
        yield return verAss;
        
        foreach (var assyName in kvp.Value)
        {
          var depAss = assyName.GetNormalisedName();
          yield return depAss;
        }
      }
    }
  }

  private static string GetNormalisedName(this AssemblyName assName) => assName.FullName.Replace(",", "\\n");
}
