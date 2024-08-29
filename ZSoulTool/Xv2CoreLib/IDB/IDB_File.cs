using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAXLib;

namespace Xv2CoreLib.IDB
{

    [Flags]
    public enum IdbRaceLock
    {
        HUM = 0x1,
        HUF = 0x2,
        SYM = 0x4,
        SYF = 0x8,
        NMC = 0x10,
        FRI = 0x20,
        MAM = 0x40,
        MAF = 0x80
    }

    public enum LB_Color : ushort
    {
        Red = 0,
        Blue = 1,
        Green = 2,
        Purple = 3
    }

    [YAXSerializeAs("IDB")]
    public class IDB_File
    {
        [YAXAttributeForClass]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore, DefaultValue = 1)]
        public int Version { get; set; } = 2;
        //0 = original IDB version (used from 1.00 to 1.17)
        //1 = updated IDB version first used since 1.18
        //2 = updated IDB version first used since 1.22

        [YAXCollection(YAXCollectionSerializationTypes.RecursiveWithNoContainingElement, EachElementName = "IDB_Entry")]
        public List<IDB_Entry> Entries { get; set; }

        public static IDB_File Load(byte[] bytes)
        {
            return new Parser(bytes).GetIdbFile();
        }
    }

    [YAXSerializeAs("IDB_Entry")]
    public class IDB_Entry
    {
        public const int OLD_ENTRY_SIZE = 48;
        public const int ENTRY_SIZE_V1 = 52; //New in 1.18
        public const int ENTRY_SIZE_V2 = 64; //New in 1.22

        #region WrapperProperties
        [YAXDontSerialize]
        public int SortID { get { return ID; } }
        [YAXDontSerialize]
        public string Index { get { return $"{ID_Binding}_{Type}"; } set { ID_Binding = value.Split('_')[0]; Type = ushort.Parse(value.Split('_')[1]); } }
        [YAXDontSerialize]
        public ushort ID { get { return ushort.Parse(ID_Binding); } set { ID_Binding = value.ToString(); } }

        #endregion

        [YAXAttributeForClass]
        [YAXSerializeAs("ID")]
        public string ID_Binding { get; set; } //ushort
        [YAXAttributeFor("Stars")]
        [YAXSerializeAs("value")]
        public ushort I_02 { get; set; }
        [YAXAttributeFor("Name")]
        [YAXSerializeAs("MSG_ID")]
        public ushort NameMsgID { get; set; }
        [YAXAttributeFor("Description")]
        [YAXSerializeAs("MSG_ID")]
        public ushort DescMsgID { get; set; }
        [YAXAttributeFor("HowMsgID")]
        [YAXSerializeAs("MSG_ID")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public ushort HowMsgID { get; set; } = ushort.MaxValue;
        [YAXAttributeFor("NEW_I_10")]
        [YAXSerializeAs("value")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public ushort NEW_I_10 { get; set; }
        [YAXAttributeForClass]
        [YAXSerializeAs("Type")]
        public ushort Type { get; set; } //ushort
        [YAXAttributeFor("I_10")]
        [YAXSerializeAs("value")]
        public ushort I_10 { get; set; } = ushort.MaxValue;
        [YAXAttributeFor("I_12")]
        [YAXSerializeAs("value")]
        public ushort I_12 { get; set; }
        [YAXAttributeFor("I_14")]
        [YAXSerializeAs("value")]
        public ushort I_14 { get; set; } = ushort.MaxValue;
        [YAXAttributeFor("BuyPrice")]
        [YAXSerializeAs("value")]
        public int I_16 { get; set; }
        [YAXAttributeFor("SellPrice")]
        [YAXSerializeAs("value")]
        public int I_20 { get; set; }
        [YAXAttributeFor("RaceLock")]
        [YAXSerializeAs("value")]
        public IdbRaceLock RaceLock { get; set; } //int
        [YAXAttributeFor("TPMedals")]
        [YAXSerializeAs("value")]
        public int I_28 { get; set; }
        [YAXAttributeFor("STPMedals")]
        [YAXSerializeAs("value")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public int NEW_I_32 { get; set; }
        [YAXAttributeFor("NEW_I_36")]
        [YAXSerializeAs("value")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public int NEW_I_36 { get; set; }
        [YAXAttributeFor("Model")]
        [YAXSerializeAs("value")]
        public int I_32 { get; set; } //int32
        [YAXAttributeFor("LimitBurst.EEPK_Effect")]
        [YAXSerializeAs("ID")]
        public ushort I_36 { get; set; } = ushort.MaxValue;
        [YAXAttributeFor("LimitBurst.Color")]
        [YAXSerializeAs("value")]
        public LB_Color I_38 { get; set; } = (LB_Color)ushort.MaxValue;
        [YAXAttributeFor("LimitBurst.Description")]
        [YAXSerializeAs("MSG_ID")]
        public ushort I_40 { get; set; }
        [YAXAttributeFor("LimitBurst.Talisman")]
        [YAXSerializeAs("ID1")]
        public ushort I_42 { get; set; } = ushort.MaxValue;
        [YAXAttributeFor("LimitBurst.Talisman")]
        [YAXSerializeAs("ID2")]
        public ushort I_44 { get; set; } = ushort.MaxValue;
        [YAXAttributeFor("LimitBurst.Talisman")]
        [YAXSerializeAs("ID3")]
        public ushort I_46 { get; set; } = ushort.MaxValue;

        //New in 1.18
        [YAXAttributeFor("NEW_I_12")]
        [YAXSerializeAs("value")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public ushort NEW_I_12 { get; set; } = ushort.MaxValue;
        [YAXAttributeFor("NEW_I_14")]
        [YAXSerializeAs("value")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public ushort NEW_I_14 { get; set; } = ushort.MaxValue;



        [YAXCollection(YAXCollectionSerializationTypes.RecursiveWithNoContainingElement, EachElementName = "Effect")]
        public List<IBD_Effect> Effects { get; set; } // size 3

        [YAXDontSerializeIfNull]
        [YAXCollection(YAXCollectionSerializationTypes.RecursiveWithNoContainingElement, EachElementName = "MsgComponent")]
        public List<object> MsgComponents { get; set; } //Only for LB Mod Installer


        public string GetRaceLockAsString()
        {
            if ((ushort)RaceLock == 255) return null;
            if ((ushort)RaceLock == 0) return null;

            bool first = true;
            List<string> str = new List<string>();
            IdbRaceLock raceLock = (IdbRaceLock)RaceLock;

            if (raceLock.HasFlag(IdbRaceLock.HUM))
            {
                str.Add("HUM");
            }
            if (raceLock.HasFlag(IdbRaceLock.HUF))
            {
                str.Add("HUF");
            }
            if (raceLock.HasFlag(IdbRaceLock.SYM))
            {
                str.Add("SYM");
            }
            if (raceLock.HasFlag(IdbRaceLock.SYF))
            {
                str.Add("SYF");
            }
            if (raceLock.HasFlag(IdbRaceLock.NMC))
            {
                str.Add("NMC");
            }
            if (raceLock.HasFlag(IdbRaceLock.FRI))
            {
                str.Add("FRI");
            }
            if (raceLock.HasFlag(IdbRaceLock.MAM))
            {
                str.Add("MAM");
            }
            if (raceLock.HasFlag(IdbRaceLock.MAF))
            {
                str.Add("MAF");
            }

            StringBuilder str2 = new StringBuilder();
            str2.Append("(");
            foreach(string s in str)
            {
                if (first)
                {
                    str2.Append(String.Format("{0}", s));
                    first = false;
                }
                else
                {
                    str2.Append(String.Format(", {0}", s));
                }
            }
            str2.Append(")");

            return str2.ToString();
        }
    }

    [YAXSerializeAs("Effect")]
    public class IBD_Effect
    {
        public const int OLD_ENTRY_SIZE = 224;
        public const int ENTRY_SIZE_V1 = 232;
        public const int ENTRY_SIZE_V2 = 236;

        [YAXAttributeFor("Type")]
        [YAXSerializeAs("value")]
        public int I_00 { get; set; } = -1;
        [YAXAttributeFor("ActivationType")]
        [YAXSerializeAs("value")]
        public int I_04 { get; set; } = -1;
        [YAXAttributeFor("NumActTimes")]
        [YAXSerializeAs("value")]
        public int I_08 { get; set; } = -1;
        [YAXAttributeFor("NEW_I_12")]
        [YAXSerializeAs("value")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public int NEW_I_12 { get; set; } = 0;
        [YAXAttributeFor("Timer")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_12 { get; set; } = -1;
        [YAXAttributeFor("AbilityValues")]
        [YAXSerializeAs("values")]
        [YAXFormat("0.0##########")]
        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = ", ")]
        public float[] F_16 { get; set; } //size 6
        [YAXAttributeFor("I_40")]
        [YAXSerializeAs("value")]
        public int I_40 { get; set; }
        [YAXAttributeFor("ActivationChance")]
        [YAXSerializeAs("value")]
        public int I_44 { get; set; }
        [YAXAttributeFor("Multipliers")]
        [YAXSerializeAs("values")]
        [YAXFormat("0.0##########")]
        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = ", ")]
        public float[] F_48 { get; set; } //size 6
        [YAXAttributeFor("I_72")]
        [YAXSerializeAs("values")]
        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = ", ")]
        public int[] I_72 { get; set; } //size 6
        [YAXAttributeFor("Health")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_96 { get; set; }
        [YAXAttributeFor("Ki")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_100 { get; set; }
        [YAXAttributeFor("KiRecovery")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_104 { get; set; }
        [YAXAttributeFor("Stamina")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_108 { get; set; }
        [YAXAttributeFor("StaminaRecovery")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_112 { get; set; }
        [YAXAttributeFor("EnemyStaminaEraser")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_116 { get; set; }
        [YAXAttributeFor("F_120")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_120 { get; set; }
        [YAXAttributeFor("GroundSpeed")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_124 { get; set; }
        [YAXAttributeFor("AirSpeed")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_128 { get; set; }
        [YAXAttributeFor("BoostSpeed")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_132 { get; set; }
        [YAXAttributeFor("DashSpeed")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_136 { get; set; }
        [YAXAttributeFor("BasicAttack")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_140 { get; set; }
        [YAXAttributeFor("BasicKiBlast")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_144 { get; set; }
        [YAXAttributeFor("StrikeSuper")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_148 { get; set; }
        [YAXAttributeFor("KiSuper")]
        [YAXSerializeAs("value")]
        [YAXFormat("0.0##########")]
        public float F_152 { get; set; }
        [YAXAttributeFor("F_156")]
        [YAXSerializeAs("values")]
        [YAXFormat("0.0##########")]
        [YAXCollection(YAXCollectionSerializationTypes.Serially, SeparateBy = ", ")]
        public float[] F_156 { get; set; } //size 17

        //New in 1.18
        [YAXAttributeFor("NEW_I_48")]
        [YAXSerializeAs("value")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public int NEW_I_48 { get; set; }
        [YAXAttributeFor("NEW_I_52")]
        [YAXSerializeAs("value")]
        [YAXErrorIfMissed(YAXExceptionTypes.Ignore)]
        public int NEW_I_52 { get; set; }

        public IBD_Effect()
        {
            F_16 = new float[6];
            F_48 = new float[6];
            I_72 = new int[6];
            F_156 = new float[17];
        }
    }


}
