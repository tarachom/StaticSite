using Gtk;

namespace StaticSite;

class Program
{
    public static void Main()
    {
        Application.Init();
        firstWindow = new FirstWindow();
        Application.Run();
    }

    public static void Quit()
    {
        Application.Quit();
    }

    public static FirstWindow? firstWindow { get; private set; }
}
