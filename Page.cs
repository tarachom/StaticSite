using Microsoft.Data.Sqlite;

namespace StaticSite;

public class Page
{
    public bool IsNew { get; set; }
    public long ID { get; set; } = 0;
    public string GroupName { get; set; } = "";
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";

    public static string ToHtml(string value)
    {
        return value.Replace("\n", "<br />");
    }

    public static SqliteConnection? Conn { get; set; } = null;
    public static void CreateDataBase()
    {
        string query = @"
CREATE TABLE IF NOT EXISTS pages 
(
    id integer PRIMARY KEY AUTOINCREMENT NOT NULL,
    name text NOT NULL DEFAULT '',
    group_name text NOT NULL DEFAULT '',
    value text NOT NULL DEFAULT ''
);

CREATE INDEX IF NOT EXISTS group_name_idx ON pages(group_name);
";

        using (SqliteCommand command = new SqliteCommand(query, Page.Conn))
        {
            command.ExecuteNonQuery();
        }
    }

    public static void Vacuum()
    {
        string query = @"VACUUM";

        using (SqliteCommand command = new SqliteCommand(query, Page.Conn))
        {
            command.ExecuteNonQuery();
        }
    }

    public static void InsertPage(Page page)
    {
        string query = @"
INSERT INTO pages(name, group_name, value)
VALUES(@name, @group_name, @value)
RETURNING id";

        using (SqliteCommand command = new SqliteCommand(query, Page.Conn))
        {
            command.Parameters.AddWithValue("name", page.Name);
            command.Parameters.AddWithValue("group_name", page.GroupName);
            command.Parameters.AddWithValue("value", page.Value);

            object? result = command.ExecuteScalar();

            if (result != null)
            {
                page.ID = (long)result;
                page.IsNew = false;
            }
        }
    }

    public static void UpdatePage(Page page)
    {
        string query = @"
UPDATE pages 
SET 
    name = @name, 
    group_name = @group_name, 
    value = @value
WHERE id = @id
";

        using (SqliteCommand command = new SqliteCommand(query, Page.Conn))
        {
            command.Parameters.AddWithValue("id", page.ID);
            command.Parameters.AddWithValue("name", page.Name);
            command.Parameters.AddWithValue("group_name", page.GroupName);
            command.Parameters.AddWithValue("value", page.Value);
            command.ExecuteNonQuery();
        }
    }

    public static void DeletePage(Page page)
    {
        string query = @"DELETE FROM pages WHERE id = @id";

        using (SqliteCommand command = new SqliteCommand(query, Page.Conn))
        {
            command.Parameters.AddWithValue("id", page.ID);
            command.ExecuteNonQuery();
        }
    }

    public static Page? SelectPage(long id)
    {
        Page? page = null;

        string query = @"
SELECT id, name, group_name, value
FROM pages
WHERE id = @id
";

        using (SqliteCommand command = new SqliteCommand(query, Page.Conn))
        {
            command.Parameters.AddWithValue("id", id);
            SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                page = new Page
                {
                    ID = (long)reader["id"],
                    Name = (string)reader["name"],
                    GroupName = (string)reader["group_name"],
                    Value = (string)reader["value"]
                };
            }

            reader.Close();
        }

        return page;
    }

    public static List<Page> SelectPages(string groupName)
    {
        List<Page> listPages = new List<Page>();

        string query = @"
SELECT id, name, group_name, value
FROM pages
WHERE group_name = @group_name
";

        using (SqliteCommand command = new SqliteCommand(query, Page.Conn))
        {
            command.Parameters.AddWithValue("group_name", groupName);
            SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                listPages.Add(new Page
                {
                    ID = (long)reader["id"],
                    Name = (string)reader["name"],
                    GroupName = (string)reader["group_name"],
                    Value = (string)reader["value"]
                });
            }

            reader.Close();
        }

        return listPages;
    }
}