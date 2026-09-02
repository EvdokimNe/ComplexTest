using System;
using System.Text;
using UnityEngine;
namespace SaveSystem.SaveSystem.Serialization
{
    /// <summary>
    /// Формат файла: строка заголовка + '\n' + данные как есть.
    /// Заголовок разбирается до данных и независимо от их формата, поэтому под ним может лежать
    /// хоть json, хоть бинарь; поле длины и base64 при этом не нужны.
    /// </summary>
    public static class EnvelopeCodec
    {
        private const byte Separator = (byte)'\n';
        private const byte CarriageReturn = (byte)'\r';

        public static byte[] Encode(SaveHeader header, byte[] payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(header));
            var result = new byte[headerBytes.Length + 1 + payload.Length];

            Buffer.BlockCopy(headerBytes, 0, result, 0, headerBytes.Length);
            result[headerBytes.Length] = Separator;
            Buffer.BlockCopy(payload, 0, result, headerBytes.Length + 1, payload.Length);

            return result;
        }

        public static bool TryDecode(byte[] bytes, out SaveEnvelope envelope, out string error)
        {
            envelope = default;

            if (bytes == null || bytes.Length == 0)
            {
                error = "файл пуст";
                return false;
            }

            int separator = Array.IndexOf(bytes, Separator);
            if (separator <= 0)
            {
                error = "нет строки заголовка";
                return false;
            }

            // Файл мог побывать в текстовом редакторе и получить CRLF вместо LF.
            int headerLength = separator > 0 && bytes[separator - 1] == CarriageReturn ? separator - 1 : separator;

            SaveHeader header;
            try
            {
                header = JsonUtility.FromJson<SaveHeader>(Encoding.UTF8.GetString(bytes, 0, headerLength));
            }
            catch (Exception exception)
            {
                error = "заголовок не разбирается: " + exception.Message;
                return false;
            }

            if (string.IsNullOrEmpty(header.f))
            {
                error = "в заголовке нет формата";
                return false;
            }

            int payloadLength = bytes.Length - separator - 1;
            var payload = new byte[payloadLength];
            Buffer.BlockCopy(bytes, separator + 1, payload, 0, payloadLength);

            envelope = new SaveEnvelope(header, payload);
            error = null;
            return true;
        }
    }
}
