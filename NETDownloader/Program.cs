using NETDownloader.View;
using LForms.Extensions;
using LForms.Enums;

namespace NETDownloader;

public static class Program
{
	[STAThread]
	public static void Main()
	{
		try
		{
			ApplicationConfiguration.Initialize();
			Application.Run(new MainForm());
		}
		catch (Exception e)
		{
			e.HandleException(ErrorType.Critical);
		}
	}
}