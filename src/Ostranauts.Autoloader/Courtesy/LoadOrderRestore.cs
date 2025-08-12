using System;
using System.IO;

namespace OstraAutoloader.Courtesy;

public static partial class SafeLoadOrderManager
{
  public static void RestoreLoadOrderIfPossible(string loadLoc)
  {
    var plugin = AutoloaderPlugin.Instance;
    var loadingFile = new FileInfo(loadLoc);
    var backupFile = new FileInfo(loadLoc + ".old");

    if (!backupFile.Exists)
    {
      plugin.Log.LogMessage("Not restoring load_order.json backup, no file to restore");
      return;
    }

    if (loadingFile.Exists)
    {
      plugin.Log.LogMessage("Deleting auto-generated load_order.json");
      loadingFile.Delete();
    }

    try
    {
      backupFile.CopyTo(loadingFile.FullName);
      plugin.Log.LogMessage("Restored backup load_order.json.old to load_order.json");
      backupFile.Delete();
    }
    catch (Exception ex)
    {
      plugin.Log.LogError($"Unable to fully restore load_order.json!\n{ex}");
    }

  }
}
