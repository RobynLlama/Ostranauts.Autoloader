using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OstraAutoloader.Patches;

namespace OstraAutoloader.Mods;

public static class ModListing
{
  internal static readonly Dictionary<string, AutoloadMod> allModsByIdentifier = [];
  internal static AutoloadMod[] sortedMods = [];

  internal static void FindAllModsInDirectory(DirectoryInfo baseDir)
  {
    var plugin = AutoloaderPlugin.Instance;

    if (!baseDir.Exists)
    {
      plugin.Log.LogInfo($"""
      Skipping a directory in FindAllModsInDirectory(DirectoryInfo baseDir)
        Reason:    Directory does not exist
        Directory: {baseDir.FullName}

      Please check that your configured mod folder exists, mods from other sources will still load.
      """);
      return;
    }

    //Traverse down the tree first
    foreach (var dir in baseDir.GetDirectories())
    {
      FindAllModsInDirectory(dir);
    }

    foreach (var file in baseDir.GetFiles("Autoload.Meta.toml"))
    {
      if (file.Name == "Autoload.Meta.toml")
      {
        plugin.Log.LogDebug($"Found an autoload in {baseDir.Name}");

        try
        {
          var mod = AutoloadMod.FromDirectory(baseDir);
          if (mod is not null)
            if (allModsByIdentifier.ContainsKey(mod.Inf.strName))
              plugin.Log.LogWarning($"Attempting to load the same mod twice! {mod.Inf.strName}");
            else
            {
              plugin.Log.LogInfo($"Registered {mod.Inf.strName}@{mod.Inf.strModVersion}");
              allModsByIdentifier.Add(mod.Inf.strName, mod);
            }

        }
        catch (Exception ex)
        {
          plugin.Log.LogWarning($"Encountered malformed Autoload:\n{ex}");
        }
      }
    }
  }

  internal static void CreateLoadingOrder()
  {

    var _sortedTemp = new List<AutoloadMod>();
    var _added = new HashSet<AutoloadMod>();

    void ResolveAndAdd(AutoloadMod item)
    {
      if (_added.Contains(item))
        return;

      if (item.ResolveDependencies())
      {
        foreach (var mod in item.Dependencies)
          if (mod.ResolveDependencies())
            ResolveAndAdd(mod);

        _sortedTemp.Add(item);
        _added.Add(item);
      }
    }

    void ResolveLoadingGroup(AutoloadMod[] items, ModLoadingGroup group)
    {
      foreach (var item in items)
      {
        if (item.MetaInf.LoadingGroup != group)
          continue;

        if (item.ResolveDependencies())
          ResolveAndAdd(item);
      }

    }

    var mods = allModsByIdentifier.Values.ToArray();

    ResolveLoadingGroup(mods, ModLoadingGroup.WithVanilla);
    ResolveLoadingGroup(mods, ModLoadingGroup.FFUCore);
    ResolveLoadingGroup(mods, ModLoadingGroup.AfterFFU);

    sortedMods = [.. _sortedTemp];
  }
}
