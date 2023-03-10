using Gtk;

using Microsoft.Data.Sqlite;
using System.Xml;
using System.Xml.Xsl;

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
        SQLite,
        CSharp,
        Gtk,
        XSLT
    }

    const int maxName = 30;

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
        Page.Conn = new SqliteConnection($"Data Source={AppContext.BaseDirectory}database.db;");
        Page.Conn.Open();

        Page.CreateDataBase();
        Page.Vacuum();
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
        {
            string smallName = page.Name.Length > maxName ? page.Name.Substring(0, maxName) : page.Name;
            treeStore.AppendValues(itemIter, smallName, page.ID);
        }

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

                    string smallName = page.Name.Length > maxName ? page.Name.Substring(0, maxName) : page.Name;
                    CreateNotebookPage($"{smallName}", pageBox);
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

        MenuToolButton addButton = new MenuToolButton(Stock.Add) { TooltipText = "Додати" };
        addButton.Menu = AddSubMenu();
        addButton.Clicked += AddButtonClick;
        toolbar.Add(addButton);

        ToolButton upButton = new ToolButton(Stock.Edit) { TooltipText = "Редагувати" };
        upButton.Clicked += EditButtonClick;
        toolbar.Add(upButton);

        ToolButton copyButton = new ToolButton(Stock.Copy) { TooltipText = "Копіювати" };
        copyButton.Clicked += CopyButtonClick;
        toolbar.Add(copyButton);

        ToolButton deleteButton = new ToolButton(Stock.Delete) { TooltipText = "Видалити" };
        deleteButton.Clicked += DeleteButtonClick;
        toolbar.Add(deleteButton);

        //Separator
        ToolItem toolItemSeparator = new ToolItem();
        toolItemSeparator.Add(new Separator(Orientation.Horizontal));
        toolbar.Add(toolItemSeparator);

        ToolButton buildButton = new ToolButton(Stock.Convert) { Label = "Збудувати", IsImportant = true, TooltipText = "Збудувати" };
        buildButton.Clicked += BuildButtonClick;
        toolbar.Add(buildButton);

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

    void EditButtonClick(object? sender, EventArgs args)
    {
        OnRowActivated(sender!, new RowActivatedArgs());
    }

    void CopyButtonClick(object? sender, EventArgs args)
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
                    Page newPage = new Page
                    {
                        Name = page.Name,
                        GroupName = page.GroupName,
                        Value = page.Value
                    };

                    Page.InsertPage(newPage);
                    LoadGroups();
                }
            }
        }
    }

    void DeleteButtonClick(object? sender, EventArgs args)
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
                    if (Message.Request(Program.firstWindow, "Видалити запис?") == ResponseType.Yes)
                    {
                        Page.DeletePage(page);
                        LoadGroups();
                    }
                }
            }
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

    void BuildButtonClick(object? sender, EventArgs args)
    {
        string buildDir = AppContext.BaseDirectory + "html/";
        string buildDatabaseXMl = AppContext.BaseDirectory + "database.xml";

        XmlWriter xmlWriter = XmlWriter.Create(buildDatabaseXMl, new XmlWriterSettings() { Indent = true, Encoding = System.Text.Encoding.UTF8 });
        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement("root");

        foreach (string groupName in Enum.GetNames<Groups>())
        {
            xmlWriter.WriteStartElement("group");
            xmlWriter.WriteAttributeString("name", groupName);

            foreach (Page page in Page.SelectPages(groupName))
            {
                xmlWriter.WriteStartElement("page");
                xmlWriter.WriteAttributeString("id", page.ID.ToString());

                xmlWriter.WriteStartElement("name");
                xmlWriter.WriteString(page.Name);
                xmlWriter.WriteEndElement(); //name

                xmlWriter.WriteStartElement("value");
                xmlWriter.WriteCData(page.Value);
                xmlWriter.WriteEndElement(); //value

                xmlWriter.WriteEndElement(); //page
            }

            xmlWriter.WriteEndElement(); //group
        }

        xmlWriter.WriteEndElement(); //root
        xmlWriter.WriteEndDocument();
        xmlWriter.Close();

        //
        //
        //

        XslCompiledTransform xsltCodeGnerator = new XslCompiledTransform();
        xsltCodeGnerator.Load(AppContext.BaseDirectory + "xslt/template.xslt");

        foreach (string groupName in Enum.GetNames<Groups>())
        {
            string itemBuildDir = buildDir + groupName + "/";

            if (Directory.Exists(itemBuildDir))
                Directory.Delete(itemBuildDir, true);

            Directory.CreateDirectory(itemBuildDir);

            int counter = 0;

            foreach (Page page in Page.SelectPages(groupName))
            {
                XsltArgumentList xsltArgumentList = new XsltArgumentList();
                xsltArgumentList.AddParam("ID", "", page.ID);
                xsltArgumentList.AddParam("Group", "", groupName);
                xsltArgumentList.AddParam("YearCreate", "", DateTime.Now.Year);

                string fileBuildName = itemBuildDir + page.ID.ToString() + ".html";

                FileStream fileStream = new FileStream(fileBuildName, FileMode.Create);
                xsltCodeGnerator.Transform(buildDatabaseXMl, xsltArgumentList, fileStream);

                if (counter == 0)
                    File.Copy(fileBuildName, itemBuildDir + "index.html");

                counter++;
            }
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