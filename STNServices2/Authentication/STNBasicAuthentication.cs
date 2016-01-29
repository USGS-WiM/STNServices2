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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Configuration;


using OpenRasta.Authentication;
using OpenRasta.Authentication.Basic;

namespace STNServices2.Authentication
{
    public class STNBasicAuthentication : IBasicAuthenticator
    {
        #region Properties

        public string Realm
        {
            get { return "STN-Secured-Domain-Realm"; }
        }//end Relm
        #endregion

        #region Methods
        public AuthenticationResult Authenticate(BasicAuthRequestHeader header)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["STNEntities"].ConnectionString;
            try
            {
                using (STNEntities2 aSTNE = new STNEntities2(string.Format(connectionString, header.Username, header.Password)))
                {
                    List<String> memberRoles = new List<string>();
                    List<MEMBER> MemberList = aSTNE.MEMBERS.AsEnumerable()
                                    .Where(members => members.USERNAME.ToUpper() == header.Username.ToUpper()).ToList();

                    MemberList.ForEach(m => memberRoles.Add(m.ROLE.ROLE_NAME));


                    if (MemberList.Any())
                    {
                        return new AuthenticationResult.Success(header.Username, memberRoles.ToArray());
                    }

                    else
                    {
                        return new AuthenticationResult.Failed();
                    }
                }//end using

            }
            catch (EntityException)
            {
                return new AuthenticationResult.Failed();
            }
        }//end Authenticate

        //Add public method for decoding base 64 later
        public static BasicAuthRequestHeader ExtractBasicHeader(string value)
        {
            try
            {
                var basicBase64Credentials = value.Split(' ')[1];

                var basicCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(basicBase64Credentials)).Split(':');

                if (basicCredentials.Length != 2)
                    return null;

                return new BasicAuthRequestHeader(basicCredentials[0], basicCredentials[1]);
            }
            catch
            {
                return null;
            }

        }//end ExtractBasicHeader
        #endregion

    }//end Class STNBAsicAuthentication

    public enum RoleType
    {
        e_Public = 1,
        e_Field = 2,
        e_Manager = 3,
        e_Administrator = 4
    }
}//end namespace