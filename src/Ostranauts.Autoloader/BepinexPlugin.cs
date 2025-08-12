using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using OstraAutoloader.Patches;
using UnityEngine;

namespace OstraAutoloader;

[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
public class AutoloaderPlugin : BaseUnityPlugin
{
  internal static AutoloaderPlugin Instance = null!;
  internal ManualLogSource Log;
  internal ConfigEntry<bool> BackupNeeded;
  internal ConfigEntry<bool> UninstallMode;

  public AutoloaderPlugin()
  {
    Instance ??= this;
    Log = Logger;
    BackupNeeded = Config.Bind("Backup", "BackupNeeded", true, "This is managed automatically by Ostra.Autoloader to keep track of if a backup of the original load_order.json file was made");
    UninstallMode = Config.Bind("Uninstall", "UninstallMode", false, "Set this to true and run the game once. Ostra.Autoloader will undo any changes it made, where possible");
  }

  private void Awake()
  {
    Log = Logger;

    // Log our awake here so we can see it in LogOutput.txt file
    Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");
    Log.LogDebug($"Plugins Path: {Application.dataPath}");

    Harmony patcher = new(LCMPluginInfo.PLUGIN_GUID);

    try
    {
      patcher.PatchAll(typeof(DataHandler_Patches));
    }
    catch (Exception ex)
    {
      Log.LogFatal($"Failed to run a patch. Autoloader is aborting\n{ex}");
      return;
    }
  }
}
