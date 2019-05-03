using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Honeywords.API.Models;
using System.Net.Http;

namespace Honeywords.API.Data
{
    public class HoneywordRepository : IHoneywordRepository
    {
        public async void GenerateHoneyWords(DataContext context, User user, int count, string password)
        {
            Random ran = new Random();

            List<string> passwords = new List<string>();

            passwords.Add(password);
             for (int i = 0; i < count; i++)
            {
                char[] chars = password.ToCharArray();
                for (int j = 0; j < chars.Length; j++)
                {
                    if (Char.IsDigit(chars[j]))
                    {
                        chars[j] = ran.Next(0, 9).ToString().ToCharArray()[0];
                    }
                }
                passwords.Add(new String(chars));
            }
            passwords.Shuffle();
            string mypwdHash="";

            foreach (var pw in passwords)
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(pw, out passwordHash, out passwordSalt);
                Console.WriteLine("pw: "+pw+" password: "+password);
                
                await context.Passwords.AddAsync(new Password {
                   PasswordHash = passwordHash,
                   PasswordSalt = passwordSalt,
                   UserId = user.Id 
                });
                if(pw == password) {
                    //mypwdHash = System.Text.Encoding.UTF8.GetString(passwordHash);
                    mypwdHash = BitConverter.ToString(passwordHash);
                    mypwdHash = mypwdHash.Replace("-","");
                }
            }
            HttpClient webclient = new HttpClient();
            // string sUrl = "http://192.168.102.48/zkweb/honeychecker/honey_update.php?username="+user.Username+"&userhash="+mypwdHash;
            string sUrl = "http://www2.cs.siu.edu/~zwong/honey_update.php?username="+user.Username+"&userhash="+mypwdHash;
            Console.WriteLine(sUrl);
            try
            {
                HttpResponseMessage resp = await webclient.GetAsync(sUrl);
                resp.EnsureSuccessStatusCode();
                string responseBody = await resp.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }
 
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
    public static class CustomExtensions
        {
            public static void Shuffle<T>(this IList<T> list)
            {
                RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
                int n = list.Count;
                while (n > 1)
                {
                    byte[] box = new byte[1];
                    do provider.GetBytes(box);
                    while (!(box[0] < n * (Byte.MaxValue / n)));
                    int k = (box[0] % n);
                    n--;
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }

        }
}