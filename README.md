# Gibbed's Let Us Cling Together Tools

Tools for modding the PSP remake of Tactics Ogre: Let Us Cling Together.

[![Build status](https://ci.appveyor.com/api/projects/status/9cl2i5x0j2rlwgih/branch/main?svg=true)](https://ci.appveyor.com/project/gibbed/gibbed-letusclingtogether/branch/main)

# Targets

## [Tactics Ogre: Let Us Cling Together (2010)](https://en.wikipedia.org/wiki/Tactics_Ogre%3A_Let_Us_Cling_Together_%282010_video_game%29)

Intended to work with all available regional versions of the game, though only the following have been tested:

* PlayStation Portable (PSP)
    * JP (`ULJM05753`): タクティクスオウガ　運命の輪
        * Tactics Ogre: Unmei no Wa
        * Tactics Ogre: Wheel of Fortune
        * Tactics Ogre: Wheel of Fate
    * US (`ULUS10565`): Tactics Ogre: Let Us Cling Together
    * EU/AU (`ULES10500`): Tactics Ogre: Let Us Cling Together

# What?

*Experimental!*

# Tools

## FILETABLE / Archives

* [`PackFILETABLE`](projects/Gibbed.LetUsClingTogether.PackFILETABLE): Packer for `FILETABLE.BIN` (and corresponding `.BIN` files).
* [`LookupFILETABLE`](projects/Gibbed.LetUsClingTogether.LookupFILETABLE): Translates offsets in `.BIN` files to their corresponding file entries.
* [`UnpackFILETABLE`](projects/Gibbed.LetUsClingTogether.UnpackFILETABLE): Unpacker for `FILETABLE.BIN` (and corresponding `.BIN` files).

_`UnpackFILETABLE` takes advantage of [filetable definitions](configs/filetables) to identify & describe files within the ID-based filesystem._

## Assets

### Spreadsheets / Tables

* `ExportSheet`: Exporter for compiled spreadsheet files (`*.xlc`).

_Requires corresponding sheet format files that describe the various spreadsheet data formats._

### Scenarios / Screenplays

* [`DisassembleScript`](projects/Gibbed.LetUsClingTogether.DisassembleScript): A cutscene script disassembler (`*.script`). For research purposes.
* [`ExportEventMessages`](projects/Gibbed.LetUsClingTogether.ExportEventMessages): Exporter for event message files (`*.emes`).
* `ExportScreenplayInvocation`: Exporter for screenplay invocation files (`*.invk`).
* `ExportScreenplayProgress`: Exporter for screenplay progress files (`*.pgrs`).
* `ExportScreenplayTask`: Exporter for screenplay progress files (`*.task`).
* [`ImportEventMessages`](projects/Gibbed.LetUsClingTogether.ImportEventMessages): Importer for event message files (`*.emes`)— incomplete, still needs work.

### Graphics

* [`ExportSprite`](projects/Gibbed.LetUsClingTogether.ExportSprite): Exporter for sprite files (`*.spr`, `*.ashg`)— incomplete, still needs work.

### Sounds

* [`ExportSound`](projects/Gibbed.LetUsClingTogether.ExportSound): Exporter for sound data (`*.scd`)— incomplete, still needs work.

# TODO

* Everything.

# Community

If you'd like to discuss these tools, modding LUCT, or LUCT in general, the author of **[the excellent One Vision mod](http://ngplus.net/index.php?/files/file/43-tactics-ogre-one-vision/)** (*raics*) has allowed the Discord channel for One Vision to be used for open discussion on these topics.

[On the NGPlus site](http://ngplus.net/), click the "Chat" link at the top of the site to get the Discord server invite link. The channel is **#one-vision** under the **Mods** category.
