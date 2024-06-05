using System.Collections.Generic;
using System.Linq;
using System.IO;

//UNLEASHED: this is just Dimps's EMB format. just because.
//UNLEASHED: Original code from LibXenoverse, coverted to C#
namespace XV2SSEdit
{
    public class SSP
    {
        public class SSF
        {
            public byte[] m_data;

            public SSF(byte[] data)
            {
                m_data = data;
            }
        }

        public List<SSF> Souls = new List<SSF>();
        int unknownVal = 0;
        byte[] SSP_SIG = {0x23, 0x53, 0x53, 0x50, 0xFE, 0xFF, 0x20, 0x00}; // #SSP

        public SSP()
        {
        
        }

        public void SSPRead(string path)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(path));

            br.BaseStream.Seek(0x08, SeekOrigin.Begin);

            unknownVal = br.ReadInt32();

            int file_count = br.ReadInt32();

            br.BaseStream.Seek(0x18, SeekOrigin.Begin);

            int data_table_addr = br.ReadInt32();
            int filename_table_addr = br.ReadInt32();

            int data_base_offset_entry = 0;

            for (int i = 0; i < file_count; i++)
            {
                data_base_offset_entry = data_table_addr + i * 8;

                br.BaseStream.Seek(data_base_offset_entry, SeekOrigin.Begin);

                int data_addr = br.ReadInt32();
                int data_size = br.ReadInt32();

                br.BaseStream.Seek(data_base_offset_entry + data_addr, SeekOrigin.Begin);
                byte[] data = br.ReadBytes(data_size);

                SSF df = new SSF(data);
                Souls.Add(df);
            }

            br.Close();
        }

        public void SSPWrite(string path)
        {
            FileStream writeStream = new FileStream(path, FileMode.Create);
            BinaryWriter br = new BinaryWriter(writeStream);

            br.Write(SSP_SIG);
        
            int file_total_int = Souls.Count();
     
            br.Write(unknownVal);
            br.Write(Souls.Count());

            StaticMethods.writeNull(ref br, 8);
            StaticMethods.writeNull(ref br, 8);

            int data_table_address = (int)br.BaseStream.Position;
            StaticMethods.writeNull(ref br, file_total_int * 8);

            for (int i = 0; i < file_total_int; i++)
            {
                StaticMethods.fixPadding(ref br, 0x40);
                int file_data_address = (int)br.BaseStream.Position - i * 8 - 32;
                br.Write(Souls[i].m_data);

                br.BaseStream.Seek(data_table_address + i * 8, SeekOrigin.Begin);
                br.Write(file_data_address);

                int file_data_size = Souls[i].m_data.Length;
                br.Write(file_data_size);

                br.BaseStream.Seek(0, SeekOrigin.End);
            }

            br.BaseStream.Seek(0x18, SeekOrigin.Begin);
            br.Write(data_table_address);

            int string_table_address = 0;
            br.Write(string_table_address);

            writeStream.Close();
            br.Close();
        }
    }
}