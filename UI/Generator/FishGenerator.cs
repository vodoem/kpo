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

    private const int BatchSize = 4000;
    private const double TargetDurationSeconds = 45.0;
    private static readonly char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    /// <summary>
    /// Создаёт список случайных рыб и передаёт информацию о прогрессе указанному контроллеру.
    /// </summary>
    /// <param name="progressForm">Контроллер, отображающий ход выполнения пользователю.</param>
    /// <param name="cancellationToken">Токен отмены для прерывания генерации.</param>
    /// <returns>Список сгенерированных рыб.</returns>
    public static async Task<List<Fish>> GenerateFishListAsync(ProgressFormController progressForm, CancellationToken cancellationToken)
    {
        if (progressForm is null)
        {
            throw new ArgumentNullException(nameof(progressForm));
        }

        var result = new List<Fish>(RecordCount);
        var random = new Random();
        var stopwatch = Stopwatch.StartNew();
        var processed = 0;

        while (processed < RecordCount)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentBatch = Math.Min(BatchSize, RecordCount - processed);
            for (var i = 0; i < currentBatch; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                result.Add(CreateRandomFish(random));
            }

            processed += currentBatch;

            var caption = $"Создано {processed:N0} из {RecordCount:N0}";
            if (progressForm.ReportProgress(currentBatch, caption))
            {
                throw new OperationCanceledException(cancellationToken);
            }

            await DelayIfNeededAsync(stopwatch, processed, cancellationToken).ConfigureAwait(false);
        }

        stopwatch.Stop();

        if (stopwatch.Elapsed < TimeSpan.FromSeconds(30))
        {
            var remaining = TimeSpan.FromSeconds(30) - stopwatch.Elapsed;
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining, cancellationToken).ConfigureAwait(false);
            }
        }

        return result;
    }

    private static async Task DelayIfNeededAsync(Stopwatch stopwatch, int processed, CancellationToken cancellationToken)
    {
        var targetElapsed = TargetDurationSeconds * (processed / (double)RecordCount);
        var elapsed = stopwatch.Elapsed.TotalSeconds;

        if (elapsed >= targetElapsed)
        {
            return;
        }

        var delay = targetElapsed - elapsed;
        var cappedDelay = Math.Min(delay, 0.5);
        if (cappedDelay > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(cappedDelay), cancellationToken).ConfigureAwait(false);
        }
    }

    private static Fish CreateRandomFish(Random random)
    {
        return random.Next(4) switch
        {
            0 => CreateRandomBream(random),
            1 => CreateRandomCarp(random),
            2 => CreateRandomMackerel(random),
            _ => CreateRandomTuna(random)
        };
    }

    private static Bream CreateRandomBream(Random random) => new(
        NextDecimal(random, 500, 7000),
        random.Next(1, 21),
        random.Next(0, 2) == 0,
        NextDecimal(random, 1, 25),
        RandomString(random, 8));

    private static Carp CreateRandomCarp(Random random) => new(
        NextDecimal(random, 500, 9000),
        random.Next(1, 31),
        random.Next(0, 2) == 0,
        NextDecimal(random, 1, 30),
        RandomString(random, 6));

    private static Mackerel CreateRandomMackerel(Random random) => new(
        NextDecimal(random, 300, 5000),
        random.Next(1, 16),
        random.Next(0, 2) == 0,
        NextDecimal(random, 5, 40),
        NextDecimal(random, 1, 10));

    private static Tuna CreateRandomTuna(Random random) => new(
        NextDecimal(random, 2000, 35000),
        random.Next(1, 31),
        random.Next(0, 2) == 0,
        NextDecimal(random, 5, 40),
        NextDecimal(random, 10, 80));

    private static decimal NextDecimal(Random random, double min, double max)
    {
        var value = min + random.NextDouble() * (max - min);
        return Math.Round((decimal)value, 2);
    }

    private static string RandomString(Random random, int length)
    {
        var buffer = new char[length];
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = Alphabet[random.Next(Alphabet.Length)];
        }

        return new string(buffer);
    }
}
