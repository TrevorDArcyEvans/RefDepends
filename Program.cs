namespace RefDepends;

using System.Reflection;

public static class Program
{
  public static void Main(string[] args)
  {
    var targetFolder = args.Length == 1 ? args[0] : Environment.CurrentDirectory;

    // [dll] --> [where used]
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
          var thisRefName = thisRef.ToString();
          if (!refDepsMap.ContainsKey(thisRefName))
          {
            refDepsMap[thisRefName] = new();
          }

          refDepsMap[thisRefName].Add(assyName.ToString());
        }
      }
      catch
      {
        // can't load some .NET assys
      }
    }

    foreach (var kvp in refDepsMap.OrderBy(x => x.Key))
    {
      Console.WriteLine(kvp.Key);
      foreach (var val in kvp.Value)
      {
        Console.WriteLine($"  {val}");
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
