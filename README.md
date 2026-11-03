# iQuarters web

**The 2010 coin-tossing game, playable in your browser. C# gameplay, WebAssembly, and WebGL — no Unity.**

[**Play iQuarters**](https://strawberrypoptarts.github.io/iQuarters-web/)

Flick a quarter, bounce it across the table, and land it in a glass. This independent web port includes the recovered assets, C# physics, menus, scoring, round progression, animations, replay, and save logic.

## Playing

- Press **Play** to enable browser audio, then use the original game menus.
- Flick with a mouse or touch; swipe strength uses event timestamps.
- Games and high scores are saved in this browser. Clearing site data clears them.
- Wide screens use a centered portrait playfield.

## Build locally

Install **.NET SDK 10.0.401** and **Python 3**, then run from this repository:

```sh
dotnet workload install wasm-tools
dotnet run --project Recovered/Verification -c Release
dotnet publish Recovered/Web -c Release -p:UseSharedCompilation=false
python3 Recovery/tools/package_web.py
python3 -m http.server 8077 --directory dist/web
```

Open http://localhost:8077/. The output in `dist/web` is a static site; no application server is needed. GitHub Actions builds, tests, and deploys it to Pages on source changes to `main`. Select **GitHub Actions** in the repository's Pages settings.

## Source layout

| Directory | Contents |
| --- | --- |
| `Recovered/Core` | C# physics, input, scoring, and game rules |
| `Recovered/Presentation` | C# menus, effects, camera, replay, and animation logic |
| `Recovered/Web` | Browser adapters and WebGL renderer |
| `Recovered/Verification` | C# verification suite and recovered collider checks |
| `Recovery/converted` | Meshes, textures, audio, animation curves, and scene data |
| `Recovery/tools` | Static-site packaging |

Everything needed to build is in this repository; no sibling checkout, submodule, or original IPA is required. The source was separated from [iQuarters](https://github.com/strawberrypoptarts/iQuarters) at commit `0147cdc94f5226cf2b95d8746f57038a37708f51`. The copies now evolve independently; gameplay fixes must be deliberately kept in sync.

## Compatibility and accuracy

The renderer uses **WebGL 1 (OpenGL ES 2.0)** through vendored Three.js r160. A browser with WebAssembly and a working WebGL driver is required. WebGPU, threads, SIMD, and cross-origin isolation are not required.

The native C# suite contains 76 checks. Adding `?verify=1` to the game URL runs 61 focused checks inside WebAssembly; the exhaustive collider sweeps run natively. Verification saves use separate storage.

The gameplay and recovered assets come from the iOS reconstruction, but browser lighting and transparency are still being refined. Pixel-identical rendering and compatibility across all physical devices have not been established. [Implementation notes](Recovery/WEB_PORT.md) · [Build and browser details](Recovered/Web/README.md).

## Native iOS version

The iPhone/iPad app and IPA releases remain in [strawberrypoptarts/iQuarters](https://github.com/strawberrypoptarts/iQuarters). This repository is the standalone browser version.

## Third-party code

Three.js r160 is included with its [MIT license](Recovered/Web/wwwroot/vendor/THREE-LICENSE.txt). That license applies to Three.js, not automatically to the game or its recovered assets.
