# GitmoSharp

GitmoSharp is a .NET library and CLI tool for Git CI helper functions, backed by LibGit2Sharp and Octokit.NET. It simplifies common git-related actions used in CI processes such as fetching/cloning remote repositories and opening GitHub pull requests.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Bootstrap and Build
- Restore NuGet packages:
  - `dotnet restore` -- takes 56 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
- Build the solution:
  - `dotnet build` -- takes 10 seconds. NEVER CANCEL. Set timeout to 60+ seconds.
  - `dotnet build --configuration Release` -- takes 2-3 seconds for Release builds.
- Create NuGet packages:
  - `dotnet pack` -- takes 2 seconds. Creates packages for both GitmoSharp library and gitmo CLI tool.
- Clean build artifacts:
  - `dotnet clean` -- takes 1 second. Cleans all projects in solution.
- ALWAYS run restore before build when working with a fresh clone.

### Dependencies and Warnings
- The project targets .NET Core 2.2 (end-of-life) and .NET Standard 2.0.
- Building works on .NET 8 SDK with compatibility warnings.
- EXPECT security warnings for outdated packages (SharpZipLib 1.1.0, Microsoft.NETCore.App 2.2.0).
- Package vulnerabilities are known issues but do not prevent building.

### Testing Limitations
- Unit tests CANNOT RUN due to .NET Core 2.2 runtime dependency.
- Test project exists at `GitmoSharp.Test/GitmoSharp.Test.csproj` with xUnit framework.
- `dotnet test` will fail with framework not found error.
- Do NOT attempt to run tests - they require .NET Core 2.2 runtime which is not available.

### CLI Tool Limitations  
- CLI tool (`gitmo`) CANNOT RUN due to .NET Core 2.2 runtime dependency.
- `dotnet run --project gitmo` will fail with framework not found error.
- CLI supports `open-pr` command for creating GitHub pull requests.
- Do NOT attempt to run the CLI tool directly.

## Validation

### Build Validation
- ALWAYS validate changes by running the full build process:
  - `dotnet restore` (expect 45-60 seconds)
  - `dotnet build` (expect 8-15 seconds)
  - `dotnet pack` (expect 1-3 seconds, optional but recommended)
- Build should complete with warnings but zero errors.
- Successful build produces NuGet packages:
  - `GitmoSharp/bin/Debug/GitmoSharp.0.8.1.nupkg` (library package)
  - `gitmo/bin/Debug/gitmo.0.2.3.nupkg` (CLI tool package)
  - `GitmoSharp/bin/Release/GitmoSharp.0.8.1.nupkg` (Release library package)
  - `gitmo/bin/Release/gitmo.0.2.3.nupkg` (Release CLI tool package)

### Code Validation
- Since runtime testing is not possible, validate changes through:
  - Successful compilation
  - Code review of public API changes
  - Examination of generated assemblies in `bin/Debug/` directories
- ALWAYS ensure GitmoSharp library builds without errors when making changes.

### Manual Testing Scenarios
- Due to .NET Core 2.2 runtime limitations, manual testing is limited to:
  - Build verification
  - Package generation verification
  - Static code analysis

## Projects Structure

### GitmoSharp Library
- **Location**: `GitmoSharp/GitmoSharp.csproj`
- **Purpose**: Main library providing Git CI helper functions
- **Target**: .NET Standard 2.0
- **Key Classes**:
  - `Gitmo.cs` - Main library class for git operations
  - `GitExtensions.cs` - Git operation extensions
  - `GitSearchIndex.cs` - Git search functionality
  - `Zipper.cs` - Archive creation utilities

### gitmo CLI Tool
- **Location**: `gitmo/gitmo.csproj`
- **Purpose**: Command-line interface for GitmoSharp
- **Target**: .NET Core 2.2
- **Commands**: `open-pr` (opens GitHub pull requests)
- **Key Files**:
  - `Program.cs` - CLI entry point and argument parsing
  - `OpenPR.cs` - Pull request creation functionality

### GitmoSharp.Test
- **Location**: `GitmoSharp.Test/GitmoSharp.Test.csproj`
- **Purpose**: Unit tests for GitmoSharp library
- **Target**: .NET Core 2.2
- **Framework**: xUnit
- **Status**: Cannot run due to runtime dependency

## Common Commands

### Repository Root Contents
```
ls -la
.git/                  # Git repository metadata
.gitignore            # Git ignore patterns
GitmoSharp/           # Main library project
GitmoSharp.Test/      # Test project
GitmoSharp.sln        # Visual Studio solution file
LICENSE               # MIT License
README.md             # Basic project description
gitmo/                # CLI tool project
```

### Build Times and Timeouts
- **Restore**: 45-60 seconds normal, set timeout to 120+ seconds
- **Build Debug**: 8-15 seconds normal, set timeout to 60+ seconds  
- **Build Release**: 2-3 seconds normal, set timeout to 30+ seconds
- **Pack**: 1-3 seconds normal, set timeout to 30+ seconds
- **Clean**: 1 second normal, set timeout to 30+ seconds
- **NEVER CANCEL** long-running commands - build processes may take time
- Always wait for completion rather than canceling

### Package Dependencies
- **LibGit2Sharp 0.26.0** - Git operations
- **Octokit 0.32.0** - GitHub API integration
- **SharpZipLib 1.1.0** - Archive creation (has security warnings)
- **Mono.Options 5.3.0.1** - CLI argument parsing
- **xUnit 2.4.1** - Testing framework

## Important Notes

### Runtime Environment
- Requires .NET SDK for building (compatible with .NET 8)
- Runtime execution requires .NET Core 2.2 (not available in most modern environments)
- Build and package creation work fine on newer .NET versions

### GitHub Integration
- Library provides GitHub pull request creation functionality
- Requires GitHub credentials (username/token) for API access
- CLI tool automates PR creation with repository path, branch, and PR details

### Development Workflow
- Make changes to library code in `GitmoSharp/`
- Verify builds complete successfully
- Test CLI changes through build verification only
- Package generation confirms successful compilation

Always prioritize build verification over runtime testing due to framework limitations.