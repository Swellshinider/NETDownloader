using LealForms.Controls.Forms;
using LealForms.Controls.Miscellaneous;
using LealForms.Controls.Panels;
using LealForms.Extensions;

namespace NETDownloader.View;

internal sealed class MainForm : LealForm
{
	private readonly MenuStrip _menuStrip = new();
	private readonly SplitContainer _backgroundPanel = new();
	private readonly LealPanel _leftPanel = new(false, true);
	private readonly LealPanel _containerPanel = new(false, true);
	private readonly LealSeparator _lateralSeparator = new();

	private int _lastSplitterLeftSize = 0;
	private bool _isResizing = false;

	public MainForm() : base(true)
	{
		Text = $"NETDownloader | by: Swellshinider";
		MinimumSize = new(640, 320); // 360p
		ResizeBegin += Form_ResizeBegin;
		Resize += Form_Resizing;
		ResizeEnd += Form_ResizeEnd;
		FormClosing += Form_Closing;
	}

	public override void ReDraw()
	{
		_lateralSeparator.DockTopBottomLeftWithPadding(0, 0, 0);
		_containerPanel.DockFillWithPadding(_lateralSeparator.Width, 0, 0, 0);
	}

	public override void LoadComponents()
	{
		#region [ MenuStrip ]
		Program.Logger.Debug("Loading MenuStrip.");

		this.Add(_menuStrip);
		_menuStrip.Dock = DockStyle.Top;
		_menuStrip.BackColor = Color.FromArgb(0xCC, 0xCC, 0xCC);

		MenuStripLoad_Settings();
		MenuStripLoad_Help();

		Program.Logger.Debug("MenuStrip loaded.");
		#endregion

		#region [ Controls ]
		this.Add(_backgroundPanel);
		Program.Logger.Debug("Loading Base Controls.");
		
		_backgroundPanel.SuspendLayout();
		_backgroundPanel.DockFillWithPadding(0, 0, 0, _menuStrip.Height);
		_backgroundPanel.Panel1MinSize = 64;
		_backgroundPanel.Panel2MinSize = 490;
		_backgroundPanel.SplitterDistance = 250; // TODO: get the last size saved from settings
		_backgroundPanel.SplitterIncrement = 5;
		_backgroundPanel.SplitterWidth = 6;
		_backgroundPanel.SplitterMoving += SplitterMoving;
		_backgroundPanel.SplitterMoved += SplitterMoved;
		
		_backgroundPanel.Panel1.Add(_leftPanel);
		_backgroundPanel.Panel2.Add(_lateralSeparator);
		_backgroundPanel.Panel2.Add(_containerPanel);
		
		_leftPanel.Dock = DockStyle.Fill;
		_lateralSeparator.LineThickness = 3;
		_lateralSeparator.LineSpacing = 1;
		_lateralSeparator.Width = 5;
		_lateralSeparator.Orientation = Orientation.Vertical;
		_lateralSeparator.LineColor = Color.Cyan.Darken(0.2);
		
		_lateralSeparator.DockTopBottomLeftWithPadding(0, 0, 0);
		_containerPanel.DockFillWithPadding(_lateralSeparator.Width, 0, 0, 0);
		_backgroundPanel.ResumeLayout(true);
		
		Program.Logger.Debug("Base Controls loaded.");
		#endregion
		
		#region [ Theme ]
		Program.Logger.Debug("Loading Themes.");
		
		// Checks if the user's system's theme is set to dark mode.
		// TODO: Also check the settings preference (if the user wants the dark theme, even if the system is not set)
		if (ExternalExtensions.ShouldSystemUseDarkMode())
		{
			if (!Handle.UseImmersiveDarkMode(true)) 
			{	
				Program.Logger.Warn("Was not possible to set the window theme to dark." + 
					"OS version does not support dark mode or if the window handle is invalid");
			}
		}
		
		Program.Logger.Debug("Themes loaded.");
		#endregion
	}

	private void MenuStripLoad_Settings()
	{
		var fileMenu = CreateMenuItem("Settings");

		_menuStrip.Items.Add(fileMenu);
		fileMenu.DropDownItems.Add(CreateMenuItem("Appearance"));
		fileMenu.DropDownItems.Add(CreateMenuItem("Preferences", Keys.Control | Keys.OemPeriod));
		fileMenu.DropDownItems.Add(new ToolStripSeparator());
		fileMenu.DropDownItems.Add(CreateMenuItem("Exit", Keys.Alt | Keys.F4, (s, e) => Close()));
	}

	private void MenuStripLoad_Help()
	{
		var helpMenu = CreateMenuItem("Help");

		_menuStrip.Items.Add(helpMenu);
		helpMenu.DropDownItems.Add(CreateMenuItem("About", Keys.F1));
		helpMenu.DropDownItems.Add(new ToolStripSeparator());
		helpMenu.DropDownItems.Add(CreateMenuItem("Documentation"));
		helpMenu.DropDownItems.Add(CreateMenuItem("Report Issue"));
		helpMenu.DropDownItems.Add(CreateMenuItem("Release Notes"));
		helpMenu.DropDownItems.Add(new ToolStripSeparator());
		helpMenu.DropDownItems.Add(CreateMenuItem("View License"));
		helpMenu.DropDownItems.Add(new ToolStripSeparator());
		helpMenu.DropDownItems.Add(CreateMenuItem("Debug Console", Keys.Control | Keys.Shift | Keys.Y));
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
	}
	
	private void Form_Closing(object? sender, FormClosingEventArgs e)
	{
		// TODO: check if has any process running before closing it
	}

	private void SplitterMoving(object? sender, SplitterCancelEventArgs e)
	{
		// TODO: check when splitter has a good size to display only icons on the left panel
	}

	private void SplitterMoved(object? sender, SplitterEventArgs e)
	{
		if (!_isResizing)
			_lastSplitterLeftSize = _backgroundPanel.SplitterDistance;
	}
	
	private static ToolStripMenuItem CreateMenuItem(string text, Keys shortcutKeys = Keys.None, EventHandler? handler = null)
	{
		var item = new ToolStripMenuItem(text);

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