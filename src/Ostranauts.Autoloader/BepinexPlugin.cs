using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LitJson;
using OstraAutoloader.Mods;
using OstraAutoloader.Patches;
using UnityEngine;

namespace OstraAutoloader;

[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
public class AutoloaderPlugin : BaseUnityPlugin
{
  internal static ManualLogSource Log = null!;

  private void Awake()
  {
    Log = Logger;

    // Log our awake here so we can see it in LogOutput.txt file
    Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");

    Log.LogDebug($"BasePath: {Application.dataPath}");

    string modsFolder = Path.Combine(Application.dataPath, "Mods");
    string loadOrder = Path.Combine(modsFolder, "loading_order.json");

    FileInfo loadFile = new(loadOrder);

    Harmony patcher = new(LCMPluginInfo.PLUGIN_GUID);

    try
    {
      patcher.PatchAll(typeof(Path_Patches));
    }
    catch (Exception ex)
    {
      Log.LogFatal($"Failed to run a patch. Autoloader is aborting\n{ex}");
      return;
    }

    DirectoryInfo pluginsDir = new(Paths.PluginPath);
    DirectoryInfo modsDir = new(Path.Combine(Application.dataPath, "Mods"));

    Path_Patches.ModsPath = modsDir.FullName + Path.DirectorySeparatorChar;
    Path_Patches.PluginPath = pluginsDir.FullName + Path.DirectorySeparatorChar;

    Log.LogInfo($"Searching plugins for any downloaded mods | {pluginsDir.FullName}");
    ModListing.FindAllModsInDirectory(pluginsDir);

    Log.LogInfo($"Searching mods dir for any manual mods | {modsDir.FullName}");
    ModListing.FindAllModsInDirectory(modsDir);

    ModListing.CreateLoadingOrder();

    Log.LogInfo($"populating load_order.json automatically with {ModListing.allModsByIdentifier.Count} autoload mods");
    WriteLoadingOrder(loadOrder);
  }

  internal void WriteLoadingOrder(string filePath)
  {
    JsonModList data = new()
    {
      strName = "Mod Loading Order",
      aLoadOrder = ["core", .. ModListing.sortedMods.Select(x => x.VirtualDir)],
      //This is in the loading_order.json for the sample mod so I copied it
      aIgnorePatterns = ["StreamingAssets/data/names_full"],
    };

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
