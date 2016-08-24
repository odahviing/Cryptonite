using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cryptonite
{
    internal static class HostDetails
    {
        #region Privates
        /* Please use m_ prefix for properties holders */
        private static Type myType = typeof(HostDetails);
        private static string m_currentUser = null, m_hostName = null, m_domain = null, m_os = null, m_hostkey = null;
        private static bool? m_isdomain = null;
        private static uint? m_driveserial = null;
        private static IPAddress m_localip = null;
        #endregion


        #region Properties
        internal static String HostKey
        {
            get
            {
                if (m_hostkey == null)
                {
                    string hashString = HostName + DriveSerial;
                    m_hostkey = Helper.GetSha256Hash(hashString).Substring(0, 20);
                }
                return m_hostkey;
            }
        }
        internal static String CurrentUser
        {
            get
            {
                if (m_currentUser == null)
                {
                    m_currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    m_currentUser = m_currentUser.Substring(m_currentUser.IndexOf(@"\") + 1);
                }
                return m_currentUser;
            }
            private set
            {
                m_currentUser = value;
            }
        }
        internal static String HostName
        {
            get
            {
                if (m_hostName == null)
                {
                    m_hostName = Environment.MachineName.ToLower();
                }
                return m_hostName;
            }
            private set
            {
                m_hostName = value;
            }
        }
        internal static IPAddress LocalIP
        {
            get
            {
                if (m_localip == null)
                {
                    m_localip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                }
                return m_localip;
            }
            private set
            {
                m_localip = value;
            }
        }
        internal static Boolean IsDomain
        {
            get
            {
                if (m_isdomain == null)
                {
                    if (Domain == HostDetails.HostName)
                        m_isdomain = true;
                    else
                        m_isdomain = false;
                }
                return (bool)m_isdomain;
            }
            private set
            {
                m_isdomain = value;
            }
        }
        internal static String Domain
        {
            get
            {
                if (m_domain == null)
                {
                    m_domain = Environment.UserDomainName;
                }
                return m_domain;
            }
            private set
            {
                m_domain = value;
            }
        }
        internal static String OS
        {
            get
            {
                if (m_os == null)
                {
                    m_os = Environment.OSVersion.ToString();
                }
                return m_os;
            }
            private set
            {
                m_os = value;
            }
        }

        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(
            string PathName, 
            StringBuilder VolumeNameBuffer, 
            UInt32 VolumeNameSize, 
            ref UInt32 VolumeSerialNumber, 
            ref UInt32 MaximumComponentLength, 
            ref UInt32 FileSystemFlags, 
            StringBuilder FileSystemNameBuffer, 
            UInt32 FileSystemNameSize);

        internal static UInt32 DriveSerial
        {
            get
            {
                if (m_driveserial == null)
                {
                    StringBuilder devNullString = new StringBuilder(256); UInt32 devNullInt = 0;
                    UInt32 SerialNumber = 0; 
                    GetVolumeInformation(null, devNullString, 256, ref SerialNumber, ref devNullInt, ref devNullInt, devNullString, 256);
                    m_driveserial = SerialNumber;
                }
                return (uint)m_driveserial;
            }
            private set
            {
                m_driveserial = value;
            }
        }
        #endregion

        #region Methods
        internal static void UpdateField(string fieldName)
        {
            FieldInfo myField = myType.GetField("m_" + fieldName.ToLower(), BindingFlags.Static|BindingFlags.NonPublic);
            myField.SetValue(null, null);
        }
        #endregion
    }
}
