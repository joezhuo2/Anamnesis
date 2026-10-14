# Save System: Editor Wiring & Playtest

All scripts live in `Assets/scripts/Save/` (assembly `CrystalFlux.Save`). The bridge to Misc types is `Assets/scripts/Misc/SaveBridge.cs` and needs no wiring.

## 1. Asset catalog

- Run **Tools > Anamnesis > Rebuild Save Catalog** after adding or renaming attacks, awakenings, contracts/anomalies, difficulties, modes or wave sequences. Commit `Assets/Resources/SaveAssetCatalog.asset`.
- Player builds rebuild it automatically (build preprocessor).
- If an asset that a save references is gone from the catalog, that save shows as **Incompatible** and cannot be loaded.

## 2. Scene `Assets/New.unity`

### Controller
- Add an empty GameObject `SaveSystem` with **RunSaveController**. Exactly one per scene.

### Overlay canvas
Put the three panels below on a Canvas that draws above the Escape menu and death screen (higher `Sort Order`, or last in hierarchy). Each panel's GameObject starts active. The scripts hide themselves in `Awake`.

**ConfirmDialogUI**
- `root`: the panel object
- `messageText`: TMP text
- `yesButton`, `noButton`

**SaveNamePromptUI**
- `root`, `titleText`
- `input`: TMP_InputField (character limit is set from code, 40)
- `errorText`: TMP text, empty by default
- `confirmButton`, `cancelButton`

**SaveListPanelUI**
- `root`, `titleText`
- `buildsTab`, `runsTab`: Buttons. `buildsTabHighlight`, `runsTabHighlight`: Images that are enabled on the active tab
- `searchField`: optional TMP_InputField. If left empty, one is created at runtime at the top of the panel (`searchFieldSize` / `searchFieldOffset`)
- `sortDropdown`: TMP_Dropdown. Options are filled from code: Date, Wave, Highest DPS, Name
- `listContent`: the Content of a ScrollRect, with VerticalLayoutGroup + ContentSizeFitter (vertical: Preferred)
- `rowPrefab`: see below
- `emptyText`: shown when the list is empty
- `loadButton`, `renameButton`, `deleteButton`, `closeButton`

### Row prefab
A Button GameObject with:
- **SaveRowUI**: `nameText`, `waveText`, `dateText`, `badgeText` (shows "Incompatible" / "Read only"), `highlight` (Image, enabled when selected), `button`, `tooltipOffset`
- **TooltipTrigger**: the existing hover tooltip. The row fills it with wave, mode/difficulty, highest DPS, attacks, playtime and date

### Home screen
- **Load Build** button: add **SaveMenuButtons**, `action = LoadBuild`
- **Load Run** button: add **SaveMenuButtons**, `action = LoadRun`
- Both hide when a wave starts. Set `hideRoot` to hide a wrapper object instead.
- **LoadedBuildLabelUI** (optional): `root` (wrapper shown while a build is queued), `label`, `clearButton`. A queued build is applied when the player presses Regular or Unlimited.

### Escape menu (SettingsPanelUI children)
- **Save Progress** button: **SaveMenuButtons**, `action = SaveProgress`. It is non-interactable before the run starts, while a reward/anomaly/contract/synergy pick is open, and after death. Saving returns to the home screen.
- **Save Build** button: **SaveMenuButtons**, `action = SaveBuild`. Mid-wave it saves the build from the start of the current wave (waves chain with no idle gap). After saving, the label shows "Build Saved" until the next wave.

### Death screen (DeathScreenUI panel)
- **Save Build** button: **SaveMenuButtons**, `action = SaveBuild`.

Escape closes the confirm dialog first, then the name prompt, then the save list (via `UIModal`). Typing in the name or search field doesn't trigger the skill-tree hotkey.

## 3. Save files

`%USERPROFILE%/AppData/LocalLow/<Company>/<Product>/saves/{builds|runs}/<id>.json`, one file per save. Loading a run deletes its file. If the restore fails, the file is written back.

## 4. Playtest checklist

- [ ] Rebuild catalog; no console errors on Play
- [ ] Start Regular run, clear 2 waves, open Escape during wave 3: Save Progress enabled; save as "A" → returns to home
- [ ] Load Run → list shows "A" with hover info (wave, mode/difficulty, DPS, attacks, playtime, date) → Load → confirm → run starts at the saved wave
- [ ] After loading: stats, gold, level/xp, attacks, awakenings, skill tree nodes + unspent points, synergies, contract all match pre-save. "A" is gone from the list
- [ ] Mid-wave Save Progress → loads at the start of that wave with the state from wave start
- [ ] With a reward/anomaly/contract/synergy panel open, Save Progress is disabled
- [ ] Save Progress again in a loaded run: name is prefilled; confirm triggers the overwrite prompt
- [ ] Die → death screen Save Build "B" → home → Load Build → pick "B" → label shows → choose difficulty/mode → Regular → wave 1 with full build, no pre-run picks, contract kept, owned rares/awakenings not offered
- [ ] Escape Save Build mid-wave saves the wave-start build (gold/xp gained this wave not included)
- [ ] Unlock a skill node on the home screen, then Load Run: that node is not unlocked after load
- [ ] Hold GoldBuff with gold, save/load twice: its stat bonus does not grow
- [ ] Ironman run: Save Build → shows "Read only", Load disabled. Save Progress in ironman loads and stays ironman
- [ ] Unlimited mode: Save Progress + Load Run resumes in Unlimited at the right wave
- [ ] List: tab switch, search (name / attack / mode / difficulty, multi-word), sort (Date/Wave/DPS/Name), rename (incl. to an existing name → overwrite prompt), delete (confirm)
- [ ] Edit a save JSON and change an id in `referencedIds` → shows Incompatible with reason, Load disabled, hover still works
- [ ] Put a garbage `.json` in the saves folder → list still opens
- [ ] Escape closes dialog → prompt → list in that order; typing a name doesn't open the skill tree
