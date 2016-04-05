//------------------------------------------------------------------------------
//----- S3Bucket ---------------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jon Baier USGS Wisconsin Internet Mapping
//              Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles File upload methods to AWS S3 bucket.
//
//discussion:   
//              Upload  http://docs.aws.amazon.com/AmazonS3/latest/dev/UploadObjSingleOpNET.html
//              Getting http://docs.aws.amazon.com/AmazonS3/latest/dev/RetrievingObjectUsingNetSDK.html

#region Comments
//03.25.12 - JB - Created
//06.19.2014 jkn - finally starting to implement
//06.27.2014 jkn - added additional stream copying methods to make s3 get method to work
#endregion

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Text;
using System.Configuration;

namespace STNServices2.Utilities
{
    public class S3Bucket
    {
        #region "Properties"
        public String BucketName { get; set; }
        private IAmazonS3 s3Client { get; set; }
        #endregion
        #region "Constructor and IDisposable Support"
        #region Constructor
        public S3Bucket(String aBucketName)
        {
            BucketName = aBucketName;
            s3Client = AWSClientFactory.CreateAmazonS3Client(ConfigurationManager.AppSettings["AWSAccessKey"],
                                                                ConfigurationManager.AppSettings["AWSSecretKey"], Amazon.RegionEndpoint.USEast1
                                                                );
        }
        #endregion
        #region IDisposable Support
        // Track whether Dispose has been called.
        private bool disposed = false;

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        } //End Dispose

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                if (disposing)
                {
                    // TODO:Dispose managed resources here.
                    //ie component.Dispose();
                    s3Client.Dispose();

                }//EndIF

                // TODO:Call the appropriate methods to clean up
                // unmanaged resources here.
                //ComRelease(Extent);
                BucketName = string.Empty;

                // Note disposing has been done.
                disposed = true;


            }//EndIf
        }//End Dispose
        #endregion
        #endregion
        #region Methods
        //Upload Object
        public void PutObject(String ObjectName, Stream aStream)
        {
            try
            {
                string bucket = Path.GetDirectoryName(ObjectName);
                // simple object put
                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = BucketName,
                    Key = ObjectName,
                    InputStream = aStream
                };

                PutObjectResponse response1 = s3Client.PutObject(request);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                S3Exception(amazonS3Exception);
            }
        }//end put

        //Download Object
        public Stream GetObject(String ObjectName)
        {
            //http://theburningmonk.com/2011/06/s3-use-using-block-to-get-the-stream/
            //Because the stream is com­ing from the Ama­zon S3 ser­vice and is fed  
            //in chunks, the code needs to ensure that the con­nec­tion to S3 stays 
            //open until all the data has been received.
            try
            {
                GetObjectRequest request = new GetObjectRequest()
                {
                    BucketName = BucketName,
                    Key = ObjectName
                };

                using (GetObjectResponse response = s3Client.GetObject(request))
                {
                    //Loads the entire stream before returning
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        var data = ReadStream(response.ResponseStream);

                        return new MemoryStream(data);
                    }

                    throw new AmazonS3Exception(response.HttpStatusCode.ToString());
                    //writes stream to disk for testing purposes
                    //response.WriteResponseStreamToFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.test"));

                }//end using
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                S3Exception(amazonS3Exception);
                return null;
            }

        }//end GetObject

        //Delete Object
        public Boolean DeleteObject(String ObjectName)
        {
            try
            {
                DeleteObjectRequest request = new DeleteObjectRequest()
                {
                    BucketName = BucketName,
                    Key = ObjectName
                };

                DeleteObjectResponse response = s3Client.DeleteObject(request);

                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                S3Exception(amazonS3Exception);
                return false;
            }
        }//end DeleteObjects

        //List Objects in Bucket
        public List<S3Object> ListObjects(String prefix)
        {
            try
            {
                ListObjectsRequest request = new ListObjectsRequest();
                request.BucketName = BucketName;
                //Only files that start with 'prefix'
                if (prefix != null)
                    request.Prefix = prefix;
                ListObjectsResponse response = s3Client.ListObjects(request);

                return response.S3Objects;

            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                S3Exception(amazonS3Exception);
            }

            return new List<S3Object>();
        }//end ListObjects
        #endregion
        #region Helper Methods
        private void S3Exception(AmazonS3Exception amazonS3Exception)
        {
            if (amazonS3Exception.ErrorCode != null &&
                (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
            {
                Console.WriteLine("Please check the provided AWS Credentials.");
            }
            else
            {
                Console.WriteLine("An error occurred with the message '{0}'", amazonS3Exception.Message);
            }
        }
        private byte[] ReadStream(Stream stream, int initialLength = -1)
        {
            byte[] buffer;
            int read = 0;
            int chunk = 0;
            try
            {
                //initialize variables
                if (initialLength < 1) initialLength = 32768;
                buffer = new byte[initialLength];
                //loop through the stream and copy contents
                while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
                {
                    read += chunk;
                    if (read == buffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        //Check if Done and return
                        if (nextByte == -1) return buffer;

                        byte[] newBuffer = new byte[buffer.Length * 2];
                        Array.Copy(buffer, newBuffer, buffer.Length);
                        newBuffer[read] = (byte)nextByte;
                        buffer = newBuffer;
                        read++;
                    }//end if                
                }//next

                return resizeByte(buffer, read);
            }
            catch (Exception ex)
            {
                throw;
            }
        }//end ReadStream
        private byte[] resizeByte(byte[] buffer, int size)
        {
            byte[] resizedByte = new byte[size];
            Array.Copy(buffer, resizedByte, size);
            return resizedByte;
        }//end resizeByte
        #endregion

    }//end class
}//end namespace