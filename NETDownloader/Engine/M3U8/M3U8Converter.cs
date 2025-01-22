using System.Diagnostics;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Events;

namespace NETDownloader.Engine.M3U8;

public sealed class M3U8Converter
{
	public delegate void FileStarted(DownloadData urlData);
	public event FileStarted? OnFileStarted;

	public delegate void FileProgress(DownloadData urlData, ConversionProgressEventArgs eventArgs);
	public event FileProgress? OnFileProgress;

	public delegate void FileCompleted(DownloadData urlData, string finalPath, TimeSpan timeSpent);
	public event FileCompleted? OnFileCompleted;

	public delegate void FileCancelled(DownloadData urlData, CancellationToken cancellationToken);
	public event FileCancelled? OnFileCancelled;

	public delegate void FileError(DownloadData urlData, Exception exception, TimeSpan timeSpent);
	public FileError? OnErrorHappened;

	private readonly bool _useGpu;
	private readonly int _maxThreads;

	public M3U8Converter(bool useGpu = true, int maxThreads = 4)
	{
		_useGpu = useGpu;
		_maxThreads = maxThreads;
	}

	public async Task ConvertAsync(List<DownloadData> urls, string outputFolder, CancellationToken cancellationToken)
	{
		using var semaphore = new SemaphoreSlim(_maxThreads);

		var processTasks = urls.Select(async urlData =>
		{
			await semaphore.WaitAsync();

			try
			{
				await ConvertSingleAsync(urlData, outputFolder, cancellationToken);
			}
			finally
			{
				semaphore.Release();
			}

		}).ToList();

		await Task.WhenAll(processTasks);
	}

	private async Task ConvertSingleAsync(DownloadData urlData, string outputFolder, CancellationToken cancellationToken)
	{
		var finalPath = Path.Combine(outputFolder, urlData.Data.ToString());
		var conversion = Xabe.FFmpeg.FFmpeg.Conversions.New()
			.AddParameter($"-i \"{urlData.Url}\"", ParameterPosition.PreInput)
			.AddParameter(_useGpu ? "-c:v h264_nvenc" : "-c:v libx264")
			.SetPriority(ProcessPriorityClass.AboveNormal)
			.SetOutput(finalPath);

		if (urlData.Data.Extension.Equals(ExtensionType.MP3))
			conversion.AddParameter("-vn");

		var stopWatch = Stopwatch.StartNew();
		stopWatch.Start();
		
		try
		{
			OnFileStarted?.Invoke(urlData);

			conversion.OnProgress += (sender, eventArgs) =>
			{
				OnFileProgress?.Invoke(urlData, eventArgs);
			};

			await conversion.Start(cancellationToken);

			stopWatch.Stop();
			OnFileCompleted?.Invoke(urlData, finalPath, stopWatch.Elapsed);
		}
		catch (OperationCanceledException oe)
		{
			OnFileCancelled?.Invoke(urlData, cancellationToken);
			Program.Logger.Warn("User cancelled operation", oe);
		}
		catch (Exception ex)
		{
			OnErrorHappened?.Invoke(urlData, ex, stopWatch.Elapsed);
			Program.Logger.Warn("Unexpected error occurred", ex);
		}
	}
}