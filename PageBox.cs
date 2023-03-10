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

        ToolButton tagP = new ToolButton(Stock.Edit) { Label = "P", IsImportant = true, TooltipText = "P" };
        tagP.Clicked += (object? sender, EventArgs args) => { AddTag(("p", "")); };
        toolbar.Add(tagP);

        ToolButton tagh5 = new ToolButton(Stock.Edit) { Label = "h5", IsImportant = true, TooltipText = "h5" };
        tagh5.Clicked += (object? sender, EventArgs args) => { AddTag(("h5", "")); };
        toolbar.Add(tagh5);

        ToolButton tagA = new ToolButton(Stock.Edit) { Label = "A", IsImportant = true, TooltipText = "A" };
        tagA.Clicked += (object? sender, EventArgs args) => { AddTag(("a", "target=\"_blank\" href=\"\"")); };
        toolbar.Add(tagA);

        ToolButton tagCode = new ToolButton(Stock.Edit) { Label = "Code", IsImportant = true, TooltipText = "Code" };
        tagCode.Clicked += (object? sender, EventArgs args) => { AddTag(("pre", ""), "code"); };
        toolbar.Add(tagCode);

        ToolButton tagBR = new ToolButton(Stock.Edit) { Label = "BR", IsImportant = true, TooltipText = "BR" };
        tagBR.Clicked += (object? sender, EventArgs args) => { AddOneTag(("br", "")); };
        toolbar.Add(tagBR);

        ToolButton tagHR = new ToolButton(Stock.Edit) { Label = "HR", IsImportant = true, TooltipText = "HR" };
        tagHR.Clicked += (object? sender, EventArgs args) => { AddOneTag(("hr", "")); };
        toolbar.Add(tagHR);

        ToolButton tagComment = new ToolButton(Stock.Edit) { Label = "Comment", IsImportant = true, TooltipText = "Comment" };
        tagComment.Clicked += (object? sender, EventArgs args) => { AddCommentTag(); };
        toolbar.Add(tagComment);

        ToolButton tagEscape = new ToolButton(Stock.Edit) { Label = "Escape", IsImportant = true, TooltipText = "Escape" };
        tagEscape.Clicked += (object? sender, EventArgs args) => { EscapeCode(); };
        toolbar.Add(tagEscape);

        hBox.PackStart(toolbar, false, false, 2);

        return hBox;
    }

    void AddTag((string, string) tagAndAttr, string tag2 = "")
    {
        Gtk.TextIter start;
        Gtk.TextIter end;

        Вміст.Buffer.GetSelectionBounds(out start, out end);

        {
            string selectedText = Вміст.Buffer.GetText(start, end, true);
            Вміст.Buffer.DeleteInteractive(ref start, ref end, true);

            selectedText = $"<{tagAndAttr.Item1} {tagAndAttr.Item2}>" + selectedText + $"</{tagAndAttr.Item1}>";

            if (!String.IsNullOrEmpty(tag2))
                selectedText = $"<{tag2}>" + selectedText + $"</{tag2}>";

            Вміст.Buffer.Insert(ref start, selectedText);
        }
    }

    void AddOneTag((string, string) tagAndAttr)
    {
        Gtk.TextIter start;
        Gtk.TextIter end;

        Вміст.Buffer.GetSelectionBounds(out start, out end);

        {
            string selectedText = Вміст.Buffer.GetText(start, end, true);
            Вміст.Buffer.DeleteInteractive(ref start, ref end, true);

            selectedText = selectedText + $"<{tagAndAttr.Item1} {tagAndAttr.Item2} />";

            Вміст.Buffer.Insert(ref start, selectedText);
        }
    }

void AddCommentTag()
{
    Gtk.TextIter start;
    Gtk.TextIter end;

    Вміст.Buffer.GetSelectionBounds(out start, out end);

    {
        string selectedText = Вміст.Buffer.GetText(start, end, true);
        Вміст.Buffer.DeleteInteractive(ref start, ref end, true);

        selectedText = $"<!-- [ " + selectedText + " ] -->";

        Вміст.Buffer.Insert(ref start, selectedText);
    }
}

    void EscapeCode()
    {
        Gtk.TextIter start;
        Gtk.TextIter end;

        if (Вміст.Buffer.GetSelectionBounds(out start, out end))
        {
            string selectedText = Вміст.Buffer.GetText(start, end, true);
            Вміст.Buffer.DeleteInteractive(ref start, ref end, true);

            selectedText = selectedText.Replace("<", "&lt;");
            selectedText = selectedText.Replace(">", "&gt;");

            Вміст.Buffer.Insert(ref start, selectedText);
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