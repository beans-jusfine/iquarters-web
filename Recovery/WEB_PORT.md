# Web port evidence

## Reuse

`Recovered/Web/IQuarters.Web.csproj` references its local Core project and compiles these sources from `Recovered/Presentation`:

- RecoveryViewController (game progression, physics orchestration, input)
- GameplayEffects and GameplayPresentation (score effects, sound rules, replay)
- AdaptivePresentation (HUD anchors and camera rules)
- MainMenuController (original menu animation and navigation)
- LegacyScene and LegacyAssetCache (hierarchy and Hermite animation sampling)
- GameStorage (same save format, browser storage adapter)

The browser compilation branches select a browser geometry handle and prepare scene data on the browser thread. Presentation sources were copied from the iOS reconstruction; this repository builds independently.

## Browser replacements

- Scene graph transforms: System.Numerics, with the same handedness conversion.
- Rendering: pinned Three.js r160, explicit WebGL 1 context, original meshes/textures.
- Lighting: original lights, masks, transforms, material emission and glass treatment translated to WebGL.
- Audio: predecoded Web Audio buffers and independent playback channels.
- Input: pointer/coalesced event timestamps into TimedFlickGesture.
- Persistence: localStorage using the original C# save serialization.
- Timing: requestAnimationFrame with the same bounded delta and fixed-step C# physics.

## Renderer repairs during initial testing

- Corrected texture upload orientation.
- Centered the portrait playfield on wide screens so off-screen menu animation panels stay outside the playfield.
- Separated Lambert diffuse and Phong specular materials; the table must not acquire its unused blue specular color.
- Applied original light masks to each material's illumination.
- Removed Three.js's default spotlight position offset; recovered transforms supply the position.
- Disabled depth writes for glass, additive glow, and transparent shadow planes; discard fully transparent texels.
- Used an sRGB texture/output pipeline.
- Sent changed scene nodes rather than serializing all nodes on every frame.

## Checks and limits

The complete 76-check native C# suite passes with recovered collider fixtures. iOS shared source still compiles with zero warnings/errors. Browser verification is available at `?verify=1` and executes the 61 focused checks in WebAssembly (the exhaustive collider sweep runs natively). All 61 focused checks passed in the desktop browser WebAssembly runtime. A normal launch does not run tests.

Menus and gameplay have been inspected in the desktop browser. This does not establish performance or compatibility on physical iPhones, iPads, or Android devices, nor pixel-identical SceneKit/WebGL lighting. The original reconstruction's documented fidelity gaps remain applicable.
