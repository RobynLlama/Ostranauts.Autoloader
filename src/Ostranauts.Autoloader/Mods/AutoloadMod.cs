using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OstraAutoloader.Mods;

public class AutoloadMod
{
  public readonly ModInfo Inf;
  public readonly AutoloadMetaInf MetaInf;
  public readonly DirectoryInfo BaseDir;
  public AutoloadMod[] Dependencies = [];

  public bool DependenciesResolved => _state == ResolutionState.Resolved;
  public bool FailedToLoad => _state == ResolutionState.Failed;
  private ResolutionState _state = ResolutionState.Pending;
  private bool _visited = false;

  public AutoloadMod(ModInfo info, AutoloadMetaInf meta, DirectoryInfo baseDir)
  {
    Inf = info;
    MetaInf = meta;
    BaseDir = baseDir;
  }

  public enum ResolutionState
  {
    Pending,
    Resolved,
    Failed,
  }

  public bool ResolveDependencies()
  {
    if (_visited)
    {
      AutoloaderPlugin.Log.LogWarning($"Recursive dependency detected in {Inf.strName}");
      return false;
    }

    if (DependenciesResolved)
      return true;

    if (FailedToLoad)
      return false;

    List<AutoloadMod> temp = [];
    _visited = true;

    bool failed = false;

    bool TryLoadMod(string name, bool hardDep = true)
    {
      string depType = hardDep ? "Dependency" : "Soft Dependency";

      if (!ModListing.allModsByIdentifier.TryGetValue(name, out var mod))
      {
        if (hardDep)
          AutoloaderPlugin.Log.LogWarning($"Unable to resolve dependency {name} for mod {Inf.strName}");

        return false;
      }
      else
      {
        if (mod.DependenciesResolved || (!mod.FailedToLoad && mod.ResolveDependencies()))
        {
          //Check if the mods are in compatible loading groups
          bool compat = (int)MetaInf.LoadingGroup >= (int)mod.MetaInf.LoadingGroup;

          if (compat)
          {
            temp.Add(mod);
            return true;
          }
          else
          {
            AutoloaderPlugin.Log.LogWarning($"{depType} {name} is in a later loading group than mod {Inf.strName}");
            return false;
          }
        }
        else
        {
          if (hardDep)
            AutoloaderPlugin.Log.LogWarning($"Unable to resolve Dependency {name} for mod {Inf.strName}");
          return false;
        }
      }
    }

    foreach (var item in MetaInf.Dependencies)
    {
      if (!TryLoadMod(item))
      {
        failed = true;
        break;
      }
    }

    //Soft Dependencies don't interrupt resolution if they fail
    if (!failed)
      foreach (var item in MetaInf.SoftDependencies)
      {
        if (!TryLoadMod(item, hardDep: false))
          AutoloaderPlugin.Log.LogDebug($"Failed to resolve a soft dependency {item} for {Inf.strName}");
      }

    Dependencies = [.. temp];
    _state = failed ? ResolutionState.Failed : ResolutionState.Resolved;
    _visited = false;

    return !failed;
  }

  public static AutoloadMod? FromDirectory(DirectoryInfo dir)
  {

    AutoloaderPlugin.Log.LogDebug($"Attempting to create AutoloadMod from {dir.Name}");

    if (!dir.Exists)
    {
      AutoloaderPlugin.Log.LogWarning($"Unable to create an Autoload from {dir.FullName}");
      return null;
    }

    string infoPath = Path.Combine(dir.FullName, "mod_info.json");
    string metaPath = Path.Combine(dir.FullName, "Autoload.Meta.toml");

    FileInfo modInfo = new(infoPath);
    FileInfo modMeta = new(metaPath);

    if (!modInfo.Exists)
    {
      AutoloaderPlugin.Log.LogWarning($"{dir.FullName} does not contain a mod_info.json");
      return null;
    }

    if (!modMeta.Exists)
    {
      AutoloaderPlugin.Log.LogWarning($"{dir.FullName} does not contain an Autoload.Meta.toml");
      return null;
    }

    try
    {
      Dictionary<string, JsonModInfo> dict = [];

      DataHandler.JsonToData(modInfo.FullName, dict);

      if (dict.Count < 1 || dict.Values.FirstOrDefault() is not JsonModInfo _rawInfo)
      {
        AutoloaderPlugin.Log.LogWarning($"{modInfo.FullName} cannot be deserialized into a mod info");
        return null;
      }

      var infoObj = new ModInfo(_rawInfo);
      var metaObj = AutoloadMetaInf.FromFile(modMeta);

      if (metaObj is null)
      {
        AutoloaderPlugin.Log.LogWarning($"{modMeta.FullName} cannot be deserialized into a mod meta");
        return null;
      }

      return new(infoObj, metaObj, dir);
    }
    catch (Exception ex)
    {
      AutoloaderPlugin.Log.LogWarning($"Failed to create Autoload mod\n{ex}");
      return null;
    }

  }
}
