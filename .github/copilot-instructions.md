## Project Context & Tools
- Unity 6 (6000.4.6f1), URP 2D, New Input System ONLY.
- Main Scene: `Assets/New.unity` (`SampleScene` is unused placeholder).
- `WaveManager` is a scene GameObject, NOT a prefab.
- Use `/graphify-out` knowledge graph for file dependency & architectural lookup.
- Use `/memories/repo/anamnesis.md` history for past debugging and decision context.

## Coding
- Abbreviate lengthy local variable/parameter names to acronyms (e.g., `EntityStatManager` → `esm`).
- Read context first. Stop and ask before implementation if instructions are ambiguous.
- Match existing codebase formatting and architectural conventions.
- Prioritize modifying existing code over adding new classes/methods.

## Architecture
- **Object Pooling:** Reset all state inside `Setup()` methods.
- **Pause Handling:** Check `Time.timeScale == 0f` and early-return in Player and Enemy AI update loops.
- **Safety**: Use TryGet over Get, use null checks and early returns when applicable

## Response Formatting
- Output code only. Skip conversational text and restating prompt specs.
- Limit commentary to maximum 2 bullet points for non-obvious logic or breaking changes.

## Documentation
- Auto-update docs when modifying files upon verified completion.
- **CHANGELOG.md:**
  - Header: `[VERYMAJOR].[MAJOR].[MINOR]_[PATCH] - YYYY-MM-DD [- UPDATENAME]`
  - `UPDATENAME` is optional; reserve for major milestone releases.
  - Include a `### Highlights` section summarizing key features on every MAJOR patch.
- **ROADMAP.md:**
  - Add significant MINOR patches with a brief description.