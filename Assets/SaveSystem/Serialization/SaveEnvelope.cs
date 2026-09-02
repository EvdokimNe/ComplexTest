namespace SaveSystem.SaveSystem.Serialization
{
    /// <summary>Разобранный файл сохранения: заголовок и сырые данные под ним.</summary>
    public readonly struct SaveEnvelope
    {
        public readonly SaveHeader Header;
        public readonly byte[] Payload;

        public SaveEnvelope(SaveHeader header, byte[] payload)
        {
            Header = header;
            Payload = payload;
        }
    }
}
