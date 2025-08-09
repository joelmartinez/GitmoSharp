# GitmoSharp - Copilot Instructions

## About This Repository

GitmoSharp is a .NET 8 library that provides helper functions for common git-related operations, particularly useful in CI/CD scenarios. It's backed by LibGit2Sharp and Octokit.NET to simplify git repository management and GitHub operations.

## Key Components

### Core Library (`GitmoSharp/`)
- **Gitmo.cs**: Main class providing git repository operations (cloning, committing, pushing, etc.)
- **GitExtensions.cs**: Extension methods for git operations
- **GitSearchIndex.cs**: Git search functionality
- **Zipper.cs**: Archive creation utilities

### CLI Tool (`gitmo/`)
- **Program.cs**: Command-line interface for GitmoSharp operations
- **OpenPR.cs**: Functionality for opening GitHub pull requests

### Tests (`GitmoSharp.Test/`)
- **GitmoTest.cs**: Unit tests for the library

## Technology Stack

- **.NET 8**: For modern performance and cross-platform compatibility
- **LibGit2Sharp**: Native git operations
- **Octokit.NET**: GitHub API integration
- **SharpZipLib**: Archive creation
- **Mono.Options**: CLI argument parsing

## Development Guidelines

### Code Style
- Follow C# naming conventions (PascalCase for public members, camelCase for private fields)
- Use explicit types where clarity is needed
- Include XML documentation comments for public APIs
- Maintain async/await patterns where appropriate

### Dependencies
- Keep dependencies minimal and up-to-date
- Prefer stable, well-maintained packages
- Document any breaking changes when updating dependencies

### Copilot Instructions Maintenance
- Always keep these copilot-instructions.md up to date with any changes made to the project
- Update framework versions, dependencies, and project structure when they change
- Reflect any architectural or technology stack changes in the documentation
- Include new development practices or guidelines as they are established

### Testing
- Add unit tests for new functionality in `GitmoSharp.Test`
- Test both success and error scenarios
- Mock external dependencies (GitHub API, file system operations)

### Git Operations
- Use LibGit2Sharp for all git operations
- Handle authentication properly for remote operations
- Provide clear error messages for git failures

### GitHub Integration
- Use Octokit.NET for GitHub API operations
- Handle rate limiting appropriately
- Support both personal access tokens and GitHub Apps authentication

## Common Tasks

### Adding New Git Operations
1. Add the method to the appropriate class in `GitmoSharp/`
2. Include proper error handling and logging
3. Add corresponding tests in `GitmoSharp.Test/`
4. Update XML documentation

### CLI Commands
1. Add command parsing in `Program.cs`
2. Create dedicated handler classes if complex
3. Follow existing pattern for option handling
4. Include help text and examples

### Dependency Updates
1. Update package references in `.csproj` files
2. Test for breaking changes
3. Update code if APIs have changed
4. Run full test suite to ensure compatibility

## Build and Test

- Build: `dotnet build GitmoSharp.sln`
- Test: `dotnet test GitmoSharp.Test/`
- Package: `dotnet pack GitmoSharp/`

## CI/CD

The project includes a GitHub Actions workflow (`.github/workflows/ci.yml`) that:
- Runs on every push to main branch
- Runs on every pull request targeting main
- Sets up .NET 8 environment
- Restores dependencies, builds the solution, and runs all tests
- Uses Release configuration for builds and tests

## Security Considerations

- Never commit authentication tokens or credentials
- Use secure methods for handling GitHub tokens
- Validate all user inputs, especially file paths
- Follow principle of least privilege for git operations