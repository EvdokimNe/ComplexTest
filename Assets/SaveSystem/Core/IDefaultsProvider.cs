namespace SaveSystem.SaveSystem.Core
{
    /// <summary>
    /// Даёт валидный объект, когда сохранения нет или оно не читается. Так вызывающий код
    /// никогда не получает null и не разбирает сбой сам.
    /// В проекте сюда обычно подставляют ScriptableObject с дефолтным профилем, который
    /// настраивают дизайнеры.
    /// </summary>
    public interface IDefaultsProvider<out T> where T : class
    {
        T CreateDefault();
    }
}
