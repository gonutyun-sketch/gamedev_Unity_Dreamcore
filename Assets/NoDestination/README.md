# NO DESTINATION — THE FIELD prototype

Unity 6000.3.9f1 / URP 17.3.0. First playable vertical slice, not the full six-station game.

## Play
Open `Assets/NoDestination/Scenes/NoDestination.unity` and press Play, or use Unity's **NO DESTINATION → Play Prototype** menu. The environment is generated on entering Play Mode. The original SampleScene and project settings are preserved.

- WASD: walk; mouse: look; left Shift: walk faster; E: examine/interact; Escape: pause, resume or restart.
- Wait about 11 seconds for the arrival announcement. Walk to the front vestibule and press E.
- Follow the path across the wheat to the house. Examine the tabletop radio and the photograph on the back wall.
- Turn around: a platform has appeared immediately outside the house. Approach its green doorway and press E to return.
- The suitcase is missing and a new seat obstructs the aisle. This is the end of this first slice; the other stations and endings are not implemented.

## Implementation
`Scripts/Prototype.cs`: deterministic procedural environment, first-person CharacterController, center-screen raycast interaction, station state, two memory objects, anomaly, train mutation, synthesized ambience, minimal pause menu.
`Editor/PrototypeWorkshop.cs`: isolated scene creation, play command, Windows build, and local file-triggered verification workflow. Builds use only the prototype scene and do not replace the project's build settings.

All geometry, low-resolution noise materials and sounds are generated locally. No purchased or downloaded assets. Spoken announcements currently use subtitles and synthesized static/horn; no recorded voice acting. The slice has no save system, complete settings menu, remaining stations, or branching endings. Graphics are a blockout art pass and require further art development.

## Verification
`Artifacts/verification.txt` records runtime assertions if successful. Five `Artifacts/*.png` images are captured from the actual Unity game render. The verification coroutine follows the loop programmatically; this is not proof of a full manual keyboard/mouse playthrough. Windows Computer Use screenshot capture failed with `SetIsBorderRequired` / `0x80004002`; accessibility and Unity Refresh keyboard command worked.

## Windows build status
The Windows build was attempted and failed (`Artifacts/build-result.txt`: 80 errors). Unity's installed WindowsStandaloneSupport/Variations/mono/Managed assemblies produce BadImageFormatException in MovedFromExtractor. The installed UnityEngine.CoreModule.dll begins with zero bytes instead of a valid PE header. This is outside the project source. No installation files were modified. Play Mode is usable; a standalone executable is not delivered. Repair the Unity 6000.3.9f1 Windows build support installation before retrying.

Latest successful runtime run also checked CharacterController traversal through the carriage aisle and along the field path through the house doorway. The final subsequent edits only corrected the two-sided station label and restricted automatic verification to the editor.
