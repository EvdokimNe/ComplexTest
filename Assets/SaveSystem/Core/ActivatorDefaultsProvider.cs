namespace SaveSystem.SaveSystem.Core
{
    /// <summary>Дефолт «пустой объект»: new T(). Хватает для прогресса и флагов.</summary>
    public sealed class ActivatorDefaultsProvider<T> : IDefaultsProvider<T> where T : class, new()
    {
        public T CreateDefault() => new T();
    }
}
