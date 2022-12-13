using System.Collections.Generic;
using System.Text;
using System;

public class WritePackage
{
    // Stores the data in a list, as it is dynamic and can continously be increased in size
    private List<byte> byteBuffer;

    public WritePackage()
    {
        // Sets the byte buffer to begin writing
        byteBuffer = new List<byte>();
    }

    public void Int(int _data)
    {
        // Converts an integer to bytes and adds it to the list
        byteBuffer.AddRange(BitConverter.GetBytes(_data));
    }

    public void Float(float _data)
    {
        // Converts an integer to bytes and adds it to the list
        byteBuffer.AddRange(BitConverter.GetBytes(_data));
    }

    public void String(string _data)
    {
        // Converts an integer for the length of the string to bytes, and converts the string to bytes and adds both to the list
        byteBuffer.AddRange(BitConverter.GetBytes(_data.Length));
        byteBuffer.AddRange(Encoding.ASCII.GetBytes(_data));
    }

    public byte[] assembleData()
    {
        // Assembles the byte buffer as an array so it can be sent
        byte[] _data = byteBuffer.ToArray();
        return _data;
    }

    public int returnLength()
    {
        // Returns the length of the writen package
        return byteBuffer.Count;
    }
}
