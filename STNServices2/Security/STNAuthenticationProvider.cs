
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//              
//  
//   purpose:  validate user and password for required authentication. 
//
//discussion:   
//
//     

#region Comments
// 03.23.16 - JKN - created
#endregion

using System;
using System.Linq;
using System.Data.Entity;

using OpenRasta.Security;
using STNServices2.Security;
using WiM.Security;
using STNDB;
using STNServices2.Utilities.ServiceAgent;


namespace STNServices2.Security
{
    public class STNAuthenticationProvider : IAuthenticationProvider
    {
        public Credentials GetByUsername(string username)
        {
            using (EasySecureString securedPassword = new EasySecureString("***REMOVED***"))
            {
                using (STNAgent sa = new STNAgent("fradmin", securedPassword))
                {
                    member user = sa.Select<member>().Include(r=>r.role).AsEnumerable().FirstOrDefault(u=>string.Equals(u.username, username,StringComparison.OrdinalIgnoreCase));
                    if (user == null) return (null);
                    return (new WiMCredentials()
                    {  
                        Username = user.username, 
                        salt = user.salt,
                        Password = user.password,
                        Roles = new string[] { user.role.role_name }
                    });
                }//end using
            }//end using
        }

        public bool ValidatePassword(Credentials credentials, string suppliedPassword)
        {
            if (credentials == null) return (false);
            WiMCredentials creds = (WiMCredentials)credentials;
            bool authenticated =  Cryptography.VerifyPassword(suppliedPassword, creds.salt, creds.Password);
            return authenticated;
        }
    }//end Class STNBAsicAuthentication

    public enum RoleType
    {
        e_Public = 1,
        e_Field = 2,
        e_Manager = 3,
        e_Administrator = 4
    }
}//end namespace