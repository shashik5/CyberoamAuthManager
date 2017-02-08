using System.Configuration;
using System.IO;
using System.Xml;
using CryptManager;
using System;

namespace CyberoamAuthManager
{
    /// <summary>
    /// Class to store user details.
    /// </summary>
    class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Class to manage user details.
    /// </summary>
    static class UserDetails
    {
        /// <summary>
        /// Method to get decoded user details from xml.
        /// </summary>
        /// <param name="mode">If set as 'debug', it loads xml from project folder.</param>
        /// <returns>User details.</returns>
        public static User GetUser(string mode)
        {
            string xmlContent, userString;
            XmlDocument xml;
            string[] userData;

            try
            {
                xmlContent = File.ReadAllText(mode == "debug" ? ConfigurationManager.AppSettings["DataPathStandalone"] : string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\Cyberoam Auth Manager\\dat.xml"));
                xml = new XmlDocument();
                xml.LoadXml(xmlContent);
                userString = Crypto.Decode(xml.SelectSingleNode("/CyberoamAuthManager/Auth").InnerText);
            }
            catch (Exception)
            {
                userString = "u;p";
            }

            userData = userString.Split(';');

            return new User
            {
                Username = userData[0],
                Password = userData[1]
            };
        }
    }
}
