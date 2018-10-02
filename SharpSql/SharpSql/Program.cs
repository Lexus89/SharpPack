using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections;
using Microsoft.Win32;

namespace SharpSQL
{
    class Program
    {


        public static void WriteLogo()
        {
            Console.WriteLine("\r\n\r\n                          SSSSSSSSSSSSSSSSSSSSSS    LOGO                Ooof                               ");
            Console.WriteLine("                                     QQQQQQQQQQQQQQQQQQQQQQ          WOW                                                 ");
            Console.WriteLine("       SUCH ART?                         LLLLLLLLLLLLLLLLLLLLLL                    SHARP                       ");
            Console.WriteLine();
            Console.WriteLine();

        }

        public static void ExecSQL(SqlConnection sqlConnection, string query)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;
            cmd.CommandText = query;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.Connection = sqlConnection;
            sqlConnection.Open();

            reader = cmd.ExecuteReader();
            System.Console.WriteLine(reader.ToString());
            System.Data.DataTable schemaTable = reader.GetSchemaTable();

            ArrayList columns = new ArrayList();

            foreach (System.Data.DataRow row in schemaTable.Rows)
            {
                foreach (System.Data.DataColumn column in schemaTable.Columns)
                {
                    Console.WriteLine(String.Format("{0} = {1}",
                       column.ColumnName, row[column]));
                    if (column.ColumnName == "ColumnName")
                    {
                        columns.Add(row[column]);
                    }
                }
            }

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.WriteLine(String.Format("{0} : {1}", columns[i], reader.GetValue(i)));
                }
                Console.WriteLine();
            }
            sqlConnection.Close();
        }

        public static void PrintUsage()
        {
            Console.WriteLine("Usage: SharpSQL.exe <Command> -ConnectionString <string>");

        }

        public static List<string> EnumDsn()
        {
            List<string> list = new List<string>();
            list.AddRange(EnumDsn(Registry.CurrentUser));
            list.AddRange(EnumDsn(Registry.LocalMachine));
            return list;
        }

        public static IEnumerable<string> EnumDsn(RegistryKey rootKey)
        {
            RegistryKey regKey = rootKey.OpenSubKey(@"Software\ODBC\ODBC.INI\ODBC Data Sources");
            if (regKey != null)
            {
                foreach (string name in regKey.GetValueNames())
                {
                    string value = regKey.GetValue(name, "").ToString();
                    yield return name + ": " + value;
                }
            }
        }
        static void Main(string[] args)
        {
            WriteLogo();

            var watch = System.Diagnostics.Stopwatch.StartNew();
            SqlConnection sqlConnection = null;
            if (args.Length != 0)
            {
                string command = args[0];


                if (string.Equals(command, "ODBC", StringComparison.CurrentCultureIgnoreCase))
                {
                    List<string> dsns = EnumDsn();
                    foreach (string dsn in dsns)
                    {

                        System.Console.WriteLine(dsn);
                    }
                    return;

                }

                for (int i = 1; i < args.Length; i++)
                {
                    string argName = args[i];
                    if (string.Equals(argName, "-ConnectionString", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Console.WriteLine("Connecting to: " + args[(i + 1)]);

                        sqlConnection = new SqlConnection(args[(i + 1)]);

                    }
                }
                if (sqlConnection == null)
                {
                    PrintUsage();
                    return;
                }


                if (string.Equals(command, "Query", StringComparison.CurrentCultureIgnoreCase))
                {

                    string query = null;

                    for (int i = 1; i < args.Length; i++)
                    {
                        string argName = args[i];
                        if (string.Equals(argName, "-Query", StringComparison.CurrentCultureIgnoreCase))
                        {
                            query = args[(i + 1)];
                            ExecSQL(sqlConnection, query);
                        }
                    }
                    if (query == null)
                    {
                        PrintUsage();
                        return;
                    }
                }
                else if (string.Equals(command, "Check-DBAccess", StringComparison.CurrentCultureIgnoreCase))
                {
                    string query = "SELECT name FROM sys.sysdatabases WHERE HAS_DBACCESS(name) = 1;";
                    ExecSQL(sqlConnection, query);

                }
                else if (string.Equals(command, "Get-Databases", StringComparison.CurrentCultureIgnoreCase))
                {
                    string query = "SELECT* FROM sys.databases;";
                    ExecSQL(sqlConnection, query);

                }
                else if (string.Equals(command, "Get-Tables", StringComparison.CurrentCultureIgnoreCase))
                {
                    string query = "SELECT Distinct TABLE_NAME FROM information_schema.TABLES;";
                    ExecSQL(sqlConnection, query);

                }
                else if (string.Equals(command, "Check-CurrentDBPerms", StringComparison.CurrentCultureIgnoreCase))
                {
                    string query = "SELECT *   FROM fn_my_permissions(null, 'DATABASE');";
                    ExecSQL(sqlConnection, query);

                }
                else
                {
                    PrintUsage();
                    return;
                }

            }
            else
            {
                PrintUsage();
            }
        }
    }
}
