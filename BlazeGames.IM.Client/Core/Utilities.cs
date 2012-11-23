using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Collections.Specialized;
using System.IO.Compression;
using System.IO;

namespace BlazeGames.IM.Client.Core
{
    internal class Utilities
    {
        public static string MD5(byte[] inputBytes)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static void SubmitBug(Exception ex)
        {
            using (var wc = new WebClient())
            {
                var data = new NameValueCollection();
                data["title"] = ex.Message;
                data["body"] = string.Format(@"User: {0}
MD5Hash: {5}
Build Info: {6}
Source: {3}
Method Name: {4}

Message
{1}

Stack Trace
{2}", App.Account, ex.Message, ex.StackTrace, ex.Source, ex.TargetSite.Name, App.Instance.MD5Hash, App.Instance.RetrieveLinkerTimestamp().ToString());

                var response = Encoding.Default.GetString(wc.UploadValues("https://blaze-games.com/issues/?Act=Create", "POST", data));
                Console.WriteLine(response);
            }
        }

        public static void UploadImage(System.Drawing.Image img, Contact SendTo)
        {
            MemoryStream ImageStream = new MemoryStream();
            img.Save(ImageStream, System.Drawing.Imaging.ImageFormat.Png);

            byte[] image = Compress(ImageStream.ToArray());
            using (WebClient wc = new WebClient())
            {
                wc.UploadDataCompleted += (sender, e) =>
                {
                    string Url = Encoding.Default.GetString(e.Result);
                    SendTo.SendMessage(string.Format("<Span xmlns=\"default\"><Image Source=\"{0}\" /></Span>", Url));
                    System.Windows.MessageBox.Show(Url);
                };

                wc.UploadDataAsync(new Uri("http://blaze-games.com/files/upload/&file_name=UploadedImage.png"), image);
            }
        }

        public static string GetLocalAddress()
        {
            string Address = "";

            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                    Address = ip.ToString();
            }

            return Address;
        }

        #region Compression
        public static byte[] Decompress(byte[] zippedData)
        {
            byte[] decompressedData = null;
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (MemoryStream inputStream = new MemoryStream(zippedData))
                {
                    using (GZipStream zip = new GZipStream(inputStream, CompressionMode.Decompress))
                    {
                        zip.CopyTo(outputStream);
                    }
                }
                decompressedData = outputStream.ToArray();
            }

            return decompressedData;
        }

        public static byte[] Compress(byte[] plainData)
        {
            byte[] compressesData = null;
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    zip.Write(plainData, 0, plainData.Length);
                }
                compressesData = outputStream.ToArray();
            }

            return compressesData;
        }
        #endregion
    }
}
