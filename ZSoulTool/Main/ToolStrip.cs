using Msgfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Expando;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Xv2CoreLib.IDB;
using YAXLib;

//toolstrip functions are to be placed here to clean things up a bit
namespace XV2SSEdit
{
    public partial class Form1 : Form
    {
        ///
        /// File
        /// 
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveXV2SSEdit();

            //UNLEASHED: added msgbox
            hasSavedChanges = true;
            MessageBox.Show("Save Successful and files writtin to 'data' folder\nTo see changes in-game, the XV2Patcher must be installed.", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Forms.Settings form = new Forms.Settings(settings))
            {
                form.ShowDialog();
                if (form.Finished)
                {
                    settings = form.settings;
                    settings.Save();
                    MessageBox.Show(this, "Please restart the application for the changes to take affect.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void setAsDefaultProgramForSSFPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileAssociations.SetAssoc(".ssp", "XV2SS_EDITOR_FILE", "SSP File");
            MessageBox.Show("Extension Set Successfully");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        ///
        /// Super Soul Options
        /// 
        private void createNewSoulToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = AddSSNew(Properties.Resources.Soul_BlankText);

            if (index < 0)
                return;

            itemList.Items.Clear();

            for (int i = 0; i < Items.Length; i++)
            {
                itemList.Items.Add(BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0]);
            }

            itemList.SelectedIndex = index;
        }

        private void createNewLimitBurstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //search for closest unused id after vanilla ids
            //should be 1003+, but we start searching from 1000 just because
            ushort FreeID = 1000;
            int LastUsedIndex = 0;
            bool foundProperID = false;
            while (!foundProperID)
            {
                if (FreeID == 0xFFFF )
                {
                    MessageBox.Show(this, "Could not find enough free space avaiable for Limit Bursts!", "No Room", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!UsedIDs.Contains(FreeID))
                {
                    //check the next two after this to see if they are free
                    bool FreeID2 = UsedIDs.Contains((ushort)(FreeID + 1));
                    bool FreeID3 = UsedIDs.Contains((ushort)(FreeID + 2));

                    if (!FreeID2 && !FreeID3)
                        foundProperID = true;
                    else
                        FreeID++;
                }
                else
                {
                    LastUsedIndex = UsedIDs.IndexOf(FreeID) + 1;
                    FreeID++;
                }
            }

            //start expanding
            idbItem[] SoulExpand = new idbItem[Items.Length + 3];

            //add each soul to new list up to last new index
            Array.Copy(Items, 0, SoulExpand, 0, LastUsedIndex);

            //add new souls
            byte[] tmp = new byte[IDB.Idb_Size];
            for (int i = 0; i < 3; i++)
            {
                int soulPos = i * IDB.Idb_Size;

                //copy soul data
                Array.Copy(Properties.Resources.BlankLB, soulPos, tmp, 0, IDB.Idb_Size);

                //fix soul id
                Array.Copy(BitConverter.GetBytes(FreeID + i), tmp, 2);

                //add it to expand list
                SoulExpand[LastUsedIndex + i].Data = new byte[IDB.Idb_Size];
                Array.Copy(tmp, SoulExpand[LastUsedIndex + i].Data, IDB.Idb_Size);
                //SoulExpand[LastUsedIndex + i].Data = tmp;

                //copy what limit bursts use
                SoulExpand[LastUsedIndex + i].msgIndexName = 0;
                SoulExpand[LastUsedIndex + i].msgIndexDesc = 0;
                SoulExpand[LastUsedIndex + i].msgIndexHow = 0;
                SoulExpand[LastUsedIndex + i].msgIndexBurst = 0;
                SoulExpand[LastUsedIndex + i].msgIndexBurstBTL = 0xD2;
                SoulExpand[LastUsedIndex + i].msgIndexBurstPause = 0xA0;
            }

            //add rest of original souls
            Array.Copy(Items, LastUsedIndex, SoulExpand, LastUsedIndex + 3, Items.Length - LastUsedIndex);

            //finish adding souls
            Items = SoulExpand;

            itemList.Items.Clear();
            for (int i = 0; i < Items.Length; i++)
            {
                itemList.Items.Add(BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0]);
            }

            //remember to add new id to used ids list
            UsedIDs.Insert(LastUsedIndex, FreeID);
            UsedIDs.Insert(LastUsedIndex + 1, (ushort)(FreeID + 1));
            UsedIDs.Insert(LastUsedIndex + 2, (ushort)(FreeID + 2));

            //go to new soul
            itemList.SelectedIndex = LastUsedIndex;
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(this, "Are you sure you want to remove this Super Soul?", "Remove", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
            if (result != DialogResult.Yes)
                return;

            //remove super soul  
            //UNLEASHED: there are probably better methods to do this, but working with Lists is just so much easier.

            List<idbItem> Reduce = Items.ToList<idbItem>();
            Reduce.RemoveAt(currentSuperSoulIndex);
            Items = Reduce.ToArray();
            itemList.Items.Clear();

            for (int i = 0; i < Items.Length; i++)
            {
                itemList.Items.Add(BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0]);
            }

            //UNLEASHED: return to first item
            itemList.SelectedIndex = 0;
        }

        //TODO: Update SSP import
        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //add/import super soul
            //load ssp file
            OpenFileDialog browseFile = new OpenFileDialog();
            browseFile.Filter = "Super Soul Package | *.ssp";
            browseFile.Title = "Select the Super Soul Package you want to import.";
            if (browseFile.ShowDialog() == DialogResult.Cancel)
                return;

            //and we are done, rebuild itemlist
            if (!importSSP(browseFile.FileName))
            {
                MessageBox.Show("Error occurred when installing SSP file.");
                return;
            }

            itemList.Items.Clear();

            for (int i = 0; i < Items.Length; i++)
            {
                itemList.Items.Add(BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0]);
            }

            itemList.SelectedIndex = 0;
            MessageBox.Show("SSP imported successfully");
        }

        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            (new Export(this, Names, Descs, Burst, BurstBTLHUD, BurstPause, HowTo)).Show();
        }

        private void store_defaultBtn_Click(object sender, EventArgs e)
        {
            //Race.Text = "255";
            Dlc_Flag.Text = "-1";
            Prog_Flag.Text = "30";
            Cost.Text = "1000";
            Sell.Text = "100";
            Cost_TP.Text = "1";
            Cost_STP.Text = "1";
            //Rarity.SelectedIndex = 1;
        }

        private void removeCurrentSuperSoulFromShopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Race.Text = "0";
            Dlc_Flag.Text = "255";
            Prog_Flag.Text = "32767";
            Cost.Text = "0";
            Sell.Text = "0";
            Cost_TP.Text = "0";
            //Rarity.SelectedIndex = 5;
        }

        private void copyCurrentSuperSoulToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<byte> copiedBytes = new List<byte>();
            copiedBytes.AddRange(new byte[] { 0x23, 0x53, 0x46, 0x32 }); //#SF2
            copiedBytes.AddRange(new byte[] { 0x02, 0x00, 0x00, 0x00 }); //Version number
            copiedBytes.AddRange(new byte[] { 0x00, 0x00 }); //LB soul count
            copiedBytes.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }); //Limit Burst Identifiers

            //could be better, but oh well this works
            List<string> languages = new List<string> { "en", "es", "ca", "fr", "de", "it", "pt", "pl", "ru", "tw", "zh", "kr", "ja" };
            string MsgText = "";
            int TextLength = 0;
            foreach (string lang in languages)
            {
                //Name
                MsgText = fullMsgListNames[lang].data[Items[currentSuperSoulIndex].msgIndexName].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }

            foreach (string lang in languages)
            {
                //Description
                MsgText = fullMsgListDescs[lang].data[Items[currentSuperSoulIndex].msgIndexDesc].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }

            foreach (string lang in languages)
            {
                //HowTo
                MsgText = fullMsgListHowTo[lang].data[Items[currentSuperSoulIndex].msgIndexHow].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }

            foreach (string lang in languages)
            {
                //Burst
                MsgText = fullMsgListBurst[lang].data[Items[currentSuperSoulIndex].msgIndexBurst].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }
            
            foreach (string lang in languages)
            {
                //Burst HUD
                MsgText = fullMsgListBurstBTLHUD[lang].data[Items[currentSuperSoulIndex].msgIndexBurstBTL].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }
            
            foreach (string lang in languages)
            {
                //Burst Pause
                MsgText = fullMsgListBurstPause[lang].data[Items[currentSuperSoulIndex].msgIndexBurstPause].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }

            byte[] tmp = new byte[IDB.Idb_Size];
            Array.Copy(Items[currentSuperSoulIndex].Data, 0, tmp, 0, IDB.Idb_Size);
            copiedBytes.AddRange(tmp);
            clipboardData = copiedBytes.ToArray();
            MessageBox.Show("Super Soul copied successfully");
        }

        //TODO: do we really want this?
        private void copyWithLimitToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<byte> copiedBytes = new List<byte>();
            copiedBytes.AddRange(new byte[] { 0x23, 0x53, 0x46, 0x32 }); //#SF2
            copiedBytes.AddRange(new byte[] { 0x02, 0x00, 0x00, 0x00 }); //Version number
            copiedBytes.AddRange(new byte[] { 0x03, 0x00 }); //LB soul count
            copiedBytes.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); //Limit Burst Identifiers

            //could be better, but oh well this works
            List<string> languages = new List<string> { "en", "es", "ca", "fr", "de", "it", "pt", "pl", "ru", "tw", "zh", "kr", "ja" };
            string MsgText = "";
            int TextLength = 0;
            foreach (string lang in languages)
            {
                //Name
                MsgText = fullMsgListNames[lang].data[Items[currentSuperSoulIndex].msgIndexName].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }

            foreach (string lang in languages)
            {
                //Description
                MsgText = fullMsgListDescs[lang].data[Items[currentSuperSoulIndex].msgIndexDesc].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }

            foreach (string lang in languages)
            {
                //HowTo
                MsgText = fullMsgListHowTo[lang].data[Items[currentSuperSoulIndex].msgIndexHow].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }

            foreach (string lang in languages)
            {
                //Burst
                MsgText = fullMsgListBurst[lang].data[Items[currentSuperSoulIndex].msgIndexBurst].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }

            foreach (string lang in languages)
            {
                //Burst HUD
                MsgText = fullMsgListBurstBTLHUD[lang].data[Items[currentSuperSoulIndex].msgIndexBurstBTL].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }

            foreach (string lang in languages)
            {
                //Burst Pause
                MsgText = fullMsgListBurstPause[lang].data[Items[currentSuperSoulIndex].msgIndexBurstPause].Lines[0];
                TextLength = MsgText.Length * 2;
                copiedBytes.AddRange(BitConverter.GetBytes(TextLength));
                copiedBytes.AddRange(System.Text.Encoding.Unicode.GetBytes(MsgText.ToCharArray()));
            }

            byte[] tmp = new byte[IDB.Idb_Size];
            Array.Copy(Items[currentSuperSoulIndex].Data, 0, tmp, 0, IDB.Idb_Size);
            copiedBytes.AddRange(tmp);

            //TODO: Grab Actual Limit Bursts
            copiedBytes.AddRange(tmp);
            copiedBytes.AddRange(tmp);
            copiedBytes.AddRange(tmp);
            clipboardData = copiedBytes.ToArray();
            MessageBox.Show("Super Soul copied successfully");
        }

        private void createNewSoulFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clipboardData == null)
            {
                MessageBox.Show("Clipboard is empty.");
                return;
            }

            int index = AddSSNew(clipboardData);

            if (index < 0)
                return;

            itemList.Items.Clear();

            for (int i = 0; i < Items.Length; i++)
            {
                itemList.Items.Add(BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0]);
            }
            itemList.SelectedIndex = index;
        }

        private void duplicateCurrentSoulToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] tmp = new byte[IDB.Idb_Size];
            Array.Copy(Items[currentSuperSoulIndex].Data, 0, tmp, 0, IDB.Idb_Size);

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

            //start expanding
            idbItem[] SoulExpand = new idbItem[Items.Length + 1];

            //add each soul to new list up to last new index
            Array.Copy(Items, 0, SoulExpand, 0, LastUsedIndex);

            //fix soul id
            Array.Copy(BitConverter.GetBytes(FreeID), tmp, 2);

            //add it to expand list
            SoulExpand[LastUsedIndex].Data = tmp;

            //copy original soul msg ids
            SoulExpand[LastUsedIndex].msgIndexName = Items[currentSuperSoulIndex].msgIndexName;
            SoulExpand[LastUsedIndex].msgIndexDesc = Items[currentSuperSoulIndex].msgIndexDesc;
            SoulExpand[LastUsedIndex].msgIndexHow = Items[currentSuperSoulIndex].msgIndexHow;
            SoulExpand[LastUsedIndex].msgIndexBurst = Items[currentSuperSoulIndex].msgIndexBurst;
            SoulExpand[LastUsedIndex].msgIndexBurstBTL = Items[currentSuperSoulIndex].msgIndexBurstBTL;
            SoulExpand[LastUsedIndex].msgIndexBurstPause = Items[currentSuperSoulIndex].msgIndexBurstPause;

            //add rest of original souls
            Array.Copy(Items, LastUsedIndex, SoulExpand, LastUsedIndex + 1, Items.Length - LastUsedIndex);

            //finish adding souls
            Items = SoulExpand;

            itemList.Items.Clear();
            for (int i = 0; i < Items.Length; i++)
            {
                itemList.Items.Add(BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0]);
            }

            //remember to add new id to used ids list
            UsedIDs.Insert(LastUsedIndex, FreeID);

            //go to new soul
            itemList.SelectedIndex = LastUsedIndex;
        }

        private void replaceCurrentSoulFromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (clipboardData == null)
            {
                MessageBox.Show("Clipboard is empty.");
                return;
            }

            var result = MessageBox.Show(this, "Are you sure you want to replace this Super Soul?", "Replace", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            //replace everythig except index
            Array.Copy(clipboardData, clipboardData.Length - (IDB.Idb_Size-2), Items[itemList.SelectedIndex].Data, 2, IDB.Idb_Size-2);

            //reload (scuffed)
            int index = itemList.SelectedIndex;
            itemList.SelectedIndex = 0;
            itemList.SelectedIndex = index;
        }

        //will export current soul as Lazybones format xml
        private void exportSoulasXml(object sender, EventArgs e)
        {
            byte[] copiedBytes = new byte[IDB.Idb_Size + 0x10];
            Array.Copy(new byte[] { 0x23, 0x49, 0x44, 0x42, 0xFE, 0xFF, 0x07, 0x00, 0x01, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00 }, copiedBytes, 0x10);
            Array.Copy(Items[currentSuperSoulIndex].Data, 0, copiedBytes, 0x10, IDB.Idb_Size);

            //get a save location
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "LazyBone Installer XML | *.xml";
            saveFileDialog1.Title = "Save Super Soul as XML";

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //sspFile.SSPWrite(saveFileDialog1.FileName);

                //Parser(copiedBytes, saveFileDialog1.FileName, true);

                IDB_File idbFile = IDB_File.Load(copiedBytes);
                YAXSerializer serializer = new YAXSerializer(typeof(IDB_File));
                serializer.SerializeToFile(idbFile, saveFileDialog1.FileName);

                //MessageBox.Show("Super Souls Exported Successfully");
            }
        }

        ///
        /// Msg Options
        /// 
        private void addNewNameMsg(object sender, EventArgs e)
        {
            List<string> languages = new List<string> { "en", "es", "ca", "fr", "de", "it", "pt", "pl", "ru", "tw", "zh", "kr", "ja" };

            //Names
            foreach (string lang in languages)
            {
                Names = fullMsgListNames[lang];

                msgData[] MsgExpand_Names = new msgData[Names.data.Length + 1]; //create tmp expand
                Array.Copy(Names.data, MsgExpand_Names, Names.data.Length); //copy all original entries to tmp
                MsgExpand_Names[MsgExpand_Names.Length - 1].NameID = "talisman_" + Names.data.Length.ToString("000");
                MsgExpand_Names[MsgExpand_Names.Length - 1].ID = Names.data.Length;
                MsgExpand_Names[MsgExpand_Names.Length - 1].Lines = new string[] { "New Name Entry" };

                //finish
                Names.data = MsgExpand_Names;
                fullMsgListNames[lang] = Names;
            }

            //makes sure text is set back to correct language
            Names = fullMsgListNames[currentLanguge];
            Name_ID.Text = Names.data[Names.data.Length - 1].ID.ToString();
        }

        private void addNewInfoMsg(object sender, EventArgs e)
        {
            List<string> languages = new List<string> { "en", "es", "ca", "fr", "de", "it", "pt", "pl", "ru", "tw", "zh", "kr", "ja" };
            foreach (string lang in languages)
            {
                Descs = fullMsgListDescs[lang];

                msgData[] MsgExpand = new msgData[Descs.data.Length + 1]; 
                Array.Copy(Descs.data, MsgExpand, Descs.data.Length);
                MsgExpand[MsgExpand.Length - 1].NameID = "talisman_eff_" + Descs.data.Length.ToString("000");
                MsgExpand[MsgExpand.Length - 1].ID = Descs.data.Length;
                MsgExpand[MsgExpand.Length - 1].Lines = new string[] { "New Description Entry" };

                //finish
                Descs.data = MsgExpand;
                fullMsgListDescs[lang] = Descs;
            }

            //makes sure text is set back to correct language
            Descs = fullMsgListDescs[currentLanguge];
            Info_ID.Text = Descs.data[Descs.data.Length - 1].ID.ToString();
        }

        private void addNewHowMsg(object sender, EventArgs e)
        {
            List<string> languages = new List<string> { "en", "es", "ca", "fr", "de", "it", "pt", "pl", "ru", "tw", "zh", "kr", "ja" };
            foreach (string lang in languages)
            {
                HowTo = fullMsgListHowTo[lang];

                msgData[] MsgExpand = new msgData[HowTo.data.Length + 1];
                Array.Copy(HowTo.data, MsgExpand, HowTo.data.Length);
                MsgExpand[MsgExpand.Length - 1].NameID = "talisman_how_" + HowTo.data.Length.ToString("000");
                MsgExpand[MsgExpand.Length - 1].ID = HowTo.data.Length;
                MsgExpand[MsgExpand.Length - 1].Lines = new string[] { "New Location Lookup Entry" };

                //finish
                HowTo.data = MsgExpand;
                fullMsgListHowTo[lang] = HowTo;
            }

            //makes sure text is set back to correct language
            HowTo = fullMsgListHowTo[currentLanguge];
            How_ID.Text = HowTo.data[HowTo.data.Length - 1].ID.ToString();
        }

        private void addNewBurstMsg(object sender, EventArgs e)
        {
            List<string> languages = new List<string> { "en", "es", "ca", "fr", "de", "it", "pt", "pl", "ru", "tw", "zh", "kr", "ja" };
            short test = 0;

            //Limit Burst Desctiption
            foreach (string lang in languages)
            {
                Burst = fullMsgListBurst[lang];

                msgData[] MsgExpand_Burst = new msgData[Burst.data.Length + 1]; //create tmp expand
                Array.Copy(Burst.data, MsgExpand_Burst, Burst.data.Length); //copy all original entries to tmp
                MsgExpand_Burst[MsgExpand_Burst.Length - 1].NameID = "talisman_olt_" + Burst.data.Length.ToString("000");
                MsgExpand_Burst[MsgExpand_Burst.Length - 1].ID = Burst.data.Length;
                MsgExpand_Burst[MsgExpand_Burst.Length - 1].Lines = new string[] { "New LB Desc Entry" };

                if (lang == "en")
                {
                    test = (short)MsgExpand_Burst[MsgExpand_Burst.Length - 1].ID;
                }

                //finish
                Burst.data = MsgExpand_Burst;
                fullMsgListBurst[lang] = Burst;
            }

            //Limit Burst Battle Pop-up Text
            foreach (string lang in languages)
            {
                BurstBTLHUD = fullMsgListBurstBTLHUD[lang];

                msgData[] MsgExpand_BurstBTL = new msgData[BurstBTLHUD.data.Length + 1]; //create tmp expand
                Array.Copy(BurstBTLHUD.data, MsgExpand_BurstBTL, BurstBTLHUD.data.Length); //copy all original entries to tmp
                MsgExpand_BurstBTL[MsgExpand_BurstBTL.Length - 1].NameID = "BHD_OLT_000_" + test.ToString();
                MsgExpand_BurstBTL[MsgExpand_BurstBTL.Length - 1].ID = BurstBTLHUD.data.Length;
                MsgExpand_BurstBTL[MsgExpand_BurstBTL.Length - 1].Lines = new string[] { "New LB Battle Desc Entry" };

                //finish
                BurstBTLHUD.data = MsgExpand_BurstBTL;
                fullMsgListBurstBTLHUD[lang] = BurstBTLHUD;
            }

            //Limit Burst Pause Text
            foreach (string lang in languages)
            {
                BurstPause = fullMsgListBurstPause[lang];

                msgData[] MsgExpand_Burst = new msgData[BurstPause.data.Length + 1]; //create tmp expand
                Array.Copy(BurstPause.data, MsgExpand_Burst, BurstPause.data.Length); //copy all original entries to tmp

                //UNLEASHED:i'm guessing MSG IDs are zero based so calling length is like IDs + 1
                MsgExpand_Burst[MsgExpand_Burst.Length - 1].NameID = "BHD_OLT_000_" + test.ToString();
                MsgExpand_Burst[MsgExpand_Burst.Length - 1].ID = BurstPause.data.Length;
                MsgExpand_Burst[MsgExpand_Burst.Length - 1].Lines = new string[] { "New LB Battle Desc Entry" };

                //finish
                BurstPause.data = MsgExpand_Burst;
                fullMsgListBurstPause[lang] = BurstPause;
            }

            //makes sure text is set back to correct language
            Burst = fullMsgListBurst[currentLanguge];
            BurstBTLHUD = fullMsgListBurstBTLHUD[currentLanguge];
            BurstPause = fullMsgListBurstPause[currentLanguge];

            LB_Desc.Text = Burst.data[Burst.data.Length - 1].ID.ToString();
        }

        ///
        /// Other
        /// 
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("XV2 Super Soul Editor Version 1.90\n\nCredits:\nDemonBoy - Tool Creator\nLazybone & Unleashed - Help with fixes/additions\nMugenAttack - Original Source code", "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
