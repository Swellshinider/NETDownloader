using System.Text.Json.Serialization;

namespace NETDownloader.Configuration;

[Serializable]
public class ViewData
{
	[JsonPropertyName("size")]
	public Size Size { get; set; }

	[JsonPropertyName("location")]
	public Point Location { get; set; }

	[JsonPropertyName("left_panel_width")]
	public int LeftPanelWidth { get; set; }

	[JsonPropertyName("bottom_panel_height")]
	public int BottomPanelHeight { get; set; }
}