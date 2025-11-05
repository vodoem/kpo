using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using UI.ViewModels;

namespace UI;

/// <summary>
/// Класс для динамического отображения представлений, соответствующих модели представления.
/// Используется для поиска и создания представлений, основанных на типе модели представления.
/// </summary>
public class ViewLocator : IDataTemplate
{
  /// <summary>
  /// Строит представление для указанной модели представления.
  /// </summary>
  /// <param name="parAm">Модель представления, для которой нужно создать представление.</param>
  /// <returns>Представление, соответствующее модели представления, или текст с ошибкой, если представление не найдено.</returns>
  public Control? Build(object? parAm)
  {
    if (parAm is null)
      return null;

    var name = parAm.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
    var type = Type.GetType(name);

    if (type != null)
    {
      return (Control)Activator.CreateInstance(type)!;
    }

    return new TextBlock { Text = "Not Found: " + name };
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
