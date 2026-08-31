---
name: update-documentation
description: Update All documentation (CHANGELOG, README, and ROADMAP) based on git diff for a specific version
---

# Update Documentation (Changelog, README, and Roadmap) Skill

This skill automates the process of updating project documentation based on git changes for a specific version release.

## When to Use

- After completing a set of changes for a new version
- When preparing release documentation
- When you need to document what changed in a version
- When User inputs "Update Documentation for vX.Y.Z"

## Workflow

### 1. Get Git Diff
```bash
git diff --name-only
git diff -- 'Assets/scripts/**' 'Assets/data/**' 'Assets/*.unity' 'CHANGELOG.md' 'README.md' 'ROADMAP.md' 'TODO.md'
```

### 2. Analyze Changes
Categorize changes into:
- **Added**: New interfaces, classes, fields, methods, files
- **Changed**: Modified behavior, refactored code, renamed things
- **Fixed**: Bug fixes, null-safety, logic corrections
- **Removed**: Deleted code, files, features
- **Updated**: Modified documentation, comments, or other non-code elements

### 3. Update CHANGELOG.md
Add a new version section at the top (after the header) in Keep a Changelog format, implementing whichever of the following categories apply, with UPDATE NAME only being included if there are notable changes to highlight:
```markdown
## [vX.Y.Z] - YYYY-MM-DD [- UPDATE NAME]

### Added
- **`New Thing`** — description of what it does

### Changed
- **`Class`** — what changed and why

### Fixed
- **Issue description** — what was fixed

### Removed
- **`Old Thing`** — what was removed and why

### Updated
- **`Class`** — what was updated and why
```

### 4. Update README.md
Modify the **Project Structure** section to reflect:
- New interfaces in Core/
- New implementations (e.g., "implements IAnnouncer")
- Removed/renamed components

### 5. Update ROADMAP.md
Add a new entry at the top of the "Pre [vX.Y.0]" section:
```markdown
- [vX.Y.Z] **Short Title**: Brief description of key changes; list major components affected
```

### 6. Update TODO.md
Remove any entries that have been completed in this version

### 7. Update GAME.md
Add any new attacks, player upgrades, etc. that have been added in this version, and remove any that have been removed, and update any that have been changed. Follow the existing format.

## Rules

- Only document *notable* changes (skip graphify-out/, .VSCodeCounter/, manifest files)
- Follow existing formatting/style in each file
- Use backticks for code symbols (`ClassName`, `methodName`, `fieldName`)
- Keep entries concise but informative
- Date format: YYYY-MM-DD

## Example Prompt
> "Update Documentation for v0.2.16"