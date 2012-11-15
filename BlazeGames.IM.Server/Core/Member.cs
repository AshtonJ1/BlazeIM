using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Linq;
using MySql.Data.MySqlClient;

namespace BlazeGames.IM.Server.Core
{
    public class Member
    {
        public static bool Executing = false;

        public string LoginName,
            PasswordHash,
            Nickname,
            BirthDay,
            Email,
            FirstName,
            LastName,
            StreetAddress,
            State,
            City,
            Country,
            VerificationHash,
            Notes,
            MemberData,
            SecurityQuestion,
            SecurityAnswer,
            WebSessionKey,
            UserHostAddress,
            PIN;

        public int ID,
            ZIP,
            Authority,
            Cash;

        public bool EmailVerified,
            IsValid,
            RequestSecure,
            GlobalSession,
            MobileNotifications,
            EmailNotifications;

        public byte StatusCode = 0x00;

        public List<string> LinkedDevices = new List<string>();

        public List<string> PendingFriends = new List<string>();
        public List<string> BlockedFriends = new List<string>();
        public List<string> Friends = new List<string>();

        private MySqlConnection SqlConnection;

        public Member()
        {
            this.ID = 0;
        }

        public Member(string LoginName, string PasswordHash, int MemberID, string PIN, MySqlConnection SqlConnection)
        {
            this.LoginName = LoginName;
            this.PasswordHash = PasswordHash;
            this.PIN = PIN;

            this.SqlConnection = SqlConnection;

            MySqlCommand MemberLoadQuery = new MySqlCommand("SELECT ID FROM members WHERE LoginName=@LoginName AND PasswordHash=@PasswordHash AND PIN=@PIN, AND GlobalSession=true;", SqlConnection);
            MemberLoadQuery.Parameters.AddWithValue("@LoginName", this.LoginName);
            MemberLoadQuery.Parameters.AddWithValue("@PasswordHash", this.PasswordHash);
            MemberLoadQuery.Parameters.AddWithValue("@PIN", this.PIN);
            MySqlDataReader MemberLoadReader = MemberLoadQuery.ExecuteReader();

            if (MemberLoadReader.Read())
                this.ID = MemberLoadReader.GetInt32("ID");
            else
                this.ID = 0;

            MemberLoadReader.Close();

            this.Load();
        }

        public Member(int ID, MySqlConnection SqlConnection)
        {
            this.ID = ID;

            this.SqlConnection = SqlConnection;

            this.Load();
        }

        public Member(string Account, MySqlConnection SqlConnection)
        {
            this.SqlConnection = SqlConnection;

            while (Executing)
                System.Threading.Thread.Sleep(10);

            Executing = true;

            MySqlCommand MemberLoadQuery = new MySqlCommand("SELECT ID FROM members WHERE LoginName=@Account OR Email=@Account", SqlConnection);
            MemberLoadQuery.Parameters.AddWithValue("@Account", Account);
            using (MySqlDataReader MemberLoadReader = MemberLoadQuery.ExecuteReader())
            {
                if (MemberLoadReader.Read())
                    this.ID = MemberLoadReader.GetInt32("ID");
                else
                    this.ID = 0;
            }

            Executing = false;

            this.Load();
        }

        public void Save()
        {
            MySqlCommand MemberSaveQuery = new MySqlCommand(@"UPDATE members SET LoginName=@LoginName, PasswordHash=@PasswordHash, Nickname=@Nickname, BirthDay=@BirthDay, Email=@Email, FirstName=@FirstName, LastName=@LastName, StreetAddress=@StreetAddress, State=@State, City=@City, ZIP=@ZIP, Country=@Country, EmailVerified=@EmailVerified, VerificationHash=@VerificationHash, Authority=@Authority, Notes=@Notes, MemberData=@MemberData, SecurityQuestion=@SecurityQuestion, SecurityAnswer=@SecurityAnswer, Cash=@Cash, RequestSecure=@RequestSecure, LinkedDevices=@LinkedDevices, MobileNotifications=@MobileNotifications, EmailNotifications=@EmailNotifications, PendingFriends=@PendingFriends, BlockedFriends=@BlockedFriends, Friends=@Friends WHERE ID=@ID", SqlConnection);

            MemberSaveQuery.Parameters.AddWithValue("@LoginName", this.LoginName);
            MemberSaveQuery.Parameters.AddWithValue("@PasswordHash", this.PasswordHash);
            MemberSaveQuery.Parameters.AddWithValue("@Nickname", this.Nickname);
            MemberSaveQuery.Parameters.AddWithValue("@BirthDay", this.BirthDay);
            MemberSaveQuery.Parameters.AddWithValue("@Email", this.Email);
            MemberSaveQuery.Parameters.AddWithValue("@FirstName", this.FirstName);
            MemberSaveQuery.Parameters.AddWithValue("@LastName", this.LastName);
            MemberSaveQuery.Parameters.AddWithValue("@StreetAddress", this.StreetAddress);
            MemberSaveQuery.Parameters.AddWithValue("@State", this.State);
            MemberSaveQuery.Parameters.AddWithValue("@City", this.City);
            MemberSaveQuery.Parameters.AddWithValue("@ZIP", this.ZIP);
            MemberSaveQuery.Parameters.AddWithValue("@Country", this.Country);
            MemberSaveQuery.Parameters.AddWithValue("@EmailVerified", this.EmailVerified);
            MemberSaveQuery.Parameters.AddWithValue("@VerificationHash", this.VerificationHash);
            MemberSaveQuery.Parameters.AddWithValue("@Authority", this.Authority);
            MemberSaveQuery.Parameters.AddWithValue("@Notes", this.Notes);
            MemberSaveQuery.Parameters.AddWithValue("@MemberData", this.MemberData);
            MemberSaveQuery.Parameters.AddWithValue("@SecurityQuestion", this.SecurityQuestion);
            MemberSaveQuery.Parameters.AddWithValue("@SecurityAnswer", this.SecurityAnswer);
            MemberSaveQuery.Parameters.AddWithValue("@Cash", this.Cash);
            MemberSaveQuery.Parameters.AddWithValue("@RequestSecure", this.RequestSecure);
            MemberSaveQuery.Parameters.AddWithValue("@GlobalSession", this.GlobalSession);
            MemberSaveQuery.Parameters.AddWithValue("@PIN", this.PIN);
            MemberSaveQuery.Parameters.AddWithValue("@ID", this.ID);
            MemberSaveQuery.Parameters.AddWithValue("@LinkedDevices", String.Join(",", this.LinkedDevices.ToArray()));
            MemberSaveQuery.Parameters.AddWithValue("@MobileNotifications", MobileNotifications);
            MemberSaveQuery.Parameters.AddWithValue("@EmailNotifications", EmailNotifications);
            MemberSaveQuery.Parameters.AddWithValue("@PendingFriends", String.Join(",", this.PendingFriends.Distinct().ToArray()));
            MemberSaveQuery.Parameters.AddWithValue("@BlockedFriends", String.Join(",", this.BlockedFriends.Distinct().ToArray()));
            MemberSaveQuery.Parameters.AddWithValue("@Friends", String.Join(",", this.Friends.Distinct().ToArray()));

            MemberSaveQuery.ExecuteNonQuery();
        }

        public void Load()
        {
            if (this.ID == 0)
            {
                this.LoginName = "Guest";
                this.PasswordHash = "";
                this.Nickname = "";
                this.BirthDay = "";
                this.Email = "";
                this.FirstName = "";
                this.LastName = "";
                this.StreetAddress = "";
                this.State = "";
                this.City = "";
                this.Country = "";
                this.VerificationHash = "";
                this.Notes = "";
                this.MemberData = "";
                this.SecurityQuestion = "";
                this.SecurityAnswer = "";
                this.PIN = "0000";

                this.ID = 0;
                this.ZIP = 0;
                this.Authority = 0;
                this.Cash = 0;

                this.EmailVerified = false;
                this.RequestSecure = false;
                this.GlobalSession = false;
                this.IsValid = false;

                return;
            }

            MySqlCommand MemberLoadQuery = new MySqlCommand("SELECT * FROM members WHERE ID=@ID", SqlConnection);
            MemberLoadQuery.Parameters.AddWithValue("@ID", this.ID);

            while (Executing)
                System.Threading.Thread.Sleep(10);

            Executing = true;
            using (MySqlDataReader MemberLoadReader = MemberLoadQuery.ExecuteReader(System.Data.CommandBehavior.SingleRow))
            {
                if (MemberLoadReader.Read())
                {
                    this.LoginName = MemberLoadReader.GetString("LoginName");
                    this.PasswordHash = MemberLoadReader.GetString("PasswordHash");
                    this.Nickname = MemberLoadReader.GetString("Nickname");
                    this.BirthDay = MemberLoadReader.GetString("BirthDay");
                    this.Email = MemberLoadReader.GetString("Email");
                    this.FirstName = MemberLoadReader.GetString("FirstName");
                    this.LastName = MemberLoadReader.GetString("LastName");
                    this.StreetAddress = MemberLoadReader.GetString("StreetAddress");
                    this.State = MemberLoadReader.GetString("State");
                    this.City = MemberLoadReader.GetString("City");
                    this.Country = MemberLoadReader.GetString("Country");
                    this.VerificationHash = MemberLoadReader.GetString("VerificationHash");
                    this.Notes = MemberLoadReader.GetString("Notes");
                    this.MemberData = MemberLoadReader.GetString("MemberData");
                    this.SecurityQuestion = MemberLoadReader.GetString("SecurityQuestion");
                    this.SecurityAnswer = MemberLoadReader.GetString("SecurityAnswer");
                    this.PIN = MemberLoadReader.GetString("PIN");

                    this.ID = MemberLoadReader.GetInt32("ID");
                    this.ZIP = MemberLoadReader.GetInt32("ZIP");
                    this.Authority = MemberLoadReader.GetInt32("Authority");
                    this.Cash = MemberLoadReader.GetInt32("Cash");

                    this.LinkedDevices.AddRange(MemberLoadReader.GetString("LinkedDevices").Split(','));

                    this.PendingFriends.AddRange(MemberLoadReader.GetString("PendingFriends").Split(','));
                    this.BlockedFriends.AddRange(MemberLoadReader.GetString("BlockedFriends").Split(','));
                    this.Friends.AddRange(MemberLoadReader.GetString("Friends").Split(','));

                    this.EmailVerified = MemberLoadReader.GetBoolean("EmailVerified");
                    this.RequestSecure = MemberLoadReader.GetBoolean("RequestSecure");
                    this.GlobalSession = MemberLoadReader.GetBoolean("GlobalSession");
                    this.MobileNotifications = MemberLoadReader.GetBoolean("MobileNotifications");
                    this.EmailNotifications = MemberLoadReader.GetBoolean("EmailNotifications");
                    this.IsValid = true;
                }
                else
                {
                    this.LoginName = "Guest";
                    this.PasswordHash = "";
                    this.Nickname = "";
                    this.BirthDay = "";
                    this.Email = "";
                    this.FirstName = "";
                    this.LastName = "";
                    this.StreetAddress = "";
                    this.State = "";
                    this.City = "";
                    this.Country = "";
                    this.VerificationHash = "";
                    this.Notes = "";
                    this.MemberData = "";
                    this.SecurityQuestion = "";
                    this.SecurityAnswer = "";
                    this.PIN = "0000";

                    this.ID = 0;
                    this.ZIP = 0;
                    this.Authority = 0;
                    this.Cash = 0;

                    this.EmailVerified = false;
                    this.RequestSecure = false;
                    this.GlobalSession = false;
                    this.IsValid = false;
                }
            }
            Executing = false;
        }

        public static string TryLoginWithPassword(string LoginName, string Password, MySqlConnection SqlConnection)
        {
            MySqlCommand LoginCheckQuery = new MySqlCommand("SELECT Nickname FROM members WHERE LoginName=@LoginName AND PasswordHash=@PasswordHash AND EmailVerified=true OR Email=@LoginName AND PasswordHash=@PasswordHash AND EmailVerified=true", SqlConnection);
            LoginCheckQuery.Parameters.AddWithValue("@LoginName", LoginName);
            LoginCheckQuery.Parameters.AddWithValue("@PasswordHash", Password);
            object Nickname = LoginCheckQuery.ExecuteScalar();

            if (Nickname == null)
                return null;
            else
                return (string)Nickname;

            //using (MySqlDataReader LoginCheckReader = LoginCheckQuery.ExecuteReader())
            //    return LoginCheckReader.Read();
        }

        public static Member Null()
        {
            return null;
        }

        public static int FindMember(string Search, MySqlConnection SqlConnection)
        {
            MySqlCommand LoginCheckQuery = new MySqlCommand("SELECT ID FROM members WHERE Email=@Search OR Nickname=@Search", SqlConnection);
            LoginCheckQuery.Parameters.AddWithValue("@Search", Search);
            object IDobj = LoginCheckQuery.ExecuteScalar();

            if (IDobj != null)
                return (int)IDobj;
            else
                return -1;
        }
    }
}