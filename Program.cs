using System;
using System.Configuration;
using System.IO;
using Microsoft.SqlServer.Management.Smo;

namespace Atd.RestoreHelper
{
    internal class Program
    {
        public Program()
        {
        }

        private static void Main(string[] args)
        {
            var theServer = new Server(ConfigurationManager.AppSettings["Server"]);
            theServer.ConnectionContext.LoginSecure = false;
            theServer.ConnectionContext.Login = ConfigurationManager.AppSettings["Username"];
            theServer.ConnectionContext.Password = ConfigurationManager.AppSettings["Password"];
            var myDb = theServer.Databases[ConfigurationManager.AppSettings["Database"]];

            System.IO.Directory.CreateDirectory(DateTime.Today.ToString("yyyyMMdd"));

            ScriptStoredProcedures(myDb);
            ScriptViews(myDb);
            ScriptTables(myDb);
            ScriptUserDefinedFunctions(myDb);
        }

        private static void ScriptItem(string name, IScriptable item)
        {
            using (var sw = new StreamWriter(string.Format("{0}/{1}.sql", DateTime.Today.ToString("yyyyMMdd"), name)))
            {

                var so = new ScriptingOptions { ScriptDrops = true, IncludeIfNotExists = true };
                foreach (var s in item.Script(so))
                {
                    sw.WriteLine(s);
                }
                sw.WriteLine();
                sw.WriteLine("GO");
                sw.WriteLine();
                foreach (var s in item.Script())
                {
                    sw.WriteLine(s);
                }
            }
        }

        private static void ScriptStoredProcedures(Database myDB)
        {
            foreach (StoredProcedure sp in myDB.StoredProcedures)
            {
                if (sp.IsSystemObject || !sp.Name.ToLower().StartsWith(ConfigurationManager.AppSettings["StartsWith"].ToLower()))
                {
                    continue;
                }

                ScriptItem(sp.Name, sp);
            }
        }

        private static void ScriptTables(Database myDB)
        {


            foreach (Table tb in myDB.Tables)
            {
                if (tb.IsSystemObject || !tb.Name.ToLower().StartsWith(ConfigurationManager.AppSettings["StartsWith"].ToLower()))
                {
                    continue;
                }

                ScriptItem(tb.Name, tb);
                ScriptTriggers(tb);
            }
        }

        private static void ScriptTriggers(Table tb)
        {
            foreach (Trigger tr in tb.Triggers)
            {
                if (tr.IsSystemObject || !tr.Name.ToLower().StartsWith(ConfigurationManager.AppSettings["StartsWith"].ToLower()))
                {
                    continue;
                }

                ScriptItem(tr.Name, tr);
            }
        }

        private static void ScriptUserDefinedFunctions(Database myDB)
        {
            foreach (UserDefinedFunction fn in myDB.UserDefinedFunctions)
            {
                if (fn.IsSystemObject || !fn.Name.ToLower().StartsWith(ConfigurationManager.AppSettings["StartsWith"].ToLower()))
                {
                    continue;
                }

                ScriptItem(fn.Name, fn);
            }
        }

        private static void ScriptViews(Database myDB)
        {
            foreach (View vw in myDB.Views)
            {
                if (vw.IsSystemObject || !vw.Name.ToLower().StartsWith(ConfigurationManager.AppSettings["StartsWith"].ToLower()))
                {
                    continue;
                }

                ScriptItem(vw.Name, vw);
            }
        }
    }
}