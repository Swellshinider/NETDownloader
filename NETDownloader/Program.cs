using System.Diagnostics;
using System.Security.Principal;
using LealLogger;
using LealLogger.Factory;
using LealForms.Enums;
using LealForms.Extensions;
using NETDownloader.View;
using NETDownloader.Configuration;
using System.Runtime.InteropServices;
using LealForms;

namespace NETDownloader;

internal static partial class Program
{
	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool AttachConsole(int dwProcessId);

	[LibraryImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool FreeConsole();
	
	[LibraryImport("kernel32.dll")]
	private static partial IntPtr GetConsoleWindow();
	
	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);
	
	private static readonly int SW_HIDE = 0;
	private static readonly int SW_SHOW = 5;
	private static MainForm _mainView;

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
		catch (UnauthorizedAccessException ue)
		{
			Logger = new LoggerBuilder()
						.SetQueueCapacity(100)
						.AddConsoleHandler(LogLevel.DEBUG)
						.Build();
			Logger.Warn("Logger loaded without file handler", ue);
		}
		finally 
		{
			ApplicationConfiguration.Initialize();
			_mainView = new();
		}
	}

	[STAThread]
	internal static void Main()
	{
		try
		{
			AttachConsole(-1);
			HideConsole();
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
			Application.Run(_mainView);
		}
		catch (Exception e)
		{
			Logger.Fatal("Fatal error occurred at Main()", e);
			e.HandleException(ErrorType.Critical);
		}
		finally
		{
			_mainView.Settings.Location = _mainView.Location;
			SettingsManager.UserSettings = _mainView.Settings;
			SettingsManager.Save();
			
			Logger.Dispose();
			FreeConsole();
		}
	}
	
	internal static void HideConsole() 
		=> ShowWindow(GetConsoleWindow(), SW_HIDE);
	
	internal static void ShowConsole() 
		=> ShowWindow(GetConsoleWindow(), SW_SHOW);

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