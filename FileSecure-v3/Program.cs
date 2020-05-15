using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileSecure_v3
{
    class Program
    {
        static int CycleSize = 1024 * 1024 * 100; // Increasing this value can reduce the amount of cycle that is required to encrypt an file but also require higher memory consumption
        static int Encrypt(byte[] PasswordToKey,string OpenPath, string SavePath)
        {
            // Generate a random Nonce
            byte[] nonce = new byte[128 / 8];
            new Random().NextBytes(nonce);
            // Setup the Crypto Engine
            GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
            cipher.Init(true, new AeadParameters(new KeyParameter(PasswordToKey), 128, nonce));
            // Read the file ad start the encryption
            using (FileStream plainfile = new FileStream(OpenPath, FileMode.Open))
            {
                byte[] buffer = new byte[CycleSize];
                plainfile.Seek(0, SeekOrigin.Begin);
                int bytesRead = plainfile.Read(buffer, 0, CycleSize);
                int TotalFileSize = nonce.Length;
                using (FileStream encryptfile = new FileStream(SavePath, FileMode.OpenOrCreate))
                {
                    encryptfile.Write(nonce, 0, nonce.Length);
                    int CycleCount = 0;
                    while (bytesRead > 0)
                    {
                        CycleCount = CycleCount + 1;
                        if (bytesRead < CycleSize)
                            encryptfile.Seek(0, SeekOrigin.End);
                        else
                            encryptfile.Seek(0, SeekOrigin.Current);
                        byte[] cipherText = new byte[cipher.GetOutputSize(bytesRead)];
                        int len = cipher.ProcessBytes(buffer, 0, bytesRead, cipherText, 0);
                        if (bytesRead < CycleSize)
                            cipher.DoFinal(cipherText, len);
                        Console.WriteLine("Cycle #" + CycleCount + "'s Data Length: " + cipherText.Length);
                        TotalFileSize = TotalFileSize + cipherText.Length;
                        encryptfile.Write(cipherText, 0, cipherText.Length);
                        bytesRead = plainfile.Read(buffer, 0, CycleSize);
                    }
                    Console.WriteLine("Cycle Completed, Final Size: "+TotalFileSize);
                    Array.Clear(buffer, 0, buffer.Length); // Clean the memory so that it stops hogging the memory if you decided to keep it running after it finishes the task.
                }
                return TotalFileSize;
            }
        }
        static public int Decrypt(byte[] PasswordToKey, string OpenPath, string SavePath)
        {
            using (FileStream encryptfile = File.OpenRead(OpenPath))
            {
                // Setup the Crypto Engine
                byte[] nonce = new byte[16];
                encryptfile.Seek(0, SeekOrigin.Begin);
                encryptfile.Read(nonce, 0, nonce.Length);
                encryptfile.Seek(0, SeekOrigin.Current);
                GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
                cipher.Init(false, new AeadParameters(new KeyParameter(PasswordToKey), 128, nonce));
                byte[] buffer = new byte[CycleSize];
                int bytesRead = encryptfile.Read(buffer, 0, CycleSize);
                int TotalFileSize = 0;
                using (FileStream plainfile = new FileStream(SavePath, FileMode.OpenOrCreate))
                {
                    int CycleCount = 0;
                    while (bytesRead > 0)
                    {
                        CycleCount = CycleCount + 1;
                        if (bytesRead < CycleSize)
                            plainfile.Seek(0, SeekOrigin.End);
                        else
                            plainfile.Seek(0, SeekOrigin.Current);
                        var cipherText = new byte[cipher.GetOutputSize(bytesRead)];
                        var len = cipher.ProcessBytes(buffer, 0, bytesRead, cipherText, 0);
                        if (bytesRead < CycleSize)
                            cipher.DoFinal(cipherText, len);
                        Console.WriteLine("Cycle #" + CycleCount + "'s Data Length: " + cipherText.Length);
                        TotalFileSize = TotalFileSize + cipherText.Length;
                        plainfile.Write(cipherText, 0, cipherText.Length);
                        bytesRead = encryptfile.Read(buffer, 0, CycleSize);
                    }
                    Console.WriteLine("Cycle Completed, Final Size: " + TotalFileSize);
                    Array.Clear(buffer, 0, buffer.Length); // Clean the memory so that it stops hogging the memory if you decided to keep it running after it finishes the task.
                }
                return TotalFileSize;
            }
        }
        static void Main(string[] args)
        {
            // Some variable that we will need to do the jobs
            string Password;
            string OpenPath;
            bool AllFileInFolder;
            string SavePath;
            bool IsEncryption;
            // Some credit stuff and notes
            Console.Title = "FileSecure v3 by zhiyan114";
            Console.WriteLine("Thank you for choosing FileSecure v3, a light-weighted command prompt file protection software ");
            Console.WriteLine("This version has many improvement including switching encryption to AES-GCM as well as removing unnecessary need for IV input while still maintaining the same security level. Do note that some feature (such as ability to use randomly generated key) will not be included.");
            Console.WriteLine("\n Press Enter to continue");
            Console.ReadLine();
            // Prompt user to enter an encryption password
            Console.Clear();
            Console.WriteLine("Please type a password that you would like to use to encrypt your file and press enter to continue (Remember the password, otherwise your file cannot be recovered without a backup):");
            Password = Console.ReadLine();
            while (true)
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    // Nah, user can't use a password that is only containing whitespace or nothing. This only exists because human is dumb.
                    Console.Clear();
                    Console.WriteLine("Invalid Password. Password cannot be empty or only containing whitespace. Try typing it again: ");
                    Password = Console.ReadLine();
                }
                else
                    break;
            }
            Console.Clear();
            // Ask them if they want to encrypt just a single file or all the file in a folder
            Console.WriteLine("Now, please type Y if you would like to encrypt all the files in a folder or N if you only want to encrypt a single file.");
            string Response = Console.ReadLine();
            while(true)
            {
                
                if(Response.ToLower() == "y")
                {
                    // User Selected Mass File Encryption
                    AllFileInFolder = true;
                    break;
                } else if(Response.ToLower() == "n")
                {
                    // User only wants single file encryption
                    AllFileInFolder = false;
                    break;
                } else
                {
                    // Invalid response, oh come on...
                    Console.Clear();
                    Console.WriteLine("Invalid Response, Type Y for All files in a folder or N for only single file");
                    Response = Console.ReadLine();
                }
            }
            Console.Clear();
            // Ask the user if they want to encrypt or decrypt the file
            Console.WriteLine("Type Y if you would like to encrypt the file or type N if you would like to decrypt the file:");
            string EncOrDecResponse = Console.ReadLine();
            while (true)
            {

                if (EncOrDecResponse.ToLower() == "y")
                {
                    // User want to encrypt
                    IsEncryption = true;
                    break;
                }
                else if (EncOrDecResponse.ToLower() == "n")
                {
                    // User want to decrypt
                    IsEncryption = false;
                    break;
                }
                else
                {
                    // Invalid response, oh come on...
                    Console.Clear();
                    Console.WriteLine("Invalid Response, Type Y for Encryption or N Decryption");
                    EncOrDecResponse = Console.ReadLine();
                }
            }
            Console.Clear();
            if (AllFileInFolder == true)
            {

            } else if(AllFileInFolder == false)
            {
                // Single File Encryption Method
                // Now let the user select the file that they want to encrypt
                Console.WriteLine("Now type or paste the path of the file that you would like to be encrypted/decrypted: ");
                OpenPath = Console.ReadLine();
                while (true)
                {
                    if (string.IsNullOrWhiteSpace(OpenPath))
                    {
                        // They tried this method again, gonna fail like last time
                        Console.Clear();
                        Console.WriteLine("An empty or whitespace only path name is not allowed, please type/paste your path again: ");
                        OpenPath = Console.ReadLine();
                    }
                    else if (!File.Exists(OpenPath))
                    {
                        // File not found
                        Console.Clear();
                        Console.WriteLine("The file that you try to encrypt does not exist, please try again: ");
                        OpenPath = Console.ReadLine();
                    }
                    else
                        break;
                }
                Console.Clear();
                // Now prompt them to select a path that they want the file to write on
                Console.WriteLine("Please type/paste a path that you would like the file to be written on: ");
                SavePath = Console.ReadLine();
                while(true)
                {
                    if (string.IsNullOrWhiteSpace(SavePath))
                    {
                        // They tried this method again, gonna fail like last time
                        Console.Clear();
                        Console.WriteLine("An empty or whitespace only path name is not allowed, please type/paste your path again: ");
                        SavePath = Console.ReadLine();
                    } else if(!Directory.Exists(Path.GetDirectoryName(SavePath)))
                    {
                        // Let the user know that the directory doesn't exist
                        Console.Clear();
                        Console.WriteLine("The Save Directory does not exist, please choose a different location: ");
                        SavePath = Console.ReadLine();
                    } else if (File.Exists(SavePath))
                    {
                        // File Already Existed, let them choose other path
                        Console.Clear();
                        Console.WriteLine("The file location you tried to save has already existed, please choose a different location: ");
                        SavePath = Console.ReadLine();
                    }
                    else
                        break;
                }
                Console.Clear();
                // Start the encryption operation
                byte[] PasswordToKey = new PasswordDeriveBytes(Password, Encoding.UTF8.GetBytes(Password)).GetBytes(256 / 8);
                if(IsEncryption == true)
                {
                    // Single File Encryption
                    try
                    {
                        Console.WriteLine("Encrypting File, please wait...");
                        Encrypt(PasswordToKey, OpenPath, SavePath);
                        Console.WriteLine("Encryption Completed, press Enter to exit the application");
                        Console.ReadLine();
                    } catch(UnauthorizedAccessException) {
                        Console.Clear();
                        Console.WriteLine("Permission Denied while accessing the file. Please try again by running the application as administrator. Press enter to close the application.");
                        Console.ReadLine();
                        return;
                    } catch(Exception ex)
                    {
                        Console.Clear();
                        Console.WriteLine("Unexpected error occured, error message: "+ex.Message+" (Press enter to exit the application)");
                        Console.ReadLine();
                        return;
                    }

                } else if(IsEncryption == false)
                {
                    // Single File Decryption
                    try
                    {
                        Console.WriteLine("Decrypting File, please wait...");
                        Decrypt(PasswordToKey, OpenPath, SavePath);
                        Console.WriteLine("Decryption Completed, press Enter to exit the application");
                        Console.ReadLine();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.Clear();
                        Console.WriteLine("Permission Denied while accessing the file. Please try again by running the application as administrator. Press enter to close the application.");
                        Console.ReadLine();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.Clear();
                        Console.WriteLine("Unexpected error occured, error message: " + ex.Message + " (Press enter to exit the application)");
                        Console.ReadLine();
                        return;
                    }
                }

            }
        }
    }
}
