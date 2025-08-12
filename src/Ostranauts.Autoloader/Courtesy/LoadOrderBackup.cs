using System;
using System.IO;

namespace OstraAutoloader.Courtesy;

public static partial class SafeLoadOrderManager
{
  public static void BackupLoadOrderIfRequired(string loadLoc)
  {
    var plugin = AutoloaderPlugin.Instance;
    var loadingFile = new FileInfo(loadLoc);
    var backupFile = new FileInfo(loadLoc + ".old");

    if (!plugin.BackupNeeded.Value)
    {
      plugin.Log.LogDebug("Skipping backup, we've already made one");
      return;
    }

    plugin.BackupNeeded.Value = false;
    plugin.Config.Save();

    if (!loadingFile.Exists)
    {
      plugin.Log.LogMessage("Skipping backup, there is no load_order.json file");
      return;
    }

    if (backupFile.Exists)
    {
      plugin.Log.LogMessage("Skipping backup, already exists");
      return;
    }

    try
    {
      loadingFile.CopyTo(backupFile.FullName);
      plugin.Log.LogMessage("Copied original load_order.json to load_order.json.old");
    }
    catch (Exception ex)
    {
      plugin.Log.LogError($"Unable to backup load_order.json!\n{ex}");
    }

  }
}
