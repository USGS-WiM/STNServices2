//------------------------------------------------------------------------------
//----- Storage ----------------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   
//           
//discussion: 
//

#region "Comments"
//mm.dd.yyyy jkn - Created
#endregion

#region "Imports"
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Web;
#endregion
namespace STNServices2.Utilities
{
    public class Storage
    {
        #region "Properties"
        public string StorageName { get; set; }
        #endregion
        #region "Constructor and IDisposable Support"
        public Storage(string aStorageName)
        {
            StorageName = aStorageName + "Storage";
        }//end Storage
        #region "IDisposable Support"
        #endregion
        #endregion
        #region "Methods"
        public void PutObject(String ObjectName, Stream aStream)
        {
            string directory = Path.Combine(StorageName, Path.GetDirectoryName(ObjectName));
            try
            {
                if (!Directory.Exists(Path.Combine(directory)))
                    Directory.CreateDirectory(directory);

                using (var fileStream = File.Create(Path.Combine(StorageName, ObjectName)))
                {
                    //reset stream position to 0 prior to copying to filestream;
                    aStream.Position = 0;
                    aStream.CopyTo(fileStream);
                }//end using

            }
            catch (Exception)
            {

            }
        }


        //Download Object
        public Stream GetObject(String ObjectName)
        {
            string objfile = Path.Combine(StorageName, ObjectName);
            try
            {
                return File.OpenRead(objfile);
            }
            catch (Exception)
            {
                return null;
            }

        }


        //Delete Object
        public Boolean DeleteObject(String ObjectName)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {
                return false;
            }
        }


        //List Objects in Bucket

        #endregion
        #region "Helper Methods"

        #endregion
    }//end class Storage
}//end namespace

