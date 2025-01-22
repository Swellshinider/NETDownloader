using LealForms.Controls.Forms;
using LealForms.Controls.Miscellaneous;
using LealForms.Controls.Panels;
using LealForms.Extensions;
using NETDownloader.Configuration;
using NETDownloader.View.Containers;

namespace NETDownloader.View;

internal sealed class MainView : LealForm
{
	private readonly MenuStrip _menuStrip = new();
	private readonly LealSeparator _topSeparator = new();
	private readonly LealPanel _containerPanel = new(false, true);
	private readonly DashboardView _dashboardView = new();
	private readonly UserSettings _settings = SettingsManager.UserSettings;

	public MainView() : base(true)
	{
		Text = $"NETDownloader | by: Swellshinider";
		Size = new(1280, 720); // 720p
		MinimumSize = new(640, 320); // 360p
	}
	
	public UserSettings Settings => _settings;

	public override void ReDraw()
	{
		_settings.Size = Size;
		_settings.Location = Location;
		_settings.Maximized = WindowState == FormWindowState.Maximized;
	}

	public override void LoadComponents()
	{
		#region [ Settings ]
		Program.Logger.Debug("Loading Settings...");
		
		if (!_settings.Location.Equals(Point.Empty))
			Location = _settings.Location;
			
		if (_settings.Maximized)
			WindowState = FormWindowState.Maximized;
		else
			Size = _settings.Size;
			
		if (_settings.ConsoleVisible)
			Program.ShowConsole();
			
		Program.Logger.Debug("Settings loaded.");
		#endregion
		
		#region [ MenuStrip ]
		Program.Logger.Debug("Loading MenuStrip.");

		this.Add(_menuStrip);
		_menuStrip.Dock = DockStyle.Top;
		_menuStrip.BackColor = _settings.Colors.BackgroundColor;

		MenuStripLoad_Settings();
		MenuStripLoad_Help();

		Program.Logger.Debug("MenuStrip loaded.");
		#endregion

		#region [ Controls ]
		Program.Logger.Debug("Loading Base Controls.");
		_topSeparator.Height = 3;
		_topSeparator.LineThickness = 3;
		_topSeparator.Orientation = Orientation.Horizontal;
		_topSeparator.LineColor = _settings.Colors.HighLightColor;
		
		this.Add(_topSeparator);
		_topSeparator.DockTopLeftRightWithPadding(_menuStrip.Height, 0, 0);
		
		this.Add(_containerPanel);
		_containerPanel.DockFillWithPadding(0, 0, 0, _menuStrip.Height + _topSeparator.Height);
		_containerPanel.Add(_dashboardView);
		
		Program.Logger.Debug("Base Controls loaded.");
		#endregion
		
		#region [ Theme ]
		Program.Logger.Debug("Loading Themes.");
		
		// Checks if the user's system's theme is set to dark mode.
		if (ExternalExtensions.ShouldSystemUseDarkMode())
		{
			if (!Handle.UseImmersiveDarkMode(true)) 
			{	
				Program.Logger.Warn("Was not possible to set the window theme to dark." + 
					"OS version does not support dark mode or if the window handle is invalid");
			}
		}
		
		_containerPanel.BackColor = _settings.Colors.BackgroundColor;
		
		Program.Logger.Debug("Themes loaded.");
		#endregion
	}

	private void MenuStripLoad_Settings()
	{
		var settingsMenu = CreateMenuItem("Settings");
		settingsMenu.DropDown.BackColor = _settings.Colors.BackgroundColor;
		
		settingsMenu.ForeColor = _settings.Colors.ForegroundColor;
		settingsMenu.DropDownOpened += (s, e) => settingsMenu.ForeColor = Color.Black;
		settingsMenu.DropDownClosed += (s, e) => settingsMenu.ForeColor = _settings.Colors.ForegroundColor;
		
		_menuStrip.Items.Add(settingsMenu);
		settingsMenu.DropDownItems.Add(CreateMenuItem("Appearance"));
		settingsMenu.DropDownItems.Add(CreateMenuItem("Preferences", Keys.Control | Keys.OemPeriod));
		settingsMenu.DropDownItems.Add(new ToolStripSeparator());
		settingsMenu.DropDownItems.Add(CreateMenuItem("Exit", Keys.Alt | Keys.F4, (s, e) => Close()));
	}

	private void MenuStripLoad_Help()
	{
		var helpMenu = CreateMenuItem("Help");
		helpMenu.DropDown.BackColor = _settings.Colors.BackgroundColor;
		
		helpMenu.ForeColor = _settings.Colors.ForegroundColor;
		helpMenu.DropDownOpened += (s, e) => helpMenu.ForeColor = Color.Black;
		helpMenu.DropDownClosed += (s, e) => helpMenu.ForeColor = _settings.Colors.ForegroundColor;

		_menuStrip.Items.Add(helpMenu);
		helpMenu.DropDownItems.Add(CreateMenuItem("About", Keys.F1));
		helpMenu.DropDownItems.Add(new ToolStripSeparator());
		helpMenu.DropDownItems.Add(CreateMenuItem("Report Issue"));
		helpMenu.DropDownItems.Add(CreateMenuItem("Disclaimer"));
		helpMenu.DropDownItems.Add(CreateMenuItem("Documentation"));
		helpMenu.DropDownItems.Add(CreateMenuItem("Release Notes"));
		helpMenu.DropDownItems.Add(new ToolStripSeparator());
		helpMenu.DropDownItems.Add(CreateMenuItem("View License"));
		helpMenu.DropDownItems.Add(new ToolStripSeparator());
		helpMenu.DropDownItems.Add(CreateMenuItem("Debug Console", Keys.Control | Keys.Shift | Keys.Y, ToggleConsole));
	}

	private ToolStripMenuItem CreateMenuItem(string text, Keys shortcutKeys = Keys.None, EventHandler? handler = null)
	{
		var item = new ToolStripMenuItem(text)
		{
			ForeColor = _settings.Colors.ForegroundColor,
			BackColor = _settings.Colors.BackgroundColor
		};
		
		if (shortcutKeys != Keys.None)
		{
			item.ShortcutKeys = shortcutKeys;

			if (shortcutKeys == (Keys.Control | Keys.OemPeriod))
				item.ShortcutKeyDisplayString = "Ctrl + .";
			else if (shortcutKeys == (Keys.Control | Keys.Shift | Keys.OemPeriod))
				item.ShortcutKeyDisplayString = "Ctrl + Shift + .";
		}

		if (handler != null)
			item.Click += handler;

		return item;
	}

	private void ToggleConsole(object? sender, EventArgs e)
	{
		_settings.ConsoleVisible = !_settings.ConsoleVisible;
		
		if (_settings.ConsoleVisible)
			Program.ShowConsole();
		else 
			Program.HideConsole();
	}
}