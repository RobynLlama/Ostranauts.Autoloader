using System;
using BepInEx;
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
  public AutoloaderPlugin()
  {
    Instance ??= this;
    Log = Logger;
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
