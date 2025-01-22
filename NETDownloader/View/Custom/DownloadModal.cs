using System.Drawing.Text;
using LealForms;
using LealForms.Controls.Buttons;
using LealForms.Controls.Forms;
using LealForms.Controls.Miscellaneous;
using LealForms.Controls.Panels;
using LealForms.Controls.TextBoxes;
using LealForms.Extensions;
using LealForms.Models;
using NETDownloader.Configuration;
using NETDownloader.Engine;
using NETDownloader.Engine.Files;

namespace NETDownloader.View.Custom;

public sealed class DownloadModal : LealModal
{
	public event EventHandler<DownloadData>? DownloadDataGenerated;

	private readonly LealPanel _background = new(true, true);
	private readonly LealTextBox _titleTextBox = new();
	private readonly LealCombo _comboFileType = new(50);
	private readonly LealTextBox _seasonTextBox = new();
	private readonly LealTextBox _episodeTextBox = new();
	private readonly LealTextBox _yearTextBox = new();

	private string _currentText = string.Empty;
	private UrlType _urlType = UrlType.Unknown;
	private TitleType _titleType = TitleType.Series;

	public DownloadModal(Size startSize, Point pointToScreenLocation) : base(startSize, pointToScreenLocation)
	{
	}

	public override void ReDraw()
	{
		this.GenerateRoundRegion();
	}

	public override void LoadComponents()
	{
		_background.Dock = DockStyle.Fill;
		this.Add(_background);

		BackColor = Color.Black;
		var settings = SettingsManager.UserSettings;

		var urlInput = new LealTextBox()
		{
			Height = 50,
			Width = Width - LealConstants.GAP * 2,
			Placeholder = "Url",
			BorderStyle = BorderStyle.FixedSingle,
			ForeColor = settings.Colors.ForegroundColor,
			BackColor = settings.Colors.SecondaryBackgroundColor,
		};
		urlInput.TextChanged += UrlTextChanged;
		_background.Add(urlInput);
		urlInput.AddX(LealConstants.GAP);

		_background.Add(_titleTextBox);
		_titleTextBox.Height = 50;
		_titleTextBox.Width = (int)(Width * 0.6);
		_titleTextBox.Placeholder = "Title";
		_titleTextBox.BorderStyle = BorderStyle.FixedSingle;
		_titleTextBox.ForeColor = SettingsManager.UserSettings.Colors.ForegroundColor;
		_titleTextBox.BackColor = SettingsManager.UserSettings.Colors.SecondaryBackgroundColor;
		_titleTextBox.AddX(LealConstants.GAP);

		_background.Add(_comboFileType);
		_comboFileType.Width = urlInput.Width - _titleTextBox.Width - LealConstants.GAP;
		_comboFileType.DropdownItemHeight = 25;
		_comboFileType.DropdownBackColor = SettingsManager.UserSettings.Colors.SecondaryBackgroundColor;
		_comboFileType.DropdownForeColor = SettingsManager.UserSettings.Colors.ForegroundColor;
		_comboFileType.ComboText.ForeColor = SettingsManager.UserSettings.Colors.ForegroundColor;
		_comboFileType.ForeColor = SettingsManager.UserSettings.Colors.ForegroundColor;
		_comboFileType.BackColor = SettingsManager.UserSettings.Colors.SecondaryBackgroundColor;
		_comboFileType.DropdownButton.ForeColor = SettingsManager.UserSettings.Colors.HighLightColor;
		_comboFileType.ComboText.ResetText();
		_comboFileType.AddItem(new LealComboItem("Series", TitleType.Series));
		_comboFileType.AddItem(new LealComboItem("Movie", TitleType.Movie));
		_comboFileType.AddItem(new LealComboItem("Song", TitleType.Song));
		_comboFileType.ItemSelected += TitleTypeChanged;

		_background.Add(_seasonTextBox);
		_seasonTextBox.Height = 50;
		_seasonTextBox.Width = 100;
		_seasonTextBox.Placeholder = "Season: ";
		_seasonTextBox.BorderStyle = BorderStyle.FixedSingle;
		_seasonTextBox.ForeColor = settings.Colors.ForegroundColor;
		_seasonTextBox.BackColor = settings.Colors.SecondaryBackgroundColor;
		_seasonTextBox.AddX(LealConstants.GAP);
	
		_background.Add(_episodeTextBox);
		_episodeTextBox.Height = 50;
		_episodeTextBox.Width = 100;
		_episodeTextBox.Placeholder = "Episode: ";
		_episodeTextBox.BorderStyle = BorderStyle.FixedSingle;
		_episodeTextBox.ForeColor = settings.Colors.ForegroundColor;
		_episodeTextBox.BackColor = settings.Colors.SecondaryBackgroundColor;
		_episodeTextBox.AddX(LealConstants.GAP);

		_background.Add(_yearTextBox);
		_yearTextBox.Height = 50;
		_yearTextBox.Width = 100;
		_yearTextBox.Visible = false;
		_yearTextBox.Placeholder = "Year: ";
		_yearTextBox.BorderStyle = BorderStyle.FixedSingle;
		_yearTextBox.ForeColor = settings.Colors.ForegroundColor;
		_yearTextBox.BackColor = settings.Colors.SecondaryBackgroundColor;
		_yearTextBox.AddX(LealConstants.GAP);

		var buttonAdd = new LealButton((s, e) => AddNew())
		{
			Height = 50,
			Width = 125,
			Text = "Add",
			ForeColor = SettingsManager.UserSettings.Colors.HighLightColor
		};
		_background.Add(buttonAdd);

		var buttonClose = new LealButton((s, e) => { _comboFileType.HideDropdown(); Close(); })
		{
			Height = 50,
			Width = 125,
			Text = "Close",
			ForeColor = SettingsManager.UserSettings.Colors.HighLightColor,
		};
		_background.Add(buttonClose);

		_background.WaterFallChildControlsOfTypeByY<LealTextBox>(LealConstants.GAP, LealConstants.GAP / 2);
		_background.CentralizeWithSpacingChildrensOfTypeByX<LealButton>(LealConstants.GAP * 2);
		buttonAdd.DockBottomWithPadding(LealConstants.GAP);
		buttonClose.DockBottomWithPadding(LealConstants.GAP);
		_comboFileType.SetXAfterControl(_titleTextBox, LealConstants.GAP);
		_comboFileType.SetY(_titleTextBox.Location.Y);
		_yearTextBox.Location = _seasonTextBox.Location;
		_episodeTextBox.SetXAfterControl(_seasonTextBox, LealConstants.GAP);
		_episodeTextBox.SetYAfterControl(_titleTextBox, LealConstants.GAP / 2);
		ReDraw();
	}

	private void TitleTypeChanged(object? sender, LealComboItem? e)
	{
		_titleType = Enum.Parse<TitleType>(e!.Value.ToString()!);
		_yearTextBox.Text = "";
		_seasonTextBox.Text = "";
		_episodeTextBox.Text = "";

		switch (_titleType)
		{
			case TitleType.Movie:
				_yearTextBox.Visible = true;
				_seasonTextBox.Visible = false;
				_episodeTextBox.Visible = false;
				break;
			case TitleType.Song:
				_yearTextBox.Visible = true;
				_seasonTextBox.Visible = false;
				_episodeTextBox.Visible = false;
				break;
			case TitleType.Series:
			default:
				_yearTextBox.Visible = false;
				_seasonTextBox.Visible = true;
				_episodeTextBox.Visible = true;
				break;
		}
	}

	private void AddNew()
	{
		if (string.IsNullOrEmpty(_currentText))
		{
			MessageBox.Show("Url cannot be empty", "Invalid Url", MessageBoxButtons.OK);
			return;
		}

		if (_urlType.Equals(UrlType.Unknown))
		{
			MessageBox.Show("Unknown url type, please make sure the url you are inserting is supported!",
				"Invalid Url", MessageBoxButtons.OK);
			return;
		}
		
		if (!int.TryParse(_seasonTextBox.Text, out var season))
		{
			MessageBox.Show("Season text must be a number!",
				"Invalid Number", MessageBoxButtons.OK);
			return;
		}
		
		if (!int.TryParse(_episodeTextBox.Text, out var episode))
		{
			MessageBox.Show("Season text must be a number!",
				"Invalid Number", MessageBoxButtons.OK);
			return;
		}
		
		if (string.IsNullOrEmpty(_titleTextBox.Text))
			_titleTextBox.Text = "Untitled";
			
		var seriesData = new SeriesData(_titleTextBox.Text, season, episode, _titleType, ExtensionType.MP4);
		var downloadData = new DownloadData(_currentText, seriesData);
		DownloadDataGenerated?.Invoke(this, downloadData);
	}

	private async void UrlTextChanged(string text, EventArgs e)
	{
		_currentText = text;
		_urlType = await AnalyzeUrl(text);
		
		Program.Logger.Info($"UrlType <{_urlType}> for <{text}>");
	}

	private static async Task<UrlType> AnalyzeUrl(string text)
	{
		if (string.IsNullOrEmpty(text))
			return UrlType.Unknown;

		if (text.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
			return UrlType.Mp4;

		if (text.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
			return UrlType.Mp3;

		if (text.Contains("youtu.be") || text.Contains("youtube.com"))
			return UrlType.YTube;

		if (text.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
			return UrlType.M3U8Playlist;
		
		if (!Uri.TryCreate(text, UriKind.Absolute, out Uri? uriResult) || 
			!(uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
		{
			return UrlType.Unknown;
		}

		// Check for M3U8-specific markers
		using var client = new HttpClient();

		var content = await client.GetStringAsync(text);

		if (content.Contains("#EXTM3U") &&
			content.Contains("#EXT-X-STREAM-INF"))
		{
			return UrlType.M3U8Playlist;
		}

		return UrlType.Unknown;
	}
}