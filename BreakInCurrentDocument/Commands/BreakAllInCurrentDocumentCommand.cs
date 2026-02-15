using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE80;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.VSSdkCompatibility;
using Microsoft.VisualStudio.Shell;
using DTE = EnvDTE.DTE;

namespace BreakInCurrentDocument.Commands;

[VisualStudioContribution]
internal class BreakAllInCurrentDocumentCommand : Microsoft.VisualStudio.Extensibility.Commands.Command
{
    private readonly AsyncServiceProviderInjection<DTE, DTE2> _dte;

    /// <summary>
    /// guidVSDebugGroup — the GUID for VS debug command groups, defined in VsDebugGuids.h.
    /// </summary>
    private static readonly Guid VsDebugGroupGuid = new("C9DD4A58-47FB-11D2-83E7-00C04F9902C1");

    /// <summary>
    /// UICONTEXT_Debugging — active during any debug session (run or break mode).
    /// </summary>
    private static readonly Guid DebuggingUIContext = new("ADFC4E61-0397-11D1-9F4E-00A0C911004F");

    public BreakAllInCurrentDocumentCommand(
        VisualStudioExtensibility extensibility,
        TraceSource traceSource,
        AsyncServiceProviderInjection<DTE, DTE2> dte)
        : base(extensibility)
    {
        _dte = dte;
    }

    public override CommandConfiguration CommandConfiguration => new("%BreakInCurrentDocument.BreakAllInCurrentDocumentCommand.DisplayName%")
    {
        Placements =
        [
            // Debug menu — Execution group (IDG_EXECUTION = 0x0004)
            CommandPlacement.VsctParent(VsDebugGroupGuid, id: 0x0004, priority: 0x0100),

            // Debug toolbar — Execution group (IDG_DEBUG_TOOLBAR_EXECUTION = 0x011B)
            CommandPlacement.VsctParent(VsDebugGroupGuid, id: 0x011B, priority: 0x0100),
        ],
        Icon = new(ImageMoniker.Custom("BreakInCurrentDocument"), IconSettings.IconOnly),
        VisibleWhen = ActivationConstraint.UIContext(DebuggingUIContext),
        EnabledWhen = ActivationConstraint.UIContext(VisualStudioPackage.DebugRunModeContext),
    };

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        var dte = await _dte.GetServiceAsync();

        if (dte.Debugger.CurrentMode != EnvDTE.dbgDebugMode.dbgRunMode)
        {
            return;
        }

        var activeWindowBefore = dte.ActiveWindow;
        if (activeWindowBefore is null)
        {
            return;
        }

        dte.Debugger.Break();

        try
        {
            var activeWindowAfter = dte.ActiveWindow;

            if (activeWindowAfter is null || activeWindowBefore == activeWindowAfter)
            {
                return;
            }

            if (activeWindowAfter.Document == null)
            {
                activeWindowAfter.Close();
            }

            activeWindowBefore.Activate();
        }
        catch (COMException)
        {
            // The debugger opened a window without a Document (e.g., disassembly).
            try { activeWindowBefore.Activate(); } catch { }
        }
    }
}
