using Npgsql;

namespace BookStore {
  /// <summary>
  /// Сервис для работы с базой данных книжного магазина
  /// </summary>
  public static class BookStoreDatabase {
    private static NpgsqlConnection? _connection;

    /// <summary>
    /// Инициализирует подключение к базе данных
    /// </summary>
    public static async Task InitializeAsync(string connectionString) {
      try {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        var dataSource = dataSourceBuilder.Build();
        _connection = await dataSource.OpenConnectionAsync();
        Console.WriteLine("Подключение к базе данных установлено");
      }
      catch (Exception ex) {
        Console.WriteLine($"Ошибка подключения: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Выполняет запрос и возвращает данные
    /// </summary>
    public static async Task<List<List<object>>> ExecuteQueryAsync(string query) {
      if (_connection == null) {
        throw new InvalidOperationException("Соединение не инициализировано");
      }

      var result = new List<List<object>>();
      using var cmd = new NpgsqlCommand(query, _connection);

      await using var reader = await cmd.ExecuteReaderAsync();
      while (await reader.ReadAsync()) {
        var row = new List<object>();
        for (int i = 0; i < reader.FieldCount; i++) {
          row.Add(reader.GetValue(i));
        }
        result.Add(row);
      }
      return result;
    }

    /// <summary>
    /// Выполняет команду без возврата данных
    /// </summary>
    public static async Task ExecuteCommandAsync(string query) {
      if (_connection == null) {
        throw new InvalidOperationException("Соединение не инициализировано");
      }
      using var cmd = new NpgsqlCommand(query, _connection);
      await cmd.ExecuteNonQueryAsync();
    }
  }
}
