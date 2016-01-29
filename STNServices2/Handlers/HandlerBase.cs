using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Data;
using System.Data.EntityClient;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;
using System.Configuration;

using OpenRasta.Web;
using OpenRasta.Security;

using STNServices2.Authentication;



namespace STNServices2.Handlers
{
    public abstract class HandlerBase
    {
        #region Constants
        protected const string AdminRole = "Admin";
        protected const string ManagerRole = "Manager";
        protected const string FieldRole = "Field";
        protected const string CitizenManagerRole = "CitizenManager";
        protected const string PublicRole = "Public";

        #endregion

        #region "Base Properties"
        //protected String connectionString = @"metadata=res://*/STNModel.csdl|res://*/STNModel.ssdl|res://*/STNModel.msl;provider=Oracle.DataAccess.Client;provider connection string=""DATA SOURCE=STNRDS;USER ID={0};PASSWORD={1}""";
        protected String connectionString = ConfigurationManager.ConnectionStrings["STNEntities"].ConnectionString;

        // will be automatically injected by DI in OpenRasta
        public ICommunicationContext Context { get; set; }

        public string username
        {
            get { return Context.User.Identity.Name; }
        }

        public abstract string entityName { get; }
        #endregion
        #region Base Routed Methods

        protected List<T> GetList<T>() where T : HypermediaEntity
        {
            try
            {
                List<T> entities = null;
                using (STNEntities2 aSTNE = GetRDS())
                {
                    // Get the metadata workspace from the connection.
                    MetadataWorkspace workspace = aSTNE.MetadataWorkspace;

                    // Get a collection of the entity containers.
                    ReadOnlyCollection<EntityContainer> containers =
                            workspace.GetItems<EntityContainer>(
                                                DataSpace.CSpace);

                    EntitySet es;
                    if (containers[0].TryGetEntitySetByName(entityName, true, out es))
                    {
                        string queryString =
                            @"SELECT VALUE anEntity FROM STNEntities." + es.Name + " as anEntity";
                        var entityQuery =
                            aSTNE.CreateQuery<T>(queryString,
                                new ObjectParameter("ent", es.ElementType.Name));

                        entities = entityQuery.ToList();

                        if (entities != null)
                            entities.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                    }//end if
                }//end using

                return entities;
            }
            catch
            { return null; }
        }//end HttpMethod.GET
        protected HypermediaEntity Post(HypermediaEntity anEntityObject)
        {
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        // Get the metadata workspace from the connection.
                        MetadataWorkspace workspace = aSTNE.MetadataWorkspace;

                        // Get a collection of the entity containers.
                        ReadOnlyCollection<EntityContainer> containers =
                            workspace.GetItems<EntityContainer>(DataSpace.CSpace);

                        EntitySet es;
                        if (containers[0].TryGetEntitySetByName(entityName, true, out es))
                        {
                            //Only works for single key tables
                            string queryString = @"SELECT VALUE anEntity FROM STNEntities." +
                                                 es.Name + " as anEntity ORDER BY anEntity." +
                                                 es.ElementType.KeyMembers[0] + " DESC";

                            ObjectQuery<HypermediaEntity> entityQuery =
                                aSTNE.CreateQuery<HypermediaEntity>(queryString);

                            //Get next key
                            Decimal nextKey = 1;
                            if (entityQuery.Count() > 0)
                            {
                                var lastKey = entityQuery.First().EntityKey.EntityKeyValues[0].Value;
                                nextKey = (Decimal)lastKey + 1;
                            }

                            // Create the key that represents the entity object
                            EntityKey objectKey =
                                new EntityKey("STNEntities." + es.Name,
                                                es.ElementType.KeyMembers[0].ToString(),
                                                nextKey);
                            anEntityObject.EntityKey = objectKey;

                            //Use reflection to sync key field with entity key
                            Type t = anEntityObject.GetType();
                            System.Reflection.PropertyInfo pi = t.GetProperty(es.ElementType.KeyMembers[0].ToString());
                            pi.SetValue(anEntityObject, nextKey, null);

                            //Save new object
                            aSTNE.AddObject(entityName, anEntityObject);
                            aSTNE.SaveChanges();

                        }

                        if (anEntityObject != null)
                            anEntityObject.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return anEntityObject;
            }
            catch
            { return null; }
        }//end HttpMethod.POST
        protected HypermediaEntity Put(HypermediaEntity anEntityObject, Decimal entityId)
        {
            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        // Define an ObjectStateEntry and EntityKey for the current object. 
                        EntityKey key = default(EntityKey);
                        object originalObject = null;

                        // Create the detached object's entity key. 
                        key = aSTNE.CreateEntityKey("STNEntities." + entityName, anEntityObject);

                        // Get the original item based on the entity key from the context 
                        // or from the database. 
                        if (aSTNE.TryGetObjectByKey(key, out originalObject))
                        {
                            // Call the ApplyCurrentValues method to apply changes 
                            // from the updated item to the original version. 
                            aSTNE.ApplyCurrentValues(key.EntitySetName, anEntityObject);
                        }
                        else
                        {
                            //Return BadRequest if lookup fails
                            throw new Exception();
                        }

                        //Save new object
                        aSTNE.SaveChanges();

                    }//end using
                }//end using

                //Return OK instead of created, Flex incorrectly treats 201 as error
                return anEntityObject;
            }
            catch
            { return null; }
        }//end HttpMethod.PUT

        [HttpOperation(HttpMethod.GET)]
        public virtual OperationResult Get(Decimal entityId)
        {

            //Return BadRequest if there is no ID
            if (entityId <= 0)
                return new OperationResult.BadRequest();

            HypermediaEntity entity = null;
            try
            {
                using (STNEntities2 aSTNE = GetRDS())
                {
                    // Get the metadata workspace from the connection.
                    MetadataWorkspace workspace = aSTNE.MetadataWorkspace;

                    // Get a collection of the entity containers.
                    ReadOnlyCollection<EntityContainer> containers =
                            workspace.GetItems<EntityContainer>(
                                                DataSpace.CSpace);

                    EntitySet es;
                    if (containers[0].TryGetEntitySetByName(entityName, true, out es))
                    {
                        //Only works for single key tables
                        string queryString =
                            @"SELECT VALUE anEntity FROM STNEntities." + es.Name + " as anEntity WHERE anEntity." + es.ElementType.KeyMembers[0] + " = @entId";
                        ObjectQuery<HypermediaEntity> entityQuery =
                            aSTNE.CreateQuery<HypermediaEntity>(queryString,
                                new ObjectParameter("entId", entityId));

                        entity = entityQuery.ToList<HypermediaEntity>().First();
                        if (entity != null)
                            entity.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                    }//end if

                }//end using


                return new OperationResult.OK { ResponseResource = entity };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.GET

        [STNRequiresRole(new string[] { AdminRole, ManagerRole, FieldRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public virtual OperationResult Delete(Decimal entityId)
        {
            //Return BadRequest if missing required fields
            if (entityId <= 0)
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (STNEntities2 aSTNE = GetRDS(securedPassword))
                    {
                        // Get the metadata workspace from the connection.
                        MetadataWorkspace workspace = aSTNE.MetadataWorkspace;

                        // Get a collection of the entity containers.
                        ReadOnlyCollection<EntityContainer> containers =
                                 workspace.GetItems<EntityContainer>(
                                                    DataSpace.CSpace);

                        //fetch the object to be updated (assuming that it exists)
                        EntitySet es;
                        if (containers[0].TryGetEntitySetByName(entityName, true, out es))
                        {
                            //Only works for single key tables
                            string queryString =
                                @"SELECT VALUE anEntity FROM STNEntities." + es.Name + " as anEntity WHERE anEntity." + es.ElementType.KeyMembers[0] + " = @entId";
                            ObjectQuery<HypermediaEntity> entityQuery =
                                aSTNE.CreateQuery<HypermediaEntity>(queryString,
                                    new ObjectParameter("entId", entityId));

                            List<HypermediaEntity> entities = entityQuery.ToList<HypermediaEntity>();
                            entities.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual));

                            if (entities.Count > 0)
                            {
                                //Take the first if there is more than one
                                HypermediaEntity objectToBeDeleted = entities.First();
                                //Delete it
                                aSTNE.DeleteObject(objectToBeDeleted);
                            }
                            else
                            {
                                //Return NotFound if nothing returned
                                return new OperationResult.NotFound();
                            }
                        }
                        else
                        {
                            //Return BadRequest if lookup fails
                            return new OperationResult.BadRequest();
                        }

                        //Save Changes
                        aSTNE.SaveChanges();

                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { };
            }
            catch
            { return new OperationResult.BadRequest(); }
        }//end HttpMethod.DELETE
        #endregion

        #region "Base Methods"
        public bool IsAuthorized(string role)
        {
            return Context.User.IsInRole(role);
        }

        public bool IsAuthorizedToEdit(string OwnerUserName)
        {
            if (string.Equals(OwnerUserName.ToUpper(), username.ToUpper()))
                return true;

            if (IsAuthorized(AdminRole))
                return true;


            return false;
        }

        protected STNEntities2 GetRDS(EasySecureString password)
        {
            return new STNEntities2(string.Format(connectionString, Context.User.Identity.Name, password.decryptString()));
        }

        protected STNEntities2 GetRDS()
        {
            return new STNEntities2(string.Format(connectionString, "FRPUBLIC", "STN_Pub1ic"));
        }

        protected EasySecureString GetSecuredPassword()
        {
            //return new EasySecureString("cafOR4_yR");
            return new EasySecureString(STNBasicAuthentication.ExtractBasicHeader(Context.Request.Headers["Authorization"]).Password);
        }

        protected MEMBER GetLoggedInDataManager(STNEntities2 aRDS)
        {
            MEMBER manager = aRDS.MEMBERS.FirstOrDefault(dm => dm.USERNAME.ToUpper().Equals(username.ToUpper(), StringComparison.CurrentCultureIgnoreCase));

            return manager;
        }

        protected DateTime? ValidDate(string date)
        {
            DateTime tempDate;
            try
            {
                if (date == null) return null;
                if (!DateTime.TryParse(date, out tempDate))
                {
                    //try oadate
                    tempDate = DateTime.FromOADate(Convert.ToDouble(date));

                }


                return tempDate;
                // 
            }
            catch (Exception)
            {

                return null;
            }

        }//end ValidDate



        public string GetState(State state)
        {
            string result = string.Empty;
            switch (state)
            {
                case State.AL:
                    result = "ALABAMA";
                    break;
                case State.AK:
                    result = "ALASKA";
                    break;
                case State.AS:
                    result = "AMERICAN SAMOA";
                    break;
                case State.AZ:
                    result = "ARIZONA";
                    break;
                case State.AR:
                    result = "ARKANSAS";
                    break;
                case State.CA:
                    result = "CALIFORNIA";
                    break;
                case State.CO:
                    result = "COLORADO";
                    break;
                case State.CT:
                    result = "CONNECTICUT";
                    break;
                case State.DE:
                    result = "DELAWARE";
                    break;
                case State.DC:
                    result = "DISTRICT OF COLUMBIA";
                    break;
                case State.FM:
                    result = "FEDERATED STATES OF MICRONESIA";
                    break;
                case State.FL:
                    result = "FLORIDA";
                    break;
                case State.GA:
                    result = "GEORGIA";
                    break;
                case State.GU:
                    result = "GUAM";
                    break;
                case State.HI:
                    result = "HAWAII";
                    break;
                case State.ID:
                    result = "IDAHO";
                    break;
                case State.IL:
                    result = "ILLINOIS";
                    break;
                case State.IN:
                    result = "INDIANA";
                    break;
                case State.IA:
                    result = "IOWA";
                    break;
                case State.KS:
                    result = "KANSAS";
                    break;
                case State.KY:
                    result = "KENTUCKY";
                    break;
                case State.LA:
                    result = "LOUISIANA";
                    break;
                case State.ME:
                    result = "MAINE";
                    break;
                case State.MH:
                    result = "MARSHALL ISLANDS";
                    break;
                case State.MD:
                    result = "MARYLAND";
                    break;
                case State.MA:
                    result = "MASSACHUSETTS";
                    break;
                case State.MI:
                    result = "MICHIGAN";
                    break;
                case State.MN:
                    result = "MINNESOTA";
                    break;
                case State.MS:
                    result = "MISSISSIPPI";
                    break;
                case State.MO:
                    result = "MISSOURI";
                    break;
                case State.MT:
                    result = "MONTANA";
                    break;
                case State.NE:
                    result = "NEBRASKA";
                    break;
                case State.NV:
                    result = "NEVADA";
                    break;
                case State.NH:
                    result = "NEW HAMPSHIRE";
                    break;
                case State.NJ:
                    result = "NEW JERSEY";
                    break;
                case State.NM:
                    result = "NEW MEXICO";
                    break;
                case State.NY:
                    result = "NEW YORK";
                    break;
                case State.NC:
                    result = "NORTH CAROLINA";
                    break;
                case State.ND:
                    result = "NORTH DAKOTA";
                    break;
                case State.MP:
                    result = "NORTHERN MARIANA ISLANDS";
                    break;
                case State.OH:
                    result = "OHIO";
                    break;
                case State.OK:
                    result = "OKLAHOMA";
                    break;
                case State.OR:
                    result = "OREGON";
                    break;
                case State.PW:
                    result = "PALAU";
                    break;
                case State.PA:
                    result = "PENNSYLVANIA";
                    break;
                case State.PR:
                    result = "PUERTO RICO";
                    break;
                case State.RI:
                    result = "RHODE ISLAND";
                    break;
                case State.SC:
                    result = "SOUTH CAROLINA";
                    break;
                case State.SD:
                    result = "SOUTH DAKOTA";
                    break;
                case State.TN:
                    result = "TENNESSEE";
                    break;
                case State.TX:
                    result = "TEXAS";
                    break;
                case State.UT:
                    result = "UTAH";
                    break;
                case State.VT:
                    result = "VERMONT";
                    break;
                case State.VI:
                    result = "VIRGIN ISLANDS";
                    break;
                case State.VA:
                    result = "VIRGINIA";
                    break;
                case State.WA:
                    result = "WASHINGTON";
                    break;
                case State.WV:
                    result = "WEST VIRGINIA";
                    break;
                case State.WI:
                    result = "WISCONSIN";
                    break;
                case State.WY:
                    result = "WYOMING";
                    break;
                default:
                    result = string.Empty;
                    break;
            }//end switch

            return ToCapital(result);
        }
        public State GetStateByName(string name)
        {
            try
            {
                switch (name.ToUpper())
                {
                    case "ALABAMA":
                    case "AL":
                    case "ALA":
                        return State.AL;

                    case "ALASKA":
                    case "AK":
                        return State.AK;

                    case "AMERICAN SAMOA":
                    case "AS":
                        return State.AS;

                    case "ARIZONA":
                    case "AZ":
                    case "ARIZ":
                        return State.AZ;

                    case "ARKANSAS":
                    case "AR":
                    case "ARK":
                        return State.AR;

                    case "CALIFORNIA":
                    case "CA":
                    case "CALIF":
                        return State.CA;

                    case "COLORADO":
                    case "CO":
                    case "COLO":
                        return State.CO;

                    case "CONNECTICUT":
                    case "CT":
                    case "CONN":
                        return State.CT;

                    case "DELAWARE":
                    case "DE":
                    case "DEL":
                        return State.DE;

                    case "DISTRICT OF COLUMBIA":
                    case "DC":
                    case "D.C.":
                        return State.DC;

                    case "FEDERATED STATES OF MICRONESIA":
                    case "FM":
                    case "FSM":
                        return State.FM;

                    case "FLORIDA":
                    case "FL":
                    case "FLA":
                        return State.FL;

                    case "GEORGIA":
                    case "GA":
                        return State.GA;

                    case "GUAM":
                    case "GU":
                        return State.GU;

                    case "HAWAII":
                    case "HI":
                        return State.HI;

                    case "IDAHO":
                    case "ID":
                        return State.ID;

                    case "ILLINOIS":
                    case "IL":
                    case "ILL.":
                        return State.IL;

                    case "INDIANA":
                    case "IN":
                    case "IND":
                        return State.IN;

                    case "IOWA":
                    case "IA":
                        return State.IA;

                    case "KANSAS":
                    case "KS":
                    case "KANS":
                        return State.KS;

                    case "KENTUCKY":
                    case "KY":
                        return State.KY;

                    case "LOUISIANA":
                    case "LA":
                        return State.LA;

                    case "MAINE":
                    case "ME":
                        return State.ME;

                    case "MARSHALL ISLANDS":
                    case "MH":
                        return State.MH;

                    case "MARYLAND":
                    case "MD":
                        return State.MD;

                    case "MASSACHUSETTS":
                    case "MA":
                    case "MASS":
                        return State.MA;

                    case "MICHIGAN":
                    case "MI":
                    case "MICH":
                        return State.MI;

                    case "MINNESOTA":
                    case "MN":
                    case "MINN":
                        return State.MN;

                    case "MISSISSIPPI":
                    case "MS":
                    case "MISS":
                        return State.MS;

                    case "MISSOURI":
                    case "MO":
                        return State.MO;

                    case "MONTANA":
                    case "MT":
                    case "MONT":
                        return State.MT;

                    case "NEBRASKA":
                    case "NE":
                    case "NEBR":
                        return State.NE;

                    case "NEVADA":
                    case "NV":
                    case "NEV":
                        return State.NV;

                    case "NEW HAMPSHIRE":
                    case "NH":
                        return State.NH;

                    case "NEW JERSEY":
                    case "NJ":
                        return State.NJ;

                    case "NEW MEXICO":
                    case "NM":
                        return State.NM;

                    case "NEW YORK":
                    case "NY":
                        return State.NY;

                    case "NORTH CAROLINA":
                    case "NC":
                        return State.NC;

                    case "NORTH DAKOTA":
                    case "ND":
                        return State.ND;

                    case "NORTHERN MARIANA ISLANDS":
                    case "MP":
                        return State.MP;

                    case "OHIO":
                    case "OH":
                        return State.OH;

                    case "OKLAHOMA":
                    case "OK":
                    case "OKLA":
                        return State.OK;

                    case "OREGON":
                    case "OR":
                    case "ORE":
                        return State.OR;

                    case "PALAU":
                    case "PW":
                        return State.PW;

                    case "PENNSYLVANIA":
                    case "PA":
                        return State.PA;

                    case "PUERTO RICO":
                    case "PR":
                        return State.PR;

                    case "RHODE ISLAND":
                    case "RI":
                        return State.RI;

                    case "SOUTH CAROLINA":
                    case "SC":
                        return State.SC;

                    case "SOUTH DAKOTA":
                    case "SD":
                        return State.SD;

                    case "TENNESSEE":
                    case "TN":
                    case "TENN":
                        return State.TN;

                    case "TEXAS":
                    case "TX":
                    case "TEX":
                        return State.TX;

                    case "UTAH":
                    case "UT":
                        return State.UT;

                    case "VERMONT":
                    case "VT":
                        return State.VT;

                    case "VIRGIN ISLANDS":
                    case "VI":
                        return State.VI;

                    case "VIRGINIA":
                    case "VA":
                        return State.VA;

                    case "WASHINGTON":
                    case "WA":
                    case "WASH":
                        return State.WA;

                    case "WEST VIRGINIA":
                    case "WV":
                    case "W.VA":
                        return State.WV;

                    case "WISCONSIN":
                    case "WI":
                    case "WIS":
                        return State.WI;

                    case "WYOMING":
                    case "WY":
                    case "WYO":
                        return State.WY;
                }// end switch

                throw new Exception("Not Available");

            }
            catch (Exception)
            {
                return State.UNSPECIFIED;
            }//end try
        }
        public enum State
        {

            UNSPECIFIED,
            AL,
            AK,
            AS,
            AZ,
            AR,
            CA,
            CO,
            CT,
            DE,
            DC,
            FM,
            FL,
            GA,
            GU,
            HI,
            ID,
            IL,
            IN,
            IA,
            KS,
            KY,
            LA,
            ME,
            MH,
            MD,
            MA,
            MI,
            MN,
            MS,
            MO,
            MT,
            NE,
            NV,
            NH,
            NJ,
            NM,
            NY,
            NC,
            ND,
            MP,
            OH,
            OK,
            OR,
            PW,
            PA,
            PR,
            RI,
            SC,
            SD,
            TN,
            TX,
            UT,
            VT,
            VI,
            VA,
            WA,
            WV,
            WI,
            WY
        }

        public string GetProvince(Province province)
        {
            string result = string.Empty;
            switch (province)
            {
                case Province.UNSPECIFIED:
                    return string.Empty;
                case Province.AB:
                    result = "ALBERTA";
                    break;
                case Province.BC:
                    result = "BRITISH COLUMBIA";
                    break;
                case Province.MB:
                    result = "MANITOBA";
                    break;
                case Province.NB:
                    result = "NEW BRUNSWICK";
                    break;
                case Province.NL:
                    result = "NEWFOUNDLAND AND LABRADOR";
                    break;
                case Province.NT:
                    result = "NORTWEST TERRITORIES";
                    break;
                case Province.NS:
                    result = "NOVA SCOTIA";
                    break;
                case Province.NU:
                    result = "NUNAVUT";
                    break;
                case Province.ON:
                    result = "ONTARIO";
                    break;
                case Province.PE:
                    result = "PRINCE EDWARD ISLAND";
                    break;
                case Province.QC:
                    result = "QUEBEC";
                    break;
                case Province.SK:
                    result = "SASKATCHEWAN";
                    break;
                case Province.YT:
                    result = "YUKON";
                    break;
                default:
                    result = string.Empty;
                    break;
            }//end switch

            return ToCapital(result);
        }
        public Province GetProvinceByName(string name)
        {
            switch (name.ToUpper())
            {
                case "ALBERTA":
                case "AB":
                    return Province.AB;

                case "BRITISH COLUMBIA":
                case "COLOMBIE-BRITANNIQUE":
                case "BC":
                    return Province.BC;

                case "MANITOBA":
                case "MB":
                    return Province.MB;

                case "NEW BRUNSWICK":
                case "NOUVEAU-BRUNSWICK":
                case "NB":
                    return Province.NB;

                case "NEWFOUNDLAND AND LABRADOR":
                case "TERRE-NEUVE-ET-LABRADOR":
                case "NL":
                    return Province.NL;

                case "NOVA SCOTIA":
                case "NOUVELLE-ÉCOSSE":
                case "NS":
                    return Province.NS;

                case "NORTWEST TERRITORIES":
                case "TERRITOIRES DU NORD-OUEST":
                case "NT":
                    return Province.NT;

                case "NUNAVUT":
                case "NU":
                    return Province.NU;

                case "ONTARIO":
                case "ON":
                    return Province.ON;

                case "PRINCE EDWARD ISLAND":
                case " 	ÎLE-DU-PRINCE-ÉDOUARD":
                case "PE":
                    return Province.PE;

                case "QUEBEC":
                case "QC":
                    return Province.QC;

                case "SASKATCHEWAN":
                case "SK":
                    return Province.SK;

                case "YUKON":
                case "YT":
                    return Province.YT;

            }// end switch

            //throw new Exception("Not Available");
            return Province.UNSPECIFIED;
        }
        public enum Province
        {
            UNSPECIFIED,
            AB,
            BC,
            MB,
            NB,
            NL,
            NT,
            NS,
            NU,
            ON,
            PE,
            QC,
            SK,
            YT
        }

        public string GetCountry(Country country)
        {
            string result = string.Empty;
            switch (country)
            {
                case Country.UNSPECIFIED:
                    result = string.Empty;
                    break;
                case Country.USA:
                    result = "UNITED STATES OF AMERICA";
                    break;
                case Country.CA:
                    result = "CANADA";
                    break;
                default:
                    result = string.Empty;
                    break;
            }//end switch

            return ToCapital(result);
        }
        public Country GetCountryByName(string name)
        {
            switch (name.ToUpper())
            {
                case "CANADA":
                case "CA":
                case "C.A.":
                    return Country.CA;

                case "UNITED STATES OF AMERICA":
                case "UNITED STATES":
                case "USA":
                case "U.S.A":
                    return Country.USA;
            }// end switch

            //throw new Exception("Not Available");
            return Country.UNSPECIFIED;
        }
        public enum Country
        {
            UNSPECIFIED,
            USA,
            CA
        }

        protected string ToCapital(string phrase)
        {
            StringBuilder sb = new StringBuilder(phrase.Length);
            // Upper the first char.
            sb.Append(char.ToUpper(phrase[0]));

            for (int i = 1; i < phrase.Length; i++)
            {
                // Get the current char.
                char c = phrase[i];

                // Upper if after a space.
                if (char.IsWhiteSpace(phrase[i - 1]))
                    c = char.ToUpper(c);
                else
                    c = char.ToLower(c);

                sb.Append(c);
            }//next i

            return sb.ToString();
        }

        #endregion

    }//end class HandlerBase

}//end namespace