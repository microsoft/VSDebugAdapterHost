# Visual Studio Debug Adapter Host

The Visual Studio Debug Adapter Host is a component included in Visual Studio 2017 Update 15.6 or later which allows integration of [debug adapters written for Visual Studio Code](https://code.visualstudio.com/docs/extensionAPI/api-debugging) into Visual Studio.

## What is this project?

This project contains sample code demonstrating extension packaging and features of the Debug Adapter Host, as well as tools to make testing debug adapters in Visual Studio easier:
- The [Sample Debug Adapter](src/sample/SampleDebugAdapter), which uses the [Microsoft.VisualStudio.Shared.VsCodeDebugProtocol](https://www.nuget.org/packages/Microsoft.VisualStudio.Shared.VsCodeDebugProtocol/) NuGet package to implement a C#-based debug adapter and demonstrates many [VS-specific extensions to the Debug Adapter Protocol](https://github.com/Microsoft/VSDebugAdapterHost/wiki/New-functionality-supported-by-the-Visual-Studio-Debug-Adapter-Host).
- A [VSIX Project](src/sample/SampleDebugAdapter.VSIX), which packages the Sample Debug Adapter for usage in Visual Studio.
- The [Debug Adapter Launcher](src/tools/EngineLauncher) utility, which provides UI for launching a debug adapter in Visual Studio.

## Getting Started

Start with the [Wiki](https://github.com/Microsoft/VSDebugAdapterHost/wiki), which contains documentation and walkthroughs to help you bring an existing debug adapter into Visual Studio.

If you run into bugs or missing features while integrating a debug adapter with Visual Studio, file an [Issue](https://github.com/Microsoft/VSDebugAdapterHost/issues).

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
