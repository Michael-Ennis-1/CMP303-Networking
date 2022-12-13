using System;
using System.Text;

namespace NetworkServer
{
    class ReadPackage
    {
        // Variables for determining the position the packet should read from, and for storing bytes to be read
        public int readPos = 0;
        public byte[] data;

        // Stores the input bytes in a byte array to be used later
        public ReadPackage(byte[] _data)
        {
            data = new byte[_data.Length];
            Array.Copy(_data, data, _data.Length);
        }

        // Converts an int from the data byte array and returns it. Moves the read pos forward by the size of the int (4 bytes)
        public int Int()
        {
            int _value = BitConverter.ToInt32(data, readPos);
            readPos += 4;

            return _value;
        }

        // Converts a float from the data byte array and returns it. Moves the read pos forward by the size of the float (4 bytes)
        public float Float()
        {
            float _value = BitConverter.ToSingle(data, readPos);
            readPos += 4;

            return _value;
        }

        // Converts a string.
        public string String()
        {
            // Converts an int from the data byte array, which is the length of the string. Moves the read pos forward by the size of the int
            int _length = BitConverter.ToInt32(data, readPos);
            readPos += 4;

            // Converts a string from the data byte array and returns it. Moves the read position forward depending on the length of the string.
            string _string = Encoding.ASCII.GetString(data, readPos, _length);
            readPos += _length;

            return _string;
        }
    }
}
