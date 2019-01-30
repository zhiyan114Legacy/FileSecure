using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;

namespace FileSecure_v2
{
    public partial class Form1 : Form
    {

        // This software was orginally designed to use without any other dll references except license database xD which of course was removed.
        bool RunningTask = false;
        string OpenLocation = null;
        string SaveLocation = null;
        public Form1()
        {
            InitializeComponent();
            //Registery.LoadMainDB(); // This is not needed ok
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox4.Text = "An encryption software and open source by zhiyan114 (This MOTD is no longer a live version). \n\n Source can be found here: https://github.com/zhiyan114/FileSecure-v2";
            MessageBox.Show("Thank You for using FileSecure v2 developed by zhiyan114."); // comment this line if u dont want this message to show but please leave this uncomment if your sharing the software to someone else unless your just sharing the source.
        }

        private void StartTask(string msg)
        {
            if(RunningTask == false)
            {
                label8.Invoke(((Action)(() => { label8.Text = msg; })));
                RunningTask = true;
            }
        }
        private void EndTask(string msg)
        {
            if(RunningTask == true)
            {
                label8.Invoke(((Action)(() => { label8.Text = msg; })));
                RunningTask = false;
                if (!timer1.Enabled)
                    timer1.Start();
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (RijndaelManaged keygen = new RijndaelManaged())
            {
                keygen.GenerateIV();
                if (!string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    DialogResult result = MessageBox.Show("The IV box doesn't seem like it empty, do you still want to generate another IV (This will clear the existed IV that is currently on the textbox)?", "None Empty IV", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        textBox2.Text = Convert.ToBase64String(keygen.IV);
                        MessageBox.Show("Successfully Generated AES IV","Key Generated");
                    }
                    else
                    {
                        MessageBox.Show("Your IV did not generated", "Action Abort");
                    }
                }
                else
                {
                    textBox2.Text = Convert.ToBase64String(keygen.IV);
                    MessageBox.Show("Successfully Generated AES IV", "Key Generated");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (RijndaelManaged keygen = new RijndaelManaged())
            {
                keygen.GenerateKey();
                if(!string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    DialogResult result = MessageBox.Show("The Key box doesn't seem like it empty, do you still want to generate another key (This will clear the existed key that is currently on the textbox)?", "None Empty Key", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if(result == DialogResult.Yes)
                    {
                        textBox1.Text = Convert.ToBase64String(keygen.Key);
                        MessageBox.Show("Successfully Generated AES Key", "Key Generated");
                    } else
                    {
                        MessageBox.Show("Your Key did not generated","Action Abort");
                    }
                } else
                {
                    textBox1.Text = Convert.ToBase64String(keygen.Key);
                    MessageBox.Show("Successfully Generated AES Key", "Key Generated");
                }
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label8.Invoke(((Action)(() => { label8.Text = "Waiting For Task..."; })));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            OpenLocation = openFileDialog1.FileName;
            label4.Text = openFileDialog1.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "";
            if (DialogResult.OK != saveFileDialog1.ShowDialog())
                return;
            SaveLocation = saveFileDialog1.FileName;
            label5.Text = saveFileDialog1.FileName;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = checkBox1.Checked;
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            if (!RunningTask)
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text) || (string.IsNullOrWhiteSpace(textBox2.Text) & !checkBox2.Checked))
                {
                    MessageBox.Show("Key or/and IV textbox are empty, please make sure it filled with valid key", "Missing Keys");
                    return;
                }
                if (string.IsNullOrWhiteSpace(OpenLocation) || string.IsNullOrWhiteSpace(SaveLocation))
                {
                    MessageBox.Show("Please select a file and a location to save the file result before try again", "Missing Files");
                    return;
                }
                try
                {
                    using (FileStream tempfile = new FileStream(SaveLocation, FileMode.OpenOrCreate))
                    {
                        using (RijndaelManaged EncryptService = new RijndaelManaged())
                        {
                            try
                            {
                                if (checkBox2.Checked)
                                {
                                    //PasswordDeriveBytes. Yes, this is a deprecated method, change it whenever u can
                                    EncryptService.Key = new PasswordDeriveBytes(textBox1.Text,null).GetBytes(256 / 8);//.CryptDeriveKey("AES", "SHA512", 256, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                                    EncryptService.IV = new PasswordDeriveBytes(textBox1.Text, null).GetBytes(128 / 8); //new byte[] { 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48}; // 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64
                                }
                                else
                                {
                                    EncryptService.Key = Convert.FromBase64String(textBox1.Text);
                                    EncryptService.IV = Convert.FromBase64String(textBox2.Text);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("The Key/IV that you supplied was invalid", "Invalid Key");
                                return;
                            }
                            using (ICryptoTransform DecryptMode = EncryptService.CreateDecryptor())
                            {
                                using (CryptoStream DecryptStream = new CryptoStream(tempfile, DecryptMode, CryptoStreamMode.Write))
                                {
                                    try
                                    {
                                        StartTask("Decrypting File...");
                                        using (var EncryptedFile = File.Open(OpenLocation, FileMode.Open))
                                        {
                                            await Task.Run(() => { EncryptedFile.CopyTo(DecryptStream); });
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Decryption File Error, probably missing file/Corrupted Encrypted File/file being used by another process? \n\n Detailed Error Message: " + ex.Message, "Encryption Error");
                                        EndTask("Decryption File Failed...");
                                        return;
                                    }
                                }
                            }
                        }
                        EndTask("Decryption File Completed...");
                    }
                }
                catch (Exception ex)
                {
                    EndTask("Decryption File Failed...");
                    MessageBox.Show("There is Error while Opening/Writing on this file, probably permission denied or you just likely used an incorrect key to decrypt the file", "Error");
                    return;
                }
            }
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            if (!RunningTask)
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text) || (string.IsNullOrWhiteSpace(textBox2.Text) & !checkBox2.Checked))
                {
                    MessageBox.Show("Key or/and IV textbox are empty, please make sure it filled with valid key", "Missing Keys");
                    return;
                }
                if(string.IsNullOrWhiteSpace(OpenLocation) || string.IsNullOrWhiteSpace(SaveLocation))
                {
                    MessageBox.Show("Please select a file and a location to save the file result before try again","Missing Files");
                    return;
                }
                try
                {
                    using (FileStream tempfile = new FileStream(SaveLocation, FileMode.OpenOrCreate))
                    {
                        using (RijndaelManaged EncryptService = new RijndaelManaged())
                        {
                            try
                            {
                                if(checkBox2.Checked)
                                {
                                    //PasswordDeriveBytes Rfc2898DeriveBytes
                                    EncryptService.Key = new PasswordDeriveBytes(textBox1.Text, null).GetBytes(256 / 8);//.CryptDeriveKey("AES", "SHA512", 256, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
                                    EncryptService.IV = new PasswordDeriveBytes(textBox1.Text, null).GetBytes(128 / 8); //new byte[] { 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48}; // 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64
                                } else
                                {
                                    EncryptService.Key = Convert.FromBase64String(textBox1.Text);
                                    EncryptService.IV = Convert.FromBase64String(textBox2.Text);
                                }
                            } catch(Exception ex)
                            {
                                MessageBox.Show("The Key/IV that you supplied was invalid. ERROR: "+ex.Message,"Invalid Key");
                                return;
                            }
                            using (ICryptoTransform EncryptMode = EncryptService.CreateEncryptor())
                            {
                                using (CryptoStream EncryptStream = new CryptoStream(tempfile, EncryptMode, CryptoStreamMode.Write))
                                {
                                    try
                                    {
                                        StartTask("Encrypting File...");
                                        using (var UnencryptedFile = File.Open(OpenLocation, FileMode.Open))
                                        {
                                            await Task.Run(() => { UnencryptedFile.CopyTo(EncryptStream); });
                                        }

                                    } catch(Exception ex) {
                                        MessageBox.Show("Encrypting File Error, probably missing file or file being used by another process? \n\n Detailed Error Message: "+ex.Message,"Encryption Error");
                                        EndTask("Encryption File Failed...");
                                        return;
                                    }
                                }
                            }
                        }
                        EndTask("Encryption File Completed...");
                    }
                } catch(Exception ex)
                {
                    MessageBox.Show("There is Error while Opening/Writing on this file, probably permission denied?", "Error");
                    return;
                }                 
            }
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox2.Checked)
            {
                textBox2.Enabled = false;
                button2.Enabled = false;
                button1.Enabled = false;
            }
            else
            {
                textBox2.Enabled = true;
                button2.Enabled = true;
                button1.Enabled = true;
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            using (Form toolbox = new ToolBox())
            {
                toolbox.ShowDialog();
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/zhiyan114/FileSecure-v2"); // leave this here as credit
        }
    }
}
