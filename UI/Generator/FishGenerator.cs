using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Duz_vadim_project;
using ProgressDisplay;

namespace UI.Generator;

/// <summary>
/// Обеспечивает асинхронную генерацию большого объёма тестовых данных о рыбах.
/// </summary>
public static class FishGenerator
{
  /// <summary>
  /// Количество записей, создаваемых по умолчанию.
  /// </summary>
  public const int RecordCount = 1_000_000;

  /// <summary>
  /// Количество записей, создаваемых за одну итерацию цикла.
  /// </summary>
  private const int BatchSize = 4000;

  /// <summary>
  /// Целевое время генерации в секундах, используемое для равномерного заполнения прогресса.
  /// </summary>
  private const double TargetDurationSeconds = 45.0;

  /// <summary>
  /// Минимально допустимая длительность генерации.
  /// </summary>
  private const double MinimumDurationSeconds = 30.0;

  /// <summary>
  /// Максимально допустимая длительность генерации.
  /// </summary>
  private const double MaximumDurationSeconds = 60.0;

  /// <summary>
  /// Символы, используемые для генерации случайных строковых значений.
  /// </summary>
  private static readonly char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

  /// <summary>
  /// Создаёт список случайных рыб и передаёт информацию о прогрессе через предоставленный интерфейс.
  /// </summary>
  /// <param name="parProgressSession">Интерфейс для взаимодействия с формой прогресса.</param>
  /// <returns>Список сгенерированных рыб.</returns>
  public static async Task<List<Fish>> GenerateFishListAsync(IProgressSession parProgressSession)
  {
    if (parProgressSession is null)
    {
      throw new ArgumentNullException(nameof(parProgressSession));
    }

    var result = new List<Fish>(RecordCount);
    var random = new Random();
    var stopwatch = Stopwatch.StartNew();
    var processed = 0;

    while (processed < RecordCount)
    {
      parProgressSession.CancellationToken.ThrowIfCancellationRequested();

      var currentBatch = Math.Min(BatchSize, RecordCount - processed);
      for (var index = 0; index < currentBatch; index++)
      {
        parProgressSession.CancellationToken.ThrowIfCancellationRequested();
        result.Add(CreateRandomFish(random));
      }

      processed += currentBatch;

      var caption = $"Создано {processed:N0} из {RecordCount:N0}";
      if (parProgressSession.ReportProgress(currentBatch, caption))
      {
        parProgressSession.ReportProgress(0, "Генерация отменена пользователем.");
        throw new OperationCanceledException(parProgressSession.CancellationToken);
      }

      await DelayIfNeededAsync(stopwatch, processed, parProgressSession.CancellationToken).ConfigureAwait(false);
    }

    stopwatch.Stop();

    var minimumDuration = TimeSpan.FromSeconds(MinimumDurationSeconds);
    var maximumDuration = TimeSpan.FromSeconds(MaximumDurationSeconds);

    if (stopwatch.Elapsed < minimumDuration)
    {
      var remaining = minimumDuration - stopwatch.Elapsed;
      if (stopwatch.Elapsed + remaining > maximumDuration)
      {
        remaining = maximumDuration - stopwatch.Elapsed;
      }

      if (remaining > TimeSpan.Zero)
      {
        await Task.Delay(remaining, parProgressSession.CancellationToken).ConfigureAwait(false);
      }
    }

    parProgressSession.ReportProgress(0, "Генерация завершена.");

    return result;
  }

  /// <summary>
  /// Выполняет дополнительную задержку, если фактическое время генерации опережает целевое.
  /// </summary>
  /// <param name="parStopwatch">Таймер, отслеживающий продолжительность генерации.</param>
  /// <param name="parProcessed">Количество уже обработанных записей.</param>
  /// <param name="parCancellationToken">Токен отмены операции.</param>
  private static async Task DelayIfNeededAsync(Stopwatch parStopwatch, int parProcessed, CancellationToken parCancellationToken)
  {
    var targetElapsed = TargetDurationSeconds * (parProcessed / (double)RecordCount);
    targetElapsed = Math.Min(targetElapsed, MaximumDurationSeconds);
    var elapsed = parStopwatch.Elapsed.TotalSeconds;

    if (elapsed >= targetElapsed)
    {
      return;
    }

    var delay = targetElapsed - elapsed;
    var cappedDelay = Math.Min(delay, 0.5);
    if (cappedDelay > 0)
    {
      await Task.Delay(TimeSpan.FromSeconds(cappedDelay), parCancellationToken).ConfigureAwait(false);
    }
  }

  /// <summary>
  /// Создаёт случайный экземпляр рыбы одного из поддерживаемых типов.
  /// </summary>
  /// <param name="parRandom">Генератор случайных чисел.</param>
  /// <returns>Случайно созданная рыба.</returns>
  private static Fish CreateRandomFish(Random parRandom)
  {
    return parRandom.Next(4) switch
    {
      0 => CreateRandomBream(parRandom),
      1 => CreateRandomCarp(parRandom),
      2 => CreateRandomMackerel(parRandom),
      _ => CreateRandomTuna(parRandom)
    };
  }

  /// <summary>
  /// Формирует случайного леща с произвольными параметрами.
  /// </summary>
  /// <param name="parRandom">Генератор случайных чисел.</param>
  /// <returns>Экземпляр леща.</returns>
  private static Bream CreateRandomBream(Random parRandom)
  {
    return new Bream(
      NextDecimal(parRandom, 500, 7000),
      parRandom.Next(1, 21),
      parRandom.Next(0, 2) == 0,
      NextDecimal(parRandom, 1, 25),
      RandomString(parRandom, 8));
  }

  /// <summary>
  /// Формирует случайного карпа с произвольными параметрами.
  /// </summary>
  /// <param name="parRandom">Генератор случайных чисел.</param>
  /// <returns>Экземпляр карпа.</returns>
  private static Carp CreateRandomCarp(Random parRandom)
  {
    return new Carp(
      NextDecimal(parRandom, 500, 9000),
      parRandom.Next(1, 31),
      parRandom.Next(0, 2) == 0,
      NextDecimal(parRandom, 1, 30),
      RandomString(parRandom, 6));
  }

  /// <summary>
  /// Формирует случайную скумбрию с произвольными параметрами.
  /// </summary>
  /// <param name="parRandom">Генератор случайных чисел.</param>
  /// <returns>Экземпляр скумбрии.</returns>
  private static Mackerel CreateRandomMackerel(Random parRandom)
  {
    return new Mackerel(
      NextDecimal(parRandom, 300, 5000),
      parRandom.Next(1, 16),
      parRandom.Next(0, 2) == 0,
      NextDecimal(parRandom, 5, 40),
      NextDecimal(parRandom, 1, 10));
  }

  /// <summary>
  /// Формирует случайного тунца с произвольными параметрами.
  /// </summary>
  /// <param name="parRandom">Генератор случайных чисел.</param>
  /// <returns>Экземпляр тунца.</returns>
  private static Tuna CreateRandomTuna(Random parRandom)
  {
    return new Tuna(
      NextDecimal(parRandom, 2000, 35000),
      parRandom.Next(1, 31),
      parRandom.Next(0, 2) == 0,
      NextDecimal(parRandom, 5, 40),
      NextDecimal(parRandom, 10, 80));
  }

  /// <summary>
  /// Возвращает случайное десятичное число в указанном диапазоне.
  /// </summary>
  /// <param name="parRandom">Генератор случайных чисел.</param>
  /// <param name="parMin">Минимальное значение.</param>
  /// <param name="parMax">Максимальное значение.</param>
  /// <returns>Случайное число с двумя знаками после запятой.</returns>
  private static decimal NextDecimal(Random parRandom, int parMin, int parMax)
  {
    var value = parRandom.NextDouble() * (parMax - parMin) + parMin;
    return Math.Round((decimal)value, 2);
  }

  /// <summary>
  /// Формирует случайную строку указанной длины.
  /// </summary>
  /// <param name="parRandom">Генератор случайных чисел.</param>
  /// <param name="parLength">Длина создаваемой строки.</param>
  /// <returns>Случайная строка.</returns>
  private static string RandomString(Random parRandom, int parLength)
  {
    var buffer = new char[parLength];
    for (var index = 0; index < buffer.Length; index++)
    {
      buffer[index] = Alphabet[parRandom.Next(Alphabet.Length)];
    }

    return new string(buffer);
  }
}
