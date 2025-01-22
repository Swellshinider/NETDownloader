namespace NETDownloader.Engine.Files;

public sealed record SeriesData : FileData
{
	public SeriesData(string title, int season, int episode, TitleType type, ExtensionType extension)
		: base(title, type, extension)
	{
        Season = season;
        Episode = episode;
    }

    public int Season { get; }
    public int Episode { get; }

    public override string ToString() => $"{CleanedTitle} - S{Season:D2}E{Episode:D2}{ExtensionText}";
}