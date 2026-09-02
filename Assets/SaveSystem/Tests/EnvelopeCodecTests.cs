using System.Text;
using NUnit.Framework;
using SaveSystem.SaveSystem.Serialization;

namespace SaveSystem.Tests
{
    public class EnvelopeCodecTests
    {
        [Test]
        public void Encode_then_decode_returns_header_and_payload()
        {
            byte[] payload = Encoding.UTF8.GetBytes("{\"Level\":7}");
            var header = new SaveHeader
            {
                f = "json",
                v = 2,
                t = "test-progress",
                h = Fnv1a64.ComputeHex(payload),
                app = "1.0.0",
                utc = "2026-09-02T10:14:03Z"
            };

            byte[] file = EnvelopeCodec.Encode(header, payload);

            Assert.IsTrue(EnvelopeCodec.TryDecode(file, out SaveEnvelope envelope, out string error), error);
            Assert.AreEqual("json", envelope.Header.f);
            Assert.AreEqual(2, envelope.Header.v);
            Assert.AreEqual("test-progress", envelope.Header.t);
            Assert.AreEqual(payload, envelope.Payload);
        }

        [Test]
        public void Empty_file_is_rejected()
        {
            Assert.IsFalse(EnvelopeCodec.TryDecode(new byte[0], out _, out string error));
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void File_without_header_line_is_rejected()
        {
            byte[] file = Encoding.UTF8.GetBytes("{\"Level\":7}");

            Assert.IsFalse(EnvelopeCodec.TryDecode(file, out _, out string error));
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void Header_survives_windows_line_ending()
        {
            byte[] payload = Encoding.UTF8.GetBytes("{\"Level\":7}");
            var header = new SaveHeader { f = "json", v = 1, t = "test-progress", h = Fnv1a64.ComputeHex(payload) };

            byte[] file = EnvelopeCodec.Encode(header, payload);
            byte[] withCrLf = InsertCarriageReturn(file);

            Assert.IsTrue(EnvelopeCodec.TryDecode(withCrLf, out SaveEnvelope envelope, out string error), error);
            Assert.AreEqual("json", envelope.Header.f);
        }

        /// <summary>Имитирует файл, побывавший в текстовом редакторе Windows: LF превратился в CRLF.</summary>
        private static byte[] InsertCarriageReturn(byte[] file)
        {
            int separator = System.Array.IndexOf(file, (byte)'\n');
            var result = new byte[file.Length + 1];

            System.Buffer.BlockCopy(file, 0, result, 0, separator);
            result[separator] = (byte)'\r';
            result[separator + 1] = (byte)'\n';
            System.Buffer.BlockCopy(file, separator + 1, result, separator + 2, file.Length - separator - 1);

            return result;
        }
    }
}
