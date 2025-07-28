using System.Collections.Generic;
using System.IO;

namespace OstraAutoloader.Mods;

public class AutoloadMetaInf
{
  public readonly string[] Dependencies = [];
  public readonly ModLoadingGroup LoadingGroup = ModLoadingGroup.WithVanilla;

  public AutoloadMetaInf(string[] Dependencies, ModLoadingGroup LoadingGroup)
  {
    this.Dependencies = Dependencies;
    this.LoadingGroup = LoadingGroup;
  }

  public static AutoloadMetaInf? FromFile(FileInfo file)
  {
    if (!file.Exists)
      return null;

    //Manually serialize from a basic text format
    //because Newtonsoft.Json can't exist in the game's
    //env without a whole bunch of extra dependencies

    using TextReader reader = new StreamReader(file.FullName);

    var header = "AUTOLOAD.META";

    //read the header
    var firstLine = reader.ReadLine().Trim();
    if (!firstLine.Equals(header))
    {
      AutoloaderPlugin.Log.LogInfo($"Error parsing Autoloader inf, bad header: {firstLine}");
      return null;
    }

    //this could be much more idiomatic but w/e
    string? next;

    bool depLoop = false;
    List<string> _deps = [];
    ModLoadingGroup? group = null;

    while ((next = reader.ReadLine()) != null)
    {
      next = next.Trim();

      //AutoloaderPlugin.Log.LogInfo($"NextLine: {next}");

      if (string.IsNullOrEmpty(next))
        continue;

      if (next[0] == '#')
        continue;

      if (depLoop)
      {

        if (next.Equals("END", System.StringComparison.InvariantCultureIgnoreCase))
        {
          depLoop = false;
          continue;
        }

        _deps.Add(next);
      }
      else if (next.Equals("BEGIN Dependencies", System.StringComparison.InvariantCultureIgnoreCase))
        depLoop = true;
      else if (next.Contains(":"))
      {
        //property assignment
        var items = next.ToLowerInvariant().Split(':');

        if (items.Length != 2)
        {
          AutoloaderPlugin.Log.LogWarning($"Ignoring malformed property: {next}");
          continue;
        }

        var prop = items[0].Trim();
        var item = items[1].Trim();

        //AutoloaderPlugin.Log.LogInfo($"Parsing prop: {prop}:{item}");

        switch (prop)
        {
          case "loadgroup":
            switch (item)
            {
              case "withvanilla":
                group = ModLoadingGroup.WithVanilla;
                break;
              case "ffucore":
                group = ModLoadingGroup.FFUCore;
                break;
              case "afterffu":
                group = ModLoadingGroup.AfterFFU;
                break;
              default:
                AutoloaderPlugin.Log.LogWarning($"Ignoring unknown group type: {item}");
                break;
            }
            break;
          default:
            AutoloaderPlugin.Log.LogWarning($"Ignoring unknown property: {prop}");
            break;
        }
      }
      else
      {
        //We shouldn't make it here
        AutoloaderPlugin.Log.LogWarning($"Ignoring malformed line: {next}");
      }
    }

    if (group is not ModLoadingGroup validGroup)
    {
      AutoloaderPlugin.Log.LogWarning("Autoload Meta has null group, unable to parse");
      return null;
    }

    return new([.. _deps], validGroup);
  }
}
