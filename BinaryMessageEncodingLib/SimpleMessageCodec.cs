using System.Text;

namespace BinaryMessageEncodingLib;

public class SimpleMessageCodec : IMessageCodec
{
    private const int MaxHeaderCount = 63;
    private const int MaxHeaderSize = 1023;
    private const int MaxPayloadSize = 256 * 1024; // 256 KiB

    public byte[] Encode(Message message)
    {
        if (message.Headers.Count > MaxHeaderCount)
        {
            throw new ArgumentException($"A message can have a maximum of {MaxHeaderCount} headers.");
        }

        using (var memoryStream = new MemoryStream())
        {
            // Encode number of headers
            memoryStream.WriteByte((byte)message.Headers.Count);

            // Encode headers
            foreach (var header in message.Headers)
            {
                if (!IsAscii(header.Key) || !IsAscii(header.Value))
                {
                    throw new ArgumentException("Header names and values must be ASCII-encoded strings.");
                }

                var nameBytes = Encoding.ASCII.GetBytes(header.Key);
                var valueBytes = Encoding.ASCII.GetBytes(header.Value);

                if (nameBytes.Length > MaxHeaderSize || valueBytes.Length > MaxHeaderSize)
                {
                    throw new ArgumentException($"Header names and values are limited to {MaxHeaderSize} bytes.");
                }

                WriteBytesWithLength(memoryStream, nameBytes);
                WriteBytesWithLength(memoryStream, valueBytes);
            }

            // Encode payload
            if (message.Payload.Length > MaxPayloadSize)
            {
                throw new ArgumentException($"The message payload is limited to {MaxPayloadSize} bytes.");
            }

            WriteBytesWithLength(memoryStream, message.Payload);

            return memoryStream.ToArray();
        }
    }

    public Message Decode(byte[] data)
    {
        using (var memoryStream = new MemoryStream(data))
        using (var binaryReader = new BinaryReader(memoryStream))
        {
            var message = new Message
            {
                Headers = new Dictionary<string, string>()
            };

            // Decode number of headers
            int headerCount = binaryReader.ReadByte();
            if (headerCount > MaxHeaderCount)
            {
                throw new ArgumentException($"A message can have a maximum of {MaxHeaderCount} headers.");
            }

            // Decode headers
            for (int i = 0; i < headerCount; i++)
            {
                var nameBytes = ReadBytesWithLength(binaryReader);
                string name = Encoding.UTF8.GetString(nameBytes);
                if (!IsAscii(name))
                {
                    throw new ArgumentException("Header names must be ASCII-encoded strings.");
                }

                var valueBytes = ReadBytesWithLength(binaryReader);
                string value = Encoding.UTF8.GetString(valueBytes);
                if (!IsAscii(value))
                {
                    throw new ArgumentException("Header values must be ASCII-encoded strings.");
                }

                message.Headers[name] = value;
            }

            // Decode payload
            message.Payload = ReadBytesWithLength(binaryReader);

            return message;
        }
    }

    private void WriteBytesWithLength(Stream stream, byte[] bytes)
    {
        var lengthBytes = BitConverter.GetBytes(bytes.Length);
        stream.Write(lengthBytes, 0, lengthBytes.Length);
        stream.Write(bytes, 0, bytes.Length);
    }

    private byte[] ReadBytesWithLength(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        return reader.ReadBytes(length);
    }

    private bool IsAscii(string value)
    {
        return value.All(c => c <= sbyte.MaxValue);
    }
}
