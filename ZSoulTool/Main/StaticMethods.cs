﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace XV2SSEdit
{
    //UNLEASHED: collection of useful methods that can be used anywhere (without creating an instance of this class)
    public static class StaticMethods
    {
        public static string readString(ref BinaryReader br)
        {
            List<byte> str = new List<byte>();
            byte cr = 0x00;
            while (br.BaseStream.Position != br.BaseStream.Length)
            {
                cr = br.ReadByte();
                if (cr != 0x00)
                    str.Add(cr);
            }
            return Encoding.ASCII.GetString(str.ToArray());
        }
        
        public static void writeString(ref BinaryWriter br, string str)
        {
            byte[] strbytes = Encoding.ASCII.GetBytes(str);
            br.Write(strbytes);
            br.Write((byte)0x00);
        }
        
        public static void writeNull(ref BinaryWriter br, int count)
        {
            byte b = 0x00;
            for (int i = 0; i < count; i++)
            {
                br.Write(b);
            }
        }
        
        public static byte[] getStringBytes(string str)
        {
            List<byte> br = new List<byte>();
            br.AddRange(System.Text.Encoding.ASCII.GetBytes(str));
            br.Add(0x00);
            return br.ToArray();
        }
        
        public static int fixPadding(ref BinaryWriter br, int multiple)
        {
            int address = (int)br.BaseStream.Position;
            int extra = multiple - (address % multiple);

            if (extra == multiple)
            {
                extra = 0;
                return 0;
            }

            byte zero = 0x00;
            for (int c = 0; c < extra; c++) br.Write(zero);
            return extra;
        }

        public static byte[] replaceBytesInByteArray(byte[] orgBytes, byte[] newBytes, int startIndex)
        {
            for (int i = 0; i < newBytes.Length; i++)
            {
                orgBytes[i + startIndex] = newBytes[i];
            }
            return orgBytes;
        }

        public static List<byte> ReplaceRange(List<byte> list, byte[] insertedData, int startIndex)
        {
            if (insertedData.Length > list.Count)
            {
                throw new InvalidOperationException("Cannot insert more data than is in the original list");
            }

            for (int i = 0; i < insertedData.Count(); i++)
            {
                list[i + startIndex] = insertedData[i];
            }
            return list;
        }    
    }
}

