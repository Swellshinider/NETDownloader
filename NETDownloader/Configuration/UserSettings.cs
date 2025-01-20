using System.Text.Json.Serialization;

namespace NETDownloader.Configuration;

[Serializable]
public class UserSettings
{
	public static UserSettings Default => new()
	{
		ViewData = new()
		{
			Location = new(0, 0),
			Size = new(1280, 720),
			LeftPanelWidth = 250,
			BottomPanelHeight = 300
		}
	};
	
	[JsonPropertyName("view_data")]
	public required ViewData ViewData { get; set; }
}