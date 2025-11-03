# THE-RPG (Cyber Wuxia, 2D, Offline-AI Optional)

A 2D cyber–wuxia RPG. Rules-first design with an optional offline LLM for flavor/dialogue polishing. Windows first, Android next. Steam/itch.io buy-out; mobile IAP unlock for AI mode.

## Quick Start (Unity)

- Unity: open the project, target Windows.
- Events JSON folder (default): `Assets/Content/events`
- Editor menus:
  - `Tools/THE-RPG/Validate Events (Assets/Content/events)` – load & validate events, report duplicates and orphan next refs
  - `Tools/THE-RPG/Validate Events...` – pick any folder
  - `Tools/THE-RPG/Load Events To Runtime Cache (Assets/Content/events)` – preload events for Play Mode
  - `Tools/THE-RPG/Load Events To Runtime Cache...` – pick any folder

## Runtime Entrypoints

- `TheRPG.Narrative.NarrativeRuntime.Graph` – in-memory events graph
- `TheRPG.Narrative.NarrativeRuntime.World` – default world state
- `PickEvent(category, world)` → event; `ResolveChoice(eventId, choiceId, world)` → effects + next

## Content Structure

- `Assets/Content/events/{category}/*.json`
  - category ∈ `town | wild | dungeon | faction | story`
  - schema aligns with `TheRPG.Content.NarrativeEvent` (see `Assets/Scripts/AI/Models.cs`)
- `Assets/Configs/feature_flags.json` – AI toggles & budgets
- `Assets/Configs/gameplay.json` – default stats/economy/factions (reference keys)

## Offline AI (Optional)

- Adapter interface: `TheRPG.AI.IAIAdapter`
- Default is `NullAIAdapter` (no dependency). You can integrate `llama.cpp` or ONNX separately.
- Rules text is the source of truth; AI output is cosmetic with strict time/token budgets.

## Build Notes

- Windows: standard Unity build. Steam/itch.io packaging external.
- Android: plan main APK + optional model pack via StreamingAssets/expansion (TBD).

## Mobile (Flutter)

- Location: `mobile/`
- Requirements: Flutter SDK installed locally (not part of repo)
- Quick run: `cd mobile && flutter run`

## Contributing

- Event JSONs should pass the Editor validator with no duplicates or orphan next refs.
- Keep keys consistent with `gameplay.json` (e.g., `player.stability`).

## License

TBD – choose a license before public release.
