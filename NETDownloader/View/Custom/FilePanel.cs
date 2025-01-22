using LealForms.Controls.Buttons;
using LealForms.Controls.Panels;
using LealForms.Extensions;
using NETDownloader.Configuration;
using NETDownloader.Engine;
using Xabe.FFmpeg.Events;

namespace NETDownloader.View.Custom;

public sealed class FilePanel : LealPanel
{
	private readonly Label _titleLabel = new();
	private readonly Label _progressLabel = new();
	private readonly LealButton _buttonError = new();
	private readonly ProgressBar _progressBar = new();

	private readonly Color _beginColor;
	private readonly Color _endColor;

	private bool _inProgress = false;
	private bool _finished = false;

	public FilePanel(DownloadData data)
	{
		DownloadData = data;
		Height = SettingsManager.UserSettings.CardHeight;
		BackColor = SettingsManager.UserSettings.Colors.BackgroundColor;
		_beginColor = SettingsManager.UserSettings.Colors.DownloadStartColor;
		_endColor = SettingsManager.UserSettings.Colors.DownloadFinishedColor;
	}

	public DownloadData DownloadData { get; }
	public bool InProgress => _inProgress;
	public bool Finished => _finished;

	protected override void LoadComponents()
	{
		BorderStyle = BorderStyle.Fixed3D;

		this.Add(_titleLabel);
		_titleLabel.Text = $"{DownloadData.Data}";
		_titleLabel.Height = 35;
		_titleLabel.AutoSize = false;
		_titleLabel.Dock = DockStyle.Top;
		_titleLabel.ForeColor = SettingsManager.UserSettings.Colors.ForegroundColor;
		_titleLabel.TextAlign = ContentAlignment.MiddleLeft;
		
		this.Add(_progressBar);
		_progressBar.Dock = DockStyle.Bottom;
		_progressBar.Height = 4;
		_progressBar.Value = 0;
		_progressBar.Minimum = 0;
		_progressBar.Maximum = 100;
		_progressBar.BackColor = SettingsManager.UserSettings.Colors.SecondaryBackgroundColor;

		this.Add(_progressLabel);
		_progressLabel.Text = $"Not started";
		_progressLabel.Height = 35;
		_progressLabel.AutoSize = false;
		_progressLabel.Dock = DockStyle.Bottom;
		_progressLabel.ForeColor = SettingsManager.UserSettings.Colors.ForegroundColor;
		_progressLabel.TextAlign = ContentAlignment.MiddleLeft;

		this.Add(_buttonError);
		_buttonError.Text = "Error details";
		_buttonError.Visible = false;
		_buttonError.ForeColor = SettingsManager.UserSettings.Colors.ForegroundColor;
		_buttonError.DockBottomLeftWithPadding(_progressBar.Height + _progressLabel.Height, 0);

		this.DockLeftRightWithPadding(0, 0);
		_progressBar.BringToFront();
	}

	public void Begin()
	{
		_inProgress = true;
		BackColor = _beginColor;

		if (_progressBar.InvokeRequired)
		{
			_progressBar.Invoke(() =>
			{
				_progressBar.Value = 0;
				_progressLabel.Text = $"Started";
			});
		}
		else
		{
			_progressBar.Value = 0;
			_progressLabel.Text = $"Started";
		}
	}

	public void UpdateProgress(ConversionProgressEventArgs progress)
	{
		var percentage = Math.Min(100, Math.Max(0, progress.Percent));
		BackColor = _beginColor.BlendColors(_endColor, percentage / 100);

		if (_progressBar.InvokeRequired)
		{
			_progressBar.Invoke(() =>
			{
				_progressBar.Value = percentage;
				_progressLabel.Text = $"Progress: {FormatSpan(progress.Duration)}/{FormatSpan(progress.TotalLength)} ({percentage}%)";
			});
		}
		else
		{
			_progressBar.Value = percentage;
			_progressLabel.Text = $"Progress: {FormatSpan(progress.Duration)}/{FormatSpan(progress.TotalLength)} ({percentage}%)";
		}
	}

	public void Finish(TimeSpan timeSpan)
	{
		_inProgress = false;
		_finished = true;
		BackColor = _endColor;

		if (_progressBar.InvokeRequired)
		{
			_progressBar.Invoke(() =>
			{
				_progressBar.Value = 100;
				_progressLabel.Text = $"Finished in {FormatSpan(timeSpan)}";
			});
		}
		else
		{
			_progressBar.Value = 100;
			_progressLabel.Text = $"Finished in {FormatSpan(timeSpan)}";
		}
	}

	internal void SetError(string message, TimeSpan timeSpan)
	{
		_inProgress = false;
		_finished = true;
		BackColor = _endColor;

		if (_progressLabel.InvokeRequired)
		{
			_progressLabel.Invoke(() =>
			{
				_progressLabel.Text = "Error occurred while processing";
				_buttonError!.Visible = true;
				_buttonError!.Click += (s, e) => MessageBox.Show(message, $"Error at {FormatSpan(timeSpan)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
			});
		}
		else
		{
			_progressLabel.Text = "Error occurred while processing";
			_buttonError!.Visible = true;
			_buttonError!.Click += (s, e) => MessageBox.Show(message, $"Error at {FormatSpan(timeSpan)}", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	internal void Cancel()
	{
		_inProgress = false;
		_finished = true;
		BackColor = _endColor;

		if (_progressLabel.InvokeRequired)
		{
			_progressLabel.Invoke(() =>
			{
				_progressLabel.Text = $"Cancelled";
			});
		}
		else
		{
			_progressLabel.Text = $"Cancelled";
		}
	}

	private static string FormatSpan(TimeSpan span)
		=> span.Hours > 0 ? $"{span.Hours:D2}h{span.Minutes:D2}m{span.Seconds:D2}s" : $"{span.Minutes:D2}m{span.Seconds:D2}s";
}