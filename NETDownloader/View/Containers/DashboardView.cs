using LealForms;
using LealForms.Controls.Buttons;
using LealForms.Controls.Panels;
using LealForms.Extensions;
using NETDownloader.Configuration;
using NETDownloader.Engine;
using NETDownloader.View.Custom;

namespace NETDownloader.View.Containers;

public sealed class DashboardView : LealPanel
{
	private readonly LealPanel _background = new(false, true);
	private readonly LealPanel _downloadPanelsContainer = new(false, true);
	private readonly LealButton _addButton;
	
	public DashboardView() : base(false, true) 
	{
		_addButton = new((s, e) => OpenModal());
	}
	
	protected override void ReDraw()
	{
		_addButton.DockTopRightWithPadding(LealConstants.GAP, LealConstants.GAP * 2);
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
}