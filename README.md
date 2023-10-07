# WinStore Links

Generates LNK files to launch Windows Store apps.

Specially created in order to use Find and Run Robot to launch Windows Store apps.

# Build

```sh
dotnet publish --configuration Release
```

The executable will be created at: 

```
WinStoreLinks\bin\Release\net6.0-windows10.0.17763.0\win-x64\publish\WinStoreLinks.exe
```

This is a single executable that packages it's dependencies, but not the framework, 
so installing .Net 6 Runtime is still needed.
