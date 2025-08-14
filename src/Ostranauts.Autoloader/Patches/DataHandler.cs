using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using LitJson;
using OstraAutoloader.Courtesy;
using OstraAutoloader.Mods;
using UnityEngine;

namespace OstraAutoloader.Patches;

[HarmonyPatch(typeof(DataHandler), nameof(DataHandler.Init))]
public static class DataHandler_Patches
{
  [HarmonyPrefix]
  public static bool Init_Prefix()
  {
    string loadPath = string.Empty;
    var plugin = AutoloaderPlugin.Instance;

    //Setup various paths
    if (File.Exists(Application.persistentDataPath + "/settings.json"))
    {
      Dictionary<string, JsonUserSettings> settings = [];
      DataHandler.JsonToData(Application.persistentDataPath + "/settings.json", settings);

      if (settings.ContainsKey("UserSettings"))
        loadPath = settings["UserSettings"].strPathMods;
    }

    if (string.IsNullOrEmpty(loadPath))
    {
      var defaults = new JsonUserSettings();
      defaults.Init();
      loadPath = defaults.strPathMods;
    }

    string modPath;
    modPath = Path.GetDirectoryName(loadPath);

    if (Directory.Exists(loadPath))
    {
      //Somehow the user has provided me with a directory
      //instead of a file for the load path

      string msg = $"""
      Error: The load path is a directory when it should be `loading_order.json` please use the file menu under option to select the correct file. Ostra.Autoloader cannot continue, shutting down.
        Path: {loadPath}
      """;

      plugin.Log.LogError(msg);
      ConsoleToGUI.instance.Log(msg, string.Empty, LogType.Error);

      return true;
    }

    plugin.Log.LogDebug($"Using mods path: {modPath}");

    if (plugin.UninstallMode.Value)
    {
      plugin.Log.LogMessage("Ostra.Autoloader is uninstalling, goodbye!");
      SafeLoadOrderManager.RestoreLoadOrderIfPossible(loadPath);
      return true;
    }

    DirectoryInfo pluginsDir = new(Paths.PluginPath);
    DirectoryInfo modsDir = new(modPath);

    plugin.Log.LogInfo($"Searching {pluginsDir.FullName}");
    ModListing.FindAllModsInDirectory(pluginsDir);

    plugin.Log.LogInfo($"Searching {modsDir.FullName}");
    ModListing.FindAllModsInDirectory(modsDir);

    ModListing.CreateLoadingOrder();

    plugin.Log.LogInfo($"populating load_order.json automatically with {ModListing.sortedMods.Length} autoload mods");

    SafeLoadOrderManager.BackupLoadOrderIfRequired(loadPath);
    WriteLoadingOrder(loadPath);
    return true;
  }

  internal static void WriteLoadingOrder(string filePath)
  {
    var plugin = AutoloaderPlugin.Instance;

    JsonModList data = new()
    {
      strName = "Mod Loading Order",
      aLoadOrder = ["core", .. ModListing.sortedMods.Select(x => x.BaseDir.FullName)],
      //This is in the loading_order.json for the sample mod so I copied it
      aIgnorePatterns = ["StreamingAssets/data/names_full"],
    };

    StringBuilder sb = new("\nAutoloaded the following mods in this order:\n");

    sb.Append("  ");
    sb.AppendLine("core");

    foreach (var item in ModListing.sortedMods)
    {
      sb.Append("  ");
      if (item.FailedToLoad)
        sb.Append("(FAILED) ");
      sb.AppendLine(item.Inf.strName);
    }

    plugin.Log.LogInfo(sb);
    ConsoleToGUI.instance.LogInfo(sb.ToString());

    using StreamWriter writer = new(filePath);
    JsonWriter jsonWriter = new(writer)
    {
      PrettyPrint = true,
      IndentValue = 2
    };

    List<JsonModList> list = [data];
    JsonMapper.ToJson(list, jsonWriter);
  }
}
