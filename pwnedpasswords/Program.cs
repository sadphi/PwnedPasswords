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
        static readonly string nl = Environment.NewLine;
        static readonly HttpClient client = new HttpClient();
        static Dictionary<string, string> hashes = new Dictionary<string, string>(); //hash -> password (not hashed)
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
                Console.WriteLine("Please specify at least one password!" + nl + nl + "The following format must be used: pwnedpasswords [password1] [password2] [...]");
                Environment.Exit(-1);
            }
#endif
            #endregion

            List<string> queryList = new List<string>(); //The list of shortened hashes to send to the HIBP API

            //Hash every password and add them to a list
            foreach (string pw in passwordList)
            {
                hashes.TryAdd(Hash(pw), pw);
            }

            var respone = QueryHIBP(new List<string>(hashes.Keys)).Result;
            PrintResult(IsPwned(respone));
        }

        /// <summary>
        /// Print all pwned passwords.
        /// </summary>
        /// <param name="pwnedPasswords">The Dictionary containing pwned passwords. (hash -> amount)</param>
        static void PrintResult(Dictionary<string, int> pwnedPasswords)
        {
            if (pwnedPasswords.Count != 0)
            {
                //Clone the current culture's NumberFormatInfo and disable decimal digits, so that they won't appear in the output
                NumberFormatInfo numberFormatInfo = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
                numberFormatInfo.NumberDecimalDigits = 0;

                Console.WriteLine($"Oh no. These passwords have been pwned:{nl}");

                foreach (var p in pwnedPasswords)
                {
                    Console.WriteLine($"{hashes[p.Key]} ({p.Key}), which has been seen {p.Value.ToString("N", numberFormatInfo)} times.");
                }
            }

            else
                Console.WriteLine("Phew — No password has been pwned!");
        }

        /// <summary>
        /// Check if some hashes are compromised, and return those that are and the amount of times they have been seen.
        /// </summary>
        /// <param name="response">The result of method QueryHIBP, containing (hash -> response).</param>
        /// <returns>A Dictionary with pairs of (hash -> amount), where 'hash' is the hashed password, and 'amount' is the amount of times the password has been seen by HIBP.</returns>
        static Dictionary<string, int> IsPwned(Dictionary<string, string> response)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (var p in response) //Each pair is hash -> response of all matching hashes from HIBP
            {
                string hashTruncated = p.Key.Substring(5); //Remove the first 5 characters from the hash, but keep the rest
                string searchResult = p.Value;

                if (searchResult.Contains(hashTruncated)) //Check if the response data contains our hash
                {
                    //Parse the amount of times this password has been "seen"
                    int start = searchResult.IndexOf(hashTruncated);
                    start += hashTruncated.Length + 1;                               //The start index of "amount of times seen" (+ 1 to remove ':')
                    int end = searchResult.IndexOf("\r\n", start);                   //The end index of "amount of times seen"
                    int length = Math.Abs(start - end);                              //The amount of digits in the number


                    int.TryParse(searchResult.Substring(start, length), out int amount);        //Substring() creates a string which is equal to the number "amount of times seen".
                                                                                                //This is parsed to an int so we can format it in the output.
                    result.Add(p.Key, amount);
                }
            }
            return result;
        }

        /// <summary>
        /// Queries the HIBP passwords API to find a match to each hash defined in the parameter.
        /// </summary>
        /// <param name="hashes">A List containing SHA-1 hashes of passwords, where each element will be checked against HIBP.</param>
        /// <returns>A Dictionary with pairs of (hash -> response), where 'hash' is the hashed password, and 'response' is the respone from HIBP (a string of matching hashes).</returns>
        static async Task<Dictionary<string, string>> QueryHIBP(List<string> hashes)
        {
            List<string> trimmedHashes = new List<string>();

            foreach (string hash in hashes)
            {
                trimmedHashes.Add(hash.Remove(5));
            }

            Dictionary<string, string> result = new Dictionary<string, string>();

            for (int i = 0; i < trimmedHashes.Count; i++)
            {
                try
                {
                    var response = await client.GetAsync($"https://api.pwnedpasswords.com/range/{trimmedHashes[i]}");

                    if (response.StatusCode == HttpStatusCode.OK)
                        result.Add(hashes[i], await response.Content.ReadAsStringAsync());

                    else
                        Console.WriteLine($"Request denied! Code: {response.StatusCode}");
                }

                catch (HttpRequestException exception)
                {
                    Console.WriteLine("Could not connect to remote:" + nl + nl + exception + nl + nl + "Press any key to quit");
                    Console.ReadKey();
                    Environment.Exit(-1);
                }
            }

            return result;
        }

        /// <summary>
        /// Generate the hash of a string using SHA-1
        /// </summary>
        /// <param name="data">The string to hash</param>
        /// <returns>The SHA-1 hash of the argument</returns>
        static string Hash(string data)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();

            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(data));

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
