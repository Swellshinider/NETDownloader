using System.Diagnostics;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Events;

namespace NETDownloader.Engine.M3U8;

public sealed class M3U8Converter : IDisposable
{
	public delegate void FileStarted(DownloadData fileData);
	public event FileStarted? OnFileStarted;

	public delegate void FileProgress(DownloadData fileData, ConversionProgressEventArgs eventArgs);
	public event FileProgress? OnFileProgress;

	public delegate void FileCompleted(DownloadData fileData, string finalPath, TimeSpan timeSpent);
	public event FileCompleted? OnFileCompleted;

	public delegate void FileCancelled(DownloadData fileData, CancellationToken cancellationToken);
	public event FileCancelled? OnFileCancelled;

	public delegate void FileError(DownloadData fileData, Exception exception);
	public FileError? OnErrorHappened;

	private readonly bool _gpuUsage;
	private readonly SemaphoreSlim _semaphore;
	private readonly CancellationTokenSource _cancellationTokenSource;

	public M3U8Converter(bool gpuUsage, int maximumThreads)
	{
		_gpuUsage = gpuUsage;
		_semaphore = new(maximumThreads);
		_cancellationTokenSource = new();
	}

	public async Task CancelAsync() => await _cancellationTokenSource.CancelAsync();

	public async Task Convert(List<DownloadData> dataList, string outputFolder)
	{
		var tasks = dataList.Select(async downloadData =>
		{
			await _semaphore.WaitAsync();

			try
			{
				await ConvertSingle(downloadData, outputFolder);
			}
			catch (Exception ex)
			{
				OnErrorHappened?.Invoke(downloadData, ex);
			}
			finally
			{
				_semaphore.Release();
			}

		}).ToList();

		await Task.WhenAll(tasks);
	}

	public async Task ConvertSingle(DownloadData downloadData, string outputFolder)
	{
		try
		{
			var finalPath = Path.Combine(outputFolder, $"{downloadData.Data.CleanedTitle}{downloadData.Data.ExtensionText}");
			var conversionProcess = Xabe.FFmpeg.FFmpeg.Conversions.New()
				.AddParameter($"-i \"{downloadData.Url}\"", ParameterPosition.PreInput)
				.AddParameter(_gpuUsage ? "-c:v h264_nvenc" : "-c:v libx264")
				.SetPriority(ProcessPriorityClass.AboveNormal)
				.SetOutput(finalPath);

			if (downloadData.Data.Type.Equals(TitleType.Song))
				conversionProcess.AddParameter("-vn");

			var stopWatch = Stopwatch.StartNew();
			stopWatch.Start();

			OnFileStarted?.Invoke(downloadData);

			conversionProcess.OnProgress += (sender, eventArgs) =>
			{
				OnFileProgress?.Invoke(downloadData, eventArgs);
			};

			await conversionProcess.Start(_cancellationTokenSource.Token);

			stopWatch.Stop();
			OnFileCompleted?.Invoke(downloadData, finalPath, stopWatch.Elapsed);
		}
		catch (OperationCanceledException)
		{
			OnFileCancelled?.Invoke(downloadData, _cancellationTokenSource.Token);
		}
		catch (Exception ex)
		{
			OnErrorHappened?.Invoke(downloadData, ex);
		}
	}

	public void Dispose()
	{
		_semaphore.Dispose();
		_cancellationTokenSource.Dispose();
	}
}