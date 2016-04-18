using System;
using STNDB;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace STNDB.Test
{
    [TestClass]
    public class STNDBTest
    {
        private string connectionString = "metadata=res://*/STNEntities.csdl|res://*/STNEntities.ssdl|res://*/STNEntities.msl;provider=Npgsql;provider connection string=';Database=STNNEW;Host=stnnew.ck2zppz9pgsw.us-east-1.rds.amazonaws.com;Username=fradmin;PASSWORD={0};Application Name=STN';";
        private string password = "***REMOVED***";

        [TestMethod]
        public void STNDBConnectionTest()
        {
            using (STNDBEntities context = new STNDBEntities(string.Format(connectionString, password)))
            {
                DbConnection conn = context.Database.Connection;
                try
                {
                    if (!context.Database.Exists()) throw new Exception("db does ont exist");
                    conn.Open();
                    Assert.IsTrue(true);

                }
                catch (Exception ex)
                {
                    Assert.IsTrue(false, ex.Message);
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open) conn.Close();
                }

            }
        }//end NSSDBConnectionTest

        [TestMethod]
        public void STNDBQueryTest()
        {
            using (STNDBEntities context = new STNDBEntities(string.Format(connectionString, password)))
            {
                try
                {
                    var testQuery = context.instruments.ToList();
                    Assert.IsNotNull(testQuery, testQuery.Count.ToString());
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(false, ex.Message);
                }

            }
        }//end NSSDBConnectionTest
    }
}
