# BinaryMessageEncodingLib

`BinaryMessageEncodingLib` is a C# library for encoding and decoding messages with headers and a binary payload. The library ensures that messages adhere to specific constraints, such as the maximum number of headers, maximum header size, and maximum payload size.

## Features

- Encode messages with headers and a binary payload.
- Decode messages from a binary format.
- Validate constraints on the number of headers, header size, and payload size.
- Ensure header names and values are ASCII-encoded strings.

## Constraints

- A message can have a maximum of 63 headers.
- Header names and values are limited to 1023 bytes each.
- The message payload is limited to 256 KiB.
- Header names and values must be ASCII-encoded strings.

## Installation

To use the `BinaryMessageEncodingLib` in your project, you can add the source files directly or compile the library and reference it in your project.

## Usage

### Encoding a Message

    using System; 
    using System.Collections.Generic;
    using System.Text;
    using BinaryMessageEncodingLib;
    public class Program 
    {
    public static void Main()
    {
    
    var message = new Message { Headers = new Dictionary<string, string> { { "header1", "value1" }, { "header2", "value2" } }, Payload = Encoding.ASCII.GetBytes("This is the payload") };

    var codec = new SimpleMessageCodec();

    byte[] encodedMessage = codec.Encode(message);

    Console.WriteLine("Encoded message: " + BitConverter.ToString(encodedMessage));
    }
    }


### Decoding a Message

    using System; 
    using System.Text;
    using BinaryMessageEncodingLib;
    public class Program 
    { 
    public static void Main() 
    { 
    // Assume encodedMessage is the byte array obtained from the encoding example byte[] encodedMessage = /* your encoded message byte array */;
    var codec = new SimpleMessageCodec();
    Message decodedMessage = codec.Decode(encodedMessage);

    Console.WriteLine("Decoded message headers:");
    foreach (var header in decodedMessage.Headers)
    {
        Console.WriteLine($"{header.Key}: {header.Value}");
    }

    Console.WriteLine("Decoded message payload: " + Encoding.ASCII.GetString(decodedMessage.Payload));
    }
    }


## Testing

The library includes unit tests to validate the encoding and decoding functionality, as well as the constraints on headers and payload size. The tests are written using the `Xunit` framework.

### Running Tests

To run the tests, use the following command:

    dotnet test


## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any improvements or bug fixes.

## Contact

For any questions or inquiries, please contact the project maintainers.
