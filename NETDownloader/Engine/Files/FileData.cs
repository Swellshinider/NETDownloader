namespace NETDownloader.Engine.Files;

public abstract record FileData
{
	public FileData(string title, TitleType type, ExtensionType extension)
	{
		Title = title;
		CleanedTitle = string.Concat(Title.Split(Path.GetInvalidFileNameChars()));
		Type = type;
		Extension = extension;
	}

	public string Title { get; protected set; }
	public string CleanedTitle { get; protected set; }
	public TitleType Type { get; protected set; }
	public ExtensionType Extension { get; protected set; }
	
	public string ExtensionText => Extension switch
    {
        ExtensionType.MP4 => ".mp4",
        ExtensionType.MP3 => ".mp3",
        _ => throw new InvalidDataException($"Invalid extension type found: <{Extension}>"),
    };

	public override abstract string ToString();
}