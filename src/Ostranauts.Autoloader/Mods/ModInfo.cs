namespace OstraAutoloader.Mods;

public class ModInfo
{
  /*
  {
    "strName": "Sample Mod",
    "strAuthor": "Blue Bottle Games, LLC",
    "strModURL": "https://bluebottlegames.com/games/ostranauts",
    "strGameVersion": "0.8.0.0",
    "strModVersion": "0.1",
    "strNotes": "This is an example mod_info.json file, for the sample mod folder SampleMod. It includes a complete, empty folder structure for all moddable data in Ostranauts, plus two minor changes: the sink has MOD written on it, and the starting salvage tug is a Modded TU-77a instead of TU-77a. Mods generally look like the game's .../Ostranauts_Data/StreamingAssets/ folder, and can contain some or all of the data types found there."
  }
  */

  public readonly string strName;
  public readonly string strAuthor;
  public readonly string strModURL;
  public readonly string strGameVersion;
  public readonly string strNotes;

  public ModInfo(
  string strName,
  string strAuthor,
  string strModURL,
  string strGameVersion,
  string strNotes
  )
  {
    this.strName = strName;
    this.strAuthor = strAuthor;
    this.strModURL = strModURL;
    this.strGameVersion = strGameVersion;
    this.strNotes = strNotes;
  }

  public ModInfo(JsonModInfo info) : this(info.strName, info.strAuthor, info.strModURL, info.strGameVersion, info.strNotes) { }
}
