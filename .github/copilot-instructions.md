## Workflow
- Do NOT invent non-existent Unity/C# APIs. Write new project methods freely following existing patterns.
- Prioritize modifying existing code over creating new classes/methods.
- NEVER destroy base GameObjects or ScriptableObjects.
- Do not write comments on my code. Instead write in the output or add a note at the end of TODO.md

## Code & Architecture
- Abbreviate long local vars/params (e.g., `EntityStatManager` → `esm`).
- **Pooling:** Fully Reset state in `Setup()`.
- **Pause:** Early-return if `Time.timeScale == 0f` in Player/Enemy AI update loops.
- **Safety:** Prefer `TryGet`, null checks, and early returns.

## Output Format
- Output code ONLY. No intro/outro fluff or spec restating.
- Max 2 bullet points of commentary for breaking/non-obvious logic.
