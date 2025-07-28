using System;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
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

    Log.LogInfo($"BasePath: {Application.dataPath}");

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
      Log.LogError($"Failed to run one or more patches, autoloaded mods may not work or may not be loaded at all!\n{ex}");
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

    Log.LogInfo("populating load_order.json automatically, only autoload mods will be included");

    var dataTop = """
      [
        {
          "strName": "Mod Loading Order",
          "strNotes": "To mod Ostranauts, place this loading_order.json file in your Mods/ folder (your game's Options->Files screen will show you where it should be), along with your mod's folder. Your mod folder should match the folder structure in the zip where you found this file. Make sure your mod folder name matches it's entry in aLoadOrder list below. 'core' refers to the base game data, which usually needs to be loaded first unless you know what you're doing.",
          "aLoadOrder": [
            "core"
      """;

    var dataBot = """
          ],
          "aIgnorePatterns": [
            "StreamingAssets/data/names_full"
          ]
        }
      ]
      """;

    StringBuilder sb = new(dataTop);
    string indent = new(' ', 6);

    int length = ModListing.sortedMods.Length;
    int noComma = length - 1;

    if (length > 0)
    {
      sb.AppendLine(",");
    }
    else
      sb.AppendLine();

    for (int i = 0; i < length; i++)
    {
      var item = ModListing.sortedMods[i];

      sb.Append(indent);
      sb.Append('"');
      sb.Append(item.VirtualDir);
      sb.Append('"');

      if (i < noComma)
        sb.Append(',');

      sb.AppendLine();
    }

    sb.AppendLine(dataBot);

    try
    {
      using var writer = new StreamWriter(loadFile.FullName, false);
      writer.Write(sb.ToString());
      writer.Flush();
    }
    catch (Exception ex)
    {
      Log.LogError($"Failed to create load_order.json, mods may not be loaded\n{ex}");
    }

  }

}
