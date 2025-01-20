using System.Text.Json.Serialization;

namespace NETDownloader.Configuration;

[Serializable]
public class UserSettings
{
	public static UserSettings Default => new()
	{
		Location = new(0, 0),
		Size = new(1280, 720),
		SplitterDistance = 250,
		Maximized = false
	};

	[JsonPropertyName("size")]
	public Size Size { get; set; }

	[JsonPropertyName("location")]
	public Point Location { get; set; }

	[JsonPropertyName("splitter_distance")]
	public int SplitterDistance { get; set; }

	[JsonPropertyName("maximized")]
	public bool Maximized { get; set; }

	[JsonPropertyName("console_visible")]
	public bool ConsoleVisible { get; set; }
}