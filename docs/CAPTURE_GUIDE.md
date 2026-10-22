# Capture guide — screenshots and GIF

The repository describes combos, juggling and a style meter, but a reader cannot
see any of it. Combat feel is the whole pitch here, so this needs a moving image.

## Setup

1. Open the project in the editor version recorded in
   `ProjectSettings/ProjectVersion.txt`.
2. Run **Tools → Hack n Slash → Build Demo Scene**, then enter Play mode.
3. Set the Game view to **16:9**, 1600×900, gizmos off.
4. Lock onto an enemy before capturing so the camera framing is stable.

## Required shots

| File | Frame |
| --- | --- |
| `docs/screenshots/01-combo.png` | Mid ground combo with the style meter partially filled |
| `docs/screenshots/02-aerial.png` | Launcher into aerial juggling, enemy airborne |
| `docs/screenshots/03-lockon.png` | Lock-on reticle, floating damage numbers and hitstop frame |
| `docs/screenshots/04-boss.png` | Boss encounter with the wave counter visible |

## GIF (10–12 s)

- Sequence: lock-on → three-hit combo → launcher → two aerial hits → dodge.
- 1280×720, 15 fps, under 5 MB. Save as `docs/screenshots/combat.gif`.

## Paste into the README

Right after the intro paragraph:

```markdown
![Stylish Combat gameplay](docs/screenshots/combat.gif)

| Ground combo | Aerial juggle | Lock-on | Boss |
| --- | --- | --- | --- |
| ![](docs/screenshots/01-combo.png) | ![](docs/screenshots/02-aerial.png) | ![](docs/screenshots/03-lockon.png) | ![](docs/screenshots/04-boss.png) |
```

## Honesty rule

Caption the editor version and commit the frames came from. Do not composite
floating damage numbers or retouch the style meter.
