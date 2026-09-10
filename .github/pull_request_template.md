## Summary

<!-- What changed and why. One or two sentences. -->

Closes #

## Changes

<!-- Bullet list of the notable changes. Call out anything non-obvious. -->

-

## Scene / asset impact

<!-- Unity work often touches things that do not review well as a diff. -->

- [ ] No prefabs, scenes, or ScriptableObjects were destroyed or recreated (only edited in place)
- [ ] `.meta` files for added/removed assets are included in this PR
- [ ] Serialized field changes are backwards compatible with existing assets, or a migration is described below

Assets touched:

## Testing

<!-- How this was verified. Unity version, scene played, what was observed. -->

- Unity version:
- Scenes / prefabs exercised:
- Result:

## Checklist

- [ ] Follows existing patterns rather than introducing new classes/methods where an existing one fits
- [ ] Pooled components fully reset their state in `Setup()`
- [ ] Update loops early-return when `Time.timeScale == 0f` (player/enemy AI)
- [ ] Null checks / `TryGet` / early returns used on external lookups
- [ ] `CHANGELOG.md` updated if this is user-visible

## Notes for reviewers

<!-- Anything breaking, risky, or worth a second pair of eyes. -->
