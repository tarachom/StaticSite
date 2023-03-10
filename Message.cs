using Gtk;

namespace StaticSite;

class Message
{
    public static void Info(Window? pwin, string message)
    {
        MessageDialog md = new MessageDialog(pwin, DialogFlags.DestroyWithParent, MessageType.Info, ButtonsType.Ok, message);
        md.WindowPosition = WindowPosition.Center;
        md.Run();
        md.Dispose();
        md.Destroy();
    }

    public static void Error(Window? pwin, string message)
    {
        MessageDialog md = new MessageDialog(pwin, DialogFlags.DestroyWithParent, MessageType.Warning, ButtonsType.Close, message);
        md.WindowPosition = WindowPosition.Center;
        md.Run();
        md.Dispose();
        md.Destroy();
    }

    public static ResponseType Request(Window? pwin, string message)
    {
        MessageDialog md = new MessageDialog(pwin, DialogFlags.DestroyWithParent, MessageType.Question, ButtonsType.YesNo, message);
        ResponseType response = (ResponseType)md.Run();
        md.Dispose();
        md.Destroy();

        return response;
    }
}
