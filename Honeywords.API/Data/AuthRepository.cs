using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Honeywords.API.Models;
using System.Net.Http;

namespace Honeywords.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext context;
        private readonly IHoneywordRepository honeypot;
        public AuthRepository(DataContext context, IHoneywordRepository honeypot)
        {
            this.context = context;
            this.honeypot = honeypot;
        }

        public async Task<User> Login(string username, string password)
        {
            var user = await this.context.Users.Include(p => p.Passwords).FirstOrDefaultAsync(x => x.Username == username);
            if (user == null) {
                return null;
            }
            // if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt)) {
            //     return null;
            // }
            var verify_flag = await VerifyPasswordHash(user, password);
            if (!verify_flag)
            {
                return null;
            }
            return user;
        }

        // private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        // {
        //     using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
        //     {
        //         var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        //         for (int i = 0; i < computedHash.Length; i++) {
        //             if (computedHash[i] != passwordHash[i]) {
        //                 return false;
        //             }
        //         }
        //         return true;
        //     }
        // }
        private async Task<bool> VerifyPasswordHash(User user, string password)
        {
            var passwords = user.Passwords;
            foreach (var pw in passwords)
            {
                bool found = false;
                using (var hmac = new System.Security.Cryptography.HMACSHA512(pw.PasswordSalt))
                {
                    var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                    for (int i = 0; i < computedHash.Length; i++) {
                        if (computedHash[i] != pw.PasswordHash[i])
                        {
                            found = false;
                            break;
                        } else {
                            string mypwdHash = BitConverter.ToString(computedHash);
                            mypwdHash = mypwdHash.Replace("-","");
                            
                            HttpClient webclient = new HttpClient();
                            // string sUrl = "http://192.168.102.48/zkweb/honeychecker/honey_verify.php?username="+user.Username+"&userhash="+mypwdHash;
                            string sUrl = "http://www2.cs.siu.edu/~zwong/honey_verify.php?username="+user.Username+"&userhash="+mypwdHash;
                            Console.WriteLine(sUrl);
                            try
                            {
                                HttpResponseMessage resp = await webclient.GetAsync(sUrl);
                                resp.EnsureSuccessStatusCode();
                                string responseBody = await resp.Content.ReadAsStringAsync();
                                Console.WriteLine(responseBody);
                                if(responseBody=="{\"honeyword\":\"1\"}"){
                                    Console.WriteLine("it's a real passwd");
                                } else {
                                    user.Weather = 998;
                                    sendnotification();
                                    Console.WriteLine("it's a fake passwd");
                                }
                            }
                            catch (System.Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                {
                    return true;
                }

            }
            return false;
        }

        public async Task<User> Register(User user, string password)
        {
            // byte[] passwordHash, passwordSalt;
            // CreatePasswordHash(password, out passwordHash, out passwordSalt);
            // user.PasswordHash = passwordHash;
            // user.PasswordSalt = passwordSalt;

            await this.context.Users.AddAsync(user);
            Console.WriteLine("user: "+user.Username+" passwd: "+password+" userid: "+user.Id);
            this.honeypot.GenerateHoneyWords(this.context, user, 10, password);
            
            await this.context.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if (await this.context.Users.AnyAsync(x => x.Username == username)) {
                return true;
            }
            return false;
        }
        public void sendnotification(){
            System.Diagnostics.Process runprogram = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo programinfo = new System.Diagnostics.ProcessStartInfo();
            programinfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            programinfo.CreateNoWindow = true;
            programinfo.UseShellExecute = false;
            programinfo.RedirectStandardOutput = true;
            programinfo.FileName = "cmd.exe";
            programinfo.Arguments = " /C \"C:\\app\\sm1.cmd\"";
            runprogram.StartInfo = programinfo;
            runprogram.Start();

        }
    }
}