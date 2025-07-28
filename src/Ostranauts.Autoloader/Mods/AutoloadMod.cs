using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OstraAutoloader.Patches;

namespace OstraAutoloader.Mods;

public class AutoloadMod
{
  public readonly ModInfo Inf;
  public readonly AutoloadMetaInf MetaInf;
  public readonly DirectoryInfo BaseDir;
  public AutoloadMod[] Dependencies = [];

  public string VirtualDir
  {
    get
    {
      string dir = BaseDir.FullName;
      dir = dir.Replace(Path_Patches.ModsPath, Path_Patches.ModsVirtual);
      dir = dir.Replace(Path_Patches.PluginPath, Path_Patches.PluginVirtual);

      return dir;
    }
  }

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

    foreach (var item in MetaInf.Dependencies)
    {
      if (!ModListing.allModsByIdentifier.TryGetValue(item, out var mod))
      {
        failed = true;
        AutoloaderPlugin.Log.LogWarning($"Unable to resolve dependency {item} for mod {Inf.strName}");
      }
      else
      {
        if (mod.DependenciesResolved || (!mod.FailedToLoad && mod.ResolveDependencies()))
        {
          //Check if the mods are in compatible loading groups
          bool compat = (int)MetaInf.LoadingGroup >= (int)mod.MetaInf.LoadingGroup;


          if (compat)
            temp.Add(mod);
          else
          {
            AutoloaderPlugin.Log.LogWarning($"Dependency {item} is in a later loading group than mod {Inf.strName}");
            failed = true;
          }
        }
        else
        {
          AutoloaderPlugin.Log.LogWarning($"Unable to resolve dependency {item} for mod {Inf.strName}");
          failed = true;
        }
      }
    }

    Dependencies = [.. temp];
    _state = failed ? ResolutionState.Failed : ResolutionState.Resolved;
    _visited = false;

    return !failed;
  }

  public static AutoloadMod? FromDirectory(DirectoryInfo dir)
  {

    AutoloaderPlugin.Log.LogInfo($"Attempting to create AutoloadMod from {dir.Name}");

    if (!dir.Exists)
    {
      AutoloaderPlugin.Log.LogWarning($"Unable to create an Autoload from {dir.FullName}");
      return null;
    }

    string infoPath = Path.Combine(dir.FullName, "mod_info.json");
    string metaPath = Path.Combine(dir.FullName, "Autoload.Meta.txt");

    FileInfo modInfo = new(infoPath);
    FileInfo modMeta = new(metaPath);

    if (!modInfo.Exists)
    {
      AutoloaderPlugin.Log.LogWarning($"{dir.FullName} does not contain a mod_info.json");
      return null;
    }

    if (!modMeta.Exists)
    {
      AutoloaderPlugin.Log.LogWarning($"{dir.FullName} does not contain an Autoload.Meta.txt");
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
