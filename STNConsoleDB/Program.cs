using System;
using STNDB;
using System.Data.Common;
using System.Linq;

namespace STNConsoleDB
{
    class Program
    {
        
        static void Main(string[] args)
        {
            string connectionString = "metadata=res://*/STNEntities.csdl|res://*/STNEntities.ssdl|res://*/STNEntities.msl;provider=Npgsql;provider connection string=';Database=STNNEW;Host=stnnew.ck2zppz9pgsw.us-east-1.rds.amazonaws.com;Username=fradmin;PASSWORD={0};Application Name=STN';";
            string password = "Ij7E9doC";
            using (STNDBEntities context = new STNDBEntities(string.Format(connectionString, password)))
            {
                Console.WriteLine("insert using");

                DbConnection conn = context.Database.Connection;
                Console.WriteLine("connected");
                try
                {
                    if (conn == null) Console.WriteLine("conn is null");
                    else Console.WriteLine("conn is not null");
                    if (context == null) Console.WriteLine("context is null");
                    else Console.WriteLine("context is not null");
                    if (context.Database == null) Console.WriteLine("context database is null");
                    else Console.WriteLine("context database is not null");

                    if (!context.Database.Exists()) throw new Exception("db does ont exist");
                    Console.WriteLine("trying to open");
                    conn.Open();
                    Console.WriteLine("connection connected");

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection Error: " + ex.Message + " inner msg: " + ex.InnerException.Message);
                    Console.WriteLine("Connection StackTrace:");
                    Console.WriteLine(ex.StackTrace);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }
            }//end using

            using (STNDBEntities context = new STNDBEntities(string.Format(connectionString, password)))
            {
                try
                {
                    var testQuery = context.instruments.ToList();
                    Console.WriteLine("No of HWMs " + testQuery.Count().ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("query Error: " + ex.Message);
                    if (ex.InnerException != null) Console.WriteLine("Inner ex Error: " + ex.InnerException.Message);
                    Console.WriteLine("query StackTrace:");
                    Console.WriteLine(ex.StackTrace);
                }

            }//end using

            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
        }//end

    }
}
