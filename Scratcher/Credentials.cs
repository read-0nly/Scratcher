using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Diagnostics;

namespace Scratcher
{
    /// <summary>
    /// Class that deals with another username credentials
    /// </summary>
    public class Credentials
    {
        /// <summary>
        /// Constructor of SecureString password, to be used by RunAs
        /// </summary>
        /// <param name="text">Plain password</param>
        /// <returns>SecureString password</returns>
        private SecureString Secure = null;
        public string username = "";
        public string domain = "";
        public Credentials()
        {
            Form2 input = new Form2(this);
            input.ShowDialog();
        }
        public void MakeSecureString(string text)
        {

            SecureString secure = new SecureString();
            foreach (char c in text)
            {
                secure.AppendChar(c);
            }

            Secure=secure;
        }

        /// <summary>
        /// Run an application under another user credentials.
        /// Working directory set to C:\Windows\System32
        /// </summary>
        /// <param name="path">Full path to the executable file</param>
        /// <param name="username">Username of desired credentials</param>
        /// <param name="password">Password of desired credentials</param>
        public void RunAs(string path, string args)
        {
            try
            {
                ProcessStartInfo myProcess = new ProcessStartInfo(path);
                myProcess.Domain = domain;
                myProcess.UserName = username;
                myProcess.Password = Secure;
                myProcess.WorkingDirectory = (System.IO.Directory.GetParent(path).FullName);
                myProcess.WindowStyle = ProcessWindowStyle.Normal;
                myProcess.UseShellExecute = false;
                myProcess.Arguments = args;
                myProcess.LoadUserProfile = true;
                Process p = Process.Start(myProcess);
            }
            catch (Exception w32E)
            {
                // The process didn't start.
                Console.WriteLine(w32E);
            }
        }
        public List<string> RunAs(string path, string args, bool getOutput)
        {
            try
            {
                ProcessStartInfo myProcess = new ProcessStartInfo(path);
                myProcess.Domain = domain;
                myProcess.UserName = username;
                myProcess.Password = Secure;
                myProcess.WorkingDirectory = (System.IO.Directory.GetParent(path).FullName);
                myProcess.WindowStyle = ProcessWindowStyle.Normal;
                myProcess.UseShellExecute = false;
                myProcess.Arguments = args;
                myProcess.LoadUserProfile = true;
                myProcess.RedirectStandardOutput = true;
                Process p = Process.Start(myProcess);
                if (getOutput)
                {
                    List<string> output = new List<String>();
                    while (!p.StandardOutput.EndOfStream)
                    {
                        output.Add(p.StandardOutput.ReadLine());
                    }
                    return output;
                }
                else
                {
                    List<string> output = new List<String>();
                    output.Add("Success");
                    return output;
                }
            }
            catch (Exception w32E)
            {
                List<string> output = new List<String>();
                output.Add("Error occured");
                output.Add(w32E.ToString());
                return output;
            }
        }

    }
}
