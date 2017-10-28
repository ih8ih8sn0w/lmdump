﻿using System;
using System.IO;
using System.Collections.Generic;

namespace lmdump
{
    public class InputBuffer
    {
        private byte[] data;
        public uint ptr;

        public InputBuffer(string filename)
        {
            data = File.ReadAllBytes(filename);
        }

        public byte[] read(int size)
        {
            if (size + ptr > data.Length)
                throw new IndexOutOfRangeException();

            var o = new byte[size];

            Array.Copy(data, ptr, o, 0, size);

            ptr += (uint)size;

            return o;
        }

        public string readString()
        {
            string s = "";
            while (data[ptr] != 0x00)
            {
                s += (char)data[ptr];
                ptr++;
            }
            return s;
        }

        public string readString(int offset)
        {
            string s = "";
            while (data[offset] != 0x00)
            {
                s += (char)data[offset];
                offset++;
            }
            return s;
        }

        public int readInt()
        {
            return ((data[ptr++] & 0xFF) << 24) | ((data[ptr++] & 0xFF) << 16) | ((data[ptr++] & 0xFF) << 8) | (data[ptr++] & 0xFF);
        }

        public short readShort()
        {
            int num = ((data[ptr++] & 0xFF) << 8) | (data[ptr++] & 0xFF);
            return (short)num;
        }

        public byte readByte()
        {
            return data[ptr++];
        }

        public float readFloat()
        {
            byte[] num = new byte[4] {
                data[ptr + 3],
                data[ptr + 2],
                data[ptr + 1],
                data[ptr]
            };
            ptr += 4;

            return BitConverter.ToSingle(num, 0);
        }

        public byte[] slice(int offset, int size)
        {
            byte[] by = new byte[size];

            Array.Copy(data, offset, by, 0, size);

            return by;
        }

        public void skip(uint size)
        {
            if (size + ptr > data.Length)
                throw new IndexOutOfRangeException();

            ptr += size;
        }
        public void reverse(uint size)
		{
			ptr -= size;
		}
    }

    public class OutputBuffer
    {
        List<byte> data = new List<byte>();

        public void write(byte[] d)
        {
            data.AddRange(d);
        }

        public void write(OutputBuffer d)
        {
            data.AddRange(d.data);
        }

        public void writeString(string str)
        {
            char[] c = str.ToCharArray();
            for (int i = 0; i < c.Length; i++)
                data.Add((byte)c[i]);
        }

        public void writeInt(int d)
        {
            data.Add((byte)((d >> 24) & 0xFF));
            data.Add((byte)((d >> 16) & 0xFF));
            data.Add((byte)((d >> 8) & 0xFF));
            data.Add((byte)((d) & 0xFF));
        }

        public void writeShort(short d)
        {
            data.Add((byte)((d >> 8) & 0xFF));
            data.Add((byte)((d) & 0xFF));
        }

        public void writeFloat(float f)
        {
            byte[] b = BitConverter.GetBytes(f);
            int p = 0;
            writeInt((b[p++] & 0xFF) | ((b[p++] & 0xFF) << 8) | ((b[p++] & 0xFF) << 16) | ((b[p++] & 0xFF) << 24));
        }

        public void writeByte(byte d)
        {
            data.Add(d);
        }

        public byte[] getBytes()
        {
            return data.ToArray();
        }

        public int Size
        {
            get
            {
                return data.Count;
            }
        }
    }

}
