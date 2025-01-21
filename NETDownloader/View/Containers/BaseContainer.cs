using LealForms.Controls.Panels;
using NETDownloader.Configuration;

namespace NETDownloader.View.Containers;

public abstract class BaseContainer : LealPanel
{
	public BaseContainer()
	{
		Dock = DockStyle.Fill;
		BackColor = ColorPalette.BackgroundColor;
	}
}