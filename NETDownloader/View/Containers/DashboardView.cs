using System.Drawing.Text;
using LealForms;
using LealForms.Controls.Buttons;
using LealForms.Controls.Panels;
using LealForms.Extensions;
using NETDownloader.Configuration;
using NETDownloader.Engine;
using NETDownloader.Engine.M3U8;
using NETDownloader.View.Custom;
using Xabe.FFmpeg.Events;

namespace NETDownloader.View.Containers;

public sealed class DashboardView : LealPanel
{
	private readonly LealPanel _background = new(false, true);
	private readonly LealPanel _downloadPanelsContainer = new(false, true);
	private readonly LealButton _addButton;
	private readonly LealButton _startButton;
	private readonly M3U8Converter _converter;

	public DashboardView() : base(false, true)
	{
		_addButton = new((s, e) => OpenModal());
		_startButton = new((s, e) => StartDownload());
		_converter = new(true, 4);
		_converter.OnFileStarted += StartedDownload;
		_converter.OnFileProgress += DownloadProgress;
		_converter.OnFileCompleted += DownloadCompleted;
	}

	public M3U8Converter Converter { get => _converter; }

	private IEnumerable<FilePanel> Panels => _downloadPanelsContainer.GetChildrenOfType<FilePanel>();

	protected override void ReDraw()
	{
		_addButton.DockTopRightWithPadding(LealConstants.GAP, LealConstants.GAP * 2);
		_startButton.DockTopRightWithPadding(LealConstants.GAP, LealConstants.GAP * 3 + _addButton.Width);
	}

	protected override void LoadComponents()
	{
		Dock = DockStyle.Fill;
		_background.Dock = DockStyle.Fill;
		_background.BackColor = SettingsManager.UserSettings.Colors.BackgroundColor;
		this.Add(_background);

		_background.Add(_addButton);
		_addButton.Text = "+";
		_addButton.Width = 50;
		_addButton.BorderColor = SettingsManager.UserSettings.Colors.ForegroundColor;
		_addButton.ForeColor = SettingsManager.UserSettings.Colors.ForegroundColor;
		
		_background.Add(_startButton);
		_startButton.Text = "Start";
		_startButton.Width = 100;
		_startButton.BorderColor = SettingsManager.UserSettings.Colors.ForegroundColor;
		_startButton.ForeColor = SettingsManager.UserSettings.Colors.ForegroundColor;

		_background.Add(_downloadPanelsContainer);
		_downloadPanelsContainer.DockFillWithPadding(
			LealConstants.GAP,
			_addButton.Width + LealConstants.GAP * 3,
			LealConstants.GAP * 2,
			_addButton.Width + LealConstants.GAP * 3);
		_downloadPanelsContainer.BorderStyle = BorderStyle.FixedSingle;

		ReDraw();
	}

	private void OpenModal()
	{
		var size = new Size(740, 400);
		var posX = _addButton.Location.X - size.Width + _addButton.Width;
		var posY = _addButton.Location.Y + _addButton.Height + (LealConstants.GAP / 2);
		var modal = new DownloadModal(size, PointToScreen(new(posX, posY)));
		modal.DownloadDataGenerated += DownloadDataGenerated;
		modal.ShowDialog();
	}

	private async void StartDownload()
	{
		var allData = Panels.Where(p => !p.Finished && !p.InProgress).Select(p => p.DownloadData);
		await _converter.Convert([.. allData], @"C:\Users\dute2\Downloads");
	}

	private void DownloadDataGenerated(object? sender, DownloadData e)
	{
		if (_downloadPanelsContainer.GetChildrenOfType<FilePanel>().Where(p => p.DownloadData == e).Count() > 0)
		{
			MessageBox.Show("This one was already added", "Already added", MessageBoxButtons.OK);
			return;
		}

		_downloadPanelsContainer.Add(new FilePanel(e));
		_downloadPanelsContainer.WaterFallChildControlsOfTypeByY<FilePanel>(0, LealConstants.GAP / 2);
	}

	private void StartedDownload(DownloadData fileData)
		=> Panels.Where(p => p.DownloadData == fileData).First().Begin();

	private void DownloadProgress(DownloadData fileData, ConversionProgressEventArgs eventArgs)
	 	=> Panels.Where(p => p.DownloadData == fileData).First().Update(eventArgs);

	private void DownloadCompleted(DownloadData fileData, string finalPath, TimeSpan timeSpent)
		=> Panels.Where(p => p.DownloadData == fileData).First().Finish(timeSpent);
}