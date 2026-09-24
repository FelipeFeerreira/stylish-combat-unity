# Stylish Combat

A Unity action-combat prototype exploring responsive third-person movement, combo chains and score-driven combat.

**Stack:** Unity 6000.3.24f1 · C# · editor automation

## Implemented systems

- Three-hit ground combos, launchers and aerial juggling.\n- Dodge invulnerability, target lock-on and a ranged attack.\n- Hitstop, camera feedback and floating damage numbers.\n- A style meter, enemy waves and a boss encounter.\n- Procedural character visuals and a reusable demo scene builder.

## Open the project

1. Install the editor version recorded in `ProjectSettings/ProjectVersion.txt` through Unity Hub.
2. Add this repository as a project and let Unity restore the packages in `Packages/manifest.json`.
3. Use **Tools → Hack n Slash → Build Demo Scene** in the editor if you need to open or generate the demo scene.
4. Enter Play mode.

Keyboard controls include WASD movement, mouse camera, left-click attacks, right-click launcher, Shift dodge, Space jump, E ranged attack and Tab/Q lock-on. Check InputHub.cs for the implemented bindings.

Scene-building commands generate or replace scene content; use them on a fresh clone before customizing the levels.

## Code structure

Assets/Scripts/Core, Player, Enemy, Camera, Game and UI; Assets/Editor/HnSSceneBuilder.cs constructs the demo.

The repository includes Assets, Packages and ProjectSettings. Unity caches, local editor state and compiled builds are excluded; let the editor regenerate them.

## Project status

This is a development prototype and code portfolio, not a released game. The systems above were identified in the source; a fresh editor import, playthrough and platform build have not been verified during this publication pass. See [verification notes](docs/PORTFOLIO_VALIDATION.md). No performance or release-readiness claims are made.
