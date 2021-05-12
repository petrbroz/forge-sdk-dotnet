# autodesk-forge-dotnet

Unofficial [Autodesk Forge](https://forge.autodesk.com) SDK for [.NET 6.0](https://devblogs.microsoft.com/dotnet/announcing-net-6-preview-3).

## Development

### Prerequisites

- [Autodesk Forge application](https://forge.autodesk.com/en/docs/oauth/v2/tutorials/create-app)
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

### Option 1: Using Visual Studio Code

- Clone the repository
- Open the solution folder with [Visual Studio Code](https://code.visualstudio.com)
- Create a new _.env_ file in the solution folder, specifying `FORGE_CLIENT_ID` and `FORGE_CLIENT_SECRET` environment variables:
    ```
    FORGE_CLIENT_ID=your-client-id
    FORGE_CLIENT_SECRET=your-client-secret
    ```
- Launch one of the predefined configurations ("Launch Console App" or "Launch Server App")

### Option 2: Manual Setup

- Clone the repository
- Navigate to the solution folder in terminal
- Build the solution
    ```bash
    dotnet build
    ```
- Set environment variables `FORGE_CLIENT_ID` and `FORGE_CLIENT_SECRET`
    ```bash
    # on Windows
    set FORGE_CLIENT_ID=your-client-id
    set FORGE_CLIENT_SECRET=your-client-secret
    # on *nix
    export FORGE_CLIENT_ID=your-client-id
    export FORGE_CLIENT_SECRET=your-client-secret
    ```
- Run the sample console application
    ```bash
    cd sample-console-app
    dotnet run
    ```
- Run the sample server application
    ```bash
    cs sample-server-app
    dotnet run
    ```
- Go to https://localhost:5001/index.html
