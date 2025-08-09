using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using LitJson;
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

    AutoloaderPlugin.Log.LogDebug($"Using mods path: {modPath}");

    DirectoryInfo pluginsDir = new(Paths.PluginPath);
    DirectoryInfo modsDir = new(modPath);

    AutoloaderPlugin.Log.LogInfo($"Searching {pluginsDir.FullName}");
    ModListing.FindAllModsInDirectory(pluginsDir);

    AutoloaderPlugin.Log.LogInfo($"Searching {modsDir.FullName}");
    ModListing.FindAllModsInDirectory(modsDir);

    ModListing.CreateLoadingOrder();

    AutoloaderPlugin.Log.LogInfo($"populating load_order.json automatically with {ModListing.sortedMods.Length} autoload mods");
    WriteLoadingOrder(loadPath);
    return true;
  }

  internal static void WriteLoadingOrder(string filePath)
  {
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

    AutoloaderPlugin.Log.LogInfo(sb);
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
