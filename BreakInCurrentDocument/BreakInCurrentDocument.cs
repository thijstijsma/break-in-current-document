using System;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

using Task = System.Threading.Tasks.Task;

namespace BreakInCurrentDocument
{
	internal sealed class BreakInCurrentDocument
	{
		public const int CommandId = 0x0100;

		public static readonly Guid CommandSet = new Guid("6a107191-e421-4952-9040-9b840ac8e922");

		private readonly AsyncPackage _package;
		private readonly DTE2 _dte;

		public static BreakInCurrentDocument Instance { get; private set; }

		private BreakInCurrentDocument(AsyncPackage package, OleMenuCommandService commandService, DTE2 dte)
		{
			_package = package ?? throw new ArgumentNullException(nameof(package));
			_dte = dte ?? throw new ArgumentNullException(nameof(dte));

			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			var menuCommandID = new CommandID(CommandSet, CommandId);
			var menuItem = new OleMenuCommand(Execute, menuCommandID);

			menuItem.BeforeQueryStatus += BeforeQueryStatus;

			commandService.AddCommand(menuItem);
		}

		private void BeforeQueryStatus(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var isRunning = _dte.Debugger.CurrentMode == dbgDebugMode.dbgRunMode;

			((OleMenuCommand) sender).Enabled = isRunning;
		}

		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			var commandService = await package.GetServiceAsync<IMenuCommandService, OleMenuCommandService>();
			var dte = await package.GetServiceAsync<DTE, DTE2>();

			Instance = new BreakInCurrentDocument(package, commandService, dte);
		}

		private void Execute(object sender, EventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var activeWindowBefore = _dte.ActiveWindow;

			_dte.Debugger.Break();

			var activeWindowAfter = _dte.ActiveWindow;

			if (activeWindowBefore == activeWindowAfter)
			{
				return;
			}

			if (activeWindowAfter.Document == null)
			{
				activeWindowAfter.Close();
			}

			activeWindowBefore.Activate();
		}
	}
}
