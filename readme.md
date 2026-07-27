# The Ultimate Lazy Utilities

> I compiled a bunch of utilities for Unity in this package so I don't have to keep rewriting them.

[![Unity](https://img.shields.io/badge/Unity-2020.1%2B-black?logo=unity)](https://unity.com/)
[![UPM](https://img.shields.io/badge/UPM-dev.theultimatelazy.utilities-blue)](https://docs.unity3d.com/Manual/upm-git.html)
[![Version](https://img.shields.io/badge/version-0.1.1-orange)](package.json)

A small, dependency-free Unity package with editor tooling and runtime bits I kept
copy-pasting between projects: a self-populating tools window, a `link.xml` generator,
Git helpers for the editor, a customizable Unity window title, and a couple of 2D
grayscale shaders for both the Built-in and Universal render pipelines.

---

## Installation

### Package Manager (Git URL)

`Window ▸ Package Manager ▸ + ▸ Add package from git URL...`

```
https://github.com/theultimatelazydev/TheUltimateLazy-Utilities.git
```

### manifest.json

```jsonc
{
  "dependencies": {
    "dev.theultimatelazy.utilities": "https://github.com/theultimatelazydev/TheUltimateLazy-Utilities.git"
  }
}
```

Pin to a specific commit or tag by appending `#<tag-or-sha>`.

**Requirements:** Unity **2020.1** or newer. No third-party dependencies.

---

## Quick start

1. Install the package.
2. Open `Tools ▸ The Ultimate Lazy Dev ▸ Ultimate Lazy Tools`.
3. Pick a tool from the sidebar.

Every tool in the package registers itself into that single window — no extra setup, no
menu-item sprawl.

---

## What's inside

| Tool | Where | What it does |
| --- | --- | --- |
| **Ultimate Lazy Tools window** | `Tools ▸ The Ultimate Lazy Dev ▸ Ultimate Lazy Tools` | Host window that auto-discovers tool tabs via reflection. Sidebar or toolbar layout, nested tabs, resizable divider. |
| **Editor Window Title Modifier** | `… ▸ Unity Editor ▸ Editor Window Title Modifier` | Rewrites the Unity main window title from a token template. Persisted in `EditorPrefs`. |
| **Link XML Editor** | `… ▸ Linker ▸ Link XML Editor` | Generates and maintains `Assets/link.xml` from assembly-name prefixes, with an ignore list. |
| **Git Log Commits** | `… ▸ Git ▸ Log Commits` | Logs every commit on the current branch since it diverged from its base branch. |
| **Git info API** | `UltimateLazy.Tools.Editor.GitUtils` | Branch name, commit hash, remote URL, arbitrary Git commands — from editor scripts. |
| **Grayscale 2D shaders** | `UltimateLazyDev/2D/...` | Sprite grayscale with an adjustable blend, in Built-in and URP variants, plus ready-made materials. |

---

## The dynamic tools window

The window doesn't know about its tools ahead of time. On enable it scans loaded
assemblies for implementations of `IUltimateLazyToolWindowTab` and builds the tab tree
from them. Drop a new class anywhere in your project and it shows up.

### Adding your own tab

```csharp
using UltimateLazy.Tools.Editor;
using UnityEditor;
using UnityEngine;

public class BuildNumberTab : IUltimateLazyToolWindowTab
{
    // Must match the target window's WindowName.
    public string WindowName => "The Ultimate Lazy Tools";

    public string TabName => "Build Number";

    // Optional: nest this tab under a parent node in the sidebar.
    public string ParentTabName => "Build";

    public void OnGUI()
    {
        EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);

        if (GUILayout.Button("Bump"))
        {
            PlayerSettings.bundleVersion = /* ... */ PlayerSettings.bundleVersion;
        }
    }
}
```

Implementations are instantiated with `Activator.CreateInstance`, so the class needs a
**public parameterless constructor**. Put the file in an editor-only assembly (or an
`Editor/` folder) since `OnGUI` uses `UnityEditor` APIs.

### Your own window

Subclass `UltimateLazyToolsWindowBase` to get a second, separately-populated window:

```csharp
public class MyToolsWindow : UltimateLazyToolsWindowBase
{
    protected override string WindowName => "My Tools";       // tabs match on this
    protected override WindowLayout Layout => WindowLayout.Sidebar; // or .Tabs
    protected override float MinWidth => 600f;
    protected override float MinHeight => 400f;

    [MenuItem("Tools/My Tools")]
    public static void ShowWindow() => GetWindow<MyToolsWindow>("My Tools");
}
```

| Member | Default | Notes |
| --- | --- | --- |
| `WindowName` | *(abstract)* | Tabs whose `WindowName` matches are collected here. |
| `Layout` | `WindowLayout.Tabs` | `Tabs` = horizontal toolbar, `Sidebar` = tree view. |
| `MinWidth` / `MinHeight` | `400` / `300` | Applied to `minSize` on enable. |

Call `RefreshTabs()` to rescan after a domain reload, or `ChangeTab("Tab Name")` to jump
to a tab programmatically.

---

## Editor Window Title Modifier

Replaces Unity's main window title with your own template. Default:

```
{activeScene} - {applicationTitle} - {platform} - {unityVersion} - {gitBranch}
```

| Token | Value |
| --- | --- |
| `{activeScene}` | Name of the currently open scene |
| `{applicationTitle}` | Project name |
| `{platform}` | Active build target |
| `{unityVersion}` | Editor version |
| `{gitBranch}` | Current Git branch, via `GitInfoEditor` |

Useful when you keep three copies of the same project open and can't tell which is which
in the taskbar. The template is stored per-machine in `EditorPrefs` under
`tuld_WindowTitle`.

> Based on [mob-sakai/MainWindowTitleModifierForUnity](https://github.com/mob-sakai/MainWindowTitleModifierForUnity).

---

## Link XML Editor

IL2CPP managed code stripping happily removes types you only ever touch by reflection.
This tool keeps `Assets/link.xml` in sync so it doesn't.

1. Open the **Link XML Editor** tab.
2. Add one or more **prefixes** (e.g. `MyGame`, `Company.`).
3. Add any **ignored assemblies** you don't want preserved.
4. Hit **Update link.xml**.

Every loaded assembly whose name starts with a prefix — excluding `*.Editor` assemblies
and anything on the ignore list — gets written as
`<assembly fullname="..." preserve="all" />`. Existing entries are left alone, and
assemblies added to the ignore list are removed from the file on the next run.

Settings live in a `LinkXmlConfig` ScriptableObject at `Assets/LinkXmlConfig.asset`,
created automatically on first use. You can also create one manually via
`Create ▸ ScriptableObjects ▸ LinkXmlConfig`.

---

## Git utilities

`GitInfoEditor` runs once on editor load and caches the current branch:

```csharp
Debug.Log(GitInfoEditor.GitBranch);      // "feature/inventory"
Debug.Log(GitInfoEditor.GitBaseBranch);  // best-guess parent branch
```

`GitUtils` wraps the `git` CLI for anything else:

```csharp
GitUtils.IsGitRepository();              // bool
GitUtils.GetCurrentGitBranch();          // rev-parse --abbrev-ref HEAD
GitUtils.GetGitCommitHash();             // rev-parse HEAD
GitUtils.GetGitCommitHashSimplified();   // rev-parse --short HEAD
GitUtils.GetGitRemoteUrl();              // config --get remote.origin.url
GitUtils.RunGitCommand("status --short");// anything else
```

Handy for stamping build numbers, embedding a commit hash in a debug overlay, or gating
editor tooling on branch name. Requires `git` on the `PATH`; commands run relative to the
editor's working directory (the project root).

---

## 2D grayscale shaders

Sprite shaders that desaturate with an adjustable blend — good for disabled UI, locked
items, or a "paused" look without a full post-processing stack.

| Render pipeline | Shader | Material |
| --- | --- | --- |
| Built-in | `UltimateLazyDev/2D/BuiltIn/Grayscale2D` | `2D/Runtime/Materials/BuiltIn/Grayscale.mat` |
| URP | `UltimateLazyDev/2D/URP/Grayscale2D` | `2D/Runtime/Materials/URP/Grayscale.mat` |

| Property | Range | Description |
| --- | --- | --- |
| `_MainTex` | Texture | Sprite texture |
| `_Color` | Color | Tint, multiplied after desaturation |
| `_Intensity` | `0 – 1` | `0` = original colors, `1` = fully grayscale |

Animate `_Intensity` for a fade-to-gray:

```csharp
_renderer.material.SetFloat("_Intensity", Mathf.PingPong(Time.time, 1f));
```

Both variants respect sprite vertex colors and alpha blending. The runtime assembly is
`UltimateLazy.Tools.2D`.

---

## Package layout

```
.
├── 2D/
│   └── Runtime/
│       ├── Materials/{BuiltIn,URP}/Grayscale.mat
│       ├── Shaders/{BuiltIn,URP}/Grayscale2D.shader
│       └── UltimateLazy.Tools.2D.asmdef
├── Editor/
│   ├── DynamicWindowEditor/     # IUltimateLazyToolWindowTab, window base, tree view
│   ├── EditorWindow/            # Editor Window Title Modifier
│   ├── Git/                     # GitUtils, GitInfoEditor, GitLogFetcherEditor
│   ├── Linker/                  # LinkXmlConfig, LinkXmlEditor
│   ├── MainWindow.cs
│   └── UltimateLazy.Tools.Editor.asmdef
└── package.json
```

| Assembly | Platforms | Namespace |
| --- | --- | --- |
| `UltimateLazy.Tools.Editor` | Editor only | `UltimateLazy.Tools.Editor` |
| `UltimateLazy.Tools.2D` | All | `UltimateLazy.Tools.2D` |

---

## Menu reference

```
Tools/
└── The Ultimate Lazy Dev/
    ├── Ultimate Lazy Tools
    ├── Git/
    │   └── Log Commits
    ├── Linker/
    │   └── Link XML Editor
    └── Unity Editor/
        └── Editor Window Title Modifier
```

---

## Roadmap

- [ ] Ship the `Samples~/DynamicWindowEditor` sample declared in `package.json`
- [ ] Grayscale variant for Sprite Renderer material property blocks (no material instancing)
- [ ] More runtime utilities (extension methods, singletons, object pooling)
- [ ] Tagged releases so consumers can pin versions

## Contributing

Issues and pull requests are welcome. Please keep editor-only code inside `Editor/`,
match the existing formatting (CSharpier defaults), and prefix new shaders with
`UltimateLazyDev/`.

## License

<!-- TODO: add a LICENSE file at the repo root and update this section. -->
No license file is present yet — until one is added, all rights are reserved.

## Credits

Built by [The Ultimate Lazy Dev](https://github.com/theultimatelazydev).
Window title tooling adapted from
[mob-sakai/MainWindowTitleModifierForUnity](https://github.com/mob-sakai/MainWindowTitleModifierForUnity).