using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.IO;
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
        static void Encrypt(string Password,string OpenPath, string SavePath)
        {
            // Generate a random Nonce
            byte[] nonce = new byte[96 / 8];
            new Random().NextBytes(nonce);
            // Create the key
            byte[] PasswordToKey = new PasswordDeriveBytes(Encoding.UTF8.GetBytes(Password), nonce).GetBytes(256 / 8);
            using (FileStream plainfile = new FileStream(OpenPath, FileMode.Open))
            {
                
                using (FileStream encryptfile = new FileStream(SavePath, FileMode.OpenOrCreate))
                {
                    // Write The nonce to the file
                    encryptfile.Write(nonce, 0, nonce.Length);
                    // Setup the Crypto Engine then Read the file and start the encryption
                    BufferedAeadBlockCipher buffblockcipher = new BufferedAeadBlockCipher(new GcmBlockCipher(new AesEngine()));
                    buffblockcipher.Init(true, new AeadParameters(new KeyParameter(PasswordToKey), 128, nonce));
                    CipherStream cryptstream = new CipherStream(plainfile, buffblockcipher, buffblockcipher);
                    try
                    {
                        Console.WriteLine("The File (" + OpenPath + ") has been queued for encryption");
                        cryptstream.CopyTo(encryptfile);
                        Console.WriteLine("The File (" + OpenPath + ") has been successfully encrypted");
                    } catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("Permission Denied while accessing the file (Save/Open location for: "+OpenPath+"). Please try again by running the application as administrator. Press enter to close the application.");
                    } catch (Exception ex)
                    {
                        Console.WriteLine("The File ("+ OpenPath+") has encounter an unknown error while encrypting. Error: "+ex.Message);
                    }
                }
            }
        }
        static public void Decrypt(string Password, string OpenPath, string SavePath)
        {
            using (FileStream encryptfile = File.OpenRead(OpenPath))
            {
                // Get the IV/Nonce from the file
                byte[] nonce = new byte[12];
                encryptfile.Read(nonce, 0, nonce.Length);
                // Create the password
                byte[] PasswordToKey = new PasswordDeriveBytes(Encoding.UTF8.GetBytes(Password), nonce).GetBytes(256 / 8);
                using (FileStream plainfile = new FileStream(SavePath, FileMode.OpenOrCreate))
                {
                    // Setup the Crypto Engine and start the encryption process
                    BufferedAeadBlockCipher buffblockcipher = new BufferedAeadBlockCipher(new GcmBlockCipher(new AesEngine()));
                    CipherStream cryptstream = new CipherStream(encryptfile, buffblockcipher, buffblockcipher);
                    buffblockcipher.Init(false, new AeadParameters(new KeyParameter(PasswordToKey), 128, nonce));
                    try
                    {
                        Console.WriteLine("The File (" + OpenPath + ") has been queued for decryption");
                        cryptstream.CopyTo(plainfile);
                        Console.WriteLine("The File ("+OpenPath+") has been successfully decrypted.");
                    } catch (UnauthorizedAccessException)
                    {
                        Console.WriteLine("Permission Denied while accessing the file (Save/Open location for: " + OpenPath + "). Please try again by running the application as administrator. Press enter to close the application.");
                    } catch (InvalidCipherTextException)
                    {
                        Console.WriteLine("The file ("+ OpenPath +") cannot be decrypted because you supplied an incorrect key or the file has been tampered.");
                    } catch(Exception ex)
                    {
                        Console.WriteLine("The File (" + OpenPath + ") has encounter an unknown error while decrypting. Error: " + ex.Message);
                    }
                    
                }
            }
        }
        static void Main(string[] args)
        {
            // Some variable that we will need to do the jobs
            bool AllFileInFolder;
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
            string Password = Console.ReadLine();
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
                // Multi-File Encryption
                // Let the user select the folder where they want all the files to be encrypted, subfolder will be excluded though.
                Console.WriteLine("Now type or paste the folder that you want all the files to be encrypted/decrypted: ");
                string OpenPath = Console.ReadLine();
                while(true)
                {
                    if (string.IsNullOrEmpty(OpenPath))
                    {
                        // Empty stuff not gonna work
                        Console.Clear();
                        Console.WriteLine("An empty or whitespace only path name is not allowed, please type/paste your path again: ");
                        OpenPath = Console.ReadLine();
                    }
                    else if (!Directory.Exists(OpenPath))
                    {
                        // Directory Not Found
                        Console.Clear();
                        Console.WriteLine("The directory was not found, please try again: ");
                        OpenPath = Console.ReadLine();
                    }
                    else if (Directory.GetFiles(OpenPath).Length <= 0)
                    {
                        // There is no files in the directory
                        Console.Clear();
                        Console.WriteLine("There isn't files in the directory, try a different directory instead: ");
                        OpenPath = Console.ReadLine();
                    }
                    else
                        break;
                }
                // Now let them choose the location to store the file.
                Console.Clear();
                Console.WriteLine("Now type or paste the folder that you want the file to be saved in: ");
                string SavePath = Console.ReadLine();
                while (true)
                {
                    if (string.IsNullOrEmpty(SavePath))
                    {
                        // Empty stuff not gonna work
                        Console.Clear();
                        Console.WriteLine("An empty or whitespace only path name is not allowed, please type/paste your path again: ");
                        SavePath = Console.ReadLine();
                    }
                    else if (!Directory.Exists(SavePath))
                    {
                        // Directory Not Found
                        Console.Clear();
                        Console.WriteLine("The directory was not found, please try again: ");
                        SavePath = Console.ReadLine();
                    }
                    else if (Directory.GetFiles(SavePath).Length > 0)
                    {
                        // There is file detected in the directory, warn user
                        Console.Clear();
                        Console.WriteLine("WARNING: There is already file in the directory that you try to save the file in. The files will be overwritten if there is a same name. Press Enter to confirm that you want to proceed, otherwise terminate the application.");
                        Console.ReadLine();
                        break;
                    }
                    else
                        break;
                }
                Console.Clear();
                if(IsEncryption == true)
                {
                    // Encrypting mass file
                    string[] Files = Directory.GetFiles(OpenPath);
                    int TotalProcessedFile = 0;
                    int TotalFileAvaible = 0;
                    Console.WriteLine("Mass File Encryption Started...");
                    foreach(string file in Files)
                    {
                        if (Path.GetFileName(file) != "")
                            TotalFileAvaible = TotalFileAvaible + 1;
                        Task.Run(() => { Encrypt(Password, file, SavePath + "/" + Path.GetFileNameWithoutExtension(file)+Path.GetExtension(file)); TotalProcessedFile = TotalProcessedFile + 1; if (TotalProcessedFile >= Files.Length) { Console.WriteLine("Mass File Encryption Process completed, press enter to exit."); }});
                    }
                    while(true)
                    {
                        Console.ReadLine();
                        if(TotalProcessedFile >= TotalFileAvaible)
                        {
                            break;
                        }
                    }
                } else if(IsEncryption == false)
                {
                    // Decrypting mass file
                    string[] Files = Directory.GetFiles(OpenPath);
                    int TotalProcessedFile = 0;
                    int TotalFileAvaible = 0;
                    Console.WriteLine("Mass File Decryption Started...");
                    foreach (string file in Files)
                    {
                        if (Path.GetFileName(file) != "")
                            TotalFileAvaible = TotalFileAvaible + 1;
                        Task.Run(() => { Decrypt(Password, file, SavePath+"/"+ Path.GetFileNameWithoutExtension(file) + Path.GetExtension(file)); TotalProcessedFile = TotalProcessedFile + 1; if (TotalProcessedFile >= Files.Length) { Console.WriteLine("Mass File Decryption Process completed, press enter to exit."); } });
                    }
                    while (true)
                    {
                        Console.ReadLine();
                        if (TotalProcessedFile >= TotalFileAvaible)
                        {
                            break;
                        }
                    }
                }
            } else if(AllFileInFolder == false)
            {
                // Single File Encryption Method
                // Now let the user select the file that they want to encrypt
                Console.WriteLine("Now type or paste the path of the file that you would like to be encrypted/decrypted: ");
                string OpenPath = Console.ReadLine();
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
                string SavePath = Console.ReadLine();
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
                if(IsEncryption == true)
                {
                    // Single File Encryption
                    try
                    {
                        Console.WriteLine("Encrypting File, please wait...");
                        Encrypt(Password, OpenPath, SavePath);
                        Console.WriteLine("Encryption Process has been Completed, press Enter to exit the application");
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
                        Decrypt(Password, OpenPath, SavePath);
                        Console.WriteLine("Decryption Process has been Completed, press Enter to exit the application");
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
