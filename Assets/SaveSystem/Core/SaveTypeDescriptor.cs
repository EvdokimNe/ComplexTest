namespace SaveSystem.SaveSystem.Core
{
    /// <summary>Идентификатор и версия схемы типа, разобранные из <see cref="SaveTypeAttribute"/>.</summary>
    public readonly struct SaveTypeDescriptor
    {
        public readonly string Id;
        public readonly int Version;

        public SaveTypeDescriptor(string id, int version)
        {
            Id = id;
            Version = version;
        }
    }
}
