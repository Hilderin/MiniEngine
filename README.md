# MiniEngine

> ⚠️ **Project Status: Archived / Discontinued**
>
> This project is **no longer maintained**. It was a personal learning project, not a finished product.
> Many features are incomplete, rough around the edges, and some parts (e.g. lighting) are not fully wired up.
> The code is published as-is for reference.

MiniEngine is a **C# 3D rendering engine** built on **Vulkan 1.2**, created as a learning experiment to explore:

- The inner workings of a modern GPU-driven renderer
- Vulkan API, bindless rendering, and compute shaders
- Meshlet-based GPU culling and indirect draw
- 3D engine architecture and component-based design
- Asset pipelines with hot-reloading

## Features

- **Vulkan 1.2 Renderer** — bindless rendering (`VK_EXT_descriptor_indexing`), 3 dedicated queues (graphics, transfer, compute), secondary command buffers, indirect draw with `DrawIndirectCount`
- **Meshlet-Based GPU Culling** — compute shader performing frustum culling per meshlet using subgroups (`GL_KHR_shader_subgroup_ballot`), dynamically updating indirect draw calls on the GPU
- **Custom Meshlet Generator** — C# reimplementation of [meshoptimizer](https://github.com/zeux/meshoptimizer)'s meshlet generation algorithm
- **Bindless Textures** — `sampler2D[]` arrays in fragment shaders using `GL_EXT_nonuniform_qualifier`
- **Dear ImGui Integration** — full ImGui.NET overlay for debug UI and profiler
- **Component-Based Game Objects** — `GameObject` with composable `GameComponent` children, transform hierarchy, scene graph
- **Asset System with Hot-Reload** — file watching, YAML-based asset definitions, hot-reloading of materials and shaders
- **Multiple 3D Format Support** — OBJ, FBX, DAE, GLB/GLTF via AssimpNet
- **Built-in Profiler** — hierarchical frame profiler with FPS counter, step timing, and debug overlay
- **Headless Mode** — renderer can operate without a window for automated/testing scenarios
- **Screenshot Support** — render framebuffer to image via SixLabors.ImageSharp

## Architecture

```
MiniEngine.Labs.Renderer     # WinForms test harness
       |
MiniEngine.Presentations.Glfw # IWindow via GLFW
       |
MiniEngine                   # GameObject, Scene, Components
       |
MiniEngine.Core              # Math, Assets, Shaders, Camera, Materials, Mesh
      / \
     /   \
MiniEngine.Rendering.Vulkan  # High-level Vulkan renderer (bindless, culling, ImGui)
    |
MiniEngine.Drivers.Vulkan    # Low-level Vulkan P/Invoke bindings

MiniEngine.Drivers.Glfw      # GLFW P/Invoke bindings
```

### Projects

| Project | Description |
|---------|-------------|
| `MiniEngine.Core` | Foundation library — math types, abstract interfaces, asset management, mesh optimization, primitive generators, shader source files (GLSL) |
| `MiniEngine` | Game object model — `GameObject`, `Scene`, `CameraObject`, `MeshObject`, components (`BasicMovementComponent`, `CameraComponent`) |
| `MiniEngine.Drivers.Vulkan` | Complete Vulkan P/Invoke wrapper — instance, device, queues, command buffers, swapchain, memory management, SPIRV compilation |
| `MiniEngine.Rendering.Vulkan` | High-level Vulkan renderer — bindless descriptors, meshlet GPU culling compute pass, ImGui overlay, pipeline caching |
| `MiniEngine.Drivers.Glfw` | GLFW3 P/Invoke wrapper — window creation, keyboard/mouse input, Vulkan surface creation |
| `MiniEngine.Presentations.Glfw` | `IWindow` implementation wrapping GLFW |
| `MiniEngine.Labs` | Console experiments — SPIRV parser, compute shaders, struct layouts |
| `MiniEngine.Labs.Renderer` | WinForms test harness hosting Vulkan rendering (triangle, cube, model, meshlets, bindless, ImGui tests) |
| `MiniEngine.Samples.ShipDefense` | Minimal game sample — scene setup, camera movement, cube mesh, material hot-reload |
| `MiniEngine.Tests` | MSTest unit tests |

## Technologies

| Technology | Usage |
|------------|-------|
| **C# / .NET 7.0** | All source code |
| **Vulkan 1.2** | GPU rendering via P/Invoke |
| **GLFW** | Window creation and input |
| **Dear ImGui (ImGui.NET)** | Debug GUI overlay |
| **AssimpNet** | 3D model loading |
| **SixLabors.ImageSharp** | Texture loading / image processing |
| **YamlDotNet** | Asset definition files |
| **shaderc** (via `glslangValidator`) | GLSL → SPIRV compilation |

## Build & Run

### Prerequisites

- [.NET 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- [Vulkan SDK](https://vulkan.lunarg.com/) (runtime + `glslangValidator`)
- Windows (some projects, like `MiniEngine.Labs.Renderer`, are Windows-only WinForms apps)

### Build

```bash
cd CSharp
dotnet restore
dotnet build MiniEngine.sln
```

Or open `CSharp/MiniEngine.sln` in Visual Studio 2022 or JetBrains Rider.

### Run the test harness

```bash
dotnet run --project CSharp/MiniEngine.Labs.Renderer
```

### Run tests

```bash
dotnet test CSharp/MiniEngine.Tests
```

## What I Learned

This was my first deep dive into:

- **Vulkan API** — writing a full Vulkan renderer from scratch using P/Invoke, managing memory, queues, synchronization, and pipeline barriers
- **Bindless Rendering** — using `VK_EXT_descriptor_indexing` to avoid descriptor set binding overhead
- **GPU-Driven Rendering** — compute shader culling with indirect draw, subgroup operations for efficient meshlet culling
- **Meshlet Generation** — implementing the mesh shader pipeline's meshlet generation step in C#
- **3D Engine Architecture** — designing a modular, layered engine with clean separation between core, rendering, and presentation layers
- **Asset Pipelines** — building an asset system with YAML definitions, multiple URI schemes, file watching, and hot-reloading

## Limitations / Known Issues

- Lighting is defined in code but not fully wired up in shaders — the base pipeline is unlit-only
- The test harness is Windows-only (WinForms dependency), though the engine libraries are cross-platform
- Many features are prototyped and not production-ready
- No mesh shader extension (`VK_NV_mesh_shader` / `VK_EXT_mesh_shader`) — meshlet culling is done in a compute shader instead
- `GetFramebufferRGBA()` throws `NotImplementedException`
- Some TODOs and rough edges remain throughout the codebase

## License

This project is provided for educational reference. See the [LICENSE](LICENSE) file for details (if present), otherwise all rights reserved.

---

*Built with curiosity and C#.*
