using System.Diagnostics;
using System.Security.Principal;
using LealLogger;
using LealLogger.Factory;
using LealForms.Enums;
using LealForms.Extensions;
using NETDownloader.View;
using NETDownloader.Configuration;
using System.Runtime.InteropServices;

namespace NETDownloader;

internal static partial class Program
{
	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool AttachConsole(int dwProcessId);

	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool FreeConsole();

	internal static Logger Logger { get; }

	static Program()
	{
		try
		{
			var logDirectory = $@"{SettingsManager.SettingsDirectory}\\Logs";
			Directory.CreateDirectory(logDirectory);
			Logger = new LoggerBuilder()
						.SetQueueCapacity(100)
						.AddConsoleHandler(LogLevel.INFO)
						.AddFileHandler(Path.Combine(logDirectory, $"NETDownloader_[{DateTime.Now:dd-MM-yyyy}].log"), LogLevel.DEBUG)
						.Build();
		}
		catch (UnauthorizedAccessException)
		{
			Logger = new LoggerBuilder()
						.SetQueueCapacity(100)
						.AddConsoleHandler(LogLevel.DEBUG)
						.Build();
		}
	}

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
				var result = MessageBox.Show("NETDownloader works better with administrator privileges.\nDo you want to restart as administrator?",
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
			Logger?.Fatal("Fatal error occurred at Main()", e);
			e.HandleException(ErrorType.Critical);
		}
		finally
		{
			Logger?.Dispose();
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
			Logger.Error("Failed to restart with administrator privileges", ex);

			if (ex.HandleException(ErrorType.Process).Equals(DialogResult.Retry))
				RestartAsAdministrator();
		}
	}
}