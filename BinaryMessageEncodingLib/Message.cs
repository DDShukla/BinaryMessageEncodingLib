namespace BinaryMessageEncodingLib;
public class Message
{
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    public byte[] Payload { get; set; }
}
