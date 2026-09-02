namespace SaveSystem.SaveSystem.Serialization
{
    /// <summary>
    /// FNV-1a 64 — короткий некриптографический хеш для контроля целостности данных.
    /// Ловит обрезанный и побитый файл, не аллоцирует и не тянет System.Security.Cryptography.
    /// От намеренной подмены сохранения не защищает: для этого нужен HMAC с ключом.
    /// </summary>
    public static class Fnv1a64
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static ulong Compute(byte[] data)
        {
            ulong hash = OffsetBasis;

            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    hash ^= data[i];
                    hash *= Prime;
                }
            }

            return hash;
        }

        public static string ComputeHex(byte[] data) => Compute(data).ToString("x16");
    }
}
