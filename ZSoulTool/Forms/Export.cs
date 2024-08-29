﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Msgfile;

namespace XV2SSEdit
{
    public partial class Export : Form
    {
        private Form1 form1;
        private msg Names;
        private msg Descs;
        private msg Burst;
        private msg BurstBTLHUD;
        private msg BurstPause;
        private msg HowTo;

        //dict full of all msg text used for each language
        //private Dictionary<string, msg> fullMsgListNames = new Dictionary<string, msg>();
        //private Dictionary<string, msg> fullMsgListDescs = new Dictionary<string, msg>();
        //private Dictionary<string, msg> fullMsgListHowTo = new Dictionary<string, msg>();
        //private Dictionary<string, msg> fullMsgListBurst = new Dictionary<string, msg>();
        //private Dictionary<string, msg> fullMsgListBurstBTLHUD = new Dictionary<string, msg>();
        //private Dictionary<string, msg> fullMsgListBurstPause = new Dictionary<string, msg>();

        private int numOfExportedItems = 0;

        public Export(Form1 form1,msg msg1,msg msg2,msg msg3,msg msg4,msg msg5, msg msg6)
        {
            this.form1 = form1;
            //UNLEASHED: the following vars are used to get rid of Warning C1690
            //apparently, the MSG class refereces Marshal objects or something...
            Names = msg1;
            Descs = msg2;
            Burst = msg3;
            BurstBTLHUD = msg4;
            BurstPause = msg5;
            HowTo = msg6;

            InitializeComponent();
        }

        List<byte[]> limitBursts = new List<byte[]>();
        List<SSObject> SSObjects = new List<SSObject>();
        SSP sspFile;

        public struct SSObject
        {
            public byte[] SSFData; //SSF Format SuperSoul
            public bool shouldExport;
            public string SSName;
            public bool exportBurst1;
            public bool exportBurst2;
            public bool exportBurst3;
            public ushort burst1ID;
            public ushort burst2ID;
            public ushort burst3ID;
            public SSObject(byte[] rb, bool se, string ssn, ushort b1ID, ushort b2ID, ushort b3ID, bool eb1, bool eb2, bool eb3)
            {
                SSFData = rb;
                shouldExport = se;
                SSName = ssn;
                burst1ID = b1ID;
                burst2ID = b2ID;
                burst3ID = b3ID;
                exportBurst1 = eb1;
                exportBurst2 = eb2;
                exportBurst3 = eb3;
            }
        }

        private void Export_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < form1.Items.Length; i++)
            {
                if (isSuperSoul(form1.Items[i].Data,i))
                {
                    exportSoulsList.Items.Add(BitConverter.ToUInt16(form1.Items[i].Data, 0).ToString() + " - " + Names.data[form1.Items[i].msgIndexName].Lines[0]);
                }
            }

            exportSoulsList.SelectedIndex = 0;
        }
    
        private bool isSuperSoul(byte[] ssData,int SSIndex)
        {
            //UNLEASHED: the only way i can think of to check if a soul is a super soul (and not a limit burst) 
            //is to check if it has Burst IDs NOT set to NULL
            //which is normally what a vanilla limit burst would have.
            //this should be put in the "help" button as it may confuse some users
            ushort Burst1ID = BitConverter.ToUInt16(ssData, IDB.IdbOffsets["LB_Soul_ID1"].Item2);
            ushort Burst2ID = BitConverter.ToUInt16(ssData, IDB.IdbOffsets["LB_Soul_ID2"].Item2);
            ushort Burst3ID = BitConverter.ToUInt16(ssData, IDB.IdbOffsets["LB_Soul_ID3"].Item2);

            bool isSS = (Burst1ID != 0xFFFF && Burst2ID != 0xFFFF && Burst3ID != 0xFFFF);
            if (isSS)
            {
                //UNLEASHED: while we are here, we might aswell build the SS Object
                List<byte> SSFFile = new List<byte>();
                SSFFile.AddRange(new byte[] { 0x23, 0x53, 0x46, 0x32 }); //#SF2
                SSFFile.AddRange(new byte[] { 0x02, 0x00, 0x00, 0x00 }); //Version number
                SSFFile.AddRange(new byte[] { 0x00, 0x00 }); //LB soul count
                SSFFile.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }); //Limit Burst Identifiers


                //could be better, but oh well this works
                List<string> languages = new List<string> { "en", "es", "ca", "fr", "de", "it", "pt", "pl", "ru", "tw", "zh", "kr", "ja" };
                string MsgText = "";
                int TextLength = 0;
                string soulName = "";

                //Name
                foreach (string lang in languages)
                {
                    //Name
                    MsgText = form1.fullMsgListNames[lang].data[form1.Items[SSIndex].msgIndexName].Lines[0];
                    TextLength = MsgText.Length * 2;
                    SSFFile.AddRange(BitConverter.GetBytes(TextLength));
                    SSFFile.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));

                    if (lang == form1.currentLanguge)
                        soulName = MsgText;
                }

                //Description
                foreach (string lang in languages)
                {
                    MsgText = form1.fullMsgListDescs[lang].data[form1.Items[SSIndex].msgIndexDesc].Lines[0];
                    TextLength = MsgText.Length * 2;
                    SSFFile.AddRange(BitConverter.GetBytes(TextLength));
                    SSFFile.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
                }

                //HowTo
                foreach (string lang in languages)
                {
                    MsgText = form1.fullMsgListHowTo[lang].data[form1.Items[SSIndex].msgIndexHow].Lines[0];
                    TextLength = MsgText.Length * 2;
                    SSFFile.AddRange(BitConverter.GetBytes(TextLength));
                    SSFFile.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
                }

                //Burst
                foreach (string lang in languages)
                {
                    MsgText = form1.fullMsgListBurst[lang].data[form1.Items[SSIndex].msgIndexBurst].Lines[0];
                    TextLength = MsgText.Length * 2;
                    SSFFile.AddRange(BitConverter.GetBytes(TextLength));
                    SSFFile.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
                }

                //Burst HUD
                foreach (string lang in languages)
                {
                    MsgText = form1.fullMsgListBurstBTLHUD[lang].data[form1.Items[SSIndex].msgIndexBurstBTL].Lines[0];
                    TextLength = MsgText.Length * 2;
                    SSFFile.AddRange(BitConverter.GetBytes(TextLength));
                    SSFFile.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
                }

                //Burst Pause
                foreach (string lang in languages)
                {
                    if (form1.Items[SSIndex].msgIndexBurstPause > -1)
                        MsgText = form1.fullMsgListBurstPause[lang].data[form1.Items[SSIndex].msgIndexBurstPause].Lines[0];
                    else
                        MsgText = "";

                    TextLength = MsgText.Length * 2;
                    SSFFile.AddRange(BitConverter.GetBytes(TextLength));
                    SSFFile.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
                }

                //Soul Data
                byte[] itempass = new byte[IDB.Idb_Size];
                Array.Copy(form1.Items[SSIndex].Data, 0, itempass, 0, IDB.Idb_Size);
                SSFFile.AddRange(itempass);

                SSObjects.Add(new SSObject(SSFFile.ToArray(),false, soulName, Burst1ID, Burst2ID, Burst3ID, false, false, false));
            }

            return isSS;       
        }

        private void exportSoulsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            //UNLEASHED: before we update, see if that last item is checked first so we can update SSObject
            SSObject ssObjTemp = SSObjects[exportSoulsList.SelectedIndex];
            bool checkedBox = exportSoulsList.GetItemCheckState(exportSoulsList.SelectedIndex) == CheckState.Checked;
            
            if (!checkedBox && ssObjTemp.shouldExport)
            {
                if (numOfExportedItems > 0)
                     numOfExportedItems--;
            }

            if (checkedBox && !ssObjTemp.shouldExport)
            {
                numOfExportedItems++;
            }

            ssObjTemp.shouldExport = checkedBox;
            SSObjects[exportSoulsList.SelectedIndex] = ssObjTemp;
            
            //UNLEASHED: now update the index
            checkBurst1.Checked = SSObjects[exportSoulsList.SelectedIndex].exportBurst1;
            checkBurst2.Checked = SSObjects[exportSoulsList.SelectedIndex].exportBurst2;
            checkBurst3.Checked = SSObjects[exportSoulsList.SelectedIndex].exportBurst3;
            checkBurst1.Text = "Export Burst 1 (" + SSObjects[exportSoulsList.SelectedIndex].burst1ID.ToString() + ")";
            checkBurst2.Text = "Export Burst 2 (" + SSObjects[exportSoulsList.SelectedIndex].burst2ID.ToString() + ")";
            checkBurst3.Text = "Export Burst 3 (" + SSObjects[exportSoulsList.SelectedIndex].burst3ID.ToString() + ")";
        }

        private byte[] getProperSSF_FormatForLimitBurstOLD(ushort LimitBurstID, ushort parentID, ushort burstSlot)
        {
            for (int i = 0; i < form1.Items.Length; i++)
            {
                if (BitConverter.ToUInt16(form1.Items[i].Data, 0) == LimitBurstID)
                {
                    int SSIndex = i;
                    List<byte> SSFFile = new List<byte>();
                    SSFFile.AddRange(new byte[] { 0x23, 0x53, 0x53, 0x46 }); // UNLEASHED: Sig

                    string nameText = Names.data[form1.Items[SSIndex].msgIndexName].Lines[0];
                    string DescText = Descs.data[form1.Items[SSIndex].msgIndexDesc].Lines[0];
                    string LBDescText = Burst.data[form1.Items[SSIndex].msgIndexBurst].Lines[0];
                    string LBBTLHUDDescText = BurstBTLHUD.data[form1.Items[SSIndex].msgIndexBurstBTL].Lines[0];
                    string LBPauseDescText = BurstPause.data[form1.Items[SSIndex].msgIndexBurstPause].Lines[0];
                    string lookupText = HowTo.data[form1.Items[SSIndex].msgIndexHow].Lines[0];

                    int nameCount = nameText.Length * 2;
                    int DescCount = DescText.Length * 2;
                    int LBDescCount = LBDescText.Length * 2;
                    int LBBTLHUDDescCount = LBBTLHUDDescText.Length * 2;
                    int LBPauseDescCount = LBPauseDescText.Length * 2;
                    int lookupCount = lookupText.Length * 2;

                    SSFFile.AddRange(BitConverter.GetBytes(nameCount));
                    SSFFile.AddRange(BitConverter.GetBytes(DescCount));
                    SSFFile.AddRange(BitConverter.GetBytes(LBDescCount));
                    SSFFile.AddRange(BitConverter.GetBytes(LBBTLHUDDescCount));
                    SSFFile.AddRange(BitConverter.GetBytes(LBPauseDescCount));
                    SSFFile.AddRange(BitConverter.GetBytes(lookupCount));

                    //UNLEASHED: assign parentID and burstSlot
                    SSFFile.AddRange(BitConverter.GetBytes(parentID));
                    SSFFile.AddRange(BitConverter.GetBytes(burstSlot));
                    SSFFile.AddRange(CharByteArray(nameText));
                    SSFFile.AddRange(CharByteArray(DescText));
                    SSFFile.AddRange(CharByteArray(LBDescText));
                    SSFFile.AddRange(CharByteArray(LBBTLHUDDescText));
                    SSFFile.AddRange(CharByteArray(LBPauseDescText));
                    SSFFile.AddRange(CharByteArray(lookupText));

                    byte[] itempass = new byte[IDB.Idb_Size - 2];
                    Array.Copy(form1.Items[SSIndex].Data, 2, itempass, 0, IDB.Idb_Size - 2);
                    SSFFile.AddRange(itempass);
                    return SSFFile.ToArray();
                }
            }
                return null; 
        }

        private byte[] getProperSSF_FormatForLimitBurst(ushort LimitBurstID)
        {
            for (int i = 0; i < form1.Items.Length; i++)
            {
                if (BitConverter.ToUInt16(form1.Items[i].Data, 0) == LimitBurstID)
                {
                    List<byte> SSFFile = new List<byte>();
                    byte[] itempass = new byte[IDB.Idb_Size];
                    Array.Copy(form1.Items[i].Data, 0, itempass, 0, IDB.Idb_Size);
                    SSFFile.AddRange(itempass);
                    return SSFFile.ToArray();
                }
            }
            return null;
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            sspFile = new SSP();
            limitBursts = new List<byte[]>();
            byte[] tempBytes;
            ushort exportIndex = 0;

            bool burst1 = false;
            bool burst2 = false;
            bool burst3 = false;
            int burstCount = 0;


            if (SSObjects.Count == 0)
            {
                MessageBox.Show("Unknown Error");
                return;
            }

            if (numOfExportedItems <= 0)
            {
                MessageBox.Show("Please select atleast 1 super soul");
                return;
            }

            for (int i = 0; i < SSObjects.Count; i++)
            {
                if (SSObjects[i].shouldExport)
                {
                    if (SSObjects[i].exportBurst1)
                    {
                        tempBytes = getProperSSF_FormatForLimitBurst(SSObjects[i].burst1ID);
                        if (tempBytes != null)
                        {
                            limitBursts.Add(tempBytes);
                            burst1 = true;
                            burstCount++;
                        }
                        else
                        {
                            MessageBox.Show("could not find LimitBurst ID " + SSObjects[i].burst1ID.ToString() + " in Super Soul with Name " +
                               SSObjects[i].SSName);
                            return;
                        }
                    }

                    if (SSObjects[i].exportBurst2)
                    {
                        tempBytes = getProperSSF_FormatForLimitBurst(SSObjects[i].burst2ID);
                        if (tempBytes != null)
                        {
                            limitBursts.Add(tempBytes);
                            burst2 = true;
                            burstCount++;
                        }
                        else
                        {
                            MessageBox.Show("could not find LimitBurst ID " + SSObjects[i].burst1ID.ToString() + " in Super Soul with Name " +
                               SSObjects[i].SSName);
                            return;
                        }
                    }

                    if (SSObjects[i].exportBurst3)
                    {
                        tempBytes = getProperSSF_FormatForLimitBurst(SSObjects[i].burst3ID);
                        if (tempBytes != null)
                        {
                            limitBursts.Add(tempBytes);
                            burst3 = true;
                            burstCount++;
                        }
                        else
                        {
                            MessageBox.Show("could not find LimitBurst ID " + SSObjects[i].burst3ID.ToString() + " in Super Soul with Name " +
                               SSObjects[i].SSName);
                            return;
                        }
                    }

                    //add limit burst data
                    byte[] FullSSF = new byte[SSObjects[i].SSFData.Length + (burstCount * IDB.Idb_Size)];
                    Array.Copy(SSObjects[i].SSFData, FullSSF, SSObjects[i].SSFData.Length);
                    int pos = SSObjects[i].SSFData.Length;
                    for (int l = 0; l < limitBursts.Count; l++)
                    {
                        Array.Copy(limitBursts[l], 0, FullSSF, pos + (l * IDB.Idb_Size), IDB.Idb_Size);
                    }

                    //fix lb identifiers
                    Array.Copy(BitConverter.GetBytes(burstCount), 0, FullSSF, 8, 2); //count
                    if (burst1)
                        Array.Copy(BitConverter.GetBytes(0), 0, FullSSF, 10, 2);
                    if (burst2)
                        Array.Copy(BitConverter.GetBytes(0), 0, FullSSF, 12, 2);
                    if (burst3)
                        Array.Copy(BitConverter.GetBytes(0), 0, FullSSF, 14, 2);

                    sspFile.Souls.Add(new SSP.SSF(FullSSF));

                    //reset data
                    limitBursts = new List<byte[]>();
                    burst1 = false;
                    burst2 = false;
                    burst3 = false;
                    burstCount = 0;
                    exportIndex++;
                }
            }

            //for (int i = 0; i < limitBursts.Count; i++)
            //{
            //    sspFile.Souls.Add(new SSP.SSF(limitBursts[i]));
            //}

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Super Soul Package File | *.ssp";
            saveFileDialog1.Title = "Save a Super Soul Package File";
        
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                sspFile.SSPWrite(saveFileDialog1.FileName);
                MessageBox.Show("Super Souls Exported Successfully");
            }       
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Select the Super Souls that you want to export to a SSP file by clicking the checkboxes next to their names\n" +
                "each super soul has 3 limit burst IDs ,if your super soul doesn't have all Burst IDs set, then it will be count as a limit burst\n" +
                "(since limit bursts don't have Burst IDs, obviously..)\n" +
                "and it won't be shown in this list\n\n\n" +
                "for each super soul that you CHECK, it will have 3 checkboxes on the right panel\n" +
                "lets say if your Super Soul's Burst ID #1 is pointing to a CUSTOM LIMIT BURST ID, then you SHOULD check that box so that the Limit Burst\n" +
                "is exported in the SSP package. otherwise, other uses won't be able to use that Limit Burst since it won't exist in the vanila IDB file..\n" +
                "same for Burst ID #2 and #3\n" +
                "and DON'T check the checkboxs if your Super Soul's Burst ID is pointing to a vanila Limit Burst ID");
        }

        private void checkBurst1_CheckedChanged(object sender, EventArgs e)
        {
            SSObject ssObjTemp = SSObjects[exportSoulsList.SelectedIndex];
            ssObjTemp.exportBurst1 = checkBurst1.Checked;
            SSObjects[exportSoulsList.SelectedIndex] = ssObjTemp;
        }

        private void checkBurst2_CheckedChanged(object sender, EventArgs e)
        {
            SSObject ssObjTemp = SSObjects[exportSoulsList.SelectedIndex];
            ssObjTemp.exportBurst2 = checkBurst2.Checked;
            SSObjects[exportSoulsList.SelectedIndex] = ssObjTemp;
        }

        private void checkBurst3_CheckedChanged(object sender, EventArgs e)
        {
            SSObject ssObjTemp = SSObjects[exportSoulsList.SelectedIndex];
            ssObjTemp.exportBurst3 = checkBurst3.Checked;
            SSObjects[exportSoulsList.SelectedIndex] = ssObjTemp;
        }

        //UNLEASHED: Mugen's code to convert string into byte array
        private byte[] CharByteArray(string text)
        {
            return System.Text.Encoding.Unicode.GetBytes(text.ToCharArray());
        }

        //TODO add function to export LB installer xml
        private void exportLB_Xml()
        {

        }
    }
}
