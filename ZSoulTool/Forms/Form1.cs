using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using Msgfile;
using XV2_Serializer.Resource;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Collections;
using CriPakTools;

namespace XV2SSEdit
{
    public partial class Form1 : Form
    {
        #region Data_types

        private ToolSettings settings { get; set; }
        private Xv2FileIO fileIO { get; set; }

        private readonly Dictionary<string, Tuple<string, int>> IdbOffsets = IDB.IdbOffsets;
        private readonly Dictionary<string, int> Effect_Start = IDB.Effect_Start;
        private readonly Dictionary<string, Tuple<string, int>> EffectOffsets = IDB.EffectOffsets;
        private readonly string[] StatNames = IDB.StatNames;

        public idbItem[] Items;
        //EffectList effList;
        //ActivatorList actList;
        //TargetList trgList;
        //LBColorList lbcList;
        //KitypeList kitList;
        //VFXList vfxList;
        string FileName;

        //OLD
        //string FileNameMsgN;
        //string FileNameMsgD;
        //string FileNameMsgH;
        //string FileNameMsgB;
        //string FileNameMsgB_BTLHUD;
        //string FileNameMsgB_Pause;
        //
        //private struct GenericMsgFile
        //{
        //    public string msgPath_m;
        //    public msg msgFile_m;
        //    public GenericMsgFile(string msgPath, msg msgFile)
        //    {
        //        msgPath_m = msgPath;
        //        msgFile_m = msgFile;
        //    }
        //}
        //
        //private List<GenericMsgFile> genericMsgListNames = new List<GenericMsgFile>();
        //private List<GenericMsgFile> genericMsgListDescs = new List<GenericMsgFile>();
        //private List<GenericMsgFile> genericMsgListHowTo = new List<GenericMsgFile>();
        //private List<GenericMsgFile> genericMsgListBurst = new List<GenericMsgFile>();
        //private List<GenericMsgFile> genericMsgListNameBurstBTLHUD = new List<GenericMsgFile>();
        //private List<GenericMsgFile> genericMsgListNameBurstPause = new List<GenericMsgFile>();

        private msg Names;
        private msg Descs;
        private msg HowTo;
        private msg Burst;
        private msg BurstBTLHUD;
        private msg BurstPause;

        //dict full of all msg paths used for each language
        private Dictionary<string, string> fullFileNameMsgN = new Dictionary<string, string>();
        private Dictionary<string, string> fullFileNameMsgD = new Dictionary<string, string>();
        private Dictionary<string, string> fullFileNameMsgH = new Dictionary<string, string>();
        private Dictionary<string, string> fullFileNameMsgB = new Dictionary<string, string>();
        private Dictionary<string, string> fullFileNameMsgB_BTLHUD = new Dictionary<string, string>();
        private Dictionary<string, string> fullFileNameMsgB_Pause = new Dictionary<string, string>();


        //dict full of all msg text used for each language
        private Dictionary<string, msg> fullMsgListNames = new Dictionary<string, msg>();
        private Dictionary<string, msg> fullMsgListDescs = new Dictionary<string, msg>();
        private Dictionary<string, msg> fullMsgListHowTo = new Dictionary<string, msg>();
        private Dictionary<string, msg> fullMsgListBurst = new Dictionary<string, msg>();
        private Dictionary<string, msg> fullMsgListBurstBTLHUD = new Dictionary<string, msg>();
        private Dictionary<string, msg> fullMsgListBurstPause = new Dictionary<string, msg>();

        //UNLEASHED: helper vars for searching
        private int lastFoundIndex = 0;
        private string lastInputString = "";
        private int currentSuperSoulIndex = -1; //UNLEASHED: helper var for deleting
        private bool hasSavedChanges = true;    //UNLEASHED: remind user to save changes.
        private byte[] clipboardData = null;    //UNLEASHED: copy and paste
        private bool startup = true;
        private string currentLanguge = "en";
        private List<ushort> UsedIDs = new List<ushort>();

        #endregion

        public Form1()
        {
            InitializeComponent();

            //setup stat list boxes for effect tabs
            foreach (string str in StatNames)
            {
                var Item = new ListViewItem(new[] { str, "0.0" });
                var Item1 = new ListViewItem(new[] { str, "0.0" });
                var Item2 = new ListViewItem(new[] { str, "0.0" });
                lstvBasic.Items.Add(Item);
                lstvEffect1.Items.Add(Item1);
                lstvEffect2.Items.Add(Item2);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            settings = ToolSettings.Load();

            //set language to default selection on settings window
            msgLanguageSelect.SelectedIndex = (int)settings.GameLanguage;

            InitDirectory();
            InitFileIO();
            LoadFiles();

            //TODO: do we still want to install by args?
            //string[] args = Environment.GetCommandLineArgs();
            //if (args.Length > 1)
            //    installSSPFromArgs(args);

            startup = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (hasSavedChanges == false)
            {
                if (MessageBox.Show("You have unsaved data, close editor?", "Unsaved Data", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    e.Cancel = true;
                else
                    e.Cancel = false;
            }
            else
                Application.Exit();
        }


        #region Details Edit

        //Basic number values
        private void tbNumberChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Tuple<string, int> type_off;
            int effectOffset = 0;

            if (tb.Name.EndsWith("_EB"))
                effectOffset = Effect_Start["_EB"];
            else if (tb.Name.EndsWith("_E1"))
                effectOffset = Effect_Start["_E1"];
            else if (tb.Name.EndsWith("_E2"))
                effectOffset = Effect_Start["_E2"];

            //type and offset for soul details
            if (effectOffset == 0)
                type_off = IdbOffsets[tb.Name];
            //type and offset for effect details
            else
                type_off = EffectOffsets[tb.Name.Remove(tb.Name.Length - 3)];

            //change value
            effectOffset += type_off.Item2;
            switch (type_off.Item1)
            {
                case "Int16":
                    short numShort;
                    if (short.TryParse(tb.Text, out numShort))
                        Array.Copy(BitConverter.GetBytes(numShort), 0, Items[itemList.SelectedIndex].Data, effectOffset, 2);
                    break;
                case "Int32":
                    int numInt;
                    if (int.TryParse(tb.Text, out numInt))
                        Array.Copy(BitConverter.GetBytes(numInt), 0, Items[itemList.SelectedIndex].Data, effectOffset, 4);
                    break;
                case "Float":
                    float numFloat;
                    if (float.TryParse(tb.Text, out numFloat))
                        Array.Copy(BitConverter.GetBytes(numFloat), 0, Items[itemList.SelectedIndex].Data, effectOffset, 4);
                    break;
                default:
                    Console.WriteLine("Unknown type???");
                    break;
            }
        }

        //comboboxes
        private void cbIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            Tuple<string, int> type_off;

            //correct offset if from details
            int offset = 0;
            if (cb.Name.EndsWith("_EB"))
                offset = Effect_Start["_EB"];
            else if (cb.Name.EndsWith("_E1"))
                offset = Effect_Start["_E1"];
            else if (cb.Name.EndsWith("_E2"))
                offset = Effect_Start["_E2"];

            //type and offset for soul details
            if (offset == 0)
                type_off = IdbOffsets[cb.Name];
            //type and offset for effect details
            else
                type_off = EffectOffsets[cb.Name.Remove(cb.Name.Length - 3)];

            int numInt;
            int size = 0;

            //Soul Details
            if (offset == 0)
            {
                if (cb.Name == "Rarity")
                {
                    numInt = (short)Rarity.SelectedIndex;
                    size = 2;
                }
                else if (cb.Name == "KiBlast")
                {
                    numInt = IDB.KiBlastList.Keys[cb.SelectedIndex];
                    size = 4;
                }
                else if (cb.Name == "LB_Color")
                {
                    numInt = IDB.LBColorList.Keys[cb.SelectedIndex];
                    size = 2;
                }
                else
                {
                    Console.WriteLine("Unknown combobox name???");
                    return;
                }
            }
            //Effect 0/1/2 Details
            else
            {
                size = 4;
                if (cb.Name.StartsWith("Effect"))
                {
                    numInt = IDB.EffectList.Keys[cb.SelectedIndex];
                }
                else if (cb.Name.StartsWith("Activate"))
                {
                    numInt = IDB.ActivatorList.Keys[cb.SelectedIndex];
                }
                else if (cb.Name.StartsWith("Vfx_Type"))
                {
                    numInt = IDB.VfxList.Keys[cb.SelectedIndex];
                }
                else if (cb.Name.StartsWith("Target"))
                {
                    numInt = IDB.TargetList.Keys[cb.SelectedIndex];
                }
                else
                {
                    Console.WriteLine("Unknown combobox name???");
                    return;
                }
            }

            //extra failsafe i guess.
            //should be impossible for size to be 0 at this point but whatever
            if (size <= 0)
            {
                Console.WriteLine("Size was still 0?????");
                return;
            }

            //get data ready
            offset += type_off.Item2;
            byte[] bytes = BitConverter.GetBytes(numInt);

            //write
            Array.Copy(bytes, 0, Items[itemList.SelectedIndex].Data, offset, size);
        }

        // Name, Description, Lookup MSG
        private void msgChangeLanguage(object sender, EventArgs e)
        {
            //get current language from language combobox
            currentLanguge = ToolSettings.LanguageSuffix[msgLanguageSelect.SelectedIndex];

            //return if stating up editor (nothing to edit yet)
            if (startup)
                return;

            //set mgs data for current language
            Names = fullMsgListNames[currentLanguge];
            Descs = fullMsgListDescs[currentLanguge];
            HowTo = fullMsgListHowTo[currentLanguge];
            Burst = fullMsgListBurst[currentLanguge];
            BurstBTLHUD = fullMsgListBurstBTLHUD[currentLanguge];
            BurstPause = fullMsgListBurstPause[currentLanguge];

            //update each text field
            txtMsgName_TextChanged(null, null);
            txtMsgDesc_TextChanged(null, null);
            txtMsgHow_TextChanged(null, null);
            txtMsgLBDesc_TextChanged(null, null);
            txtMsgLBDescBTL_TextChanged(null, null);
        }

        public int FindmsgIndex(ref msg msgdata, int id)
        {
            for (int i = 0; i < msgdata.data.Length; i++)
            {
                if (msgdata.data[i].ID == id)
                    return i;
            }
            return 0;
        }

        //kinda really only for finding custom ki blast names
        public string FindMsgTextbyNameID(ref msg msgdata, int id)
        {
            for (int i = 0; i < msgdata.data.Length; i++)
            {
                if (msgdata.data[i].NameID == "ITM_MODBLT_" + id.ToString("000"))
                    return msgdata.data[i].Lines[0];
            }
            return "Unknown Type " + id.ToString();
        }

        public int FindMsgIndexbyNameID(msg msgdata, string id)
        {
            for (int i = 0; i < msgdata.data.Length; i++)
            {
                if (msgdata.data[i].NameID == id)
                    return i;
            }
            return -1;
        }

        //Demon: uneeded. tool now supports editing every language
        //UNLEASHED: function to write empty Msg text for generic Msg files (for syncing purposes)
        //void writeToMsgText(int MsgType, string Msg, int OLT_ID = -1)
        //{
        //    switch (MsgType)
        //    {
        //        case 0:
        //            {
        //                for (int i = 0; i < genericMsgListNames.Count; i++)
        //                {
        //                    GenericMsgFile tmp = genericMsgListNames[i];
        //                    msgData[] Expand2 = new msgData[tmp.msgFile_m.data.Length + 1];
        //                    Array.Copy(tmp.msgFile_m.data, Expand2, tmp.msgFile_m.data.Length);
        //                    Expand2[Expand2.Length - 1].NameID = "talisman_" + tmp.msgFile_m.data.Length.ToString("000");
        //                    Expand2[Expand2.Length - 1].ID = tmp.msgFile_m.data.Length;
        //                    Expand2[Expand2.Length - 1].Lines = new string[] { Msg };
        //                    tmp.msgFile_m.data = Expand2;
        //                    genericMsgListNames[i] = tmp;
        //                }
        //                break;
        //            }
        //        case 1:
        //            {
        //                for (int i = 0; i < genericMsgListDescs.Count; i++)
        //                {
        //                    GenericMsgFile tmp = genericMsgListDescs[i];
        //                    msgData[] Expand = new msgData[tmp.msgFile_m.data.Length + 1];
        //                    Array.Copy(tmp.msgFile_m.data, Expand, tmp.msgFile_m.data.Length);
        //                    Expand[Expand.Length - 1].NameID = "talisman_eff_" + tmp.msgFile_m.data.Length.ToString("000");
        //                    Expand[Expand.Length - 1].ID = tmp.msgFile_m.data.Length;
        //                    Expand[Expand.Length - 1].Lines = new string[] { Msg };
        //                    tmp.msgFile_m.data = Expand;
        //                    genericMsgListDescs[i] = tmp;
        //                }
        //                break;
        //            }
        //        case 2:
        //            {
        //                for (int i = 0; i < genericMsgListBurst.Count; i++)
        //                {
        //                    GenericMsgFile tmp = genericMsgListBurst[i];
        //                    msgData[] Expand4 = new msgData[tmp.msgFile_m.data.Length + 1];
        //                    Array.Copy(tmp.msgFile_m.data, Expand4, tmp.msgFile_m.data.Length);
        //                    Expand4[Expand4.Length - 1].NameID = "talisman_olt_" + tmp.msgFile_m.data.Length.ToString("000");
        //                    OLT_ID = tmp.msgFile_m.data.Length;
        //                    Expand4[Expand4.Length - 1].ID = tmp.msgFile_m.data.Length;
        //                    Expand4[Expand4.Length - 1].Lines = new string[] { Msg };
        //                    tmp.msgFile_m.data = Expand4;
        //                    genericMsgListBurst[i] = tmp;
        //                }
        //                break;
        //            }
        //        case 3:
        //            {
        //                for (int i = 0; i < genericMsgListNameBurstBTLHUD.Count; i++)
        //                {
        //                    GenericMsgFile tmp = genericMsgListNameBurstBTLHUD[i];
        //                    msgData[] Expand5 = new msgData[tmp.msgFile_m.data.Length + 1];
        //                    Array.Copy(tmp.msgFile_m.data, Expand5, tmp.msgFile_m.data.Length);
        //                    Expand5[Expand5.Length - 1].NameID = "BHD_OLT_000_" + OLT_ID.ToString();
        //                    Expand5[Expand5.Length - 1].ID = tmp.msgFile_m.data.Length;
        //                    Expand5[Expand5.Length - 1].Lines = new string[] { Msg };
        //                    tmp.msgFile_m.data = Expand5;
        //                    genericMsgListNameBurstBTLHUD[i] = tmp;
        //                }
        //                break;
        //            }
        //        case 4:
        //            {
        //                for (int i = 0; i < genericMsgListNameBurstPause.Count; i++)
        //                {
        //                    GenericMsgFile tmp = genericMsgListNameBurstPause[i];
        //                    msgData[] Expand6 = new msgData[tmp.msgFile_m.data.Length + 1];
        //                    Array.Copy(tmp.msgFile_m.data, Expand6, tmp.msgFile_m.data.Length);
        //                    Expand6[Expand6.Length - 1].NameID = "BHD_OLT_000_" + OLT_ID.ToString();
        //                    Expand6[Expand6.Length - 1].ID = tmp.msgFile_m.data.Length;
        //                    Expand6[Expand6.Length - 1].Lines = new string[] { Msg };
        //                    tmp.msgFile_m.data = Expand6;
        //                    genericMsgListNameBurstPause[i] = tmp;
        //                }
        //                break;
        //            }
        //        case 5:
        //            {
        //                for (int i = 0; i < genericMsgListHowTo.Count; i++)
        //                {
        //                    GenericMsgFile tmp = genericMsgListHowTo[i];
        //                    msgData[] Expand = new msgData[tmp.msgFile_m.data.Length + 1];
        //                    Array.Copy(tmp.msgFile_m.data, Expand, tmp.msgFile_m.data.Length);
        //                    Expand[Expand.Length - 1].NameID = "talisman_how_" + tmp.msgFile_m.data.Length.ToString("000");
        //                    Expand[Expand.Length - 1].ID = tmp.msgFile_m.data.Length;
        //                    Expand[Expand.Length - 1].Lines = new string[] { Msg };
        //                    tmp.msgFile_m.data = Expand;
        //                    genericMsgListHowTo[i] = tmp;
        //                }
        //                break;
        //            }
        //    }
        //}

        private void updateNameMsgID(object sender, EventArgs e)
        {
            short ID;
            if (short.TryParse(Name_ID.Text, out ID))
                Array.Copy(BitConverter.GetBytes(ID), 0, Items[itemList.SelectedIndex].Data, IdbOffsets["Name_ID"].Item2, 2);

            Items[itemList.SelectedIndex].msgIndexName = FindmsgIndex(ref Names, BitConverter.ToUInt16(Items[itemList.SelectedIndex].Data, IdbOffsets["Name_ID"].Item2));
            txtMsgName.Text = Names.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0];
            itemList.Items[itemList.SelectedIndex] = BitConverter.ToUInt16(Items[itemList.SelectedIndex].Data, 0).ToString() + " - " + Names.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0];
        }

        private void updateDescMsgID(object sender, EventArgs e)
        {
            short ID;

            if (short.TryParse(Info_ID.Text, out ID))
                Array.Copy(BitConverter.GetBytes(ID), 0, Items[itemList.SelectedIndex].Data, IdbOffsets["Info_ID"].Item2, 2);

            Items[itemList.SelectedIndex].msgIndexDesc = FindmsgIndex(ref Descs, BitConverter.ToUInt16(Items[itemList.SelectedIndex].Data, IdbOffsets["Info_ID"].Item2));
            txtMsgDesc.Text = Descs.data[Items[itemList.SelectedIndex].msgIndexDesc].Lines[0];
        }

        private void updateHowMsgID(object sender, EventArgs e)
        {
            short ID;

            if (short.TryParse(How_ID.Text, out ID))
                Array.Copy(BitConverter.GetBytes(ID), 0, Items[itemList.SelectedIndex].Data, IdbOffsets["How_ID"].Item2, 2);

            Items[itemList.SelectedIndex].msgIndexHow = FindmsgIndex(ref HowTo, BitConverter.ToUInt16(Items[itemList.SelectedIndex].Data, IdbOffsets["How_ID"].Item2));
            txtMsgHowDesc.Text = HowTo.data[Items[itemList.SelectedIndex].msgIndexHow].Lines[0];
        }

        private void txtMsgName_TextChanged(object sender, EventArgs e)
        {
            if (sender != null)
            {
                Names.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0] = txtMsgName.Text;
                itemList.Items[itemList.SelectedIndex] = BitConverter.ToUInt16(Items[itemList.SelectedIndex].Data, 0).ToString() + " - " + Names.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0];
            }
            else
            {
                for (int i = 0; i < itemList.Items.Count; i++)
                {
                    itemList.Items[i] = BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0];
                }
            }
        }

        private void txtMsgDesc_TextChanged(object sender, EventArgs e)
        {
            Descs.data[Items[itemList.SelectedIndex].msgIndexDesc].Lines[0] = txtMsgDesc.Text;
        }

        private void txtMsgHow_TextChanged(object sender, EventArgs e)
        {
            HowTo.data[Items[itemList.SelectedIndex].msgIndexHow].Lines[0] = txtMsgHowDesc.Text;
        }

        // Limit Burst data
        private void LB_QuickSelect(object sender, EventArgs e)
        {
            int index = debugLBSelect.SelectedIndex;

            //failsafe i guess. shouldn't happen but can never be too safe i suppose
            if (index > IDB.LimitBurstList.Count)
                index = IDB.LimitBurstList.Count;

            //set data for limit burst
            var LBdata = IDB.LimitBurstList[index];
            LB_Aura.Text = LBdata.Item1;
            LB_Desc.Text = LBdata.Item2;
            LB_Soul_ID1.Text = LBdata.Item3;
            LB_Soul_ID2.Text = LBdata.Item4;
            LB_Soul_ID3.Text = LBdata.Item5;
            LB_Color.SelectedIndex = LBdata.Item6;
        }

        private void updateBurstMsgID(object sender, EventArgs e)
        {
            short ID;

            if (short.TryParse(LB_Desc.Text, out ID))
                Array.Copy(BitConverter.GetBytes(ID), 0, Items[itemList.SelectedIndex].Data, IDB.IdbOffsets["LB_Desc"].Item2, 2);

            Items[itemList.SelectedIndex].msgIndexBurst = FindmsgIndex(ref Burst, BitConverter.ToUInt16(Items[itemList.SelectedIndex].Data, IDB.IdbOffsets["LB_Desc"].Item2));
            txtMsgLBDesc.Text = Burst.data[Items[itemList.SelectedIndex].msgIndexBurst].Lines[0];

            //Demon: updates the in battle description text when the description id is changed
            Items[itemList.SelectedIndex].msgIndexBurstBTL = getLB_BTL_Pause_DescID(BurstBTLHUD, Burst.data[Items[itemList.SelectedIndex].msgIndexBurst].NameID);
            Items[itemList.SelectedIndex].msgIndexBurstPause = getLB_BTL_Pause_DescID(BurstPause, Burst.data[Items[itemList.SelectedIndex].msgIndexBurst].NameID);
            txtMsgLBDescBTL.Text = BurstBTLHUD.data[Items[itemList.SelectedIndex].msgIndexBurstBTL].Lines[0];
        }

        private void txtMsgLBDesc_TextChanged(object sender, EventArgs e)
        {
            //UNLEASHED: get the LB index from the soul (warning, it is shared)
            Burst.data[Items[itemList.SelectedIndex].msgIndexBurst].Lines[0] = txtMsgLBDesc.Text;
        }

        private void txtMsgLBDescBTL_TextChanged(object sender, EventArgs e)
        {
            BurstBTLHUD.data[Items[itemList.SelectedIndex].msgIndexBurstBTL].Lines[0] = txtMsgLBDescBTL.Text;
            BurstPause.data[Items[itemList.SelectedIndex].msgIndexBurstPause].Lines[0] = txtMsgLBDescBTL.Text;
        }

        //UNLEASHED: added this function to retrieve LB_BTL / LB_PAUSE desc, as the ID for it doesn't exist in SS data 
        private int getLB_BTL_Pause_DescID(msg extraBurstMsgFile, string BurstEntryName)
        {
            int BurstID = -1;
            //cut the string and only get the ID, then convert to int
            if (!Int32.TryParse(BurstEntryName.Substring(13), out BurstID))
            {
                //something went wrong...
                return -1;
            }

            string BurstBTLHUDName = "BHD_OLT_000_";
            if (BurstID < 100)
                BurstBTLHUDName += BurstID.ToString("00");
            else
                BurstBTLHUDName += BurstID.ToString(); //hopefully we can have IDs above 99 that don't need that 000 prefix

            //for (int i = 0; i < extraBurstMsgFile.data.Length; i++)
            //{
            //    if (extraBurstMsgFile.data[i].NameID == BurstBTLHUDName)
            //        return i;
            //}

            return FindMsgIndexbyNameID(extraBurstMsgFile, BurstBTLHUDName);
        }

        //TODO: update addresses
        private int AddLB(byte[] SSData)

        {
            //UNLEASHED: this function will return the ItemList index of the latest installed Super Soul
            //if the returned int is "-1" then Super Soul failed to install.

            //Create and add Blank Super Soul (its not actually blank, it uses Raditz Super Soul as a base, which is a functional soul that has no effects)
            //loading
            // OpenFileDialog browseFile = new OpenFileDialog();
            // browseFile.Filter = "Super Soul Share File | *.zss";
            // browseFile.Title = "Select the Super Soul you want to import.";
            // if (browseFile.ShowDialog() == DialogResult.Cancel)
            //     return;

            idbItem[] items_org = Items;
            byte[] blankzss = SSData;

            int nameCount = BitConverter.ToInt32(blankzss, 4);
            int DescCount = BitConverter.ToInt32(blankzss, 8);
            int LBDescCount = BitConverter.ToInt32(blankzss, 16);
            int LBDescCountBtl = BitConverter.ToInt32(blankzss, 20);
            int LBDescCountPause = BitConverter.ToInt32(blankzss, 24);



            //UNLEASHED: we are gonna skip expanding itemlist until later..

            //==================================EXPAND ITEMS CODE=========================
            ////expand the item array
            //idbItem[] Expand = new idbItem[Items.Length + 1];
            ////copy the current items to the expanded array
            //Array.Copy(Items, Expand, Items.Length);
            ////add blank IDB data
            //Expand[Expand.Length - 1].Data = new byte[748];


            //Items = Expand;

            //==================================EXPAND ITEMS CODE=========================

            //UNLEASHED: first, lets the get the ID of the last SS's ID and increment by 1
            ushort ID = BitConverter.ToUInt16(Items[Items.Length - 1].Data, 0);
            ID++;

            bool foundProperID = true;
            int newPos = Items.Length; //UNLEASHED: Length = current items count + 1 (which is a  proper ID after we expand the list)

            //UNLEASHED: after incrementing by 1, we check if its above 32700 (very close to Int16.MaxValue)
            if (ID > 32700)
            {
                foundProperID = false;
                int currentItemIndex = Items.Length - 1;
                while ((currentItemIndex - 1) > 0)
                {
                    currentItemIndex--; //UNLEASHED: skiping last SS
                    ushort currID = BitConverter.ToUInt16(Items[currentItemIndex].Data, 0);
                    ushort nextID = BitConverter.ToUInt16(Items[currentItemIndex + 1].Data, 0);

                    if (currID + 1 < nextID && ((currID + 1) <= 32700)) // our new ID can go in the middle
                    {
                        foundProperID = true;
                        newPos = currentItemIndex + 1;
                        ID = (ushort)(currID + 1);
                        break;
                    }
                }
            }

            if (foundProperID)
            {
                //expand the item array
                idbItem[] Expand = new idbItem[Items.Length + 1];

                //copy the current items to the expanded array
                Array.Copy(Items, Expand, Items.Length);

                //add blank IDB data
                Expand[Expand.Length - 1].Data = new byte[772];

                //UNLEASHED: finally, set the new array with proper IDs
                Items = Expand;

                int currentIndex = Items.Length - 1;
                int prevIndex = Items.Length - 2;

                if (prevIndex < 0) //UNLEASHED: incase something went very wrong (corrupt IDB file?)
                {
                    MessageBox.Show("Cannot add new Super Soul");
                    Items = items_org;
                    return -1;
                }

                //UNLEASHED: Swap items until we reach newPos
                while (currentIndex != newPos)
                {
                    idbItem tempIDBItem = Items[currentIndex];
                    Items[currentIndex] = Items[prevIndex];
                    Items[prevIndex] = tempIDBItem;
                    currentIndex--;
                    prevIndex--;
                }
            }

            else
            {
                MessageBox.Show("Cannot add new Super Soul");
                Items = items_org;
                return -1;
            }

            Array.Copy(BitConverter.GetBytes(ID), Items[newPos].Data, 2);

            //apply Zss data to added z-soul
            //UNLEASHED: original code was multiplying lengths by 2 (this is because of unicode names)
            //instead, when exporting the SS get the length of the strings and multiy them by 2 before writing to binary
            //so here we read the number normally
            //Array.Copy(blankzss, 12 + (nameCount * 2) + (DescCount * 2), Items[newPos].Data, 2, 718);

            //UNLEASHED: + (4) is for limit burst linker 4 bytes, only useful in SSP
            Array.Copy(blankzss, 0x1C + (nameCount) + (DescCount) + (LBDescCount) + (LBDescCountBtl) + (LBDescCountPause), Items[newPos].Data, 2, 746);

            Items[newPos].msgIndexName = 0;
            Items[newPos].msgIndexDesc = 0;
            Items[newPos].msgIndexBurst = 0;
            Items[newPos].msgIndexBurstBTL = getLB_BTL_Pause_DescID(BurstBTLHUD, Burst.data[Items[newPos].msgIndexBurst].NameID);
            Items[newPos].msgIndexBurstPause = getLB_BTL_Pause_DescID(BurstPause, Burst.data[Items[newPos].msgIndexBurst].NameID);

            return newPos;
        }

        //TODO: Do we still need this? I think I handle LBs good enough.
        private bool isLimitBurst(byte[] SSFData)
        {
            if (BitConverter.ToUInt32(SSFData, 0x18) == 0xFFFFFFFF)
                return false;

            return true;
        }

        //TODO: Again, do we need this?
        private void resolveLBIDsForParentSS(ref List<int> indexCollections, byte[] SSFData, ushort LBID)
        {
            ushort parentSSIndex = BitConverter.ToUInt16(SSFData, 28);
            ushort burstSlot = BitConverter.ToUInt16(SSFData, 30);
            //
            int parentSSItemIndex = indexCollections[parentSSIndex];

            switch (burstSlot)
            {
                case 1:
                    {
                        byte[] changedBytes = BitConverter.GetBytes(LBID);
                        Items[parentSSItemIndex].Data = StaticMethods.replaceBytesInByteArray(Items[parentSSItemIndex].Data, changedBytes, 58);
                        break;
                    }

                case 2:
                    {
                        byte[] changedBytes = BitConverter.GetBytes(LBID);
                        Items[parentSSItemIndex].Data = StaticMethods.replaceBytesInByteArray(Items[parentSSItemIndex].Data, changedBytes, 60);
                        break;
                    }

                case 3:
                    {
                        byte[] changedBytes = BitConverter.GetBytes(LBID);
                        Items[parentSSItemIndex].Data = StaticMethods.replaceBytesInByteArray(Items[parentSSItemIndex].Data, changedBytes, 62);
                        break;
                    }
            }
        }

        //ListBoxes
        private void lstvBasic_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstvBasic.SelectedItems.Count != 0)
            {
                txtEditNameb.Text = lstvBasic.SelectedItems[0].SubItems[0].Text;
                txtEditValueb.Text = lstvBasic.SelectedItems[0].SubItems[1].Text;
            }
        }

        private void txtEditValueb_TextChanged(object sender, EventArgs e)
        {
            lstvBasic.SelectedItems[0].SubItems[1].Text = txtEditValueb.Text;
            int startOffset = IDB.Stats_Start + Effect_Start["_EB"];
            float n;

            if (float.TryParse(txtEditValueb.Text, out n))
                Array.Copy(BitConverter.GetBytes(n), 0, Items[itemList.SelectedIndex].Data, startOffset + (lstvBasic.SelectedItems[0].Index * 4), 4);

        }

        private void lstvEffect1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstvEffect1.SelectedItems.Count != 0)
            {
                txtEditName1.Text = lstvEffect1.SelectedItems[0].SubItems[0].Text;
                txtEditValue1.Text = lstvEffect1.SelectedItems[0].SubItems[1].Text;
            }
        }

        private void txtEditValue1_TextChanged(object sender, EventArgs e)
        {
            lstvEffect1.SelectedItems[0].SubItems[1].Text = txtEditValue1.Text;
            int startOffset = IDB.Stats_Start + Effect_Start["_E1"];
            float n;

            if (float.TryParse(txtEditValue1.Text, out n))
                Array.Copy(BitConverter.GetBytes(n), 0, Items[itemList.SelectedIndex].Data, startOffset + (lstvEffect1.SelectedItems[0].Index * 4), 4);
        }

        private void lstvEffect2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstvEffect2.SelectedItems.Count != 0)
            {
                txtEditName2.Text = lstvEffect2.SelectedItems[0].SubItems[0].Text;
                txtEditValue2.Text = lstvEffect2.SelectedItems[0].SubItems[1].Text;
            }
        }

        private void txtEditValue2_TextChanged(object sender, EventArgs e)
        {
            lstvEffect2.SelectedItems[0].SubItems[1].Text = txtEditValue2.Text;
            int startOffset = IDB.Stats_Start + Effect_Start["_E2"];
            float n;

            if (float.TryParse(txtEditValue2.Text, out n))
                Array.Copy(BitConverter.GetBytes(n), 0, Items[itemList.SelectedIndex].Data, startOffset + (lstvEffect2.SelectedItems[0].Index * 4), 4);
        }

        #endregion

        #region Data Reading

        //UNLEASHED: added this function to search for SS name
        private void srchBtn_Click(object sender, EventArgs e)
        {
            bool found = false;
            StringComparison sc = (caseSensetiveCheckBox.Checked == true) ?
            StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            // note that "lastInputString" and "lastInputIndex" are global vars.
            if (srchBox.TextLength > 0)
            {
                // if the user actually input a different string 
                // then we need to start from beginning
                if (lastInputString != srchBox.Text)
                {
                    lastInputString = srchBox.Text;
                    lastFoundIndex = 0;
                }

                //start loop from last found index
                //no need to go from start if we already got a match before
                for (int i = lastFoundIndex; i < itemList.Items.Count; i++)
                {
                    // we want to have an incremental search
                    // keep looking for the next item that matches the search
                    if (i > lastFoundIndex)
                    {

                        if (itemList.Items[i].ToString().IndexOf(srchBox.Text, sc) >= 0)
                        {
                            lastFoundIndex = i;

                            //set the selection to the found super soul
                            itemList.SelectedIndex = i;
                            found = true;
                            break;
                        }
                    }
                }

                // super soul was never found, reset from begining
                if (found == false)
                {
                    if (lastFoundIndex > 0)
                        MessageBox.Show("couldn't find any more super souls");
                    else
                        MessageBox.Show("super soul not found.. ");
                    lastFoundIndex = 0;
                }
            }
        }

        private void itemList_SelectedIndexChanged(object sender, EventArgs e)
        {
            hasSavedChanges = false;
            currentSuperSoulIndex = itemList.SelectedIndex;
            UpdateData();
        }

        private void UpdateData()
        {
            //update textboxes
            txtMsgName.Text = Names.data[Items[itemList.SelectedIndex].msgIndexName].Lines[0];
            txtMsgDesc.Text = Descs.data[Items[itemList.SelectedIndex].msgIndexDesc].Lines[0];
            txtMsgHowDesc.Text = HowTo.data[Items[itemList.SelectedIndex].msgIndexHow].Lines[0];
            txtMsgLBDesc.Text = Burst.data[Items[itemList.SelectedIndex].msgIndexBurst].Lines[0];
            txtMsgLBDescBTL.Text = BurstBTLHUD.data[Items[itemList.SelectedIndex].msgIndexBurstBTL].Lines[0];

            Tuple<string, int> type_off;

            //Super Soul Details
            foreach (string name in IdbOffsets.Keys)
            {
                //we don't edit these (yet)
                if (name == "Type" || name == "I_22")
                    continue;

                type_off = IdbOffsets[name];

                //combo boxes are unique (for now)
                if (name == "Rarity")
                {
                    Rarity.SelectedIndex = BitConverter.ToUInt16(Items[itemList.SelectedIndex].Data, type_off.Item2);
                    continue;
                }
                if (name == "KiBlast")
                {
                    KiBlast.SelectedIndex = IDB.KiBlastList.IndexOfKey(BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, type_off.Item2));
                    continue;
                }
                if (name == "LB_Color")
                {
                    LB_Color.SelectedIndex = IDB.LBColorList.IndexOfKey(BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, type_off.Item2));
                    continue;
                }

                //everything else
                TextBox textbox = (TextBox)Controls.Find(name, true).FirstOrDefault();
                switch (type_off.Item1)
                {
                    case "Int16":
                        textbox.Text = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, type_off.Item2).ToString();
                        break;
                    case "Int32":
                        textbox.Text = BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, type_off.Item2).ToString();
                        break;
                    default:
                        Console.WriteLine("Unknown type???");
                        break;
                }
            }

            //Basic/Effect Details
            foreach (string details in Effect_Start.Keys)
            {
                //each value in current details
                foreach (string name in EffectOffsets.Keys)
                {
                    //correct offset for current effect
                    type_off = EffectOffsets[name];
                    int effectOffset = Effect_Start[details] + type_off.Item2;

                    //combo boxes are unique (for now)
                    if (name == "Effect")
                    {
                        ComboBox box = (ComboBox)Controls.Find(name + details, true).FirstOrDefault();
                        box.SelectedIndex = IDB.EffectList.IndexOfKey(BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, effectOffset));
                        continue;
                    }
                    if (name == "Activate")
                    {
                        ComboBox box = (ComboBox)Controls.Find(name + details, true).FirstOrDefault();
                        box.SelectedIndex = IDB.ActivatorList.IndexOfKey(BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, effectOffset));
                        continue;
                    }
                    if (name == "Vfx_Type2" || name == "Vfx_Type1")
                    {
                        ComboBox box = (ComboBox)Controls.Find(name + details, true).FirstOrDefault();
                        box.SelectedIndex = IDB.VfxList.IndexOfKey(BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, effectOffset));
                        continue;
                    }
                    if (name == "Target")
                    {
                        ComboBox box = (ComboBox)Controls.Find(name + details, true).FirstOrDefault();
                        box.SelectedIndex = IDB.TargetList.IndexOfKey(BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, effectOffset));
                        continue;
                    }

                    //everything else
                    TextBox textbox = (TextBox)Controls.Find(name + details, true).FirstOrDefault();
                    switch (type_off.Item1)
                    {
                        case "Int32":
                            textbox.Text = BitConverter.ToInt32(Items[itemList.SelectedIndex].Data, effectOffset).ToString();
                            break;
                        case "Float":
                            textbox.Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, effectOffset).ToString();
                            break;
                        default:
                            Console.WriteLine("Unknown type???");
                            break;
                    }
                }

                //statlist values for current details
                int startOffset = IDB.Stats_Start + Effect_Start[details];
                if (details == "_EB")
                {
                    for (int i = 0; i < lstvBasic.Items.Count; i++)
                    {
                        lstvBasic.Items[i].SubItems[1].Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, startOffset + (i * 4)).ToString();
                    }
                    continue;
                }
                if (details == "_E1")
                {
                    for (int i = 0; i < lstvEffect1.Items.Count; i++)
                    {
                        lstvEffect1.Items[i].SubItems[1].Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, startOffset + (i * 4)).ToString();
                    }
                    continue;
                }
                if (details == "_E2")
                {
                    for (int i = 0; i < lstvEffect2.Items.Count; i++)
                    {
                        lstvEffect2.Items[i].SubItems[1].Text = BitConverter.ToSingle(Items[itemList.SelectedIndex].Data, startOffset + (i * 4)).ToString();
                    }
                    continue;
                }
            }

            if (lstvBasic.SelectedItems.Count != 0)
            {
                txtEditNameb.Text = lstvBasic.SelectedItems[0].SubItems[0].Text;
                txtEditValueb.Text = lstvBasic.SelectedItems[0].SubItems[1].Text;
            }
            if (lstvEffect1.SelectedItems.Count != 0)
            {
                txtEditName1.Text = lstvEffect1.SelectedItems[0].SubItems[0].Text;
                txtEditValue1.Text = lstvEffect1.SelectedItems[0].SubItems[1].Text;
            }
            if (lstvEffect2.SelectedItems.Count != 0)
            {
                txtEditName2.Text = lstvEffect2.SelectedItems[0].SubItems[0].Text;
                txtEditValue2.Text = lstvEffect2.SelectedItems[0].SubItems[1].Text;
            }
        }

        //TODO: Do we still want this
        private void installSSPFromArgs(string[] sspArgsPath)
        {
            if (Path.GetExtension(sspArgsPath[1].ToLower()) == ".ssp")
            {
                if (!importSSP(sspArgsPath[1]))
                {
                    MessageBox.Show("Error occurred when installing SSP file.");
                    Application.Exit();
                }

                itemList.Items.Clear();

                for (int i = 0; i < Items.Length; i++)
                    itemList.Items.Add(BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0]);

                SaveXV2SSEdit();
                MessageBox.Show("SSP imported successfully");
                Application.Exit();
            }
        }

        private void InitDirectory()
        {
            if (!settings.IsValidGameDir)
            {
                using (Forms.Settings settingsForm = new Forms.Settings(settings))
                {
                    settingsForm.ShowDialog();

                    if (settingsForm.Finished)
                    {
                        settings = settingsForm.settings;
                        settings.Save();
                    }
                    else
                    {
                        MessageBox.Show(this, "The setttings were not set. The application will now close.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        Application.Exit();
                    }
                }
            }
        }

        private void InitFileIO()
        {
            if (settings.IsValidGameDir)
            {
                //data1 is where system files are. data2 is sometimes ued for updates
                //idb files shouldn't be anywhere else
                fileIO = new Xv2FileIO(settings.GameDir, false, new string[2] { "data1.cpk", "data2.cpk" });
            }
            else
            {
                throw new Exception("DBXV2 game directory is invalid. Please reset settings and set the correct path.");
            }
        }

        private void LoadFiles()
        {
            byte[] idbfile = null;

            //Load Talisman IDB
            int count = 0;
            FileName = String.Format("{0}/data/system/item/talisman_item.idb", settings.GameDir);
            idbfile = fileIO.GetFileFromGame("system/item/talisman_item.idb");
            count = BitConverter.ToInt32(idbfile, 8);

            //in case anyone is crazy enough to get this far...
            if (count >= 32768)
            {
                MessageBox.Show(this, "Super Soul count has surpased the stable limit. You can still install more, but be aware that any more past id 32767 might not be read correctly by the game.", "Super Soul Count", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            //Load the Msg files
            List<string> langsSuffix = ToolSettings.LanguageSuffix.ToList<string>();
            string filename;
            msg msgFile;

            //loop through and fill dictionaries for all language msg
            for (int i = 0; i < ToolSettings.LanguageSuffix.Length; i++)
            {
                //load msg file for names
                filename = String.Format("{0}/data/msg/proper_noun_talisman_name_{1}.msg", settings.GameDir, langsSuffix[i]);
                msgFile = msgStream.Load(fileIO.GetFileFromGame(String.Format("msg/proper_noun_talisman_name_{0}.msg", langsSuffix[i])));
                fullMsgListNames.Add(langsSuffix[i], msgFile);
                fullFileNameMsgN.Add(langsSuffix[i], filename);

                //load msg file for descriptions
                filename = String.Format("{0}/data/msg/proper_noun_talisman_info_{1}.msg", settings.GameDir, langsSuffix[i]);
                msgFile = msgStream.Load(fileIO.GetFileFromGame(String.Format("msg/proper_noun_talisman_info_{0}.msg", langsSuffix[i])));
                fullMsgListDescs.Add(langsSuffix[i], msgFile);
                fullFileNameMsgD.Add(langsSuffix[i], filename);

                //load msg file for "how to get" descriptions
                filename = String.Format("{0}/data/msg/proper_noun_talisman_how_{1}.msg", settings.GameDir, langsSuffix[i]);
                msgFile = msgStream.Load(fileIO.GetFileFromGame(String.Format("msg/proper_noun_talisman_how_{0}.msg", langsSuffix[i])));
                fullMsgListHowTo.Add(langsSuffix[i], msgFile);
                fullFileNameMsgH.Add(langsSuffix[i], filename);

                //load msgfile for limit burst descriptions
                filename = String.Format("{0}/data/msg/proper_noun_talisman_info_olt_{1}.msg", settings.GameDir, langsSuffix[i]);
                msgFile = msgStream.Load(fileIO.GetFileFromGame(String.Format("msg/proper_noun_talisman_info_olt_{0}.msg", langsSuffix[i])));
                fullMsgListBurst.Add(langsSuffix[i], msgFile);
                fullFileNameMsgB.Add(langsSuffix[i], filename);

                filename = String.Format("{0}/data/msg/quest_btlhud_{1}.msg", settings.GameDir, langsSuffix[i]);
                msgFile = msgStream.Load(fileIO.GetFileFromGame(String.Format("msg/quest_btlhud_{0}.msg", langsSuffix[i])));
                fullMsgListBurstBTLHUD.Add(langsSuffix[i], msgFile);
                fullFileNameMsgB_BTLHUD.Add(langsSuffix[i], filename);

                filename = String.Format("{0}/data/msg/pause_text_{1}.msg", settings.GameDir, langsSuffix[i]);
                msgFile = msgStream.Load(fileIO.GetFileFromGame(String.Format("msg/pause_text_{0}.msg", langsSuffix[i])));
                fullMsgListBurstPause.Add(langsSuffix[i], msgFile);
                fullFileNameMsgB_Pause.Add(langsSuffix[i], filename);
            }

            //get msg for custom ki blast names to use later
            //we never edit this so lets only care about the en one
            msgFile = msgStream.Load(fileIO.GetFileFromGame(String.Format("msg/menu_shop_text_{0}.msg", langsSuffix[0])));

            //idb items set
            this.addNewXV2SSEditStripMenuItem.Enabled = true;
            this.msgToolStripMenuItem.Enabled = true;

            Items = new idbItem[count];
            Names = fullMsgListNames[currentLanguge];
            Descs = fullMsgListDescs[currentLanguge];
            HowTo = fullMsgListHowTo[currentLanguge];
            Burst = fullMsgListBurst[currentLanguge];
            BurstBTLHUD = fullMsgListBurstBTLHUD[currentLanguge];
            BurstPause = fullMsgListBurstPause[currentLanguge];

            //these are very unlikely to ever get changes/new entries
            _ = EffectData("LB_Color");
            _ = EffectData("Vfx_Type");

            //these could change in future game updates
            List<int> KiTypes = EffectData("KiBlast");
            List<int> Targets = EffectData("Target");
            List<int> Effects = EffectData("Effect");
            List<int> Actives = EffectData("Activate");

            bool shouldWarn = false;
            for (int i = 0; i < Items.Length; i++)
            {
                Items[i].Data = new byte[IDB.Idb_Size];
                Array.Copy(idbfile, 16 + (i * IDB.Idb_Size), Items[i].Data, 0, IDB.Idb_Size);

                Items[i].msgIndexName = FindmsgIndex(ref Names, BitConverter.ToUInt16(Items[i].Data, IdbOffsets["Name_ID"].Item2));
                Items[i].msgIndexDesc = FindmsgIndex(ref Descs, BitConverter.ToUInt16(Items[i].Data, IdbOffsets["Info_ID"].Item2));
                Items[i].msgIndexHow = FindmsgIndex(ref HowTo, BitConverter.ToUInt16(Items[i].Data, IdbOffsets["How_ID"].Item2));

                //UNLEASHED: Add BTL / PAUSE LB Desc
                Items[i].msgIndexBurst = FindmsgIndex(ref Burst, BitConverter.ToUInt16(Items[i].Data, IdbOffsets["LB_Desc"].Item2));
                Items[i].msgIndexBurstBTL = getLB_BTL_Pause_DescID(BurstBTLHUD, Burst.data[Items[i].msgIndexBurst].NameID);
                Items[i].msgIndexBurstPause = getLB_BTL_Pause_DescID(BurstPause, Burst.data[Items[i].msgIndexBurst].NameID);

                UsedIDs.Add(BitConverter.ToUInt16(Items[i].Data, IdbOffsets["Index"].Item2));

                //warn user later that a super soul is outside stable index
                if (BitConverter.ToUInt16(Items[i].Data, IdbOffsets["Index"].Item2) >= 32768)
                {
                    shouldWarn = true;
                }

                //check for unknowns

                //Kiblasts
                int KiTypeID = BitConverter.ToInt32(Items[i].Data, IdbOffsets["KiBlast"].Item2);
                if (!KiTypes.Contains(KiTypeID))
                {
                    string name = FindMsgTextbyNameID(ref msgFile, KiTypeID);
                    IDB.KiBlastList.Add(KiTypeID, name);
                    KiBlast.Items.Add(name);
                }

                //effect tabs
                foreach (string effectTab in IDB.Effect_Start.Keys)
                {
                    //effect 0 is alwasy the same in vanilla souls
                    if (effectTab == "_EB")
                        continue;

                    //Target
                    int TargetID = BitConverter.ToInt32(Items[i].Data, IDB.Effect_Start[effectTab] + EffectOffsets["Target"].Item2);
                    if (!Targets.Contains(TargetID))
                    {
                        //check last id in list and fill in gaps if needed
                        int LastID = IDB.TargetList.Last().Key;
                        for (int t = LastID; t <= TargetID; t++)
                        {
                            if (Targets.Contains(t))
                                continue;
                            IDB.TargetList.Add(t, "Unknown");
                            Target_EB.Items.Add(t.ToString() + " - Unknown");
                            Target_E1.Items.Add(t.ToString() + " - Unknown");
                            Target_E2.Items.Add(t.ToString() + " - Unknown");
                            Targets.Add(t);
                        }
                    }

                    //Effects
                    int EffectID = BitConverter.ToInt32(Items[i].Data, IDB.Effect_Start[effectTab] + EffectOffsets["Effect"].Item2);
                    if (!Effects.Contains(EffectID))
                    {
                        //check last id in list and fill in gaps if needed
                        int LastID = IDB.EffectList.Last().Key;
                        for (int e = LastID; e <= EffectID; e++)
                        {
                            if (Effects.Contains(e))
                                continue;
                            IDB.EffectList.Add(e, "Unknown");
                            Effect_EB.Items.Add(e.ToString() + " - Unknown");
                            Effect_E1.Items.Add(e.ToString() + " - Unknown");
                            Effect_E2.Items.Add(e.ToString() + " - Unknown");
                            Effects.Add(e);
                        }
                    }

                    //Activators
                    int ActiveID = BitConverter.ToInt32(Items[i].Data, IDB.Effect_Start[effectTab] + EffectOffsets["Activate"].Item2);
                    if (!Actives.Contains(ActiveID))
                    {
                        //check last id in list and fill in gaps if needed
                        int LastID = IDB.EffectList.Last().Key;
                        for (int a = LastID; a <= ActiveID; a++)
                        {
                            if (Actives.Contains(a))
                                continue;
                            IDB.ActivatorList.Add(a, "Unknown");
                            Activate_EB.Items.Add(a.ToString() + " - Unknown");
                            Activate_E1.Items.Add(a.ToString() + " - Unknown");
                            Activate_E2.Items.Add(a.ToString() + " - Unknown");
                            Actives.Add(a);
                        }
                    }
                }
            }

            itemList.Items.Clear();

            for (int i = 0; i < count; i++)
                itemList.Items.Add(BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0]);

            //EffectData();
            itemList.SelectedIndex = 0;

            //for when super souls outside stable range are detected
            if (shouldWarn)
            {
                MessageBox.Show(this, "One or multiple Super Souls are using an ID outside the stable range the game accepts (32768). You can continue to add and edit souls, but be aware that Souls outside this limit may not be read correctly by the game when equipped.", "Super Soul Stability", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        //TODO: setup overload code (maybe)
        public List<int> EffectData(string Type)
        {
            //Data from external EffectData.xml will be used to override internal data
            //difference from before is that it won't be a full override, it can be used to replace specific entries.
            //if (File.Exists(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"/" + "EffectData.xml"))
            //{
            //    //DEMON: This is now considered a debug feature.
            //    //load external EffectData.xml if it is found within the exe directory.
            //    XmlDocument ed = new XmlDocument();
            //    ed.Load(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + @"/" + "EffectData.xml");
            //    effList.ConstructList(ed.SelectSingleNode("EffectData/Effects").ChildNodes);
            //    actList.ConstructList(ed.SelectSingleNode("EffectData/Activators").ChildNodes);//Changed the section name from "Activations" cause it bothered me
            //    trgList.ConstructList(ed.SelectSingleNode("EffectData/Targets").ChildNodes);
            //    //lbcList.ConstructList(ed.SelectSingleNode("EffectData/Colors").ChildNodes);
            //    //kitList.ConstructList(ed.SelectSingleNode("EffectData/Kitypes").ChildNodes);
            //    //vfxList.ConstructList(ed.SelectSingleNode("EffectData/Vfxtypes").ChildNodes);////Changed the section name from "Checkboxs" cause typo and to better reflect actual funtion.
            //}

            //Get list info from IDB class
            List<int> KnownList = new List<int>();
            if (Type == "LB_Color")
            {
                foreach (var entry in IDB.LBColorList)
                {
                    LB_Color.Items.Add(entry.Value);
                    //KnownList.Add(entry.Item1);
                }
            }
            else if (Type == "Vfx_Type")
            {
                foreach (var entry in IDB.VfxList)
                {
                    Vfx_Type1_EB.Items.Add(entry.Value);
                    Vfx_Type1_E1.Items.Add(entry.Value);
                    Vfx_Type1_E2.Items.Add(entry.Value);
                    Vfx_Type2_EB.Items.Add(entry.Value);
                    Vfx_Type2_E1.Items.Add(entry.Value);
                    Vfx_Type2_E2.Items.Add(entry.Value);
                    //KnownList.Add(entry.Item1);
                }
            }
            else if (Type == "KiBlast")
            {
                foreach (var entry in IDB.KiBlastList)
                {
                    KiBlast.Items.Add(entry.Value);
                    KnownList.Add(entry.Key);
                }
            }
            else if (Type == "Target")
            {
                foreach (var entry in IDB.TargetList)
                {
                    Target_EB.Items.Add(entry.Value);
                    Target_E1.Items.Add(entry.Value);
                    Target_E2.Items.Add(entry.Value);
                    KnownList.Add(entry.Key);
                }
            }
            else if (Type == "Effect")
            {
                foreach (var entry in IDB.EffectList)
                {
                    Effect_EB.Items.Add(entry.Key.ToString() + " - " + entry.Value);
                    Effect_E1.Items.Add(entry.Key.ToString() + " - " + entry.Value);
                    Effect_E2.Items.Add(entry.Key.ToString() + " - " + entry.Value);
                    KnownList.Add(entry.Key);
                }
            }
            else if (Type == "Activate")
            {
                foreach (var entry in IDB.ActivatorList)
                {
                    Activate_EB.Items.Add(entry.Key.ToString() + " - " + entry.Value);
                    Activate_E1.Items.Add(entry.Key.ToString() + " - " + entry.Value);
                    Activate_E2.Items.Add(entry.Key.ToString() + " - " + entry.Value);
                    KnownList.Add(entry.Key);
                }
            }

            return KnownList;
        }

        #endregion

        private void SaveXV2SSEdit()
        {
            List<byte> Finalize = new List<byte>();

            //UNLEASHED: HEADER
            Finalize.AddRange(new byte[] { 0x23, 0x49, 0x44, 0x42, 0xFE, 0xFF, 0x07, 0x00 });

            //UNLEASHED: ITEM COUNT
            Finalize.AddRange(BitConverter.GetBytes(Items.Length));

            //UNLEASHED: OFFSET OF FIRST SS
            Finalize.AddRange(new byte[] { 0x10, 0x00, 0x00, 0x00 });

            //UNLEASHED: ADD IDM ITEMS
            for (int i = 0; i < Items.Length; i++)
                Finalize.AddRange(Items[i].Data);

            FileStream fs = new FileStream(FileName, FileMode.Create);
            fs.Write(Finalize.ToArray(), 0, Finalize.Count);
            fs.Close();

            //Demon: use keys from one of these lists. they are both the same anyway
            foreach (string lang in fullMsgListNames.Keys)
            {
                msgStream.Save(fullMsgListNames[lang], fullFileNameMsgN[lang]);
                msgStream.Save(fullMsgListDescs[lang], fullFileNameMsgD[lang]);
                msgStream.Save(fullMsgListHowTo[lang], fullFileNameMsgH[lang]);
                msgStream.Save(fullMsgListBurst[lang], fullFileNameMsgB[lang]);
                msgStream.Save(fullMsgListBurstBTLHUD[lang], fullFileNameMsgB_BTLHUD[lang]);
                msgStream.Save(fullMsgListBurstPause[lang], fullFileNameMsgB_Pause[lang]);
            }
        }

        private int AddSSNew(byte[] SSData)
        {
            //UNLEASHED: this function will return the ItemList index of the latest installed Super Soul
            //if the returned int is "-1" then Super Soul failed to install.

            if (BitConverter.ToInt32(SSData, 0) != 843469603)
            {
                MessageBox.Show("Not Valid Super Soul Data.");
                return -1;
            }

            //search for closest unused id after vanilla ids
            //should be 1003+, but we start searching from 1000 just because
            ushort FreeID = 1000;
            int LastUsedIndex = 0;
            bool foundProperID = false;
            while (!foundProperID)
            {
                if (!UsedIDs.Contains(FreeID))
                {
                    foundProperID = true;
                }
                else
                {
                    LastUsedIndex = UsedIDs.IndexOf(FreeID) + 1;
                    FreeID++;
                }
            }

            //get number of LB souls
            ushort LimitNum = BitConverter.ToUInt16(SSData, 8);
            bool LimitLv1 = BitConverter.ToInt16(SSData, 10) != -1;
            bool LimitLv2 = BitConverter.ToInt16(SSData, 12) != -1;
            bool LimitLv3 = BitConverter.ToInt16(SSData, 14) != -1;

            //start expanding
            idbItem[] SoulExpand = new idbItem[Items.Length + LimitNum + 1];

            //add each soul to new list up to last new index
            Array.Copy(Items, 0, SoulExpand, 0, LastUsedIndex);

            //blank soul for adding
            byte[] NewSoulData = new byte[IDB.Idb_Size];

            //add new super soul data
            int SoulPos = (LimitNum + 1) * IDB.Idb_Size; //position of the main super soul in SSF
            Array.Copy(SSData, SSData.Length - SoulPos, NewSoulData, 0, IDB.Idb_Size); //copy main soul data
            Array.Copy(BitConverter.GetBytes(FreeID), NewSoulData, 2); //might as well fix main soul id here
            SoulExpand[LastUsedIndex].Data = new byte[IDB.Idb_Size];
            Array.Copy(NewSoulData, SoulExpand[LastUsedIndex].Data, IDB.Idb_Size); //add it to expand list
            //SoulExpand[LastUsedIndex].Data = NewSoulData; //add it to expand list

            //remember to add new id to used ids list
            UsedIDs.Insert(LastUsedIndex, FreeID);

            //add limit bursts
            int tmpNum = 1; //helps keep track of what lb soul we are on
            while (LimitNum > 0)
            {
                int LBSoulPos = LimitNum * IDB.Idb_Size;
                Array.Copy(SSData, SSData.Length - LBSoulPos, NewSoulData, 0, IDB.Idb_Size);
                Array.Copy(BitConverter.GetBytes(FreeID + tmpNum), NewSoulData, 2);

                //not needed (i think), but it bothers me when LB souls don't use default stuff lol
                NewSoulData = SoulToLB(NewSoulData);

                SoulExpand[LastUsedIndex + tmpNum].Data = new byte[IDB.Idb_Size];
                Array.Copy(NewSoulData, SoulExpand[LastUsedIndex + tmpNum].Data, IDB.Idb_Size);
                //SoulExpand[LastUsedIndex + tmpNum].Data = NewSoulData;

                //fix limit burst ids on main soul
                if (LimitLv1)
                {
                    Array.Copy(BitConverter.GetBytes(FreeID + tmpNum), 0, SoulExpand[LastUsedIndex].Data, IdbOffsets["LB_Soul_ID1"].Item2, 2);
                    LimitLv1 = false;
                }
                else if (LimitLv2)
                {
                    Array.Copy(BitConverter.GetBytes(FreeID + tmpNum), 0, SoulExpand[LastUsedIndex].Data, IdbOffsets["LB_Soul_ID2"].Item2, 2);
                    LimitLv2 = false;
                }
                else if (LimitLv3)
                {
                    Array.Copy(BitConverter.GetBytes(FreeID + tmpNum), 0, SoulExpand[LastUsedIndex].Data, IdbOffsets["LB_Soul_ID3"].Item2, 2);
                    LimitLv3 = false;
                }

                //remember to add new id to used ids list
                UsedIDs.Insert(LastUsedIndex + tmpNum, (ushort)(FreeID + tmpNum));

                tmpNum++;
                LimitNum--;
            }

            //add rest of original souls
            Array.Copy(Items, LastUsedIndex, SoulExpand, LastUsedIndex + tmpNum, Items.Length - LastUsedIndex);

            //finish adding souls
            Items = SoulExpand;

            //Expand msg
            //reminder: all msg strings are length followed immediately by string data back-to-back until soul data
            int currentOffset = 0x10;
            byte[] MsgText = null;
            byte[] MsgID = null;
            List<string> languages = new List<string> { "en", "es", "ca", "fr", "de", "it", "pt", "pl", "ru", "tw", "zh", "kr", "ja" };

            //Names
            foreach (string lang in languages)
            {
                Names = fullMsgListNames[lang];

                msgData[] MsgExpand_Names = new msgData[Names.data.Length + 1]; //create tmp expand
                Array.Copy(Names.data, MsgExpand_Names, Names.data.Length); //copy all original entries to tmp

                //UNLEASHED:i'm guessing MSG IDs are zero based so calling length is like IDs + 1
                MsgExpand_Names[MsgExpand_Names.Length - 1].NameID = "talisman_" + Names.data.Length.ToString("000");
                MsgExpand_Names[MsgExpand_Names.Length - 1].ID = Names.data.Length;

                //get text data
                int StrLength = BitConverter.ToInt32(SSData, currentOffset);
                currentOffset += 4;

                if (StrLength > 0)
                {
                    MsgText = new byte[StrLength];
                    Array.Copy(SSData, currentOffset, MsgText, 0, StrLength);
                    MsgExpand_Names[MsgExpand_Names.Length - 1].Lines = new string[] { BytetoString(MsgText) };
                }
                else
                {
                    MsgExpand_Names[MsgExpand_Names.Length - 1].Lines = new string[] { "New Name Entry" };
                }

                //fix msg id on main soul
                //we only need to do this once cause shared IDs so just do it for a single language
                if (lang == "en")
                {
                    MsgID = BitConverter.GetBytes((short)MsgExpand_Names[MsgExpand_Names.Length - 1].ID);
                    Array.Copy(MsgID, 0, Items[LastUsedIndex].Data, IdbOffsets["Name_ID"].Item2, 2);
                    Items[LastUsedIndex].msgIndexName = BitConverter.ToInt16(MsgID, 0);
                }

                //finish
                Names.data = MsgExpand_Names;
                fullMsgListNames[lang] = Names;
                currentOffset += StrLength;
            }

            //Descriptions
            foreach (string lang in languages)
            {
                Descs = fullMsgListDescs[lang];

                msgData[] MsgExpand_Descs = new msgData[Descs.data.Length + 1]; //create tmp expand
                Array.Copy(Descs.data, MsgExpand_Descs, Descs.data.Length); //copy all original entries to tmp

                //UNLEASHED:i'm guessing MSG IDs are zero based so calling length is like IDs + 1
                MsgExpand_Descs[MsgExpand_Descs.Length - 1].NameID = "talisman_eff_" + Descs.data.Length.ToString("000");
                MsgExpand_Descs[MsgExpand_Descs.Length - 1].ID = Descs.data.Length;

                //get text data
                int StrLength = BitConverter.ToInt32(SSData, currentOffset);
                currentOffset += 4;

                if (StrLength > 0)
                {
                    MsgText = new byte[StrLength];
                    Array.Copy(SSData, currentOffset, MsgText, 0, StrLength);
                    MsgExpand_Descs[MsgExpand_Descs.Length - 1].Lines = new string[] { BytetoString(MsgText) };
                }
                else
                {
                    MsgExpand_Descs[MsgExpand_Descs.Length - 1].Lines = new string[] { "New Description Entry" };
                }

                //fix msg id on main soul
                //we only need to do this once cause of shared IDs so just do it for a single language
                if (lang == "en")
                {
                    MsgID = BitConverter.GetBytes((short)MsgExpand_Descs[MsgExpand_Descs.Length - 1].ID);
                    Array.Copy(MsgID, 0, Items[LastUsedIndex].Data, IdbOffsets["Info_ID"].Item2, 2);
                    Items[LastUsedIndex].msgIndexDesc = BitConverter.ToInt16(MsgID, 0);
                }

                //finish
                Descs.data = MsgExpand_Descs;
                fullMsgListDescs[lang] = Descs;
                currentOffset += StrLength;
            }

            //How To
            foreach (string lang in languages)
            {
                HowTo = fullMsgListHowTo[lang];

                msgData[] MsgExpand_HowTo = new msgData[HowTo.data.Length + 1]; //create tmp expand
                Array.Copy(HowTo.data, MsgExpand_HowTo, HowTo.data.Length); //copy all original entries to tmp

                //UNLEASHED:i'm guessing MSG IDs are zero based so calling length is like IDs + 1
                MsgExpand_HowTo[MsgExpand_HowTo.Length - 1].NameID = "talisman_how_" + HowTo.data.Length.ToString("000");
                MsgExpand_HowTo[MsgExpand_HowTo.Length - 1].ID = HowTo.data.Length;

                //get text data
                int StrLength = BitConverter.ToInt32(SSData, currentOffset);
                currentOffset += 4;

                if (StrLength > 0)
                {
                    MsgText = new byte[StrLength];
                    Array.Copy(SSData, currentOffset, MsgText, 0, StrLength);
                    MsgExpand_HowTo[MsgExpand_HowTo.Length - 1].Lines = new string[] { BytetoString(MsgText) };
                }
                else
                {
                    MsgExpand_HowTo[MsgExpand_HowTo.Length - 1].Lines = new string[] { "New Location Lookup Entry" };
                }

                //fix msg id on main soul
                //we only need to do this once cause of shared IDs so just do it for a single language
                if (lang == "en")
                {
                    MsgID = BitConverter.GetBytes((short)MsgExpand_HowTo[MsgExpand_HowTo.Length - 1].ID);
                    Array.Copy(MsgID, 0, Items[LastUsedIndex].Data, IdbOffsets["How_ID"].Item2, 2);
                    Items[LastUsedIndex].msgIndexHow = BitConverter.ToInt16(MsgID, 0);
                }

                //finish
                HowTo.data = MsgExpand_HowTo;
                fullMsgListHowTo[lang] = HowTo;
                currentOffset += StrLength;
            }

            //Limit Burst Desctiption
            foreach (string lang in languages)
            {
                Burst = fullMsgListBurst[lang];

                msgData[] MsgExpand_Burst = new msgData[Burst.data.Length + 1]; //create tmp expand
                Array.Copy(Burst.data, MsgExpand_Burst, Burst.data.Length); //copy all original entries to tmp

                //UNLEASHED:i'm guessing MSG IDs are zero based so calling length is like IDs + 1
                MsgExpand_Burst[MsgExpand_Burst.Length - 1].NameID = "talisman_olt_" + Burst.data.Length.ToString("000");
                MsgExpand_Burst[MsgExpand_Burst.Length - 1].ID = Burst.data.Length;

                //get text data
                int StrLength = BitConverter.ToInt32(SSData, currentOffset);
                currentOffset += 4;

                if (StrLength > 0)
                {
                    MsgText = new byte[StrLength];
                    Array.Copy(SSData, currentOffset, MsgText, 0, StrLength);
                    MsgExpand_Burst[MsgExpand_Burst.Length - 1].Lines = new string[] { BytetoString(MsgText) };
                }
                else
                {
                    MsgExpand_Burst[MsgExpand_Burst.Length - 1].Lines = new string[] { "New LB Desc Entry" };
                }

                //fix msg id on main soul
                //we only need to do this once cause of shared IDs so just do it for a single language
                if (lang == "en")
                {
                    MsgID = BitConverter.GetBytes((short)MsgExpand_Burst[MsgExpand_Burst.Length - 1].ID);
                    Array.Copy(MsgID, 0, Items[LastUsedIndex].Data, IdbOffsets["LB_Desc"].Item2, 2);
                    Items[LastUsedIndex].msgIndexBurst = BitConverter.ToInt16(MsgID, 0);
                }

                //finish
                Burst.data = MsgExpand_Burst;
                fullMsgListBurst[lang] = Burst;
                currentOffset += StrLength;
            }

            //Limit Burst Battle Pop-up Text
            foreach (string lang in languages)
            {
                BurstBTLHUD = fullMsgListBurstBTLHUD[lang];

                msgData[] MsgExpand_BurstBTL = new msgData[BurstBTLHUD.data.Length + 1]; //create tmp expand
                Array.Copy(BurstBTLHUD.data, MsgExpand_BurstBTL, BurstBTLHUD.data.Length); //copy all original entries to tmp

                //UNLEASHED:i'm guessing MSG IDs are zero based so calling length is like IDs + 1
                MsgExpand_BurstBTL[MsgExpand_BurstBTL.Length - 1].NameID = "BHD_OLT_000_" + Items[LastUsedIndex].msgIndexBurst.ToString(); //BurstBTLHUD.data.Length.ToString("000");
                MsgExpand_BurstBTL[MsgExpand_BurstBTL.Length - 1].ID = BurstBTLHUD.data.Length;

                //get text data
                int StrLength = BitConverter.ToInt32(SSData, currentOffset);
                currentOffset += 4;

                if (StrLength > 0)
                {
                    MsgText = new byte[StrLength];
                    Array.Copy(SSData, currentOffset, MsgText, 0, StrLength);
                    MsgExpand_BurstBTL[MsgExpand_BurstBTL.Length - 1].Lines = new string[] { BytetoString(MsgText) };
                }
                else
                {
                    MsgExpand_BurstBTL[MsgExpand_BurstBTL.Length - 1].Lines = new string[] { "New LB Battle Desc Entry" };
                }

                //we only need to do this once cause of shared IDs so just do it for a single language
                if (lang == "en")
                {
                    MsgID = BitConverter.GetBytes((short)MsgExpand_BurstBTL[MsgExpand_BurstBTL.Length - 1].ID);
                    Items[LastUsedIndex].msgIndexBurstBTL = BitConverter.ToInt16(MsgID, 0);
                }

                //finish
                BurstBTLHUD.data = MsgExpand_BurstBTL;
                fullMsgListBurstBTLHUD[lang] = BurstBTLHUD;
                currentOffset += StrLength;
            }

            //Limit Burst Pause Text
            foreach (string lang in languages)
            {
                BurstPause = fullMsgListBurstPause[lang];

                msgData[] MsgExpand_Burst = new msgData[BurstPause.data.Length + 1]; //create tmp expand
                Array.Copy(BurstPause.data, MsgExpand_Burst, BurstPause.data.Length); //copy all original entries to tmp

                //UNLEASHED:i'm guessing MSG IDs are zero based so calling length is like IDs + 1
                MsgExpand_Burst[MsgExpand_Burst.Length - 1].NameID = "BHD_OLT_000_" + Items[LastUsedIndex].msgIndexBurst.ToString(); //BurstPause.data.Length.ToString("000");
                MsgExpand_Burst[MsgExpand_Burst.Length - 1].ID = BurstPause.data.Length;

                //get text data
                int StrLength = BitConverter.ToInt32(SSData, currentOffset);
                currentOffset += 4;

                if (StrLength > 0)
                {
                    MsgText = new byte[StrLength];
                    Array.Copy(SSData, currentOffset, MsgText, 0, StrLength);
                    MsgExpand_Burst[MsgExpand_Burst.Length - 1].Lines = new string[] { BytetoString(MsgText) };
                }
                else
                {
                    MsgExpand_Burst[MsgExpand_Burst.Length - 1].Lines = new string[] { "New LB Battle Desc Entry" };
                }

                //we only need to do this once cause of shared IDs so just do it for a single language
                if (lang == "en")
                {
                    MsgID = BitConverter.GetBytes((short)MsgExpand_Burst[MsgExpand_Burst.Length - 1].ID);
                    Items[LastUsedIndex].msgIndexBurstPause = BitConverter.ToInt16(MsgID, 0);
                }

                //finish
                BurstPause.data = MsgExpand_Burst;
                fullMsgListBurstPause[lang] = BurstPause;
                currentOffset += StrLength;
            }

            //makes sure text is set back to correct language
            Names = fullMsgListNames[currentLanguge];
            Descs = fullMsgListDescs[currentLanguge];
            HowTo = fullMsgListHowTo[currentLanguge];
            Burst = fullMsgListBurst[currentLanguge];
            BurstBTLHUD = fullMsgListBurstBTLHUD[currentLanguge];
            BurstPause = fullMsgListBurstPause[currentLanguge];

            return LastUsedIndex;
        }

        //FIX for new format
        private bool importSSP(string sspPath)
        {
            SSP sspFile = new SSP();
            sspFile.SSPRead(sspPath);

            //UNLEASHED: before we import, lets create a backup for all MSG files and the IDB
            //so we can revert back incase anything happens (mostly if there is not enough space to install SS)

            msg orgNames = Names;
            msg orgDescs = Descs;
            msg orgBurst = Burst;
            msg orgBurstBTLHUD = BurstBTLHUD;
            msg orgBurstPause = BurstPause;
            idbItem[] orgItems = Items;

            List<int> indexCollections = new List<int>();
            int index = -1;
            byte[] tempData;
            ushort intIDofLB;

            //UNLEASHED: in the SSP, it is expected that parent souls are installed first and then the child limit burst souls.
            for (int i = 0; i < sspFile.Souls.Count(); i++)
            {
                tempData = sspFile.Souls[i].m_data;
                //index = AddSS(tempData);

                if (index < 0)
                {
                    Names = orgNames;
                    Descs = orgDescs;
                    Burst = orgBurst;
                    BurstBTLHUD = orgBurstBTLHUD;
                    BurstPause = orgBurstPause;
                    Items = orgItems;
                    return false;
                }

                intIDofLB = BitConverter.ToUInt16(Items[index].Data, 0);

                if (!isLimitBurst(tempData))
                    indexCollections.Add(index);
                else
                    resolveLBIDsForParentSS(ref indexCollections, tempData, intIDofLB);
            }
            return true;
        }

        //UNLEASHED: Mugen's code to convert MSG strings from unicode to ASCII format
        //UNLEASHED: we could have use the .NET natvie System.Text methods. but this works..
        private string BytetoString(byte[] bytes)
        {
            char[] chrArray = new char[bytes.Length / 2];

            for (int i = 0; i < bytes.Length / 2; i++)
                chrArray[i] = BitConverter.ToChar(bytes, i * 2);

            return new string(chrArray);
        }

        //for setting soul info and such to that of what Limit Burst souls use
        public byte[] SoulToLB(byte[] SoulData)
        {
            byte[] tmpSoul = SoulData;

            Array.Copy(BitConverter.GetBytes(5), 0, tmpSoul, IdbOffsets["Rarity"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["Name_ID"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["Info_ID"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["How_ID"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["I_10"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(255), 0, tmpSoul, IdbOffsets["Dlc_Flag"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(-1), 0, tmpSoul, IdbOffsets["I_16"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(-1), 0, tmpSoul, IdbOffsets["I_18"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(32767), 0, tmpSoul, IdbOffsets["Prog_Flag"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["I_22"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["Cost"].Item2, 4);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["Sell"].Item2, 4);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["Race"].Item2, 4);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["Cost_TP"].Item2, 4);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["Cost_STP"].Item2, 4);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["I_44"].Item2, 4);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["KiBlast"].Item2, 4);
            Array.Copy(BitConverter.GetBytes(-1), 0, tmpSoul, IdbOffsets["LB_Aura"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(-1), 0, tmpSoul, IdbOffsets["LB_Color"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(0), 0, tmpSoul, IdbOffsets["LB_Desc"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(-1), 0, tmpSoul, IdbOffsets["LB_Soul_ID1"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(-1), 0, tmpSoul, IdbOffsets["LB_Soul_ID2"].Item2, 2);
            Array.Copy(BitConverter.GetBytes(-1), 0, tmpSoul, IdbOffsets["LB_Soul_ID3"].Item2, 2);

            return tmpSoul;
        }
    }
}