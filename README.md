# FileSplitter

## Overview

`FileSplitter` is a command-line tool for processing and splitting tabular files (CSV and Excel) into smaller files according to processing options. The project includes specific processors for CSV and Excel, row/line validation, encoding detection, and a console interface for user interaction.

## Key Features

- Support for CSV and Excel files
- Row/line validation before writing to output files
- Automatic file encoding detection
- Dedicated writers for CSV and Excel formats
- Modular architecture with processor factory and orchestrator
- Interactive console interface

## Requirements

- .NET SDK 8.0 or higher (workspace contains projects targeting .NET 8 and .NET 10)

## How to Run

1. Restore and build the project:

```bash
dotnet restore
dotnet build
```

2. Run from the repository root:

```bash
dotnet run --project FileSplitter
```

**Note:** The application has a console UI (`FileSplitter.Infrastructure.UserInterface.ConsoleUserInteraction`) that will prompt for required paths and options. See the code in `Program.cs` to understand the execution flow and supported options.

## Running Tests

```bash
dotnet test
```

## Project Structure

- `FileSplitter/` - Main project
  - `Application/` - Services and contracts (processor factory, orchestrator)
  - `Features/Csv/` - CSV readers, validators, and processor
  - `Features/Excel/` - Excel readers, validators, and processor
  - `Infrastructure/` - Encoding detection, path resolution, console UI
  - `Domain/` - Models and enums (e.g., `FileProcessingOptions`, `FileType`)
- `FileSplitter.Tests/` - Unit tests

## Contributing

- Open issues for bugs and feature requests
- Submit pull requests to `feature/*` branches and create PRs to `main` or `develop` according to the repository workflow

## License

- Check the `LICENSE` file in the repository (if it exists). If not, add an appropriate license before publishing.

## Contact

- Repository: [https://github.com/freireomarcos/file-splitter](https://github.com/freireomarcos/file-splitter)
- Author: Marcos Freire
