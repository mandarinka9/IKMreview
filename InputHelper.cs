using System;
using System.Globalization;

namespace BookStore {
  /// <summary>
  /// Вспомогательный класс для безопасного ввода данных с консоли
  /// </summary>
  public static class InputHelper {
    /// <summary>
    /// Получает строковый ввод с валидацией
    /// </summary>
    public static string GetString(string prompt, bool required = true, 
        int minLength = 0, int maxLength = 255) {
      while (true) {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();

        if (!required && string.IsNullOrEmpty(input)) {
          return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(input)) {
          Console.WriteLine("Поле не может быть пустым!");
          continue;
        }

        if (input.Length < minLength) {
          Console.WriteLine($"Минимальная длина: {minLength} символов");
          continue;
        }

        if (input.Length > maxLength) {
          Console.WriteLine($"Максимальная длина: {maxLength} символов");
          continue;
        }

        return input;
      }
    }

    /// <summary>
    /// Получает целое число с валидацией
    /// </summary>
    public static int GetInt(string prompt, int min = int.MinValue, 
        int max = int.MaxValue) {
      while (true) {
        Console.Write(prompt);
        if (int.TryParse(Console.ReadLine(), out int result)) {
          if (result >= min && result <= max) {
            return result;
          }
          Console.WriteLine($"Число должно быть от {min} до {max}");
        }
        else {
          Console.WriteLine("Пожалуйста, введите целое число");
        }
      }
    }

    /// <summary>
    /// Получает дату с валидацией
    /// </summary>
    public static DateTime GetDate(string prompt, 
        string format = "dd.MM.yyyy") {
      while (true) {
        Console.Write(prompt);
        var input = Console.ReadLine();

        if (DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, 
            DateTimeStyles.None, out DateTime result)) {
          return result;
        }
        Console.WriteLine($"Введите дату в формате {format}");
      }
    }

    /// <summary>
    /// Получает булево значение (да/нет)
    /// </summary>
    public static bool GetBool(string prompt) {
      while (true) {
        Console.Write(prompt + " (y/n/да/нет): ");
        var input = Console.ReadLine()?.Trim().ToLower();

        switch (input) {
          case "y": case "д": case "да": return true;
          case "n": case "н": case "нет": return false;
          default: Console.WriteLine("Пожалуйста, введите y/n/да/нет"); break;
        }
      }
    }

    /// <summary>
    /// Получает GUID с валидацией
    /// </summary>
    public static Guid GetGuid(string prompt) {
      while (true) {
        Console.Write(prompt);
        if (Guid.TryParse(Console.ReadLine(), out Guid result)) {
          return result;
        }
        Console.WriteLine("Введите корректный GUID (например, " +
          "550e8400-e29b-41d4-a716-446655440000)");
      }
    }

    /// <summary>
    /// Получает значение из ограниченного набора вариантов
    /// </summary>
    public static string GetOption(string prompt, params string[] options) {
      while (true) {
        Console.Write(prompt);
        var input = Console.ReadLine()?.Trim();

        foreach (var option in options) {
          if (string.Equals(input, option, StringComparison.OrdinalIgnoreCase)) {
            return option;
          }
        }

        Console.WriteLine($"Допустимые варианты: {string.Join(", ", options)}");
      }
    }
  }
}
