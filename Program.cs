using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookStore {
  public class Program {
    private static readonly List<string> _tables = new() {
      "authors", "books", "genres", "book_authors"
    };

    private static readonly List<List<string>> _tableColumns = new() {
      new List<string> { 
        "id", "surname", "name", "patronymic", 
        "birth_date", "biography" 
      },
      new List<string> { 
        "id", "title", "description", "genre_id",
        "is_available", "publication_date",
        "popularity_score", "created_at" 
      },
      new List<string> { "id", "name" },
      new List<string> { "book_id", "author_id" }
    };

    public static async Task Main(string[] args) {
      try {
        const string ConnectionString = 
          "Host=localhost:5432;" +
          "Username=postgres;" +
          "Password=a;" +
          "Database=postgres";
        
        await BookStoreDatabase.InitializeAsync(ConnectionString);

        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();
        SetupApiRoutes(app);
        await RunConsoleInterface();

        await BookStoreDatabase.CloseConnectionAsync();
        app.Run();
      }
      catch (Exception ex) {
        Console.WriteLine($"Критическая ошибка: {ex.Message}");
      }
    }

    private static void SetupApiRoutes(WebApplication app) {
      app.MapGet("/api/tables", () => Results.Json(_tables));

      app.MapGet("/api/{table}", async (string table) => {
        if (!_tables.Contains(table)) {
          return Results.BadRequest("Неверное имя таблицы");
        }
        var data = await BookStoreDatabase.ExecuteQueryAsync(
          $"SELECT * FROM {table}");
        return Results.Json(data);
      });

      app.MapPost("/api/{table}", async (HttpContext context, string table) => {
        if (!_tables.Contains(table)) {
          return Results.BadRequest("Неверное имя таблицы");
        }
        try {
          var form = await context.Request.ReadFormAsync();
          var parameters = new List<NpgsqlParameter>();
          var columns = new List<string>();
          var values = new List<string>();

          foreach (var field in _tableColumns[_tables.IndexOf(table)]) {
            if (form.ContainsKey(field)) {
              columns.Add(field);
              values.Add($"@{field}");
              parameters.Add(new NpgsqlParameter($"@{field}", form[field]));
            }
          }

          var query = $"INSERT INTO {table} " +
            $"({string.Join(", ", columns)}) " +
            $"VALUES ({string.Join(", ", values)})";
          
          await BookStoreDatabase.ExecuteCommandAsync(
            query, parameters.ToArray());
          return Results.Ok();
        }
        catch (Exception ex) {
          return Results.BadRequest(ex.Message);
        }
      });

      app.MapPut("/api/{table}/{id}", 
        async (HttpContext context, string table, string id) => {
        if (!_tables.Contains(table)) {
          return Results.BadRequest("Неверное имя таблицы");
        }
        try {
          var form = await context.Request.ReadFormAsync();
          var updates = new List<string>();
          var parameters = new List<NpgsqlParameter>();

          foreach (var field in _tableColumns[_tables.IndexOf(table)]) {
            if (form.ContainsKey(field)) {
              updates.Add($"{field} = @{field}");
              parameters.Add(new NpgsqlParameter($"@{field}", form[field]));
            }
          }

          parameters.Add(new NpgsqlParameter("@id", id));
          var idCondition = table == "genres" ? 
            "id = @id" : "id = UUID(@id)";
          
          var query = $"UPDATE {table} SET " +
            $"{string.Join(", ", updates)} " +
            $"WHERE {idCondition}";
          
          await BookStoreDatabase.ExecuteCommandAsync(
            query, parameters.ToArray());
          return Results.Ok();
        }
        catch (Exception ex) {
          return Results.BadRequest(ex.Message);
        }
      });

      app.MapDelete("/api/{table}/{id}", async (string table, string id) => {
        if (!_tables.Contains(table)) {
          return Results.BadRequest("Неверное имя таблицы");
        }
        var query = table == "genres" ?
          "DELETE FROM @table WHERE id = @id" :
          "DELETE FROM @table WHERE id = UUID(@id)";

        var parameters = new[] {
          new NpgsqlParameter("@table", table),
          new NpgsqlParameter("@id", id)
        };

        await BookStoreDatabase.ExecuteCommandAsync(query, parameters);
        return Results.Ok();
      });
    }

    private static async Task RunConsoleInterface() {
      while (true) {
        Console.WriteLine("\nМеню управления книжным магазином");
        Console.WriteLine("1. Просмотр данных");
        Console.WriteLine("2. Добавление записи");
        Console.WriteLine("3. Редактирование записи");
        Console.WriteLine("4. Удаление записи");
        Console.WriteLine("5. Выход");

        var choice = InputHelper.GetInt("Выберите действие: ", 1, 5);
        switch (choice) {
          case 1: await ShowData(); break;
          case 2: await AddData(); break;
          case 3: await UpdateData(); break;
          case 4: await DeleteData(); break;
          case 5: return;
        }
      }
    }

    private static async Task ShowData() {
      Console.WriteLine("\nДоступные таблицы:");
      for (int i = 0; i < _tables.Count; i++) {
        Console.WriteLine($"{i + 1}. {_tables[i]}");
      }

      var tableIndex = InputHelper.GetInt(
        "Выберите таблицу: ", 1, _tables.Count) - 1;
      var tableName = _tables[tableIndex];
      var data = await BookStoreDatabase.ExecuteQueryAsync(
        $"SELECT * FROM {tableName}");

      Console.WriteLine($"\nДанные из таблицы {tableName}:");
      foreach (var row in data) {
        Console.WriteLine(string.Join(" | ", row));
      }
    }

    private static async Task AddData() {
      Console.WriteLine("\nДобавление новой записи");
      Console.WriteLine("1. Автор");
      Console.WriteLine("2. Книга");
      Console.WriteLine("3. Жанр");
      Console.WriteLine("4. Связь книга-автор");

      var choice = InputHelper.GetInt("Выберите тип записи: ", 1, 4);
      switch (choice) {
        case 1: await AddAuthor(); break;
        case 2: await AddBook(); break;
        case 3: await AddGenre(); break;
        case 4: await AddBookAuthor(); break;
      }
    }

    private static async Task AddAuthor() {
      Console.WriteLine("\nДобавление нового автора");
      var surname = InputHelper.GetString("Фамилия: ");
      var name = InputHelper.GetString("Имя: ");
      var patronymic = InputHelper.GetString(
        "Отчество (необязательно): ", false);
      var birthDate = InputHelper.GetDate(
        "Дата рождения (ДД.ММ.ГГГГ): ");
      var bio = InputHelper.GetString("Биография: ");

      var query = @"INSERT INTO authors 
        (id, surname, name, patronymic, birth_date, biography) 
        VALUES (gen_random_uuid(), @surname, @name, 
        @patronymic, @birthDate, @bio)";

      var parameters = new[] {
        new NpgsqlParameter("@surname", surname),
        new NpgsqlParameter("@name", name),
        new NpgsqlParameter("@patronymic", 
          string.IsNullOrEmpty(patronymic) ? 
          DBNull.Value : patronymic),
        new NpgsqlParameter("@birthDate", birthDate),
        new NpgsqlParameter("@bio", bio)
      };

      await BookStoreDatabase.ExecuteCommandAsync(query, parameters);
      Console.WriteLine("Автор успешно добавлен");
    }

    private static async Task AddBook() {
      Console.WriteLine("\nДобавление новой книги");
      var title = InputHelper.GetString("Название книги: ");
      var description = InputHelper.GetString("Описание: ");
      var genreId = InputHelper.GetString("ID жанра: ");
      var isAvailable = InputHelper.GetBool("Доступна ли книга?");
      var pubDate = InputHelper.GetDate(
        "Дата публикации (ДД.ММ.ГГГГ): ");
      var popularity = InputHelper.GetInt(
        "Рейтинг популярности (1-10): ", 1, 10);

      var query = @"INSERT INTO books
        (id, title, description, genre_id, 
        is_available, publication_date, 
        popularity_score, created_at)
        VALUES
        (gen_random_uuid(), @title, @description, 
        @genreId, @isAvailable, @pubDate, 
        @popularity, NOW())";

      var parameters = new[] {
        new NpgsqlParameter("@title", title),
        new NpgsqlParameter("@description", description),
        new NpgsqlParameter("@genreId", genreId),
        new NpgsqlParameter("@isAvailable", isAvailable),
        new NpgsqlParameter("@pubDate", pubDate),
        new NpgsqlParameter("@popularity", popularity)
      };

      await BookStoreDatabase.ExecuteCommandAsync(query, parameters);
      Console.WriteLine("Книга успешно добавлена");
    }

    private static async Task UpdateData() {
      var tableIndex = InputHelper.GetInt(
        "Выберите таблицу (1-авторы, 2-книги, 3-жанры, 4-связи): ", 
        1, 4) - 1;
      var tableName = _tables[tableIndex];
      var id = InputHelper.GetString(
        $"Введите ID записи из {tableName}: ");

      Console.WriteLine("Выберите поле для обновления:");
      for (int i = 0; i < _tableColumns[tableIndex].Count; i++) {
        Console.WriteLine($"{i + 1}. {_tableColumns[tableIndex][i]}");
      }

      var fieldIndex = InputHelper.GetInt(
        "Номер поля: ", 1, _tableColumns[tableIndex].Count) - 1;
      var fieldName = _tableColumns[tableIndex][fieldIndex];
      Console.Write($"Новое значение для {fieldName}: ");
      var newValue = Console.ReadLine();

      var query = tableName == "genres" ?
        $"UPDATE {tableName} SET {fieldName} = @value WHERE id = @id" :
        $"UPDATE {tableName} SET {fieldName} = @value WHERE id = UUID(@id)";

      var parameters = new[] {
        new NpgsqlParameter("@value", newValue),
        new NpgsqlParameter("@id", id)
      };

      await BookStoreDatabase.ExecuteCommandAsync(query, parameters);
      Console.WriteLine("Запись успешно обновлена");
    }

    private static async Task DeleteData() {
      var tableIndex = InputHelper.GetInt(
        "Выберите таблицу (1-авторы, 2-книги, 3-жанры, 4-связи): ", 
        1, 4) - 1;
      var tableName = _tables[tableIndex];
      var id = InputHelper.GetString(
        $"Введите ID для удаления из {tableName}: ");

      var query = tableName == "genres" ?
        "DELETE FROM @table WHERE id = @id" :
        "DELETE FROM @table WHERE id = UUID(@id)";

      var parameters = new[] {
        new NpgsqlParameter("@table", tableName),
        new NpgsqlParameter("@id", id)
      };

      await BookStoreDatabase.ExecuteCommandAsync(query, parameters);
      Console.WriteLine("Запись успешно удалена");
    }
  }
}
