using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tommy;

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

    //Serialize from our lovely TOML file
    using StreamReader reader = File.OpenText(file.FullName);
    TomlTable meta = TOML.Parse(reader);

    var signature = meta["FileType"].AsString;

    if (!signature.HasValue || !string.Equals(signature.Value, "AUTOLOAD.META", StringComparison.InvariantCultureIgnoreCase))
    {
      AutoloaderPlugin.Log.LogWarning($"Skipping a malformed Autoload meta file (no or bad FileType header): {signature.HasValue}|{signature.Value}");
      return null;
    }

    string LoadGroup = meta["LoadGroup"].AsString.Value;
    ModLoadingGroup loadingGroup;

    switch (LoadGroup.ToLowerInvariant())
    {
      case "withvanilla":
        loadingGroup = ModLoadingGroup.WithVanilla;
        break;
      case "ffucore":
        loadingGroup = ModLoadingGroup.FFUCore;
        break;
      case "afterffu":
        loadingGroup = ModLoadingGroup.AfterFFU;
        break;
      default:
        AutoloaderPlugin.Log.LogWarning($"Unable to parse loading group {LoadGroup}");
        return null;
    }

    var deps = meta["dependencies"];

    if (!deps.IsTable)
    {
      AutoloaderPlugin.Log.LogWarning("Dependencies is malformed");
      return null;
    }

    return new([.. deps.Keys], loadingGroup);
  }
}
