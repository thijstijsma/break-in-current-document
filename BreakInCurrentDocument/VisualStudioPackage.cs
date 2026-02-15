using System;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace BreakInCurrentDocument;

/// <summary>
/// VSSDK package that registers a custom UI context for "debugger in run mode."
/// This context is toggled by listening to DTE debugger events and is consumed by
/// <see cref="Commands.BreakAllInCurrentDocumentCommand"/> via <c>EnabledWhen</c>.
/// </summary>
[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideBindingPath]
[Guid(PackageGuidString)]
[ProvideAutoLoad(UIContextGuids.Debugging, PackageAutoLoadFlags.BackgroundLoad)]
public sealed class VisualStudioPackage : AsyncPackage
{
    public const string PackageGuidString = "0a3b6bda-c922-4a82-afe5-3767be3b84a1";

    /// <summary>
    /// Custom UI context GUID activated only when the debugger is in run mode (not break mode).
    /// </summary>
    public const string DebugRunModeContextString = "a0f2974e-8b1d-4c5a-9e3f-6b7d8c2e1a4f";
    public static readonly Guid DebugRunModeContext = new(DebugRunModeContextString);

    private DebuggerEvents? _debuggerEvents;
    private uint _contextCookie;

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        if (await GetServiceAsync(typeof(DTE)) is not DTE2 dte) return;

        var monitorSelection = await GetServiceAsync(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
        if (monitorSelection is null) return;

        var contextGuid = DebugRunModeContext;
        if (monitorSelection.GetCmdUIContextCookie(ref contextGuid, out _contextCookie) != 0 || _contextCookie == 0)
            return;

        if (dte.Debugger.CurrentMode == dbgDebugMode.dbgRunMode)
        {
            monitorSelection.SetCmdUIContext(_contextCookie, 1);
        }

        _debuggerEvents = dte.Events.DebuggerEvents;
        _debuggerEvents.OnEnterRunMode += OnEnterRunMode;
        _debuggerEvents.OnEnterBreakMode += OnEnterBreakMode;
        _debuggerEvents.OnEnterDesignMode += OnEnterDesignMode;
    }

    // DTE COM event (un)subscription and handlers run on the UI thread.
#pragma warning disable VSTHRD010
    protected override void Dispose(bool disposing)
    {
        if (disposing && _debuggerEvents is not null)
        {
            _debuggerEvents.OnEnterRunMode -= OnEnterRunMode;
            _debuggerEvents.OnEnterBreakMode -= OnEnterBreakMode;
            _debuggerEvents.OnEnterDesignMode -= OnEnterDesignMode;
        }

        base.Dispose(disposing);
    }

    private void OnEnterRunMode(dbgEventReason reason) => SetContext(true);
    private void OnEnterBreakMode(dbgEventReason reason, ref dbgExecutionAction action) => SetContext(false);
    private void OnEnterDesignMode(dbgEventReason reason) => SetContext(false);
#pragma warning restore VSTHRD010

    private void SetContext(bool active)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        if (GetService(typeof(SVsShellMonitorSelection)) is IVsMonitorSelection monitorSelection)
        {
            monitorSelection.SetCmdUIContext(_contextCookie, active ? 1 : 0);
        }
    }
}
