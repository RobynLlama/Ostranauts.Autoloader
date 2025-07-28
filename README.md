# Ostra.Autoloader

![Language: C#](https://img.shields.io/badge/Language-C%23-blue?style=flat-square&logo=sharp)
![License: GPLv3](https://img.shields.io/badge/License-GPLv3-orange?style=flat-square&logo=gnuemacs)

![Autoloader Logo](banner.png)

Ostra.Autoloader is a simple tool that automatically generates your `loading_order.json` file for Ostranauts mods. No more manual editing or guesswork!

---

## Why use Ostra.Autoloader?

- **No manual JSON editing:** Mod authors can include a special `Autoload.Meta.txt` file in their mod folder, and Ostra.Autoloader will read it to figure out how to load the mod.
- **Supports mods outside the default folder:** Mods placed in other folders like `BepInEx/Plugins` (used by some mod managers) will be discovered automatically.
  - This means mod managers can install mods for you, finally, no more manual installing!
- **Automatic dependency handling:** Mods can declare dependencies on other mods, and Ostra.Autoloader will make sure those dependencies load first. This helps avoid common mistakes where mods fail because their dependencies are missing or loaded in the wrong order.

---

## How it works

1. **Scan for mods:** Ostra.Autoloader looks through your `Mods` folder *and* the `BepInEx/Plugins` folder for any mod folders containing an `Autoload.Meta.txt` file.
2. **Read mod info:** It reads the mod's `mod_info.json` and `Autoload.Meta.txt` to gather information about the mod and its dependencies.
3. **Resolve dependencies:** It figures out which mods depend on others and generates a correct loading order for you.
4. **Create `loading_order.json`:** Finally, it writes out a `loading_order.json` file automatically, so the game knows the right order to load your mods. No more `invalid json` errors!

---

## Installation

### Manually (Not Recommended)

> [!NOTE]
> There are no pre-built releases yet. Ostra.Autoloader is in pre-release so you will have to build it yourself, sorry.

- Download the Ostra.Autoloader.dll from a release and place it in your BepInEx/Plugins folder
- Place any downloaded mods that support Ostra.Autoloader under your game's `Mods` directory or `BepInEx/Plugins` for discovery

### Automatically (Easy)

- Download a mod manager that supports the Ostranauts community on the Thunderstore
- Select the Ostranauts community and create a new profile
- Browse the available mods and press "Install" on any that support Ostra.Autoloader. Autoloader will be downloaded as a dependency

#### Available Managers

- TBA (Community is not launched yet)

---

## Example mod folder structure

```plaintext
Mods/
└── MyCoolMod/
    ├── mod_info.json
    └── Autoload.Meta.txt

BepInEx/
└── Plugins/
    └── AnotherAwesomeMod/
        ├── mod_info.json
        └── Autoload.Meta.txt
```

---

## Notes for mod authors

- Include an `Autoload.Meta.txt` file that lists dependencies and loading group or Ostra.Autoloader will skip your mod entirely
- Ostra.Autoloader will handle the rest, ensuring your mod loads after its dependencies.

---

## Troubleshooting / Common Problems

- If your mod isn't loading, check the log file (`BepInEx/LogOutput.txt`) in your profile for warnings about missing dependencies or malformed meta files.
- Ensure the Autoload meta file is properly formatted
- Ensure the Autoload meta file is named exactly `Autoload.Meta.txt`

---

## License

This project is licensed under the GNU Public License Version 3, see [License](LICENSE) for more information

---

## Contact

For questions or support, please join the Blue Bottle Games discord and contact the mod author, Robyn, or open an issue in this repository

---

Enjoy hassle-free mod loading with **Ostra.Autoloader**!
