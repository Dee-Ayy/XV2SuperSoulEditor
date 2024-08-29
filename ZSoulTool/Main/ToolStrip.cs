using Msgfile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        
        //TODO: Reload msg text on setting change with no restart needed?
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

        private void setAsDefaultProgramForSSFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileAssociations.SetAssoc(".ssf", "XV2SS_EDITOR_FILE", "SSF File");
            MessageBox.Show("Extension Set Successfully");
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
            int index = AddSSNew(Properties.Resources.newss);

            if (index < 0)
                return;

            itemList.Items.Clear();

            for (int i = 0; i < Items.Length; i++)
            {
                itemList.Items.Add(BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0]);
            }

            itemList.SelectedIndex = index;
        }

        //TODO update(?) 
        private void createNewSoulAsLimitBurstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = AddLB(Properties.Resources.newss);

            if (index < 0)
                return;

            itemList.Items.Clear();
            for (int i = 0; i < Items.Length; i++)
            {
                itemList.Items.Add(BitConverter.ToUInt16(Items[i].Data, 0).ToString() + " - " + Names.data[Items[i].msgIndexName].Lines[0]);
            }

            itemList.SelectedIndex = index;

            Name_ID.Text = "0";
            Info_ID.Text = "0";
            KiBlast.SelectedIndex = 0;
            Race.Text = "0";
            Dlc_Flag.Text = "255";
            Prog_Flag.Text = "32767";
            Cost.Text = "0";
            Sell.Text = "0";
            Cost_TP.Text = "0";
            Rarity.SelectedIndex = 5;
            LB_Aura.Text = "-1";
            LB_Desc.Text = "0";
            LB_Soul_ID1.Text = "65535";
            LB_Soul_ID2.Text = "65535";
            LB_Soul_ID3.Text = "65535";
            LB_Color.SelectedIndex = 0;
        }
        
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
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

        ///DEMON: old code for replacing the currently selected SS with a .zss
        ///commented out for now in case i want to re add this feature
        //private void replaceImportToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    //import/replace
        //    OpenFileDialog browseFile = new OpenFileDialog();
        //    browseFile.Filter = "Super Soul File (*.zss)|*.zss";
        //    browseFile.Title = "Browse for Z Soul Share File";
        //    if (browseFile.ShowDialog() == DialogResult.Cancel)
        //        return;
        //
        //    byte[] zssfile = File.ReadAllBytes(browseFile.FileName);
        //
        //    int nameCount = BitConverter.ToInt32(zssfile, 4);
        //    int DescCount = BitConverter.ToInt32(zssfile, 8);
        //    short nameID = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 4);
        //    short DescID = BitConverter.ToInt16(Items[itemList.SelectedIndex].Data, 6);
        //
        //    Array.Copy(zssfile, 12 + (nameCount * 2) + (DescCount * 2), Items[itemList.SelectedIndex].Data, 2, 718);
        //    Array.Copy(BitConverter.GetBytes(nameID), 0, Items[itemList.SelectedIndex].Data, 4, 2);
        //    Array.Copy(BitConverter.GetBytes(DescID), 0, Items[itemList.SelectedIndex].Data, 6, 2);
        //
        //    byte[] pass;
        //
        //    if (nameCount > 0)
        //    {
        //        pass = new byte[nameCount * 2];
        //        Array.Copy(zssfile, 12, pass, 0, nameCount * 2);
        //        txtMsgName.Text = BytetoString(pass);
        //    }
        //
        //    if (DescCount > 0)
        //    {
        //        pass = new byte[DescCount * 2];
        //        Array.Copy(zssfile, 12 + (nameCount * 2), pass, 0, DescCount * 2);
        //        txtMsgDesc.Text = BytetoString(pass);
        //    }
        //
        //    UpdateData();
        //}




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
            Race.Text = "255";
            Dlc_Flag.Text = "-1";
            Prog_Flag.Text = "30";
            Cost.Text = "1000";
            Sell.Text = "100";
            Cost_TP.Text = "1";
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
            //copiedBytes.AddRange(new byte[] { 0x00, 0x00 }); //LB soul count
            //copiedBytes.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }); //Limit Burst Identifiers

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

        //TODO Update
        ///
        /// Msg Options
        /// 
        private void addNewNameMsg(object sender, EventArgs e)
        {
            //add msg name
            msgData[] Expand = new msgData[Names.data.Length + 1];
            Array.Copy(Names.data, Expand, Names.data.Length);
            Expand[Expand.Length - 1].NameID = "talisman_" + Names.data.Length.ToString("000");
            Expand[Expand.Length - 1].ID = Names.data.Length;
            Expand[Expand.Length - 1].Lines = new string[] { "New Name Entry" };
            Names.data = Expand;
            //writeToMsgText(0, "New Name Entry");
            Name_ID.Text = Names.data[Names.data.Length - 1].ID.ToString();
        }

        private void removeNameMsg(object sender, EventArgs e)
        {
            //remove msg name
            msgData[] reduce = new msgData[Names.data.Length - 1];
            Array.Copy(Names.data, reduce, Names.data.Length - 1);
            Names.data = reduce;
        }

        private void addNewInfoMsg(object sender, EventArgs e)
        {
            //add msg desc
            msgData[] Expand = new msgData[Descs.data.Length + 1];
            Array.Copy(Descs.data, Expand, Descs.data.Length);
            Expand[Expand.Length - 1].NameID = "talisman_eff_" + Descs.data.Length.ToString("000");
            Expand[Expand.Length - 1].ID = Descs.data.Length;
            Expand[Expand.Length - 1].Lines = new string[] { "New Description Entry" };
            Descs.data = Expand;
            //writeToMsgText(1, "New Description Entry");
            Info_ID.Text = Descs.data[Descs.data.Length - 1].ID.ToString();
        }

        private void removeInfoMsg(object sender, EventArgs e)
        {
            //remove msg desc
            msgData[] reduce = new msgData[Descs.data.Length - 1];
            Array.Copy(Descs.data, reduce, Descs.data.Length - 1);
            Descs.data = reduce;
        }

        private void addNewBurstMsg(object sender, EventArgs e)
        {
            byte[] blankzss = Properties.Resources.newss;

            int nameCount = BitConverter.ToInt32(blankzss, 4);
            int DescCount = BitConverter.ToInt32(blankzss, 8);
            int LBDescCount = BitConverter.ToInt32(blankzss, 16);
            int LBDescCountBtl = BitConverter.ToInt32(blankzss, 20);
            int LBDescCountPause = BitConverter.ToInt32(blankzss, 24);

            byte[] pass = null;
            msgData[] Expand4 = new msgData[Burst.data.Length + 1];
            Array.Copy(Burst.data, Expand4, Burst.data.Length);
            Expand4[Expand4.Length - 1].NameID = "talisman_olt_" + Burst.data.Length.ToString("000");
            Expand4[Expand4.Length - 1].ID = Burst.data.Length;

            if (LBDescCount > 0)
            {
                pass = new byte[LBDescCount];
                Array.Copy(blankzss, 0x1C + (nameCount) + (DescCount), pass, 0, LBDescCount);
                Expand4[Expand4.Length - 1].Lines = new string[] { BytetoString(pass) };
            }

            else
                Expand4[Expand4.Length - 1].Lines = new string[] { "New LB Desc Entry" };

            byte[] newMSGLBDescEntryIDBytes = BitConverter.GetBytes((short)Expand4[Expand4.Length - 1].ID);
            Array.Copy(newMSGLBDescEntryIDBytes, 0, Items[currentSuperSoulIndex].Data, 44, 2);
            Burst.data = Expand4;
            Items[currentSuperSoulIndex].msgIndexBurst = BitConverter.ToInt16(newMSGLBDescEntryIDBytes, 0);

            //if (LBDescCount > 0)
            //    writeToMsgText(2, BytetoString(pass));
            //else
            //    writeToMsgText(2, "New LB Desc Entry");

            int OLT_ID = Items[currentSuperSoulIndex].msgIndexBurst;
            msgData[] Expand5 = new msgData[BurstBTLHUD.data.Length + 1];
            Array.Copy(BurstBTLHUD.data, Expand5, BurstBTLHUD.data.Length);
            Expand5[Expand5.Length - 1].NameID = "BHD_OLT_000_" + Items[currentSuperSoulIndex].msgIndexBurst.ToString();// +BurstBTLHUD.data.Length.ToString("000");
            Expand5[Expand5.Length - 1].ID = BurstBTLHUD.data.Length;

            if (LBDescCountBtl > 0)
            {
                pass = new byte[LBDescCountBtl];
                Array.Copy(blankzss, 0x1C + (nameCount) + (DescCount) + (LBDescCount), pass, 0, LBDescCountBtl);
                Expand5[Expand5.Length - 1].Lines = new string[] { BytetoString(pass) };
            }
            else
                Expand5[Expand5.Length - 1].Lines = new string[] { "New LB Battle Desc Entry" };

            byte[] newMSGLBDescBtlEntryIDBytes = BitConverter.GetBytes((short)Expand5[Expand5.Length - 1].ID);
            BurstBTLHUD.data = Expand5;
            Items[currentSuperSoulIndex].msgIndexBurstBTL = BitConverter.ToInt16(newMSGLBDescBtlEntryIDBytes, 0);

            //if (LBDescCountBtl > 0)
            //    writeToMsgText(3, BytetoString(pass), OLT_ID);
            //else
            //    writeToMsgText(3, "New LB Battle Desc Entry", OLT_ID);

            msgData[] Expand6 = new msgData[BurstPause.data.Length + 1];
            Array.Copy(BurstPause.data, Expand6, BurstPause.data.Length);
            Expand6[Expand6.Length - 1].NameID = "BHD_OLT_000_" + Items[currentSuperSoulIndex].msgIndexBurst.ToString();// +BurstBTLHUD.data.Length.ToString("000");
            Expand6[Expand6.Length - 1].ID = BurstPause.data.Length;

            if (LBDescCountPause > 0)
            {
                pass = new byte[LBDescCountPause];
                Array.Copy(blankzss, 0x1C + (nameCount) + (DescCount) + (LBDescCount) + (LBDescCountBtl), pass, 0, LBDescCountPause);
                Expand6[Expand6.Length - 1].Lines = new string[] { BytetoString(pass) };
            }
            else
                Expand6[Expand6.Length - 1].Lines = new string[] { "New LB Pause Desc Entry" };

            byte[] newMSGLBDescPauseEntryIDBytes = BitConverter.GetBytes((short)Expand6[Expand6.Length - 1].ID);
            BurstPause.data = Expand6;
            //Items[currentSuperSoulIndex].msgIndexBurstPause = BitConverter.ToInt16(newMSGLBDescPauseEntryIDBytes, 0);
            //if (LBDescCountPause > 0)
            //
            //    writeToMsgText(4, BytetoString(pass), OLT_ID);
            //else
            //    writeToMsgText(4, "New LB Pause Desc Entry", OLT_ID);

            LB_Desc.Text = Expand4[Expand4.Length - 1].ID.ToString();
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
