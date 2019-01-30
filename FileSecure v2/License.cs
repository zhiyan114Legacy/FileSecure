using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileSecure_v2
{
    partial class License : Form
    {
        string dbusername = "FileSecure";
        string dbpassword = "jIUNgnt0iCedW2Eh";
        string dbhost = "zhiyan114.xyz";
        string dbdatabase = "FileSecure";
        public License()
        {
            InitializeComponent();
        }

        private void License_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Sorry, but looks like your device is not activated. Please use the license form to fill out your license information and activate the device.", "License Not Detected");
            TopMost = true;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://selly.gg/p/fd03c65f");
            MessageBox.Show("Purchase Page is now opened, you may now find your product key in your email (Paypal email if purchased using paypal). If you did not receive a key, please DM zhiyan114 immediately with your transaction ID.", "Page Opened");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string CurrentIP = null;
            int TotalActivation = 0;
            if(string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Please type your license key in order to activate the product","Empty Key");
                return;
            }
            if(Generator.CheckInternet() == false)
            {
                MessageBox.Show("You do not have a internet connection, please check your internet connection and try again","Offline");
                return;
            }
            MySqlConnection db = null;
            try
            {
                db = new MySqlConnection("server=" + dbhost + ";uid=" + dbusername + ";pwd=" + dbpassword + ";database=" + dbdatabase + ";");
                db.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to license server, please try again later", "Server Error");
                Close();
            }
            using (MySqlCommand cmd = db.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM License WHERE ProductKey='"+textBox1.Text+"'";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows == false)
                    {
                        MessageBox.Show("The license key that you were try to activate was invalid, please type the license key again and try again. If there isn't any license typo, please contact zhiyan114.","Invalid License");
                        return;
                    } else
                    {
                        while (reader.Read())
                        {
                            if (reader["Ban"].ToString() == "1")
                            {
                                MessageBox.Show("Sorry, but unfortunately your license key/account has been terminated, please contact zhiyan114 for further information.", "Access Denied");
                                Close();
                            } else if ((Generator.TimeStamp() - int.Parse(reader["LastActivation"].ToString())) < 0x5265C00 && reader["PremiumKey"].ToString() != "1")
                            {
                                MessageBox.Show("Sorry, but you have attempt to re-witelist too early, please wait for 24 hours after the activation/whitelist change", "Access Denied");
                                Close();
                            } else if (Generator.CheckIP(Generator.GetIP()) != 0x0) {
                                MessageBox.Show("Sorry, but VPN/Proxy are not allowed, please disable them and try again. If this check was a mistake, please contact zhiyan114", "No VPN Allowed");
                                Close();
                            } else
                            {
                                // Pass all the checks, now perform the whitelist
                                CurrentIP = reader["ActivateIP"].ToString();
                                TotalActivation = int.Parse(reader["WhitelistChange"].ToString());
                            }
                        }
                    }
                }
               
                if(CurrentIP != null)
                {
                    if (CurrentIP == null || CurrentIP == "")
                    {
                        // This is a brand new License Key
                        cmd.CommandText = "UPDATE License SET CurrentIP='" + Generator.GetIP() + "', ActivateIP='" + Generator.GetIP() + "', LastActivation='" + Generator.TimeStamp() + "', HWID='" + Generator.GetHWID() + "' WHERE ProductKey='" + textBox1.Text + "'";
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            MessageBox.Show("Your License Key has been successfully activated, thank you for using our product. Reboot the software for this new effect", "Activation Success");
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Error while activation, this software still activated but recommand to contact zhiyan114 to prevent future errors", "ERROR");
                            Close();
                        }

                    }
                    else
                    {
                        // Assume this is a whitelist change
                        cmd.CommandText = "UPDATE License SET CurrentIP='" + Generator.GetIP() + "', LastActivation='" + Generator.TimeStamp() + "', HWID='" + Generator.GetHWID() + "', WhitelistChange='"+(TotalActivation+1).ToString()+"' WHERE ProductKey='" + textBox1.Text + "'";
                        if (cmd.ExecuteNonQuery() == 1)
                        {
                            MessageBox.Show("Your License Key has been successfully changed, thank you for using our product. Reboot the software for this new effect", "Whitelist Change Success");
                            Close();
                        }
                        else
                        {
                            MessageBox.Show("Error while activation, this software still changed but recommand to contact zhiyan114 to prevent future errors", "ERROR");
                            Close();
                        }
                    }
                }
                
            }
        }
    }
}
