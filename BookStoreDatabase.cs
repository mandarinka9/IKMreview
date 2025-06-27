using Npgsql;

namespace BookStore {
  /// <summary>
  /// Класс для работы с базой данных книжного магазина
  /// </summary>
  public static class BookStoreDatabase {
    private static NpgsqlConnection? _connection;

    /// <summary>
    /// Инициализирует подключение к базе данных
    /// </summary>
    /// <param name="connectionString">Строка подключения к базе данных</param>
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
    /// Выполняет SQL-запрос и возвращает результат
    /// </summary>
    /// <param name="query">SQL-запрос для выполнения</param>
    /// <returns>Список строк с результатами запроса</returns>
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
    /// Выполняет SQL-команду без возврата данных
    /// </summary>
    /// <param name="query">SQL-команда для выполнения</param>
    public static async Task ExecuteCommandAsync(string query) {
      if (_connection == null) {
        throw new InvalidOperationException("Соединение не инициализировано");
      }
      using var cmd = new NpgsqlCommand(query, _connection);
      await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Закрывает соединение с базой данных
    /// </summary>
    public static async Task CloseConnectionAsync() {
    if (_connection != null) {
        await _connection.CloseAsync();
        _connection.Dispose(); 
        _connection = null;
        Console.WriteLine("Соединение с базой данных закрыто");
    }
  }
}
