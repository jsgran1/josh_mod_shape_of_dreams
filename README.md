# Damage Analyzer

Damage Analyzer is a mod for **Shape of Dreams** that provides in-run combat analytics and build-comparison information.

The project is focused on helping players understand how their build is performing during a run, including damage contribution, source breakdowns, encounter-level summaries, run-level totals, and contextual comparison information for build choices.

> **Status:** Active development. Features, calculations, UI behavior, and compatibility may change between releases.

## Features

Current and planned functionality includes:

* Encounter damage summaries
* Run-wide damage tracking
* Damage breakdown by source
* Solo and co-op player contribution tracking
* Memory and gem-aware analytics
* Build and candidate comparison support
* Contextual effect evaluation
* In-game analytics overlays

Some functionality is still experimental or validation-driven and may not yet be available in every situation.

## Requirements

* **Shape of Dreams**
* A supported mod-loading environment for the game
* Windows
* .NET SDK if building from source

The project references assemblies from a locally installed copy of Shape of Dreams. Game assemblies are **not** included in this repository.

## Installation

Prebuilt releases may be provided through the repository's GitHub Releases page.

When installing manually, the compiled mod assembly should ultimately be available to the game as:

```text
bin/DamageAnalyzer.dll
```

The included mod metadata identifies the mod as:

```text
Damage Analyzer
com.josh.damageanalyzer
```

Exact installation steps may vary depending on the game's current mod-loading workflow.

## Building from Source

Clone the repository into a location where the project can resolve the required Shape of Dreams managed assemblies.

The project is currently designed to work conveniently when placed under the game's mod directory structure.

Build with:

```powershell
dotnet build DamageAnalyzer.csproj -c Debug
```

The resulting mod assembly is:

```text
bin/DamageAnalyzer.dll
```

If your repository is located elsewhere, you may need to adjust or override the configured Shape of Dreams installation path used by the project.

## Project Structure

The repository contains the source required to build the mod, including:

```text
DamageAnalyzer.sln
DamageAnalyzer.csproj
DamageAnalyzerMod.cs
DamageAnalytics*.cs
DamageAnalyzerDiagnostics.cs
BuildComparisonContracts.cs
ContextualEffectEvaluator.cs
about/
```

Development-specific planning documents, diagnostic evidence, local editor configuration, automation scripts, and AI-assisted development tooling are intentionally not part of the public repository.

## Compatibility

Damage Analyzer is under active development and may rely on game implementation details that can change when Shape of Dreams is updated.

The metadata currently allows all game versions, but that should not be interpreted as a guarantee that every future game version will remain compatible.

If the game updates and the mod stops working, compatibility may need to be restored in a later release.

## Privacy

Damage Analyzer may inspect runtime game state in order to calculate combat analytics.

Public releases of this repository do not include the author's raw gameplay logs, Steam session data, local machine paths, or other private diagnostic artifacts.

## Disclaimer

Damage Analyzer is an unofficial community project and is not affiliated with, endorsed by, or supported by the developers or publishers of Shape of Dreams.

Shape of Dreams and related names, game data, artwork, and other game assets remain the property of their respective rights holders.

This repository does not distribute Shape of Dreams game assemblies.

## License

The original source code in this repository is licensed under the MIT License.

See [LICENSE](LICENSE) for details.

The MIT License applies only to material owned by the author of this repository. It does not grant rights to Shape of Dreams, its assets, game code, trademarks, or other third-party material.
