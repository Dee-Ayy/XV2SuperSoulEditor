﻿using System.Collections.Generic;
using System;
using System.Xml;

//Parses and reads the lists in the EffectData xml
namespace XV2SSEdit
{
    struct Effect
    {
        public int ID;
        public string Description;  
    }

    struct Activator
    {
        public int ID;
        public string Description;   
    }

    struct Target
    {
        public int ID;
        public string Description;
    }

    struct LBColor
    {
        public short ID;
        public string Description;
    }

    struct Kitype
    {
        public int ID;
        public string Description;
    }

    struct VfxType
    {
        public int ID;
        public string Description;
    }

    //UNLEASHED: made public to help with exporting
    public struct idbItem
    {
        public int msgIndexName;
        public int msgIndexDesc;
        public int msgIndexHow;
        public int msgIndexBurst;
        public int msgIndexBurstBTL;
        public int msgIndexBurstPause;
        public byte[] Data;
    }

    class EffectList
    {
        public Effect[] effects;
        public void ConstructList(XmlNodeList effectlist)
        {
            effects = new Effect[effectlist.Count];
            for (int i = 0; i < effectlist.Count; i++)
            {
                effects[i].ID = int.Parse(effectlist[i].Attributes["id"].Value);
                effects[i].Description = effectlist[i].InnerText;
            }
        }

        public int FindIndex(int ID)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].ID == ID)
                    return i;
            }
            return 0;
        }
    }

    class ActivatorList
    {
        public Activator[] activators;
        public void ConstructList(XmlNodeList activatorlist)
        {
            activators = new Activator[activatorlist.Count];
            for (int i = 0; i < activatorlist.Count; i++)
            {
                activators[i].ID = int.Parse(activatorlist[i].Attributes["id"].Value);
                activators[i].Description = activatorlist[i].InnerText;
            }
        }

        public int FindIndex(int ID)
        {
            for (int i = 0; i < activators.Length; i++)
            {
                if (activators[i].ID == ID)
                    return i;
            }
            return 0;
        }

    }

    class TargetList
    {
        public Target[] targets;
        public void ConstructList(XmlNodeList targetlist)
        {
            targets = new Target[targetlist.Count];
            for (int i = 0; i < targetlist.Count; i++)
            {
                targets[i].ID = int.Parse(targetlist[i].Attributes["id"].Value);
                targets[i].Description = targetlist[i].InnerText;
            }
        }

        public int FindIndex(int ID)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i].ID == ID)
                    return i;
            }
            return 0;
        }
    }

    class LBColorList
    {
        public LBColor[] colors;
        public void ConstructList(XmlNodeList limitcolors)
        {
            colors = new LBColor[limitcolors.Count];
            for (int i = 0; i < limitcolors.Count; i++)
            {
                colors[i].ID = short.Parse(limitcolors[i].Attributes["id"].Value);
                colors[i].Description = limitcolors[i].InnerText;
            }
        }

        public short FindIndex(int ID)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                if (colors[i].ID == ID)
                    return (short)i;
            }
            return 0;
        }
    }

    class KitypeList
    {
        public Kitype[] kitypes;
        public void ConstructList(XmlNodeList kitypelist)
        {
            kitypes = new Kitype[kitypelist.Count];
            for (int i = 0; i < kitypelist.Count; i++)
            {
                kitypes[i].ID = int.Parse(kitypelist[i].Attributes["id"].Value);
                kitypes[i].Description = kitypelist[i].InnerText;
            }
        }

        public int FindIndex(int ID)
        {
            for (int i = 0; i < kitypes.Length; i++)
            {
                if (kitypes[i].ID == ID)
                    return i;
            }
            return 0;
        }
    }

    class VFXList
    {
        public VfxType[] vfxtypes;
        public void ConstructList(XmlNodeList vfxlist)
        {
            vfxtypes = new VfxType[vfxlist.Count];
            for (int i = 0; i < vfxlist.Count; i++)
            {
                vfxtypes[i].ID = int.Parse(vfxlist[i].Attributes["id"].Value);
                vfxtypes[i].Description = vfxlist[i].InnerText;
            }
        }

        public int FindIndex(int ID)
        {
            for (int i = 0; i < vfxtypes.Length; i++)
            {
                if (vfxtypes[i].ID == ID)
                    return i;
            }
            return 0;
        }
    }

    //from LB
    /// <summary>
    /// Extra static methods for BitConverter.
    /// </summary>
    public static class BitConverter_Ex
    {
        public static ushort[] ToUInt16Array(byte[] bytes)
        {
            if (bytes == null) return new ushort[0];
            int count = bytes.Length / 2;

            ushort[] ints = new ushort[count];

            for (int i = 0; i < count; i++)
            {
                ints[i] = BitConverter.ToUInt16(bytes, i * 2);
            }

            return ints;
        }

        public static int[] ToInt32Array(byte[] bytes, int index, int count)
        {
            int[] ints = new int[count];


            for (int i = 0; i < count * 4; i += 4)
            {
                ints[i / 4] = BitConverter.ToInt32(bytes, index + i);
            }

            return ints;
        }

        public static short[] ToInt16Array(byte[] bytes, int index, int count)
        {
            short[] ints = new short[count];

            for (int i = 0; i < count * 2; i += 2)
            {
                ints[i / 2] = BitConverter.ToInt16(bytes, index + i);
            }

            return ints;
        }

        public static ushort[] ToUInt16Array(byte[] bytes, int index, int count)
        {
            ushort[] ints = new ushort[count];

            for (int i = 0; i < count * 2; i += 2)
            {
                ints[i / 2] = BitConverter.ToUInt16(bytes, index + i);
            }

            return ints;
        }

        public static bool ToBoolean(byte[] bytes, int index)
        {
            return (bytes[index] == 0) ? false : true;
        }

        public static bool ToBoolean(byte bytes)
        {
            return (bytes == 0) ? false : true;
        }

        public static bool ToBooleanFromInt32(byte[] bytes, int index)
        {
            return (BitConverter.ToInt32(bytes, index) == 0) ? false : true;
        }

        public static float[] ToFloat32Array(byte[] bytes, int index, int count)
        {
            float[] floats = new float[count];

            for (int i = 0; i < count * 4; i += 4)
            {
                floats[i / 4] = BitConverter.ToSingle(bytes, index + i);
            }

            return floats;
        }

        //GetBytes methods
        /// <summary>
        /// Converts a boolean value into a byte
        /// </summary>=
        public static byte GetBytes(bool _bool)
        {
            if (_bool == true)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static byte[] GetBytes_Bool32(bool _bool)
        {
            if (_bool == true)
            {
                return new byte[4] { 1, 0, 0, 0 };
            }
            else
            {
                return new byte[4] { 0, 0, 0, 0 };
            }
        }

        public static byte[] GetBytes(int[] intArray, int fixedSize = -1)
        {
            if (intArray == null)
                return new byte[2 * fixedSize];

            if (fixedSize == -1)
                fixedSize = intArray.Length;

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < intArray.Length; i++)
            {
                if (i == fixedSize) break;
                bytes.AddRange(BitConverter.GetBytes(intArray[i]));
            }

            if (intArray.Length < fixedSize)
            {
                for (int i = 0; i < fixedSize - intArray.Length; i++)
                {
                    bytes.AddRange(BitConverter.GetBytes((ushort)0));
                }
            }

            return bytes.ToArray();
        }

        public static byte[] GetBytes(float[] floatArray, int fixedSize = -1)
        {
            if (floatArray == null)
                return new byte[2 * fixedSize];

            if (fixedSize == -1)
                fixedSize = floatArray.Length;

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < floatArray.Length; i++)
            {
                if (i == fixedSize) break;
                bytes.AddRange(BitConverter.GetBytes(floatArray[i]));
            }

            if (floatArray.Length < fixedSize)
            {
                for (int i = 0; i < fixedSize - floatArray.Length; i++)
                {
                    bytes.AddRange(BitConverter.GetBytes((ushort)0));
                }
            }

            return bytes.ToArray();
        }

        public static byte[] GetBytes(short[] intArray, int fixedSize = -1)
        {
            if (intArray == null)
                return new byte[2 * fixedSize];

            if (fixedSize == -1)
                fixedSize = intArray.Length;

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < intArray.Length; i++)
            {
                if (i == fixedSize) break;
                bytes.AddRange(BitConverter.GetBytes(intArray[i]));
            }

            if (intArray.Length < fixedSize)
            {
                for (int i = 0; i < fixedSize - intArray.Length; i++)
                {
                    bytes.AddRange(BitConverter.GetBytes((ushort)0));
                }
            }

            return bytes.ToArray();
        }

        public static byte[] GetBytes(ushort[] intArray, int fixedSize = -1)
        {
            if (intArray == null)
                return new byte[2 * fixedSize];

            if (fixedSize == -1)
                fixedSize = intArray.Length;

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < intArray.Length; i++)
            {
                if (i == fixedSize) break;
                bytes.AddRange(BitConverter.GetBytes(intArray[i]));
            }

            if (intArray.Length < fixedSize)
            {
                for (int i = 0; i < fixedSize - intArray.Length; i++)
                {
                    bytes.AddRange(BitConverter.GetBytes((ushort)0));
                }
            }

            return bytes.ToArray();
        }

    }

}
