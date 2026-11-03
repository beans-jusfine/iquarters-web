# iQuarters for the web

A static WebAssembly + WebGL port for GitHub Pages. The browser compiles the existing C# core and compiles its own recovered gameplay, menu, animation, scoring, replay, layout, and save logic as source. `PlatformAdapter.cs` and `SceneAdapter.cs` implement the Apple API surface used by those files; they are browser adapters, not Apple frameworks.

The Three.js renderer is pinned to r160 with its MIT license, retaining WebGL 1 / OpenGL ES 2.0 compatibility. Rendering, Web Audio, pointer events, and localStorage are browser-specific. No server, Unity runtime, WebGPU, threads, SIMD, or cross-origin-isolation headers are required. WebAssembly support and a functioning WebGL driver are still required; “every device” is not a guaranteed compatibility claim.

## Build and run

From the repository root, using .NET 10.0.401 and Python 3:

```sh
dotnet workload install wasm-tools
dotnet run --project Recovered/Verification -c Release
dotnet publish Recovered/Web -c Release -p:UseSharedCompilation=false
python3 Recovery/tools/package_web.py
python3 -m http.server 8077 --directory dist/web
```

Open `http://localhost:8077/`. Start with the Play button to unlock browser audio. On wide screens, the portrait playfield is centered; on phones it fills the available width. Mouse and touch use the same timestamp-based C# flick logic.

`?verify=1` runs the 61 focused shared C# checks inside WebAssembly before offering Play. The exhaustive collider sweeps run in the 76-check native suite. Verification saves use a separate localStorage namespace.

## Deploy

`.github/workflows/pages.yml` builds and publishes the static site on pushes to main affecting the game. Configure repository Pages to use **GitHub Actions**. All asset paths are relative, including the `/iQuarters-web/` project-site prefix.

## Parity limits

Shared source keeps game decisions, physics, and recovered animation curves aligned. SceneKit and WebGL have different lighting and transparency implementations; pixel-identical rendering is not established. Physical-device browser testing is still needed, particularly older Safari and Android GPUs. Browser audio requires a user gesture, motion permission may be denied, and clearing site data clears web saves. Web saves are separate from the installed iOS app.
