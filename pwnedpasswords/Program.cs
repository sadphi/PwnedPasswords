//This Source Code Form is subject to the terms of the Mozilla Public
//License, v. 2.0. If a copy of the MPL was not distributed with this
//file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pwnedpasswords
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            string password;

            #region Program Start options
#if DEBUG
            password = "password";
#endif

#if RELEASE
            password = string.Empty;
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify a password!" + Environment.NewLine + Environment.NewLine + "The following format must be used: passwordchecker [password]");
                Console.ReadKey();
                Environment.Exit(-1);
            }

            if (args.Length == 1)
            {
                password = args[0];
            }

            else
            {
                Console.WriteLine("Only specify 1 password!");
                Console.ReadKey();
                Environment.Exit(-1);
            }
#endif
            #endregion


            string hash = Hash(password);
            string query = hash.Remove(5); //Only keep the 5 first characters in the query

            string pwList = string.Empty;
            try
            {
                var response = await client.GetAsync($"https://api.pwnedpasswords.com/range/{query}");

                if (response.StatusCode == HttpStatusCode.OK)
                    pwList = await response.Content.ReadAsStringAsync();

                else
                    Console.WriteLine($"Request denied! Code: {response.StatusCode}");
            }

            catch (HttpRequestException exception)
            {
                Console.WriteLine("Could not connect to remote:" + Environment.NewLine + Environment.NewLine + exception + Environment.NewLine + Environment.NewLine + "Press any key to quit");
                Console.ReadKey();
                Environment.Exit(-1);
            }


            string hashTruncated = hash.Substring(5); //Remove the first 5 characters from the hash

            if (pwList.Contains(hashTruncated)) //Check if the response data contains our hash
            {
                //Fetch the amount of times this password has been "seen"
                int start = pwList.IndexOf(hashTruncated);
                start += hashTruncated.Length + 1;                  //The start index of "amount of times seen" (+ 1 to remove ':')
                int end = pwList.IndexOf("\r\n", start);            //The end index of "amount of times seen"
                int length = Math.Abs(start - end);                 //The amount of digits

                string toConvert = pwList.Substring(start, length); //Create a string which will only contain the "amount of times seen"

                //Try parsing to an int so we can format it in the output
                int.TryParse(toConvert, out int amount);

                //Clone the current culture's NumberFormatInfo and disable decimal digits
                NumberFormatInfo numberFormatInfo = (NumberFormatInfo) NumberFormatInfo.CurrentInfo.Clone();
                numberFormatInfo.NumberDecimalDigits = 0;

                Console.WriteLine($"This password has been pwned, and it has been seen {amount.ToString("N", numberFormatInfo)} times!" + Environment.NewLine + $"Pwned Hash: {hash}" + Environment.NewLine + $"Pwned Password: {password}");
            }

            else
                Console.WriteLine("Phew — This password has not been pwned!");

            Console.ReadKey();
        }

        /// <summary>
        /// Create a hash of a specified password using SHA-1
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns></returns>
        static string Hash(string password)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();

            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));

            StringBuilder stringBuilder = new StringBuilder(hash.Length * 2);

            //Format the hash as uppercase hexadecimal
            foreach (var b in hash)
            {
                stringBuilder.Append(b.ToString("X2"));
            }

            return stringBuilder.ToString();
        }
    }
}
