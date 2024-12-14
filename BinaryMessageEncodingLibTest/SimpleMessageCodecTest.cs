using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BinaryMessageEncodingLib.Tests
{
    public class SimpleMessageCodecTests
    {
        private readonly IMessageCodec _codec = new SimpleMessageCodec();

        // Basic Encoding and Decoding
        [Fact]
        public void Encode_ValidMessage_ShouldReturnEncodedBytes()
        {
            // Arrange
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "header1", "value1" },
                    { "header2", "value2" }
                },
                Payload = Encoding.ASCII.GetBytes("This is the payload")
            };

            // Act
            byte[] encodedMessage = _codec.Encode(message);

            // Assert
            Assert.NotNull(encodedMessage);
            Assert.NotEmpty(encodedMessage);
        }

        [Fact]
        public void Decode_ValidEncodedMessage_ShouldReturnOriginalMessage()
        {
            // Arrange
            var originalMessage = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "header1", "value1" },
                    { "header2", "value2" }
                },
                Payload = Encoding.ASCII.GetBytes("This is the payload")
            };

            byte[] encodedMessage = _codec.Encode(originalMessage);

            // Act
            Message decodedMessage = _codec.Decode(encodedMessage);

            // Assert
            Assert.Equal(originalMessage.Headers.Count, decodedMessage.Headers.Count);
            foreach (var header in originalMessage.Headers)
            {
                Assert.True(decodedMessage.Headers.ContainsKey(header.Key));
                Assert.Equal(header.Value, decodedMessage.Headers[header.Key]);
            }
            Assert.Equal(originalMessage.Payload, decodedMessage.Payload);
        }

        // Header Validations
        [Fact]
        public void Encode_HeaderNameNotAscii_ShouldThrowArgumentException()
        {
            // Arrange
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "header1", "value1" },
                    { "header2", "välue2" } // Non-ASCII character in value
                },
                Payload = Encoding.ASCII.GetBytes("This is the payload")
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _codec.Encode(message));
        }

        [Fact]
        public void Encode_HeaderValueNotAscii_ShouldThrowArgumentException()
        {
            // Arrange
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "header1", "välue1" } // Non-ASCII character in value
                },
                Payload = Encoding.ASCII.GetBytes("This is the payload")
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _codec.Encode(message));
        }

        [Fact]
        public void Encode_TooManyHeaders_ShouldThrowArgumentException()
        {
            // Arrange
            var headers = new Dictionary<string, string>();
            for (int i = 0; i < 64; i++) // Exceeding the maximum number of headers
            {
                headers.Add($"header{i}", $"value{i}");
            }

            var message = new Message
            {
                Headers = headers,
                Payload = Encoding.ASCII.GetBytes("This is the payload")
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _codec.Encode(message));
        }

        [Fact]
        public void Encode_HeaderNameExceedsMaxHeaderSize_ThrowsArgumentException()
        {
            // Arrange
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { new string('a', 1024), "value" }
                },
                Payload = Encoding.UTF8.GetBytes("payload")
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _codec.Encode(message));
            Assert.Equal("Header names and values are limited to 1023 bytes.", exception.Message);
        }

        [Fact]
        public void Encode_HeaderValueExceedsMaxHeaderSize_ThrowsArgumentException()
        {
            // Arrange
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "name", new string('a', 1024) }
                },
                Payload = Encoding.UTF8.GetBytes("payload")
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => _codec.Encode(message));
            Assert.Equal("Header names and values are limited to 1023 bytes.", exception.Message);
        }

        [Fact]
        public void Encode_HeaderNameAndValueWithinLimit_Success()
        {
            // Arrange
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { new string('a', 1023), new string('b', 1023) }
                },
                Payload = Encoding.UTF8.GetBytes("payload")
            };

            // Act
            var encodedMessage = _codec.Encode(message);

            // Assert
            Assert.NotNull(encodedMessage);
            var decodedMessage = _codec.Decode(encodedMessage);
            Assert.Equal(message.Headers, decodedMessage.Headers);
            Assert.Equal(message.Payload, decodedMessage.Payload);
        }

        [Fact]
        public void Decode_HeaderNameNotAscii_ShouldThrowArgumentException()
        {
            // Arrange
            var encodedMessage = new List<byte>();

            // Number of headers
            encodedMessage.Add(1);

            // Non-ASCII header name
            var headerName = Encoding.UTF8.GetBytes("non-ascii-header-ä");
            encodedMessage.Add((byte)headerName.Length);
            encodedMessage.AddRange(headerName);

            // ASCII header value
            var headerValue = Encoding.ASCII.GetBytes("value");
            encodedMessage.Add((byte)headerValue.Length);
            encodedMessage.AddRange(headerValue);

            // Payload length
            var payload = Encoding.ASCII.GetBytes("This is the payload");
            var payloadLengthBytes = BitConverter.GetBytes(payload.Length);
            encodedMessage.AddRange(payloadLengthBytes);

            // Payload
            encodedMessage.AddRange(payload);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _codec.Decode(encodedMessage.ToArray()));
        }

        // Payload Validations
        [Fact]
        public void Encode_PayloadTooLarge_ShouldThrowArgumentException()
        {
            // Arrange
            var payload = new byte[256 * 1024 + 1]; // Exceeding the maximum payload size
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "header1", "value1" }
                },
                Payload = payload
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _codec.Encode(message));
        }

        [Fact]
        public void EncodeDecode_EmptyPayload_ShouldReturnOriginalMessage()
        {
            // Arrange
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "header1", "value1" }
                },
                Payload = new byte[0]
            };

            // Act
            byte[] encodedMessage = _codec.Encode(message);
            Message decodedMessage = _codec.Decode(encodedMessage);

            // Assert
            Assert.Equal(message.Headers.Count, decodedMessage.Headers.Count);
            foreach (var header in message.Headers)
            {
                Assert.True(decodedMessage.Headers.ContainsKey(header.Key));
                Assert.Equal(header.Value, decodedMessage.Headers[header.Key]);
            }
            Assert.Equal(message.Payload, decodedMessage.Payload);
        }

        [Fact]
        public void EncodeDecode_BinaryPayload_ShouldReturnOriginalMessage()
        {
            // Arrange
            var binaryPayload = new byte[] { 0x00, 0xFF, 0xAA, 0x55, 0x01, 0x02, 0x03, 0x04 };
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "header1", "value1" }
                },
                Payload = binaryPayload
            };

            // Act
            byte[] encodedMessage = _codec.Encode(message);
            Message decodedMessage = _codec.Decode(encodedMessage);

            // Assert
            Assert.Equal(message.Headers.Count, decodedMessage.Headers.Count);
            foreach (var header in message.Headers)
            {
                Assert.True(decodedMessage.Headers.ContainsKey(header.Key));
                Assert.Equal(header.Value, decodedMessage.Headers[header.Key]);
            }
            Assert.Equal(message.Payload, decodedMessage.Payload);
        }

        [Fact]
        public void EncodeDecode_LargeBinaryPayload_ShouldReturnOriginalMessage()
        {
            // Arrange
            var binaryPayload = new byte[256 * 1024]; // 256 KiB
            new Random().NextBytes(binaryPayload); // Fill with random data
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "header1", "value1" }
                },
                Payload = binaryPayload
            };

            // Act
            byte[] encodedMessage = _codec.Encode(message);
            Message decodedMessage = _codec.Decode(encodedMessage);

            // Assert
            Assert.Equal(message.Headers.Count, decodedMessage.Headers.Count);
            foreach (var header in message.Headers)
            {
                Assert.True(decodedMessage.Headers.ContainsKey(header.Key));
                Assert.Equal(header.Value, decodedMessage.Headers[header.Key]);
            }
            Assert.Equal(message.Payload, decodedMessage.Payload);
        }

        [Fact]
        public void EncodeDecode_NoHeaders_ShouldReturnOriginalMessage()
        {
            // Arrange
            var message = new Message
            {
                Headers = new Dictionary<string, string>(),
                Payload = Encoding.ASCII.GetBytes("This is the payload")
            };

            // Act
            byte[] encodedMessage = _codec.Encode(message);
            Message decodedMessage = _codec.Decode(encodedMessage);

            // Assert
            Assert.Empty(decodedMessage.Headers);
            Assert.Equal(message.Payload, decodedMessage.Payload);
        }

        [Fact]
        public void EncodeDecode_MaxHeaders_ShouldReturnOriginalMessage()
        {
            // Arrange
            var headers = new Dictionary<string, string>();
            for (int i = 0; i < 63; i++) // Maximum number of headers
            {
                headers.Add($"header{i}", $"value{i}");
            }

            var message = new Message
            {
                Headers = headers,
                Payload = Encoding.ASCII.GetBytes("This is the payload")
            };

            // Act
            byte[] encodedMessage = _codec.Encode(message);
            Message decodedMessage = _codec.Decode(encodedMessage);

            // Assert
            Assert.Equal(message.Headers.Count, decodedMessage.Headers.Count);
            foreach (var header in message.Headers)
            {
                Assert.True(decodedMessage.Headers.ContainsKey(header.Key));
                Assert.Equal(header.Value, decodedMessage.Headers[header.Key]);
            }
            Assert.Equal(message.Payload, decodedMessage.Payload);
        }

        [Fact]
        public void Encode_PayloadMaxSize_ShouldReturnEncodedBytes()
        {
            // Arrange
            var payload = new byte[256 * 1024]; // 256 KiB
            new Random().NextBytes(payload); // Fill with random data
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "header1", "value1" }
                },
                Payload = payload
            };

            // Act
            byte[] encodedMessage = _codec.Encode(message);

            // Assert
            Assert.NotNull(encodedMessage);
            Assert.NotEmpty(encodedMessage);
        }

        [Fact]
        public void EncodeDecode_PayloadMaxSize_ShouldReturnOriginalMessage()
        {
            // Arrange
            var payload = new byte[256 * 1024]; // 256 KiB
            new Random().NextBytes(payload); // Fill with random data
            var message = new Message
            {
                Headers = new Dictionary<string, string>
                {
                    { "header1", "value1" }
                },
                Payload = payload
            };

            // Act
            byte[] encodedMessage = _codec.Encode(message);
            Message decodedMessage = _codec.Decode(encodedMessage);

            // Assert
            Assert.Equal(message.Headers.Count, decodedMessage.Headers.Count);
            foreach (var header in message.Headers)
            {
                Assert.True(decodedMessage.Headers.ContainsKey(header.Key));
                Assert.Equal(header.Value, decodedMessage.Headers[header.Key]);
            }
            Assert.Equal(message.Payload, decodedMessage.Payload);
        }
    }
}
