# Contributing to Hittite Portal Scraper

Thank you for your interest in contributing to the Hittite Portal Scraper! We welcome contributions from the community.

## How to Contribute

### Reporting Bugs

If you find a bug, please create an issue on GitHub with:

- A clear, descriptive title
- Steps to reproduce the issue
- Expected behavior vs actual behavior
- Your environment (OS, .NET version, Docker version if applicable)
- Any relevant logs or error messages

### Suggesting Enhancements

Enhancement suggestions are welcome! Please create an issue with:

- A clear description of the enhancement
- The motivation behind it (what problem does it solve?)
- Any implementation ideas you might have

### Pull Requests

1. **Fork the repository** and create your branch from `main`
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following our coding standards:
   - Follow C# coding conventions
   - Keep methods focused and well-named
   - Add comments for complex logic
   - Update documentation if needed

3. **Test your changes**
   - Ensure the application builds successfully
   - Test the scraping functionality
   - Verify Docker builds work if you modified Docker files

4. **Commit your changes** with clear, descriptive commit messages
   ```bash
   git commit -m "Add feature: brief description of what you added"
   ```

5. **Push to your fork** and create a pull request
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Describe your changes** in the pull request:
   - What does this PR do?
   - Why is this change needed?
   - How has it been tested?
   - Any breaking changes?

## Development Setup

1. Install prerequisites:
   - .NET 8.0 SDK
   - Visual Studio 2022 or VS Code
   - Docker Desktop (for Docker testing)

2. Clone your fork:
   ```bash
   git clone https://github.com/your-username/HittitePortalScraper.git
   cd HittitePortalScraper
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Build and run:
   ```bash
   dotnet run --project Test/HittitePortalScraper.csproj
   ```

## Code Style

- Use meaningful variable and method names
- Keep lines under 120 characters when possible
- Use async/await properly for I/O operations
- Dispose of resources properly (use `using` statements)
- Add XML documentation comments for public methods

## Testing

Currently, the project doesn't have automated tests. If you'd like to contribute by adding test coverage, that would be highly appreciated!

## Documentation

If your change requires documentation updates:

- Update the README.md if needed
- Add inline code comments for complex logic
- Update Docker documentation if you modify Docker files

## Questions?

Feel free to open an issue if you have questions about contributing!

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
