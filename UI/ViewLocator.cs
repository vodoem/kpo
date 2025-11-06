using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using UI.ViewModels;

namespace UI;

/// <summary>
/// Предоставляет механизм поиска представления по типу модели представления.
/// Используется при динамическом создании визуальных компонентов.
/// </summary>
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Строит представление для указанной модели представления.
    /// </summary>
    /// <param name="parAm">Модель представления, для которой требуется создать представление.</param>
    /// <returns>Созданное представление или текстовое сообщение об ошибке, если подходящее представление не найдено.</returns>
    public Control? Build(object? parAm)
    {
        if (parAm is null)
        {
            return null;
        }

        var name = parAm.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Представление не найдено: " + name };
    }

    /// <summary>
    /// Проверяет, является ли переданный объект моделью представления.
    /// </summary>
    /// <param name="parData">Объект для проверки.</param>
    /// <returns><c>true</c>, если объект является моделью представления, иначе <c>false</c>.</returns>
    public bool Match(object? parData)
    {
        return parData is ViewModelBase;
    }
}
