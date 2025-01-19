using NETDownloader.View;
using LForms.Extensions;
using LForms.Enums;
using NETDownloader.Configuration;
using LealLogger;
using LealLogger.Factory;
using System.Security.Principal;
using System.Diagnostics;

namespace NETDownloader;

internal static class Program
{
	[System.Runtime.InteropServices.DllImport("kernel32.dll")]
	private static extern bool AttachConsole(int dwProcessId);

	[System.Runtime.InteropServices.DllImport("kernel32.dll")]
	private static extern bool FreeConsole();

	internal static Logger Logger => new LoggerBuilder()
					.SetQueueCapacity(100)
					.AddConsoleHandler(LogLevel.DEBUG)
					.AddFileHandler($"NETDownloader_[{DateTime.Now:dd-MM-yyyy}].trace", LogLevel.DEBUG)
					.Build();

	[STAThread]
	internal static void Main()
	{
		try
		{
			AttachConsole(-1); // Attach the console to the application
			Logger.Info("Application started.");
			var isAdministrator = IsAdministrator();

			if (!isAdministrator)
			{
				Logger.Debug("Application is not running with administrator privileges.");
				Logger.Debug("Requesting permission to restart as administrator.");
				var result = MessageBox.Show("NETDownloader works better with administrator privileges\n. Do you want to restart as administrator?",
											 "Administrator Privileges Warn",
											 MessageBoxButtons.YesNo,
											 MessageBoxIcon.Warning);

				if (result == DialogResult.Yes)
				{
					Logger.Debug("User accepted, restarting application as administrator.");
					RestartAsAdministrator();
					return;
				}

				Logger.Debug("User refused.");
			}

			Logger.Info($"Application running in {(isAdministrator ? "Administrator" : "Normal")} mode");
			ApplicationConfiguration.Initialize();
			Application.Run(new MainForm());
		}
		catch (Exception e)
		{
			Logging.Instance.Fatal("Fatal error occurred at Main()", e);
			e.HandleException(ErrorType.Critical);
		}
		finally
		{
			Logger.Dispose();
			FreeConsole(); // Detach the console when the application closes
		}
	}

	private static bool IsAdministrator()
	{
		var identity = WindowsIdentity.GetCurrent();
		var principal = new WindowsPrincipal(identity);
		return principal.IsInRole(WindowsBuiltInRole.Administrator);
	}

	private static void RestartAsAdministrator()
	{
		try
		{
			var processInfo = new ProcessStartInfo
			{
				Verb = "runas", // Triggers UAC prompt
				FileName = Application.ExecutablePath,
				UseShellExecute = true
			};

			Process.Start(processInfo);
			Application.Exit();
		}
		catch (Exception ex)
		{
			MessageBox.Show("Failed to restart with administrator privileges: " + ex.Message,
							"Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Error);
		}
	}
}