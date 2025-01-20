using LForms.Controls.Panels;

namespace NETDownloader.View.Tabs;

internal abstract class BaseTab : LealPanel
{
    private protected BaseTab() : base(true)
    {
        Dock = DockStyle.Fill;
    }
}