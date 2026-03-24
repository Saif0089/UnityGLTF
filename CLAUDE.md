# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity project extending **UnityGLTF** (Khronos Group's glTF 2.0 library) with **trace-viewer** parity — .trace export/import, behavior graph authoring via Visual Scripting, and runtime Trace components. The main package is at `Packages/UnityGLTF/`.

Related repositories:
- **trace-viewer**: `/Users/hassan/Documents/GitHub/trace-viewer` — Babylon.js-based scene viewer (TypeScript)
- **trace-sandbox**: `Packages/trace-sandbox` — Editor UI for trace-viewer (React/TypeScript)

## Architecture

### Two Export Paths

1. **GLB export** (standard UnityGLTF) → KHR_interactivity extension → Babylon's native parser handles standard ops only (`animation/start`, `event/onSelect`, `flow/branch`, etc.)
2. **.trace export** (our addition) → ZIP containing `scene.json` + `assets/*.glb` → trace-viewer's `GraphCompiler` handles ALL ops including `trace/` extensions

Trace/ ops (`trace/setActive`, `trace/playAnimation`, `pages/next`, etc.) only work through .trace export, NOT through GLB's KHR_interactivity.

### Assembly Structure

Each assembly is a separate compilation unit. Cross-assembly references must go through `.asmdef` references:

- **UnityGLTFScripts** — Core runtime (import/export). Uses `overrideReferences: true` with `Newtonsoft.Json.dll`.
- **UnityGLTF.Interactivity.Runtime** — KHR_interactivity schema nodes and export context
- **UnityGLTF.Interactivity.VisualScripting.Runtime** — VS units live here. Has a `Units/Unity.VisualScripting.asmref` that compiles units INTO the `Unity.VisualScripting.Flow` assembly (so they appear in VS node database)
- **UnityGLTF.Trace.Runtime** — Trace MonoBehaviour components (TraceInteractions, TracePages, etc.)
- **UnityGLTF.Trace.Editor** — .trace export/import, graph serializer, node registration
- **UnityGLTF.Interactivity.VisualScripting.Export.Editor** — VS unit → KHR_interactivity node exporters

**Critical**: VS units in `Units/` compile into `Unity.VisualScripting.Flow` via `.asmref`, NOT into `UnityGLTF.Interactivity.VisualScripting.Runtime`. This means they **cannot reference** `UnityGLTF.Trace.Runtime` types directly — use `GetComponent("TracePages")` by string + reflection instead.

### Three Layers Per Graph Op

Each behavior graph operation has three parts:

1. **Schema Node** (`Runtime/Scripts/Interactivity/Schema/Nodes/Trace/`) — Declares op string, configs, sockets via attributes. Example: `Trace_PlayAnimationNode.cs` with `Op = "trace/playAnimation"`.
2. **VS Unit** (`Runtime/Scripts/Interactivity/VisualScripting/Units/`) — The node users place in Script Graphs. Must have `[IncludeInSettings(true)]` and `[UnitCategory("Trace/...")]` with **forward slashes** (not backslashes).
3. **Unit Exporter** (`Editor/Scripts/Interactivity/VisualScriptingExport/UnitExporters/Trace/`) — Bridges VS unit to schema node during export. Registered via `[InitializeOnLoadMethod]`.

### .trace Export Pipeline

`TraceSceneExporter.Export()`:
1. Temporarily disables KHR_interactivity plugin
2. Resets root transforms to identity → exports GLB (mesh at origin) → restores transforms
3. Runs VS export pipeline → produces GraphData JSON via `TraceGraphDataSerializer`
4. Builds `scene.json` with behaviors (Model, Animation, BoxCollider, BehaviorGraph, etc.)
5. Bundles into ZIP: `scene.json` + `assets/*.glb`

The `TraceGraphDataSerializer` strips unsupported ops (`pointer/get`), remaps node indices, and outputs trace-viewer's flat format (`{types, declarations, variables, events, nodes}`).

### Animation Playback

`TracePlayAnimationUnit` snapshots all descendant transforms on first play and restores them on re-trigger. This is necessary because Unity's legacy `Animation` component leaves objects in their end state after `WrapMode.Once` completes (e.g. scale→0), and `anim.Sample()`/`anim.Rewind()` don't reliably reset child object transforms. The snapshot approach guarantees correct reset regardless of clip structure.

The `done` flow output fires when a non-looping clip finishes (checked via `anim.IsPlaying(clipName)` with a 3-frame grace period after `Play()` since Unity reports `isPlaying=false` on the same frame). A generation counter prevents stale callbacks from firing on re-trigger.

### Hover Export Split

`TraceOnHoverUnit` is a single VS node with `enter`/`exit` flow outputs (better UX). The exporter (`TraceOnHoverUnitExport`) emits TWO separate schema nodes (`event/onHoverIn` + `event/onHoverOut`) to match trace-viewer's format.

### Node Registration

New VS units must be registered in `TraceNodeRegistration.cs` which programmatically adds types to `BoltCore.Configuration.typeOptions` on domain reload. Without this, nodes won't appear in the Script Graph fuzzy finder. After adding new units, users must run **Tools → Trace → Force Register All Trace Nodes** then **Regenerate Nodes**.

## Current Op Strings (must match trace-viewer's GraphOps)

Events: `event/onSelect`, `event/onHoverIn`, `event/onHoverOut`, `trace/onProximity`, `event/onStart`
Actions: `trace/setActive`, `trace/getActive`, `trace/setActiveChild`, `trace/getChildCount`, `trace/navigate`, `trace/playVideo`, `trace/getProperty`, `trace/setProperty`
Animation: `trace/playAnimation`, `trace/playAnimationExternal`
Pages: `pages/next`, `pages/prev`, `pages/getActiveIndex`, `pages/getPageCount`
Flow: `flow/branch`, `flow/sequence`
Math: `math/not`, `math/and`, `math/or`, `math/eq`, `math/gt`, `math/ge`, `math/lt`, `math/add`, `math/sub`, `math/mul`, `math/div`, `math/select`, `math/floor`, `math/random`, `math/combine3`
Variables: `variable/get`, `variable/set`

## Testing

Unity Test Runner (NUnit):
- **Runtime tests**: `Tests/Runtime/` — `GLTFRootTests.cs`
- **Editor tests**: `Tests/Editor/` — `AssetImportTests.cs`
- **Interactivity tests**: `Tests/Editor/Interactivity/`

Run via Unity: **Window → General → Test Runner** or CLI: `unity -runTests -testPlatform EditMode`

## Key Conventions

- Schema node op strings must exactly match trace-viewer's `GraphOps` constants in `behaviorgraphdata.ts`
- VS unit categories use forward slashes: `[UnitCategory("Trace/Events")]` not `"Trace\\Events"`
- Event units extending `EventUnit<T>` inherit `[SpecialUnit]` which hides them from category browsing — use plain `Unit` + `IGraphEventListener` instead
- The `InteractivityUnitAnalyzer` warns on nodes without KHR exporters — Trace nodes are exempted via `IsTraceUnit()` check
- VS units cannot reference `UnityGLTF.Trace.Runtime` types — use `GetComponent("TypeName")` by string + reflection, or `SendMessage("MethodName")` for actions
- Do not include Co-Authored-By lines in git commits
- All new VS units must be added to `TraceNodeRegistration.cs` type array AND have `[IncludeInSettings(true)]` attribute
- Animation clips that modify transforms (scale, position) must have keyframes at frame 0 with initial values for proper reset behavior
