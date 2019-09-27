using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UniversalServer.IUser
{
    public class ClientStorage
    {

        #region Vars

        ClientTracker _ClientTracker;
        string _StorageDirectory;
        string ext = ".ini";

        #endregion Vars

        #region Initialization

        public ClientStorage(ClientTracker clientTracker, string storageDirectory)
        {
            _ClientTracker = clientTracker;
            _StorageDirectory = storageDirectory;

            UserStorageDirectory();
        }

        #endregion Initialization

        #region Directories

        #region UserStorageDirectory

        private string UserStorageDirectory()
        {
            string UserStorageDirectory = _StorageDirectory + @"\" + _ClientTracker.ClientID;
            if (!Directory.Exists(UserStorageDirectory)) { Directory.CreateDirectory(UserStorageDirectory); }
            return UserStorageDirectory;
        }

        #endregion UserStorageDirectory

        #region GetSettingFile

        private string GetSettingsFile(string SettingTitle)
        {
            string SettingFile = UserStorageDirectory() + @"\" + SettingTitle + ext;
            if (!File.Exists(SettingFile)) File.Create(SettingFile).Close();
            return SettingFile;
        }

        #endregion GetSettingFile

        #endregion Directories

        #region EditSetting

        public void EditSetting(string SettingTitle, string SettingValue)
        {
            string SettingsFileDirectory = GetSettingsFile(SettingTitle);
            string EncryptedSettingsValue = Encrypt(SettingValue, _ClientTracker.ClientID, 8);
            File.WriteAllText(SettingsFileDirectory, EncryptedSettingsValue);
        }

        #endregion EditSetting

        #region ReadSetting

        public string ReadSetting(string SettingTitle, string SettingValue)
        {
            string SettingsFileDirectory = GetSettingsFile(SettingTitle);
            string FileContent = File.ReadAllText(SettingsFileDirectory);
            if (FileContent != "")
            {
                return Decrypt(FileContent, _ClientTracker.ClientID, 8);
            }
            else { return ""; }
        }

        #endregion ReadSetting

        #region Encryption

        #region GenerateKey

        private static string GenerateKey(int iKeySize)
        {
            RijndaelManaged aesEncryption = new RijndaelManaged();
            aesEncryption.KeySize = iKeySize;
            aesEncryption.BlockSize = 128;
            aesEncryption.Mode = CipherMode.CBC;
            aesEncryption.Padding = PaddingMode.PKCS7;
            aesEncryption.GenerateIV();
            string ivStr = Convert.ToBase64String(aesEncryption.IV);
            aesEncryption.GenerateKey();
            string keyStr = Convert.ToBase64String(aesEncryption.Key);
            string completeKey = ivStr + "," + keyStr;

            return Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes(completeKey));
        }

        #endregion GenerateKey

        #region Encrypt

        private static string Encrypt(string iPlainStr, string iCompleteEncodedKey, int iKeySize)
        {
            RijndaelManaged aesEncryption = new RijndaelManaged();
            aesEncryption.KeySize = iKeySize;
            aesEncryption.BlockSize = 128;
            aesEncryption.Mode = CipherMode.CBC;
            aesEncryption.Padding = PaddingMode.PKCS7;
            aesEncryption.IV = Convert.FromBase64String(ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey)).Split(',')[0]);
            aesEncryption.Key = Convert.FromBase64String(ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey)).Split(',')[1]);
            byte[] plainText = ASCIIEncoding.UTF8.GetBytes(iPlainStr);
            ICryptoTransform crypto = aesEncryption.CreateEncryptor();
            byte[] cipherText = crypto.TransformFinalBlock(plainText, 0, plainText.Length);
            return Convert.ToBase64String(cipherText);
        }

        #endregion Encrypt

        #region Decrypt

        private static string Decrypt(string iEncryptedText, string iCompleteEncodedKey, int iKeySize)
        {
            RijndaelManaged aesEncryption = new RijndaelManaged();
            aesEncryption.KeySize = iKeySize;
            aesEncryption.BlockSize = 128;
            aesEncryption.Mode = CipherMode.CBC;
            aesEncryption.Padding = PaddingMode.PKCS7;
            aesEncryption.IV = Convert.FromBase64String(ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey)).Split(',')[0]);
            aesEncryption.Key = Convert.FromBase64String(ASCIIEncoding.UTF8.GetString(Convert.FromBase64String(iCompleteEncodedKey)).Split(',')[1]);
            ICryptoTransform decrypto = aesEncryption.CreateDecryptor();
            byte[] encryptedBytes = Convert.FromBase64CharArray(iEncryptedText.ToCharArray(), 0, iEncryptedText.Length);
            return ASCIIEncoding.UTF8.GetString(decrypto.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length));
        }

        #endregion Decrypt

        #endregion Encryption

    }
}
