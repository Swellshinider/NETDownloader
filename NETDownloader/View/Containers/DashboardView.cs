using System.Drawing.Text;
using System.Threading.Tasks;
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
	private CancellationTokenSource? _cts;

	public DashboardView() : base(false, true)
	{
		_addButton = new((s, e) => Button_AddNew());
		_startButton = new((s, e) => Button_StartDownload());

		_converter = new(true, 4);
		_converter.OnFileStarted += Converter_Started;
		_converter.OnFileProgress += Conversion_Progress;
		_converter.OnFileCompleted += Converter_Completed;
		_converter.OnFileCancelled += Converter_Canceled;
		_converter.OnErrorHappened += Converter_Error;
	}

	public M3U8Converter Converter { get => _converter; }

	private IEnumerable<FilePanel> FilePanels => _downloadPanelsContainer.GetChildrenOfType<FilePanel>();

	protected override void ReDraw()
	{
		this.DockFillWithPadding(0);
		_background.DockFillWithPadding(0);
		_addButton.DockTopRightWithPadding(LealConstants.GAP, LealConstants.GAP * 2);
		_startButton.DockTopRightWithPadding(LealConstants.GAP, LealConstants.GAP * 3 + _addButton.Width);
		_downloadPanelsContainer.SetX(LealConstants.GAP);
		_downloadPanelsContainer.SetY(_startButton.Height + LealConstants.GAP * 2);
		_downloadPanelsContainer.Size = new(_background.Width - LealConstants.GAP * 2, _background.Height - LealConstants.GAP * 2 - _startButton.Height);
	}

	protected override void LoadComponents()
	{
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
		_downloadPanelsContainer.BorderStyle = BorderStyle.Fixed3D;
		_downloadPanelsContainer.DockFillWithPadding(0);

		ReDraw();
	}

	private void Button_AddNew()
	{
		var size = new Size(740, 400);
		var posX = _addButton.Location.X - size.Width + _addButton.Width;
		var posY = _addButton.Location.Y + _addButton.Height + (LealConstants.GAP / 2);
		var modal = new DownloadModal(size, PointToScreen(new(posX, posY)));
		modal.DownloadDataGenerated += DownloadDataGenerated;
		modal.ShowDialog();
	}

	private async void Button_StartDownload()
	{
		var urlsData = new List<DownloadData>();

		if (_cts != null)
		{
			await _cts.CancelAsync();
			_cts.Dispose();
		}

		_cts = new();

		foreach (var filePanel in FilePanels)
		{
			if (filePanel.InProgress || filePanel.Finished)
				continue;

			urlsData.Add(filePanel.DownloadData);
		}

		await _converter.ConvertAsync(urlsData, "C:\\Users\\dute2\\Downloads", _cts.Token);
	}

	private void DownloadDataGenerated(object? sender, DownloadData e)
	{
		if (FilePanels.Where(p => p.DownloadData == e).Any())
		{
			MessageBox.Show("This one was already added", "Already added", MessageBoxButtons.OK);
			return;
		}

		_downloadPanelsContainer.Add(new FilePanel(e));
		_downloadPanelsContainer.WaterFallChildControlsOfTypeByY<FilePanel>(0, LealConstants.GAP / 2);
	}

	private void Converter_Started(DownloadData urlData)
	{
		var conversionPanel = FilePanels.FirstOrDefault(c => c.DownloadData == urlData);

		if (conversionPanel == null)
			return;

		conversionPanel.Begin();
	}

	private void Conversion_Progress(DownloadData urlData, ConversionProgressEventArgs progress)
	{
		var conversionPanel = FilePanels.FirstOrDefault(c => c.DownloadData == urlData);

		if (conversionPanel == null)
			return;

		conversionPanel.UpdateProgress(progress);
	}

	private void Converter_Completed(DownloadData urlData, string finalPath, TimeSpan timeSpan)
	{
		var conversionPanel = FilePanels.FirstOrDefault(c => c.DownloadData == urlData);

		if (conversionPanel == null)
			return;

		conversionPanel.Finish(timeSpan);
	}

	private void Converter_Canceled(DownloadData urlData, CancellationToken cancellationToken)
	{
		var conversionPanel = FilePanels.FirstOrDefault(c => c.DownloadData == urlData);

		if (conversionPanel == null)
			return;

		conversionPanel.Cancel();
	}

	private void Converter_Error(DownloadData urlData, Exception exception, TimeSpan timeSpan)
	{
		var conversionPanel = FilePanels.FirstOrDefault(c => c.DownloadData == urlData);

		if (conversionPanel == null)
			return;

		conversionPanel.SetError(exception.Message, timeSpan);
	}

	protected override void Dispose(bool disposing)
	{
		_cts?.Dispose();
		base.Dispose(disposing);
	}
}