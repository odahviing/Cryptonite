using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptonite
{
    public enum ExitCodes
    {
        // Base Codes
        OK = 000,

        // Exit Modes
        EXE_ALREADY_RUNNING = 100,
        CANT_SAVE_SETTINGS_FILES_QUIT = 101,

        // Run-time Errors
        NO_PERMISSIONS = 200,
        CANT_SAVE_SETTINGS_FILES = 201,

        // Notifications
        CANT_MAKE_FILE = 300,


        // Dev Bugs
        NOT_EXISTS_DIRECTORY = 900,
        
        



    }

    class Log
    {

    }

    public static class Helper
    {
        public static void Print(string text, params string[] para)
        {
            string final = DateTime.Now.ToString(@"dd/mm/yyyy HH:mm:ss") + " => " + String.Format(text, para) + "\n";
#if DEBUG
            Console.WriteLine(final);
#else
            // Todo: Add Log
#endif
        }

        public static void PrintError(ExitCodes Code, string text, params string[] para)
        {
            string final = DateTime.Now.ToString(@"dd/mm/yyyy HH:mm:ss") + " => " + Code.ToString() + " (" + (int)Code + ")\n" + String.Format(text, para) + "\n";
#if DEBUG
            Console.WriteLine(final);
#else
            // Todo: Add Log
#endif
        }

        public static void Exit(ExitCodes ExitCode)
        {
#if DEBUG
            Print("Quitting: {0}", ExitCode.ToString());
            Environment.Exit((int)ExitCode);

#else
            // Todo: Add Log
#endif
        }

        public static string GetSha256Hash(string plain)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(plain), 0, Encoding.UTF8.GetByteCount(plain));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        #region Converters
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
        #endregion

        #region Random Utilities
        private static readonly Random Rand = new Random();
        private static readonly object Lock = new object();
        private static int Next(Int32 num)
        {
            lock (Lock)
            {
                return Rand.Next(num);
            }
        }
        public static String randString(Int16 length = 20)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Next(s.Length)]).ToArray());
        }
        #endregion
    }
}
