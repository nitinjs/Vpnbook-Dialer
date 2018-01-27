using CsQuery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vpnbook_Dialer
{
    class Program
    {
        static void Main(string[] args)
        {
            // VPN adapters are stored in the rasphone.pdk
            // "C:\Users\Me\AppData\Roaming\Microsoft\Network\Connections\Pbk\rasphone.pbk"
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) +
                          @"\Microsoft\Network\Connections\Pbk\rasphone.pbk";

            const string pattern = @"\[(.*?)\]";
            var matches = Regex.Matches(System.IO.File.ReadAllText(path), pattern);

            Console.WriteLine("Getting vpnbook password...");
            var password = GetVpnbookPassword();
            Console.WriteLine("Password retrieved successfully..");
             
            int count = 1;
            int num = 1;
            if (matches.Count > 1)
            {
                Console.WriteLine("Select VPNBOOK VPN connection:");
                foreach (Match m in matches)
                    System.Console.WriteLine(count++ + ". " + m.Groups[1]);

                num = Convert.ToInt32(Console.ReadLine());
            }


            if (matches.Count == 1)
            {
                string connectionName = matches[num - 1].Groups[1].ToString();

                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "rasdial.exe",
                        Arguments = connectionName + " vpnbook " + password,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                proc.Start();
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    // do something with line
                    Console.WriteLine(line);
                }
                //System.Diagnostics.Process.Start("rasdial.exe", "VPNConnectionName VPNUsername VPNPassword");
            }
            else
            {
                Console.WriteLine("Press any key to exit..");
                Console.ReadLine();
            }
        }

        static string GetVpnbookPassword()
        {
            var scraper = new ScraperApp.ScraperWebClient(false);
            var strVpnbook = scraper.DownloadString("http://www.vpnbook.com/");
            CQ dom = strVpnbook;
            var elm = dom.Find("strong:contains('Password:')").First();
            var password = elm.Text().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).Last().Trim();

            return password;
        }
    }
}
