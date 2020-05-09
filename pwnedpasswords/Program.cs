//This Source Code Form is subject to the terms of the Mozilla Public
//License, v. 2.0. If a copy of the MPL was not distributed with this
//file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
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
            List<string> passwordList = new List<string>();

            #region Program Start options
#if DEBUG
            passwordList.Add("password");
#endif

#if RELEASE
            //If the user has redirected an input to stdin (by piping [|] or redirecting [<]), read each line
            if (Console.IsInputRedirected)
            {
                string rl;
                while ((rl = Console.ReadLine()) != null)
                {
                    passwordList.Add(rl);
                }
            }

            else if (args.Length != 0)
            {
                foreach (string s in args)
                {
                    passwordList.Add(s);
                }
            }

            else
            {
                Console.WriteLine("Please specify at least one password!" + Environment.NewLine + Environment.NewLine + "The following format must be used: pwnedpasswords [password1] [password2] [...]");
                Environment.Exit(-1);
            }
#endif
            #endregion

            List<string> hashList = new List<string>();
            List<string> queryList = new List<string>(); //The list of shortened hashes to send to the HIBP API

            //Hash every password and add them to a list
            foreach (string pw in passwordList)
            {
                hashList.Add(Hash(pw));
            }


            foreach (string hash in hashList)
            {
                queryList.Add(hash.Remove(5)); //Only keep the 5 first characters in the query
            }

            for (int i = 0; i < queryList.Count; i++)
            {
                string returnedHashes = string.Empty;
                try
                {
                    var response = await client.GetAsync($"https://api.pwnedpasswords.com/range/{queryList[i]}");

                    if (response.StatusCode == HttpStatusCode.OK)
                        returnedHashes = await response.Content.ReadAsStringAsync();

                    else
                        Console.WriteLine($"Request denied! Code: {response.StatusCode}");
                }

                catch (HttpRequestException exception)
                {
                    Console.WriteLine("Could not connect to remote:" + Environment.NewLine + Environment.NewLine + exception + Environment.NewLine + Environment.NewLine + "Press any key to quit");
                    Console.ReadKey();
                    Environment.Exit(-1);
                }


                string hashTruncated = hashList[i].Substring(5); //Remove the first 5 characters from the hash, but keep the rest

                if (returnedHashes.Contains(hashTruncated)) //Check if the response data contains our hash
                {
                    //Parse the amount of times this password has been "seen"
                    int start = returnedHashes.IndexOf(hashTruncated);
                    start += hashTruncated.Length + 1;                          //The start index of "amount of times seen" (+ 1 to remove ':')
                    int end = returnedHashes.IndexOf("\r\n", start);            //The end index of "amount of times seen"
                    int length = Math.Abs(start - end);                         //The amount of digits in the number


                    int.TryParse(returnedHashes.Substring(start, length), out int amount); //Substring() creates a string which is equal to the number "amount of times seen".
                                                                                           //This is parsed to an int so we can format it in the output.

                    //Clone the current culture's NumberFormatInfo and disable decimal digits, so that they won't appear in the output
                    NumberFormatInfo numberFormatInfo = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
                    numberFormatInfo.NumberDecimalDigits = 0;

                    Console.WriteLine($"This password has been pwned, and it has been seen {amount.ToString("N", numberFormatInfo)} times!" + Environment.NewLine + $"Pwned Hash: {hashList[i]}" + Environment.NewLine + $"Pwned Password: {passwordList[i]}" + Environment.NewLine);
                }

                else
                    Console.WriteLine("Phew — This password has not been pwned!");
            }
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
