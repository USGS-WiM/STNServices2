#region Comments
// 08.23.12 - JB - Fixed Case sensitivity bug
// 07.02.12 - JKN - Implemented Authorization Roles
// 05.24.12 - JB - Made member table check case insensitive
// 02.17.12 - JB - Added Base64 decode method for Basic Auth
// 01.24.12 - JB - Created
#endregion
#region Copywright
/* Authors:
 *      Jonathan Baier (jbaier@usgs.gov)
 * Copyright:
 *      2012 USGS - WiM
 */
#endregion

using System.Linq;
using System.Data.Entity;


using OpenRasta.Security;
using WiM.Authentication;
using STNDB;
using STNServices2.Utilities.ServiceAgent;


namespace STNServices2.Security
{
    public class STNAuthenticationProvider : IAuthenticationProvider
    {
        public Credentials GetByUsername(string username)
        {
            using (EasySecureString securedPassword = new EasySecureString("Ij7E9doC"))
            {
                using (STNAgent sa = new STNAgent("fradmin", securedPassword))
                {
                    member user = sa.Select<member>().Include(r=>r.role).FirstOrDefault(u=>string.Equals(u.username.ToUpper(), username.ToUpper()));
                    if (user == null) return (null);
                    return (new Credentials()
                    {  
                        Username = user.username,
                        Password = "Ij7E9doC",
                        Roles = new string[] { user.role.role_name }
                    });
                }//end using
            }//end using
        }

        public bool ValidatePassword(Credentials credentials, string suppliedPassword)
        {
            if (credentials == null) return (false);
            return (credentials.Password == suppliedPassword);
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