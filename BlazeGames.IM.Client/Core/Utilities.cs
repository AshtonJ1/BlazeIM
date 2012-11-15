using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;

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
            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
            wc.DownloadStringAsync(new Uri(string.Format("https://blaze-games.com/Support/?AddTicket=true&Severity=3&Subject=Blaze IM Bug: {4}&Message={0}<br /><br /><b>Stack Trace</b><br />{1}<br /><br /><b>OS Version</b><br />{5}&Type=Bug&Department=Services&Account={2}&Password={3}", ex.Message, ex.StackTrace, App.Account, App.Password, Guid.NewGuid().ToString().Substring(0, 8), Environment.OSVersion.ToString())));
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
