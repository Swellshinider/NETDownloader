using LealForms.Controls.Forms;
using LealForms.Controls.Miscellaneous;
using LealForms.Controls.Panels;
using LealForms.Extensions;
using NETDownloader.Configuration;

namespace NETDownloader.View;

internal sealed class MainView : LealForm
{
	private readonly MenuStrip _menuStrip = new();
	private readonly LealSeparator _topSeparator = new();
	private readonly SplitContainer _backgroundPanel = new();
	private readonly LealPanel _leftPanel = new(false, true);
	private readonly LealPanel _containerPanel = new(false, true);
	private readonly UserSettings _settings = SettingsManager.UserSettings;

	private int _lastSplitterLeftSize = 0;
	private bool _isResizing = false;

	public MainView() : base(true)
	{
		Text = $"NETDownloader | by: Swellshinider";
		Size = new(1280, 720); // 720p
		MinimumSize = new(640, 320); // 360p
		ResizeBegin += Form_ResizeBegin;
		Resize += Form_Resizing;
		ResizeEnd += Form_ResizeEnd;
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
		_menuStrip.BackColor = ColorPalette.BackgroundColor;

		MenuStripLoad_Settings();
		MenuStripLoad_Help();

		Program.Logger.Debug("MenuStrip loaded.");
		#endregion

		#region [ Controls ]
		Program.Logger.Debug("Loading Base Controls.");
		_topSeparator.Height = 3;
		_topSeparator.LineThickness = 3;
		_topSeparator.Orientation = Orientation.Horizontal;
		_topSeparator.LineColor = ColorPalette.HighLightColor;
		
		this.Add(_topSeparator);
		_topSeparator.DockTopLeftRightWithPadding(_menuStrip.Height, 0, 0);
		
		this.Add(_backgroundPanel);
		_backgroundPanel.SuspendLayout();
		_backgroundPanel.DockFillWithPadding(0, 0, 0, _menuStrip.Height + _topSeparator.Height);
		_backgroundPanel.Panel1MinSize = 64;
		_backgroundPanel.Panel2MinSize = 490;
		_backgroundPanel.SplitterDistance = _settings.SplitterDistance;
		_backgroundPanel.SplitterWidth = _topSeparator.LineThickness;
		_backgroundPanel.SplitterMoving += SplitterMoving;
		_backgroundPanel.SplitterMoved += SplitterMoved;
		_backgroundPanel.MouseUp += (s, e) => _containerPanel.Focus();
		
		_backgroundPanel.Panel1.Add(_leftPanel);
		_backgroundPanel.Panel2.Add(_containerPanel);
		
		_leftPanel.Dock = DockStyle.Fill;
		_containerPanel.Dock = DockStyle.Fill;
		
		_backgroundPanel.ResumeLayout(true);
		
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
		
		_backgroundPanel.BackColor = ColorPalette.HighLightColor;
		_containerPanel.BackColor = ColorPalette.BackgroundColor;
		_leftPanel.BackColor = ColorPalette.BackgroundColor;
		
		Program.Logger.Debug("Themes loaded.");
		#endregion
	}

	private void MenuStripLoad_Settings()
	{
		var settingsMenu = CreateMenuItem("Settings");
		settingsMenu.DropDown.BackColor = ColorPalette.BackgroundColor;
		
		settingsMenu.ForeColor = ColorPalette.ForegroundColor;
		settingsMenu.DropDownOpened += (s, e) => settingsMenu.ForeColor = Color.Black;
		settingsMenu.DropDownClosed += (s, e) => settingsMenu.ForeColor = ColorPalette.ForegroundColor;
		
		_menuStrip.Items.Add(settingsMenu);
		settingsMenu.DropDownItems.Add(CreateMenuItem("Appearance"));
		settingsMenu.DropDownItems.Add(CreateMenuItem("Preferences", Keys.Control | Keys.OemPeriod));
		settingsMenu.DropDownItems.Add(new ToolStripSeparator());
		settingsMenu.DropDownItems.Add(CreateMenuItem("Exit", Keys.Alt | Keys.F4, (s, e) => Close()));
	}

	private void MenuStripLoad_Help()
	{
		var helpMenu = CreateMenuItem("Help");
		helpMenu.DropDown.BackColor = ColorPalette.BackgroundColor;
		
		helpMenu.ForeColor = ColorPalette.ForegroundColor;
		helpMenu.DropDownOpened += (s, e) => helpMenu.ForeColor = Color.Black;
		helpMenu.DropDownClosed += (s, e) => helpMenu.ForeColor = ColorPalette.ForegroundColor;

		_menuStrip.Items.Add(helpMenu);
		helpMenu.DropDownItems.Add(CreateMenuItem("About", Keys.F1));
		helpMenu.DropDownItems.Add(new ToolStripSeparator());
		helpMenu.DropDownItems.Add(CreateMenuItem("Documentation"));
		helpMenu.DropDownItems.Add(CreateMenuItem("Report Issue"));
		helpMenu.DropDownItems.Add(CreateMenuItem("Release Notes"));
		helpMenu.DropDownItems.Add(new ToolStripSeparator());
		helpMenu.DropDownItems.Add(CreateMenuItem("View License"));
		helpMenu.DropDownItems.Add(new ToolStripSeparator());
		helpMenu.DropDownItems.Add(CreateMenuItem("Debug Console", Keys.Control | Keys.Shift | Keys.Y, ToggleConsole));
	}

	private void ToggleConsole(object? sender, EventArgs e)
	{
		_settings.ConsoleVisible = !_settings.ConsoleVisible;
		
		if (_settings.ConsoleVisible)
			Program.ShowConsole();
		else 
			Program.HideConsole();
	}

	private void Form_ResizeBegin(object? sender, EventArgs e)
	{
		_isResizing = true;
		_lastSplitterLeftSize = _backgroundPanel.SplitterDistance;
	}

	private void Form_Resizing(object? sender, EventArgs e)
		=> _backgroundPanel.SplitterDistance = _lastSplitterLeftSize;

	private void Form_ResizeEnd(object? sender, EventArgs e)
	{
		_isResizing = false;
		_backgroundPanel.SplitterDistance = _lastSplitterLeftSize;
		_settings.SplitterDistance = _lastSplitterLeftSize;
	}

	private void SplitterMoving(object? sender, SplitterCancelEventArgs e)
	{
		// TODO: check when splitter has a good size to display only icons on the left panel
	}

	private void SplitterMoved(object? sender, SplitterEventArgs e)
	{
		if (!_isResizing) 
		{
			_lastSplitterLeftSize = _backgroundPanel.SplitterDistance;
			_settings.SplitterDistance = _lastSplitterLeftSize;
		}
		
		_containerPanel.Focus();
	}
	
	private static ToolStripMenuItem CreateMenuItem(string text, Keys shortcutKeys = Keys.None, EventHandler? handler = null)
	{
		var item = new ToolStripMenuItem(text)
		{
			ForeColor = ColorPalette.ForegroundColor,
			BackColor = ColorPalette.BackgroundColor
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
}