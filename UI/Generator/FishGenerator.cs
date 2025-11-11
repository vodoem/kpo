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
    /// Создаёт список случайных рыб и передаёт информацию о прогрессе указанному контроллеру.
    /// </summary>
    /// <param name="parProgressForm">Контроллер, отображающий ход выполнения пользователю.</param>
    /// <param name="parCancellationToken">Токен отмены для прерывания генерации.</param>
    /// <returns>Список сгенерированных рыб.</returns>
    public static async Task<List<Fish>> GenerateFishListAsync(ProgressFormController parProgressForm, CancellationToken parCancellationToken)
    {
        if (parProgressForm is null)
        {
            throw new ArgumentNullException(nameof(parProgressForm));
        }

        var result = new List<Fish>(RecordCount);
        var random = new Random();
        var stopwatch = Stopwatch.StartNew();
        var processed = 0;

        while (processed < RecordCount)
        {
            parCancellationToken.ThrowIfCancellationRequested();

            var currentBatch = Math.Min(BatchSize, RecordCount - processed);
            for (var i = 0; i < currentBatch; i++)
            {
                parCancellationToken.ThrowIfCancellationRequested();
                result.Add(CreateRandomFish(random));
            }

            processed += currentBatch;

            var caption = $"Создано {processed:N0} из {RecordCount:N0}";
            if (parProgressForm.ReportProgress(currentBatch, caption))
            {
                throw new OperationCanceledException(parCancellationToken);
            }

            await DelayIfNeededAsync(stopwatch, processed, parCancellationToken).ConfigureAwait(false);
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
                await Task.Delay(remaining, parCancellationToken).ConfigureAwait(false);
            }
        }

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
    private static Bream CreateRandomBream(Random parRandom) => new(
        NextDecimal(parRandom, 500, 7000),
        parRandom.Next(1, 21),
        parRandom.Next(0, 2) == 0,
        NextDecimal(parRandom, 1, 25),
        RandomString(parRandom, 8));

    /// <summary>
    /// Формирует случайного карпа с произвольными параметрами.
    /// </summary>
    /// <param name="parRandom">Генератор случайных чисел.</param>
    /// <returns>Экземпляр карпа.</returns>
    private static Carp CreateRandomCarp(Random parRandom) => new(
        NextDecimal(parRandom, 500, 9000),
        parRandom.Next(1, 31),
        parRandom.Next(0, 2) == 0,
        NextDecimal(parRandom, 1, 30),
        RandomString(parRandom, 6));

    /// <summary>
    /// Формирует случайную скумбрию с произвольными параметрами.
    /// </summary>
    /// <param name="parRandom">Генератор случайных чисел.</param>
    /// <returns>Экземпляр скумбрии.</returns>
    private static Mackerel CreateRandomMackerel(Random parRandom) => new(
        NextDecimal(parRandom, 300, 5000),
        parRandom.Next(1, 16),
        parRandom.Next(0, 2) == 0,
        NextDecimal(parRandom, 5, 40),
        NextDecimal(parRandom, 1, 10));

    /// <summary>
    /// Формирует случайного тунца с произвольными параметрами.
    /// </summary>
    /// <param name="parRandom">Генератор случайных чисел.</param>
    /// <returns>Экземпляр тунца.</returns>
    private static Tuna CreateRandomTuna(Random parRandom) => new(
        NextDecimal(parRandom, 2000, 35000),
        parRandom.Next(1, 31),
        parRandom.Next(0, 2) == 0,
        NextDecimal(parRandom, 5, 40),
        NextDecimal(parRandom, 10, 80));

    /// <summary>
    /// Возвращает случайное десятичное число в указанном диапазоне.
    /// </summary>
    /// <param name="parRandom">Генератор случайных чисел.</param>
    /// <param name="parMin">Минимальное значение диапазона.</param>
    /// <param name="parMax">Максимальное значение диапазона.</param>
    /// <returns>Случайное десятичное число.</returns>
    private static decimal NextDecimal(Random parRandom, double parMin, double parMax)
    {
        var value = parMin + parRandom.NextDouble() * (parMax - parMin);
        return Math.Round((decimal)value, 2);
    }

    /// <summary>
    /// Возвращает случайную строку указанной длины из набора символов <see cref="Alphabet"/>.
    /// </summary>
    /// <param name="parRandom">Генератор случайных чисел.</param>
    /// <param name="parLength">Требуемая длина строки.</param>
    /// <returns>Случайная строка.</returns>
    private static string RandomString(Random parRandom, int parLength)
    {
        var buffer = new char[parLength];
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = Alphabet[parRandom.Next(Alphabet.Length)];
        }

        return new string(buffer);
    }
}
