---
name: update-documentation
description: Update All documentation (CHANGELOG, README, and ROADMAP) based on git diff for a specific version
---

# Update Documentation (Changelog, README, and Roadmap, and Game Index) Skill

This skill automates the process of updating project documentation based on git changes for a specific version release.

## When to Use

- After completing a set of changes for a new version
- When preparing release documentation
- When you need to document what changed in a version
- When User inputs "Update Documentation for vX.Y.Z"

## Workflow

### 1. Get Git Diff
```bash
# Uncommitted changes:
git diff --name-only 
# OR changes between previous release tag and HEAD:
git diff PREVIOUS_TAG..HEAD -- 'Assets/scripts/**' 'Assets/data/**'
git diff -- 'Assets/scripts/**' 'Assets/data/**' 'Assets/*.unity' 'CHANGELOG.md' 'README.md' 'ROADMAP.md' 'TODO.md'
```

### 2. Analyze Changes
Categorize changes into:
- **Added**: New interfaces, classes, fields, methods, files
- **Changed**: Modified behavior, refactored code, renamed things
- **Fixed**: Bug fixes, null-safety, logic corrections
- **Removed**: Deleted code, files, features
- **Updated**: Modified documentation, comments, or other non-code elements
- **Rebalance**: Adjusted values, mechanics, or systems to improve balance
- When writing entries - follow the existing format unless otherwise stated

### 3. Update CHANGELOG.md
Add a new version section at the top (after the header) in Keep a Changelog format, implementing whichever of the following categories apply, with UPDATE NAME only being included if there are notable changes to highlight:
```markdown
## [vVERYMAJOR.MAJOR.MINOR_PATCH] - YYYY-MM-DD [- UPDATE NAME]

### Highlights
highlights from the previous MAJOR version to the current version, including any notable changes, new features, or important fixes. only include this for VERYMAJOR and MAJOR versions. this should be listed before any of the following categories, and should be a summary of the most important changes.

### Added
- **`Thing`** — rough description for addition, and one line for each notable mention
  - feature: description

### Changed
- **`Thing`** — rough introduction for change, and one line for each notable mention
  - field: before → after (small note/description IF needed)

### Fixed
- **Issue description** — rough introduction for fix, and one line for each notable mention
  - feature: before → after (small note/description IF needed)

### Removed
- **`Old Thing`** — rough description for addition, and one line for each notable reason for removal
  - reason: description

### Updated
- **`Thing`** — rough description for update, and one line for each notable mention
  - feature: before → after (small note/description IF needed)

### Rebalance
- **`Thing`** — short description of what was rebalanced and why, create one entry per class that was rebalanced and one point per field changed
  - field: before → after (small note IF needed)
```

### 4. Update README.md
Modify the **Project Structure** section to reflect:
- New interfaces in Core/
- New implementations (e.g., "implements IAnnouncer")
- Removed/renamed components

### 5. Update ROADMAP.md
Add a new entry at the top of the "Pre [vX.Y.0]" section. These entries must be MINIMAL (similar size to existing entries), and should focus on introducing new content, and bug fixes ONLY if they are critical:
```markdown
- [vX.Y.Z] **Short Title**: Brief description of key changes; list major components affected
```

### 6. Update TODO.md
Remove any entries that have been completed in this version

### 7. Update GAME.md
Add any new attacks, player upgrades, etc. that have been added in this version, and remove any that have been removed. Also update any attack assets that were changed. Follow the existing format.

## Rules
- Only document *notable* changes (skip graphify-out/, .VSCodeCounter/, manifest files)
- Follow existing formatting/style in each file
- Use backticks for code symbols (`ClassName`, `methodName`, `fieldName`)
- Keep entries concise but informative
- Date format: YYYY-MM-DD

## Example Prompt
> "Update Documentation for v0.2.16"