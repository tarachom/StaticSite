using Gtk;

using Microsoft.Data.Sqlite;

namespace StaticSite;

class FirstWindow : Window
{
    Notebook topNotebook = new Notebook()
    {
        Scrollable = true, //Прокрутка сторінок блокнота
        EnablePopup = true,
        BorderWidth = 0,
        ShowBorder = false,
        TabPos = PositionType.Top
    };

    TreeStore treeStore = new TreeStore(
        typeof(string),
        typeof(string)
    );

    enum Columns
    {
        Name,
        Code
    }

    TreeView treeView;

    List<string> TreeRowExpanded;

    enum Groups
    {
        PostgreSQL,
        CSharp,
        Gtk,
        XSLT
    }

    public FirstWindow() : base("StaticSite")
    {
        SetDefaultSize(1200, 900);
        SetPosition(WindowPosition.Center);
        BorderWidth = 5;

        DeleteEvent += delegate
        {
            CloseDB();
            Program.Quit();
        };

        OpenDB();

        //Основний контейнер
        VBox vBox = new VBox();
        Add(vBox);

        vBox.PackStart(CreateToolBar(), false, false, 0);

        treeView = new TreeView(treeStore);
        treeView.RowActivated += OnRowActivated;
        treeView.RowExpanded += OnRowExpanded;
        treeView.RowCollapsed += OnRowCollapsed;

        TreeRowExpanded = new List<string>();

        AddColumn();

        HPaned hPaned = new HPaned() { Position = 300 };
        hPaned.Pack1(treeView, false, false);
        hPaned.Pack2(topNotebook, true, false);

        vBox.PackStart(hPaned, true, true, 0);

        LoadGroups();

        ShowAll();
    }

    #region SQL

    void OpenDB()
    {
        Page.Conn = new SqliteConnection("Data Source=database.db");
        Page.Conn.Open();

        Page.CreateDataBase();

        /*
        Page page = new Page
        {
            GroupName = Groups.XSLT.ToString(),
            Name = "test2",
            Value = "test3"
        };

        Page.InsertPage(page);
        */
    }

    void CloseDB()
    {
        if (Page.Conn != null)
            Page.Conn.Close();
    }

    #endregion

    #region TreeView

    void AddColumn()
    {
        treeView.AppendColumn(new TreeViewColumn("Розділи", new CellRendererText(), "text", (int)Columns.Name));
        treeView.AppendColumn(new TreeViewColumn("Код", new CellRendererText(), "text", (int)Columns.Code));
    }

    public void LoadGroups()
    {
        treeStore.Clear();

        TreeIter rootIter = treeStore.AppendValues("Розділи", "");

        foreach (string name in Enum.GetNames<Groups>())
        {
            TreeIter itemIter = treeStore.AppendValues(rootIter, name);
            LoadPages(itemIter, name);
        }

        treeView.ExpandToPath(treeStore.GetPath(rootIter));
    }

    void LoadPages(TreeIter itemIter, string groupName)
    {
        foreach (Page page in Page.SelectPages(groupName))
            treeStore.AppendValues(itemIter, page.Name, page.ID);

        IsExpand(itemIter);
    }

    void OnRowActivated(object sender, RowActivatedArgs args)
    {
        if (treeView.Selection.CountSelectedRows() != 0)
        {
            TreeIter iter;
            treeView.Model.GetIter(out iter, treeView.Selection.GetSelectedRows()[0]);

            string id = (string)treeView.Model.GetValue(iter, (int)Columns.Code);

            if (!String.IsNullOrEmpty(id))
            {
                Page? page = Page.SelectPage(long.Parse(id));

                if (page != null)
                {
                    PageBox pageBox = new PageBox
                    {
                        page = page
                    };

                    pageBox.SetValue();

                    CreateNotebookPage($"{page.Name} *", pageBox);
                }
            }
        }
    }

    void IsExpand(TreeIter iter)
    {
        TreePath path = treeView.Model.GetPath(iter);

        if (TreeRowExpanded.Contains(path.ToString()))
            treeView.ExpandToPath(path);
    }

    void OnRowExpanded(object sender, RowExpandedArgs args)
    {
        if (!TreeRowExpanded.Contains(args.Path.ToString()))
            TreeRowExpanded.Add(args.Path.ToString());
    }

    void OnRowCollapsed(object sender, RowCollapsedArgs args)
    {
        if (TreeRowExpanded.Contains(args.Path.ToString()))
            TreeRowExpanded.Remove(args.Path.ToString());
    }

    #endregion

    #region  ToolBar

    HBox CreateToolBar()
    {
        HBox hBox = new HBox();

        Toolbar toolbar = new Toolbar();

        MenuToolButton addButton = new MenuToolButton(Stock.Add) { Label = "Додати", IsImportant = true, TooltipText = "Додати" };
        addButton.Menu = AddSubMenu();
        addButton.Clicked += AddButtonClick;
        toolbar.Add(addButton);

        ToolButton upButton = new ToolButton(Stock.Edit) { Label = "Редагувати", IsImportant = true, TooltipText = "Редагувати" };
        toolbar.Add(upButton);

        ToolButton copyButton = new ToolButton(Stock.Copy) { Label = "Копіювати", IsImportant = true, TooltipText = "Копіювати" };
        toolbar.Add(copyButton);

        ToolButton deleteButton = new ToolButton(Stock.Delete) { Label = "Видалити", IsImportant = true, TooltipText = "Видалити" };
        toolbar.Add(deleteButton);

        hBox.PackStart(toolbar, false, false, 2);

        return hBox;
    }

    Menu AddSubMenu()
    {
        Menu Menu = new Menu();

        foreach (string name in Enum.GetNames<Groups>())
        {
            MenuItem item = new MenuItem(name) { Name = name };
            item.Activated += AddPage;
            Menu.Append(item);
        }

        Menu.ShowAll();

        return Menu;
    }

    void AddButtonClick(object? sender, EventArgs args)
    {
        if (sender != null)
        {
            MenuToolButton addButton = (MenuToolButton)sender;
            Menu Menu = (Menu)addButton.Menu;
            Menu.Popup();
        }
    }

    void AddPage(object? sender, EventArgs args)
    {
        if (sender != null)
        {
            MenuItem item = (MenuItem)sender;

            PageBox pageBox = new PageBox
            {
                page = new Page { IsNew = true, GroupName = item.Name }
            };

            CreateNotebookPage($"{item.Name} *", pageBox);
        }
    }

    #endregion

    #region Notebook

    /// <summary>
    /// Створити сторінку в блокноті
    /// </summary>
    /// <param name="tabName">Назва сторінки</param>
    /// <param name="pageWidget">Віджет для сторінки</param>
    /// <param name="insertPage">Вставити сторінку перед поточною</param>
    void CreateNotebookPage(string tabName, Widget? pageWidget, bool insertPage = false)
    {
        int numPage;
        string codePage = Guid.NewGuid().ToString();

        ScrolledWindow scroll = new ScrolledWindow() { ShadowType = ShadowType.In, Name = codePage };
        scroll.SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

        HBox hBoxLabel = CreateLabelPageWidget(tabName, codePage);

        if (insertPage)
            numPage = topNotebook.InsertPage(scroll, hBoxLabel, topNotebook.CurrentPage);
        else
            numPage = topNotebook.AppendPage(scroll, hBoxLabel);

        if (pageWidget != null)
        {
            pageWidget.Name = codePage;
            scroll.Add(pageWidget);
        }

        topNotebook.ShowAll();
        topNotebook.CurrentPage = numPage;
        topNotebook.GrabFocus();
    }

    /// <summary>
    /// Заголовок сторінки блокноту
    /// </summary>
    /// <param name="caption">Заголовок</param>
    /// <param name="codePage">Код сторінки</param>
    /// <param name="notebook">Блокнот</param>
    /// <returns></returns>
    HBox CreateLabelPageWidget(string caption, string codePage)
    {
        HBox hBoxLabel = new HBox();

        Label label = new Label { Text = caption, Expand = false, Halign = Align.Start };
        hBoxLabel.PackStart(label, false, false, 4);

        //Лінк закриття сторінки
        LinkButton lbClose = new LinkButton("Закрити", " ")
        {
            Halign = Align.Start,
            Image = new Image("images/clean.png"),
            AlwaysShowImage = true,
            Name = codePage
        };

        lbClose.Clicked += (object? sender, EventArgs args) =>
        {
            //Пошук сторінки по коду і видалення
            topNotebook.Foreach(
                (Widget wg) =>
                {
                    if (wg.Name == codePage)
                        topNotebook.DetachTab(wg);
                });
        };

        hBoxLabel.PackEnd(lbClose, false, false, 0);
        hBoxLabel.ShowAll();

        return hBoxLabel;
    }

    /// <summary>
    /// Закриває сторінку по коду
    /// </summary>
    /// <param name="codePage">Код</param>
    public void CloseNotebookPage(string codePage)
    {
        topNotebook.Foreach(
            (Widget wg) =>
            {
                if (wg.Name == codePage)
                    topNotebook.DetachTab(wg);
            });
    }

    #endregion

}