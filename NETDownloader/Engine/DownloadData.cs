using NETDownloader.Engine.Files;

namespace NETDownloader.Engine;

public sealed record DownloadData
{
	public DownloadData(string url, FileData data)
	{
		Url = url;
		Data = data;
	}

	public string Url { get; }
	public FileData Data { get; }

	public override string ToString() => Data.ToString();
}