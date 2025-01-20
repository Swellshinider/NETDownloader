using LealForms.Enums;
using LealForms.Extensions;
using Newtonsoft.Json;

namespace NETDownloader.Configuration;

public static class SettingsManager
{
	private static UserSettings? _settings;

	public static UserSettings UserSettings
	{
		get => _settings ??= Retrieve();
		set => _settings = value;
	}

	public static string SettingsDirectory
		=> Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\NETDownloader";

	public static void Save()
	{
		try
		{
			Directory.CreateDirectory(SettingsDirectory);
			var filePath = Path.Combine(SettingsDirectory, "Settings.json");
			var jsonString = JsonConvert.SerializeObject(_settings!, Formatting.Indented);
			File.WriteAllText(filePath, jsonString);
			Program.Logger.Debug("Saving settings to: " + filePath);
		}
		catch (Exception ex)
		{
			Program.Logger.Error($"Failed to save settings", ex);

			if (ex.HandleException(ErrorType.Process).Equals(DialogResult.Retry))
				Save();
		}
	}

	private static UserSettings Retrieve()
	{
		try
		{
			var filePath = Path.Combine(SettingsDirectory, "Settings.json");

			if (!File.Exists(filePath))
			{
				Program.Logger.Warn("Settings file not found, using defaults.");
				return UserSettings.Default;
			}

			var jsonString = File.ReadAllText(filePath);
			return JsonConvert.DeserializeObject<UserSettings>(jsonString) ?? UserSettings.Default;
		}
		catch (Exception ex)
		{
			Program.Logger.Error($"Failed to retrieve settings, using defaults.", ex);
			return UserSettings.Default;
		}
	}
}