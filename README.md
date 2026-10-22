# Stylish Combat — Unity Action Combat Prototype

A third-person action-combat prototype exploring responsive movement, combo
chains and score-driven encounters: a small character-action sandbox built end
to end in C#.

| | |
| --- | --- |
| **Engine** | Unity 6000.3.24f1 |
| **Language** | C# (runtime + editor tooling) |
| **Scope** | Combat systems prototype |
| **Status** | Development prototype, not a released game |

## What it demonstrates

- Three-hit ground combos, launchers and aerial juggling.
- Dodge invulnerability, target lock-on and a ranged attack.
- Hitstop, camera feedback and floating damage numbers.
- A style meter scored by variety of attacks, plus enemy waves and a boss.
- Procedural character visuals and a reusable demo scene builder.

## Open the project

1. Install the editor version recorded in `ProjectSettings/ProjectVersion.txt`
   through Unity Hub.
2. Add this repository as a project and let Unity restore the packages listed in
   `Packages/manifest.json`.
3. Use **Tools → Hack n Slash → Build Demo Scene** in the editor to open or
   generate the demo scene.
4. Enter Play mode.

Scene-building commands generate or replace scene content. Run them on a fresh
clone before customizing the levels.

### Controls

| Action | Input |
| --- | --- |
| Move / camera | `WASD` / mouse |
| Light attack / launcher | Left click / right click |
| Dodge / jump | `Shift` / `Space` |
| Ranged attack | `E` |
| Lock-on | `Tab` / `Q` |

Implemented bindings are defined in `InputHub.cs`.

## Code structure

```
Assets/Scripts/   Core, Player, Enemy, Camera, Game, UI
Assets/Editor/    HnSSceneBuilder.cs builds the demo scene
```

The repository includes `Assets`, `Packages` and `ProjectSettings`. Unity caches,
local editor state and compiled builds are excluded and regenerate on import.

## Project status

This is a development prototype and code portfolio, not a released game. The
systems above were identified in the source; a fresh editor import, playthrough
and platform build have not been verified during this publication pass. See
[verification notes](docs/PORTFOLIO_VALIDATION.md). No performance or
release-readiness claims are made.

---

## License

MIT - see [LICENSE](LICENSE). You may use, modify and redistribute this code with attribution.
