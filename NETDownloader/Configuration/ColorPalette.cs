namespace NETDownloader.Configuration;

[Serializable]
public class ColorPalette
{
    public Color BackgroundColor { get; set; }
	public Color SecondaryBackgroundColor { get; set; }
	public Color ForegroundColor { get; set; }
	public Color HighLightColor { get; set; }
	public Color ContrastBackColor { get; set; }
    public Color DownloadFinishedColor { get; set; }
    public Color DownloadStartColor { get; set; }
}