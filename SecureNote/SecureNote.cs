using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace SecureNote
{
    public partial class frmSecureNote : Form
    {
        Crypto crypto = new Crypto();
        string fname = string.Empty;
        string password = string.Empty;
        bool textchanged = false;
        string shortFname = string.Empty;
        string titleBase = "Secure Note v2.27";

        public System.Diagnostics.Process p = new System.Diagnostics.Process();

        public frmSecureNote(string ofn)
        {
            InitializeComponent();

            // Handle Keydown for tab
            rtxtMain.KeyDown += new KeyEventHandler(rtxtMain_KeyDown);

            // Handle Hyperlinks
            rtxtMain.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtxtMain_LinkClicked);

            // Handle early exit
            this.FormClosing += frmSecureNote_FormClosing;

            // Keep track of text changes
            rtxtMain.TextChanged += rtxtMain_TextChanged;

            // Handle Tab Stops
            rtxtMain.AcceptsTab = true;
            rtxtMain.SelectionTabs = new int[] { 25,50,75,100,125,150,175,200,225,250,275,300,325,350,375,400,425,450,475,500 };

            // Make sure title is correct
            this.Text = titleBase;
            textchanged = false;

            // Open the associated file if present
            if (ofn != string.Empty)
            {
                if (File.Exists(ofn))
                {
                    if (fOpen(ofn) == 1) Environment.Exit(1); 
                }
            }
        }

        private void rtxtMain_TextChanged(object sender, EventArgs e)
        {
            // Text changed!
            textchanged = true;
            this.Text = titleBase + " - " + shortFname + "*";
        }

        private void frmSecureNote_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If text changed give ask before closing
            if (textchanged == true)
            {
                var window = MessageBox.Show("Close without Saving?", "Are you sure?", MessageBoxButtons.YesNo);

                e.Cancel = (window == DialogResult.No);
            }
        }

        private void rtxtMain_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
        {
            // Call Process.Start method to open a browser with link text as URL.  
            p = System.Diagnostics.Process.Start(e.LinkText);
        }

        private int fOpen(string fn)
        {
            byte[] AESdecbytes;

            // Prepare for AES
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            byte[] cipher;

            // Read the file
            try
            {
                cipher = File.ReadAllBytes(fn);
            }
            catch
            {
                MessageBox.Show("Load Failed!", "File Error");
                return 1;
            }

            // Keep the filename for save later
            fname = fn;
            shortFname = System.IO.Path.GetFileNameWithoutExtension(fname);

            // Show password dialog
            frmPassword1 p1 = new frmPassword1();
            p1.StartPosition = FormStartPosition.CenterParent;
            p1.ShowDialog();
            password = p1.Password;

            if (password == string.Empty)
            {
                MessageBox.Show("Empty Password, Aborting!", "Password Error");
                return 1;
            }

            // Decrypt
            AES.Key = crypto.AESCreateKey(p1.Password);
            AES.IV = crypto.GetIVfromIVCipher(cipher);

            AESdecbytes = crypto.AESDecrypt(crypto.GetCipherfromIVCipher(cipher), AES.Key, AES.IV);

            if (AESdecbytes == null)
            {
                MessageBox.Show("AES Decryption Failed. Wrong Password or Corrupt File!", "AES Decryption Error");
                return 1;
            }

            // Show decrypted text in the window
            rtxtMain.Rtf = System.Text.Encoding.UTF8.GetString(Decompress(AESdecbytes));

            textchanged = false;
            this.Text = titleBase + " - " + shortFname;

            return 0;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            byte[] AESdecbytes;

            // If text changed give ask before closing
            if (textchanged == true)
            {
                var window = MessageBox.Show("Open without Saving?", "Are you sure?", MessageBoxButtons.YesNo);

                if (window == DialogResult.No) return;
            }

            // Prepare for AES
            RijndaelManaged AES = new RijndaelManaged();
            AES.KeySize = 256;
            byte[] cipher;

            // Show the Open Dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Secure Note Files (*.snt)|*.snt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(openFileDialog.FileName))
                {
                    // Read the file
                    try
                    {
                        cipher = File.ReadAllBytes(openFileDialog.FileName);
                    }
                    catch
                    {
                        MessageBox.Show("Load Failed!", "File Error");
                        return;
                    }

                    // Keep the filename for save later
                    fname = openFileDialog.FileName;
                    shortFname = System.IO.Path.GetFileNameWithoutExtension(fname);

                    // Show password dialog
                    frmPassword1 p1 = new frmPassword1();
                    p1.StartPosition = FormStartPosition.CenterParent;
                    p1.ShowDialog();
                    password = p1.Password;

                    if (password == string.Empty)
                    {
                        MessageBox.Show("Empty Password, Aborting!", "Password Error");
                        return;
                    }

                    // Decrypt
                    AES.Key = crypto.AESCreateKey(p1.Password);
                    AES.IV = crypto.GetIVfromIVCipher(cipher);

                    AESdecbytes = crypto.AESDecrypt(crypto.GetCipherfromIVCipher(cipher), AES.Key, AES.IV);

                    if (AESdecbytes == null)
                    {
                        MessageBox.Show("AES Decryption Failed. Wrong Password or Corrupt File!", "AES Decryption Error");
                        return;
                    }
                    
                    // Show decrypted text in the window
                    rtxtMain.Rtf = System.Text.Encoding.UTF8.GetString(Decompress(AESdecbytes));

                    textchanged = false;
                    this.Text = titleBase + " - " + shortFname;
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string rtf = rtxtMain.Rtf;
            byte[] AESencbytes;
            byte[] cipher;

            if (rtxtMain.Text == String.Empty) return;

            // Clear search
            rtxtMain.SelectAll();
            rtxtMain.SelectionBackColor = Color.White;
            rtxtMain.DeselectAll();

            // Encode the Rich Text box
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(rtf);

            // Compress the array
            bytes = Compress(bytes);
            
            // Prepare for AES
            RijndaelManaged AES = new RijndaelManaged();

            // Show the Save As Dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.FileName = fname;
            saveFileDialog.Filter = "Secure Note|*.snt";
            saveFileDialog.Title = "Save a Secure Note File";
            saveFileDialog.OverwritePrompt = false;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                shortFname = System.IO.Path.GetFileNameWithoutExtension(saveFileDialog.FileName);

                // Show password dialog
                frmPassword2 p2 = new frmPassword2();
                p2.Password = password;
                p2.StartPosition = FormStartPosition.CenterParent;
                p2.ShowDialog();

                if (p2.Password == String.Empty)
                {
                    MessageBox.Show("Save Aborted, Empty Password!", "Save Error");
                    return;
                }

                // keep password and file path for later
                password = p2.Password;
                fname = saveFileDialog.FileName;

                // Generate a new Key and IV for AES
                AES.KeySize = 256;
                AES.Key = crypto.AESCreateKey(p2.Password);
                AES.GenerateIV();

                // Encrypt
                AESencbytes = crypto.AESEncrypt(bytes, AES.Key, AES.IV);

                if (AESencbytes == null)
                {
                    MessageBox.Show("AES Encryption Failed!", "AES Encryption Error");
                    return;
                }

                cipher = crypto.IVCipher(AES.IV, AESencbytes);

                // Write to the file
                try
                {
                    using (BinaryWriter writer = new BinaryWriter(File.Open(saveFileDialog.FileName, FileMode.Create)))
                    {
                        writer.Write(cipher);
                    }
                }
                catch
                {
                    MessageBox.Show("Save Failed!", "File Error");
                    return;
                }

                this.Text = titleBase + " - " + shortFname;
                textchanged = false;
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If text changed give ask before closing
            if (textchanged == true)
            {
                var window = MessageBox.Show("Clear without Saving?", "Are you sure?", MessageBoxButtons.YesNo);

               if (window == DialogResult.No) return;
            }

            // Clear window and filename
            rtxtMain.Rtf = String.Empty;
            fname = string.Empty;
            password = string.Empty;
            textchanged = false;
            shortFname = string.Empty;
            this.Text = titleBase;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Exit Application
            Application.Exit();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Cut
            if (rtxtMain.SelectionLength > 0)
                rtxtMain.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Copy
            if (rtxtMain.SelectionLength > 0)
                rtxtMain.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Paste
            DataFormats.Format myFormat;

            myFormat = DataFormats.GetFormat(DataFormats.Rtf);

            if (rtxtMain.CanPaste(myFormat))
            {
                rtxtMain.Paste(myFormat);
                return;
            }

            myFormat = DataFormats.GetFormat(DataFormats.Text);

            if (rtxtMain.CanPaste(myFormat))
            {
                rtxtMain.Paste(myFormat);
                return;
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select All
            if (rtxtMain.TextLength > 0)
                rtxtMain.SelectAll();
        }

        private void fontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the font dialog.
            DialogResult result = fontDialog1.ShowDialog();

            // Check if OK was pressed.
            if (result == DialogResult.OK)
            {
                // Get Font.
                Font font = fontDialog1.Font;

                // Set font to richtextbox properties.              
                this.rtxtMain.SelectionFont = font;
            }
        }

        private void rtxtMain_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle Tab
            if (e.KeyCode == Keys.Tab)
            {
                e.Handled = false;
            }
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show the color dialog.
            DialogResult result = colorDialog1.ShowDialog();

            // Check if OK was pressed.
            if (result == DialogResult.OK)
            {
                // Get Color
                Color color = colorDialog1.Color;

                // Set Color to richtextbox properties.              
                this.rtxtMain.SelectionColor = color;
            }
        }

        private void HighlightWords(string word)
        {
            // Search
            bool first = true;
            int firstmatch=0;
            MatchCollection matches = Regex.Matches(rtxtMain.Text,@"\b"+word+@"\b",RegexOptions.IgnoreCase);

            //Apply color to all matching text
            foreach (Match match in matches)
            {
                if (first)
                {
                    first = false;
                    firstmatch = match.Index;
                }
                rtxtMain.Select(match.Index, match.Length);
                rtxtMain.SelectionBackColor = Color.Yellow;
            }

            // Set focus
            rtxtMain.DeselectAll();
            rtxtMain.SelectionStart = firstmatch;
            rtxtMain.Focus();

        }

        private void printToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            // Print right now with default settings!
            rtxtMain.Print();
        }

        private void printerSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // Allow the user to choose the page range he or she would
            // like to print.
            printDialog1.AllowSomePages = true;

            // Show the help button.
            printDialog1.ShowHelp = true;

            // Set the Document property to the PrintDocument  
            printDialog1.PrinterSettings.Copies = 1;

            DialogResult result = printDialog1.ShowDialog();

            // If the result is OK then print the document.
            if (result == DialogResult.OK)
            {
                rtxtMain.Print();
            }
        }

        private void insertImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Insert images
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Images |*.bmp;*.jpg;*.png;*.gif;*.ico";
            openFileDialog1.Multiselect = false;
            openFileDialog1.FileName = "";

            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                Image img = Image.FromFile(openFileDialog1.FileName);
                Clipboard.SetImage(img);
                rtxtMain.Paste();
                rtxtMain.Focus();
            }
            else
            {
                rtxtMain.Focus();
            }

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // About
            MessageBox.Show("Created 2018 by Shane Feek (shane.feek@gmail.com)", "About");
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Clear search
            rtxtMain.SelectAll();
            rtxtMain.SelectionBackColor = Color.White;
            rtxtMain.DeselectAll();
        }

        private void findToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Show find dialog
            frmFind f = new frmFind();
            f.StartPosition = FormStartPosition.CenterParent;
            f.ShowDialog();

            HighlightWords(f.Find);
        }

        public byte[] Compress(byte[] data)
        {
            // Compress byte array
            using (var compressedStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    zipStream.Write(data, 0, data.Length);
                    zipStream.Close();
                    return compressedStream.ToArray();
                }
            }
        }

        public byte[] Decompress(byte[] data)
        {
            // Decompress byte array
            using (var compressedStream = new MemoryStream(data))
            {
                using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        zipStream.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }

        // Context menu redirects
        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            cutToolStripMenuItem_Click(sender,e);
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Click(sender, e);
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem_Click(sender, e);
        }

        private void selectAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            selectAllToolStripMenuItem_Click(sender, e);
        }
    }
}
