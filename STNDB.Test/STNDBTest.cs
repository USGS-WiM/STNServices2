using System;
using STNDB;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Data.Entity;
using System.Configuration;

namespace STNDB.Test
{
    [TestClass]
    public class STNDBTest
    {
        private string connectionString = String.Format(ConfigurationManager.AppSettings["connectionString"], ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);
        
        [TestMethod]
        public void STNDBConnectionTest()
        {
            using (STNDBEntities context = new STNDBEntities(connectionString))
            {
                DbConnection conn = context.Database.Connection;
                try
                {
                    if (!context.Database.Exists()) throw new Exception("db does not exist");
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
            using (STNDBEntities context = new STNDBEntities(connectionString))
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

        [TestMethod]
        public void STNDBUpdateHWMsTest()
        {
            using (STNDBEntities context = new STNDBEntities(connectionString))
            {
                try
                {
                    var query = context.sites.Include(s => s.hwms).ToList();

                    foreach (var s in query)
                    {
                        var i = 0;
                        foreach (var h in s.hwms)
                        {
                            i++;
                            //update label
                            h.hwm_label = "hwm-" + i.ToString();
                        }
                        context.SaveChanges();
                    }
                    

                   // Assert.IsNotNull(query, testQuery.Count.ToString());
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(false, ex.Message);
                }

            }
        }//end NSSDBConnectionTest
    }
}
