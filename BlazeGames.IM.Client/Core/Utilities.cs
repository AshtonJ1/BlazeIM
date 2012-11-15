using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Collections.Specialized;

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

        static void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
                Console.WriteLine(e.Error.ToString());
            else
                Console.WriteLine(e.Result);
        }
    }
}
