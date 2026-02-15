# Break In Current Document

A Visual Studio extension that breaks debugger execution and automatically closes unwanted debugger windows (showing external/disassembly code), returning focus to your working document.

## The Problem

Some developers have a workflow which includes a lot of work with Edit & Continue:

1. Run the application
2. Find the spot where the code needs to be changed
3. Signal the debugger to break
4. **Close the window that opens (often with the code that was running, not your change, sometimes nothing because it was all external code)**
5. Make a change
6. Continue the application and see if the change results in the right behavior
7. Repeat steps 3 through 6

This extension does step 4 automatically when you use the "Break All in Current Document" button.

## Supported Versions

- Visual Studio 2022 17.9+ (17.x)
- Visual Studio 2026 (18.x)
- Architectures: x64, ARM64

## Technical Details

This extension uses the **VisualStudio.Extensibility + VSSDK compatibility** hybrid model. Commands are registered using the modern extensibility API while debugger interaction (`DTE2.Debugger.Break()`) uses injected VSSDK services.

## Installation

Install the extension from the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=thijstijsma.BreakInCurrentDocument).

## Build

Requires:
- Visual Studio 2022 17.9+ with the "Visual Studio extension development" workload
- .NET Framework 4.7.2 targeting pack

```shell
dotnet build
```

To debug, press F5 to launch the VS experimental instance.
