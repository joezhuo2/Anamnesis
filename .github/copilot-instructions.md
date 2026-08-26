## Project Context & Tools
- Unity 6 (6000.4.6f1), URP 2D, New Input System ONLY.
- Main Scene: `Assets/New.unity` (`SampleScene` is unused placeholder).
- `WaveManager` is a scene GameObject, NOT a prefab.
- Use `/graphify-out` knowledge graph for file dependency & architectural lookup.
- Use `/memories/repo/anamnesis.md` history for past debugging and decision context.

## Workflow
- Read context first; ALWAYS clarify ambiguity and by creating a multi-step checklist before coding.
- Do NOT invent non-existent Unity/C# APIs. Write new project methods freely following existing patterns.
- Prioritize modifying existing code over creating new classes/methods.

## Code & Architecture
- Abbreviate long local vars/params (e.g., `EntityStatManager` → `esm`).
- **Pooling:** Fully Reset state in `Setup()`.
- **Pause:** Early-return if `Time.timeScale == 0f` in Player/Enemy AI update loops.
- **Safety:** Prefer `TryGet`, null checks, and early returns.

## Output Format
- Output code ONLY. No intro/outro fluff or spec restating.
- Max 2 bullet points of commentary for breaking/non-obvious logic.

## Documentation
- Auto-update docs on verified completion.
- **CHANGELOG.md:** `[VERYMAJOR].[MAJOR].[MINOR]_[PATCH] - YYYY-MM-DD [- UPDATENAME]`. Include `### Highlights` on MAJOR patches.
- **ROADMAP.md:** Add significant MINOR patches with brief descriptions.