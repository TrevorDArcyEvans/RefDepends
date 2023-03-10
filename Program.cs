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
}
