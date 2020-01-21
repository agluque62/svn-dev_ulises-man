using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Utilities
{
    public class EncryptionHelper
    {
        private const string DES_cryptoKey = "#ndf#-mtto-cryptoKey";
        // The Initialization Vector for the DES encryption routine
        private static readonly byte[] DES_IV = new byte[8] { 240, 3, 45, 29, 0, 76, 173, 59 };
        /// <summary>
        /// Encrypts provided string parameter
        /// </summary>
        public static string DES_Cifrar(string s)
        {
            if (s == null || s.Length == 0) return string.Empty;
            string result = string.Empty;
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(s);
                TripleDESCryptoServiceProvider des =
                    new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider MD5 =
                    new MD5CryptoServiceProvider();
                des.Key =
                    MD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(DES_cryptoKey));
                des.IV = DES_IV;
                result = Convert.ToBase64String(
                    des.CreateEncryptor().TransformFinalBlock(
                        buffer, 0, buffer.Length));
            }
            catch
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// Decrypts provided string parameter
        /// </summary>
        public static string DES_Decrypt(string s)
        {
            if (s == null || s.Length == 0) return string.Empty;
            string result = string.Empty;
            try
            {
                byte[] buffer = Convert.FromBase64String(s);
                TripleDESCryptoServiceProvider des =
                    new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider MD5 =
                    new MD5CryptoServiceProvider();
                des.Key =
                    MD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(DES_cryptoKey));
                des.IV = DES_IV;
                result = Encoding.ASCII.GetString(
                    des.CreateDecryptor().TransformFinalBlock(
                    buffer, 0, buffer.Length));
            }
            catch
            {
                throw;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texto"></param>
        /// <param name="xmlKeys"></param>
        /// <returns></returns>
        public static byte[] RSA_Cifrar(string texto, string xmlKeys)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

            rsa.FromXmlString(xmlKeys);

            byte[] datosEnc = rsa.Encrypt(Encoding.Default.GetBytes(texto), false);

            return datosEnc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datosEnc"></param>
        /// <param name="xmlKeys"></param>
        /// <returns></returns>
        public static string RSA_descifrar(byte[] datosEnc, string xmlKeys)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

            rsa.FromXmlString(xmlKeys);

            byte[] datos = rsa.Decrypt(datosEnc, false);
            string res = Encoding.Default.GetString(datos);
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        static string abc = "?$0VxC |KhGcyFS4d-QuoelDn(Iv52#/i:E&BmOL9r{=UfMbtPq7Nw,%1]Y3AsZT)}g_kJ86+aXjH.pz![WR;";
        public static string CAE_cifrar(string mensaje, int desplazamiento = 47)
        {
            string cifrado = "";
            desplazamiento = desplazamiento < 0 || desplazamiento >= abc.Length ? 47 : desplazamiento;
            foreach (char m in mensaje)
            {
                int index = abc.IndexOf(m);
                index = index >= 0 ? index + desplazamiento : index;
                index = index >= abc.Length ? index - abc.Length : index;
                cifrado += index >= 0 ? abc[index] : m;
            }
            return Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(cifrado));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cifrado"></param>
        /// <param name="desplazamiento"></param>
        /// <returns></returns>
        public static string CAE_descifrar(string cifrado, int desplazamiento = 47)
        {
            string descifrado = "";
            cifrado = System.Text.Encoding.ASCII.GetString(System.Convert.FromBase64String(cifrado));
            desplazamiento = desplazamiento < 0 || desplazamiento >= abc.Length ? 47 : desplazamiento;
            foreach (char m in cifrado)
            {
                int index = abc.IndexOf(m);
                if (index >= 0)
                {
                    index -= desplazamiento;
                    index = index < 0 ? index + abc.Length : index;
                    descifrado += abc[index];
                }
                else
                    descifrado += m;
            }
            return descifrado;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string FileMd5Hash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(filePath))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }
    }
}
