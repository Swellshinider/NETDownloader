using System.Text.Json.Serialization;

namespace NETDownloader.Configuration;

[Serializable]
public class UserSettings
{
	public static UserSettings Default => new()
	{
		Location = new(0, 0),
		Size = new(1280, 720),
		CardHeight = 150,
		Maximized = false,
		Colors = new() 
		{
			BackgroundColor = Color.FromArgb(34, 40, 49),
			SecondaryBackgroundColor = Color.FromArgb(57, 62, 70),
			ForegroundColor = Color.WhiteSmoke,
			HighLightColor = Color.FromArgb(0, 173, 181),
			ContrastBackColor = Color.FromArgb(238, 238, 238),
			DownloadStartColor = Color.Red,
			DownloadFinishedColor = Color.Blue
		}
	};

	[JsonPropertyName("size")]
	public Size Size { get; set; }

	[JsonPropertyName("location")]
	public Point Location { get; set; }

	[JsonPropertyName("card_height")]
	public int CardHeight { get; set; }

	[JsonPropertyName("maximized")]
	public bool Maximized { get; set; }

	[JsonPropertyName("console_visible")]
	public bool ConsoleVisible { get; set; }

	[JsonPropertyName("colors")]
	public required ColorPalette Colors { get; set; }
}