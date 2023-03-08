using Gtk;

namespace StaticSite;

public class PageBox : VBox
{
    public Page page { get; set; } = new Page();

    Entry Назва = new Entry() { WidthRequest = 800 };
    TextView Вміст = new TextView();

    public PageBox()
    {
        HBox hBox = new HBox();
        PackStart(hBox, false, false, 10);

        Button bSaveAndClose = new Button("Зберегти та закрити");
        bSaveAndClose.Clicked += (object? sender, EventArgs args) => { Save(true); };
        hBox.PackStart(bSaveAndClose, false, false, 10);

        Button bSave = new Button("Зберегти");
        bSave.Clicked += (object? sender, EventArgs args) => { Save(); };
        hBox.PackStart(bSave, false, false, 10);

        HPaned hPaned = new HPaned() { BorderWidth = 5, Position = 800 };

        CreatePack1(hPaned);
        CreatePack2(hPaned);

        PackStart(hPaned, false, false, 5);

        ShowAll();
    }

    void CreatePack1(HPaned hPaned)
    {
        VBox vBox = new VBox();

        //Назва
        HBox hBoxName = new HBox() { Halign = Align.End };
        vBox.PackStart(hBoxName, false, false, 5);

        hBoxName.PackStart(new Label("Назва:"), false, false, 5);
        hBoxName.PackStart(Назва, false, false, 5);

        //Вміст
        HBox hBoxDesc = new HBox() { Halign = Align.End };
        vBox.PackStart(hBoxDesc, false, false, 5);

        hBoxDesc.PackStart(new Label("Вміст:") { Valign = Align.Start }, false, false, 5);

        ScrolledWindow scrollTextView = new ScrolledWindow() { ShadowType = ShadowType.In, WidthRequest = 800, HeightRequest = 600 };
        scrollTextView.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scrollTextView.Add(Вміст);

        hBoxDesc.PackStart(scrollTextView, false, false, 5);

        hPaned.Pack1(vBox, false, false);
    }

    void CreatePack2(HPaned hPaned)
    {
        VBox vBox = new VBox();
        hPaned.Pack2(vBox, false, false);
    }

    public void SetValue()
    {
        Назва.Text = page.Name;
        Вміст.Buffer.Text = page.Value;
    }

    public void GetValue()
    {
        page.Name = Назва.Text;
        page.Value = Вміст.Buffer.Text;
    }

    void Save(bool isClose = false)
    {
        GetValue();

        if (page.IsNew)
        {
            Page.InsertPage(page);
        }
        else
        {
            Page.UpdatePage(page);
        }

        Program.firstWindow!.LoadGroups();

        if (isClose)
            Program.firstWindow!.CloseNotebookPage(Name);
        
    }
}