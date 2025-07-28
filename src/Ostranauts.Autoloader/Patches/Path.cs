using System.IO;
using HarmonyLib;

namespace OstraAutoloader.Patches;

[HarmonyPatch(typeof(Path), nameof(Path.Combine))]
public static class Path_Patches
{

  internal static string PluginPath = string.Empty;
  internal static string ModsPath = string.Empty;

  public static readonly string PluginVirtual = "!PLUGINS_VIRTUAL@";
  public static readonly string ModsVirtual = "!MODS_VIRTUAL@";

  [HarmonyPrefix]
  public static bool Combine_Postfix(ref string path1, ref string path2, ref string __result)
  {
    if (path2.StartsWith(PluginVirtual))
    {
      path1 = PluginPath;
      path2 = path2.Replace(PluginVirtual, string.Empty);
    }
    else if (path2.StartsWith(ModsVirtual))
    {
      path1 = ModsPath;
      path2 = path2.Replace(ModsVirtual, string.Empty);
    }

    return true;
  }
}
