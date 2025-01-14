﻿using EnvDTE;
using System.Windows;
using FineCodeCoverage.Engine;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using Microsoft;

namespace FineCodeCoverage.Output
{
    /// <summary>
    /// Interaction logic for OutputToolWindowControl.
    /// </summary>
    internal partial class OutputToolWindowControl : UserControl, IScriptInvoker
	{
        private DTE Dte;
		private Events Events;
		private SolutionEvents SolutionEvents;

		/// <summary>
		/// Initializes a new instance of the <see cref="OutputToolWindowControl"/> class.
		/// </summary>
		public OutputToolWindowControl(ScriptManager scriptManager,IFCCEngine fccEngine)
		{
			InitializeComponent();

			ThreadHelper.JoinableTaskFactory.Run(async () =>
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
				Dte = (DTE)await OutputToolWindowCommand.Instance.ServiceProvider.GetServiceAsync(typeof(DTE));
				Assumes.Present(Dte);
				Events = Dte.Events;
				SolutionEvents = Events.SolutionEvents;
				SolutionEvents.Opened += () => Clear();
				SolutionEvents.AfterClosing += () => Clear();
			});

			FCCOutputBrowser.ObjectForScripting = scriptManager;
			scriptManager.ScriptInvoker = this;
			
			fccEngine.UpdateOutputWindow += (args) =>
			{
				ThreadHelper.JoinableTaskFactory.Run(async () =>
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

					if (string.IsNullOrWhiteSpace(args?.HtmlContent))
					{
						Clear();
						return;
					}
					
					FCCOutputBrowser.NavigateToString(args.HtmlContent);
					FCCOutputBrowser.Visibility = Visibility.Visible;
				});
			};
        }

        public object InvokeScript(string scriptName, params object[] args)
        {
			if (FCCOutputBrowser.Document != null)
			{
				return FCCOutputBrowser.InvokeScript(scriptName, args);
			}
			return null;
		}

        private void Clear()
		{
			FCCOutputBrowser.Visibility = Visibility.Hidden;
		}
	}
}