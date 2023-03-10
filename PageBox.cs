using Gtk;

namespace StaticSite;

public class PageBox : VBox
{
    public Page page { get; set; } = new Page();

    Entry Назва = new Entry();
    TextView Вміст = new TextView() { WrapMode = WrapMode.Word };

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

        PackStart(CreateToolBar(), false, false, 0);

        CreatePack1();

        ShowAll();
    }

    void CreatePack1()
    {
        VBox vBox = new VBox();

        //Назва
        HBox hBoxName = new HBox();
        vBox.PackStart(hBoxName, false, true, 5);

        hBoxName.PackStart(new Label("Назва:"), false, false, 5);
        hBoxName.PackStart(Назва, true, true, 5);

        //Вміст
        HBox hBoxDesc = new HBox();
        vBox.PackStart(hBoxDesc, true, true, 5);

        hBoxDesc.PackStart(new Label("Вміст:") { Valign = Align.Start }, false, false, 5);

        ScrolledWindow scrollTextView = new ScrolledWindow() { ShadowType = ShadowType.In };
        scrollTextView.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);
        scrollTextView.Add(Вміст);

        hBoxDesc.PackStart(scrollTextView, true, true, 5);

        PackStart(vBox, true, true, 0);
    }

    HBox CreateToolBar()
    {
        HBox hBox = new HBox();

        Toolbar toolbar = new Toolbar();

        ToolButton upButton = new ToolButton(Stock.Edit) { Label = "P", IsImportant = true, TooltipText = "P" };
        upButton.Clicked += EditButtonClick;
        toolbar.Add(upButton);

        // ToolButton copyButton = new ToolButton(Stock.Copy) { TooltipText = "Копіювати" };
        // copyButton.Clicked += CopyButtonClick;
        // toolbar.Add(copyButton);

        // ToolButton deleteButton = new ToolButton(Stock.Delete) { TooltipText = "Видалити" };
        // deleteButton.Clicked += DeleteButtonClick;
        // toolbar.Add(deleteButton);

        // //Separator
        // ToolItem toolItemSeparator = new ToolItem();
        // toolItemSeparator.Add(new Separator(Orientation.Horizontal));
        // toolbar.Add(toolItemSeparator);

        // ToolButton buildButton = new ToolButton(Stock.Convert) { Label = "Збудувати", IsImportant = true, TooltipText = "Збудувати" };
        // buildButton.Clicked += BuildButtonClick;
        // toolbar.Add(buildButton);

        hBox.PackStart(toolbar, false, false, 2);

        return hBox;
    }

    void EditButtonClick(object? sender, EventArgs args)
    {
        Gtk.TextIter A;
        Gtk.TextIter B;
        if (Вміст.Buffer.GetSelectionBounds(out A, out B))
        {
            string text =  Вміст.Buffer.GetText(A, B, true);
            Вміст.Buffer.DeleteInteractive(ref A,ref B, true);
            Вміст.Buffer.Insert(ref A, "<p>" + text + "</p>");
        }
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