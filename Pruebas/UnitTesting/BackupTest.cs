using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;

using U5kBaseDatos;

namespace UnitTesting
{
    class Cfg
    {
        public string MySqlServer { get; set; }
        public string BdtSchema { get; set; }
        public string MySqlUser { get; set; }
        public string MySqlPwd { get; set; }

    }
    [TestClass]
    public class BackupTest
    {
        Cfg cfg = new Cfg()
        {
            MySqlServer = "192.168.0.212",
            BdtSchema = "cd40_new",
            MySqlUser = "root",
            MySqlPwd = "cd40"
        };

        [TestMethod]
        public void TestMethod1()
        {
            List<U5kiDbHelper.BackupInciItem> items =
                U5kiDbHelper.Backup(cfg.MySqlServer, cfg.BdtSchema, cfg.MySqlUser, cfg.MySqlPwd, "5.6");
        }

        [TestMethod]
        public void BkpTest01()
        {
            //string cmd = "c:\\Program Files (x86)\\Roxio\\Backgrnd\\mysqldump.exe";
            string dumpCmd = string.Format("\"c:\\Program Files (x86)\\Roxio\\Backgrnd\\mysqldump.exe\" -u{0} -p{1} -h{2} --database new_cd40_trans", cfg.MySqlUser, cfg.MySqlPwd, cfg.MySqlServer, cfg.BdtSchema);

            string cmd = "cmd.exe";
            string Arguments = "/c " + dumpCmd + " > kkk.txt";

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = cmd,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                Arguments = Arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process process = Process.Start(psi);

            
            //StreamReader outputReader = process.StandardOutput;
            //StreamReader errorReader = process.StandardError;
            //process.WaitForExit();
            /*string output = */process.StandardOutput.ReadToEnd();
            string strerror = process.StandardError.ReadToEnd();
        }

        [TestMethod]
        public void BkpTest02()
        {
            List<U5kiDbHelper.BackupInciItem> items =
                U5kiDbHelper.NewBackup(cfg.MySqlServer, cfg.BdtSchema, cfg.MySqlUser, cfg.MySqlPwd, "5.6");
        }
    }
}
