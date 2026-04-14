using System.Globalization;

namespace DesktopIconAutoHide;

static class Program
{
    [STAThread]
    static void Main()
    {
        var uiCulture = CultureInfo.InstalledUICulture;
        CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
        CultureInfo.CurrentUICulture = uiCulture;

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());
    }
}
