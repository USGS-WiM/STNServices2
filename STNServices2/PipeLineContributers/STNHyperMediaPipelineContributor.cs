using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WiM.Hypermedia;
using WiM.PipeLineContributors;
using STNDB;
using WiM.Resources;
//using STNServices2.Resources;

namespace STNServices2.PipeLineContributors
{
    public class STNHyperMediaPipelineContributor:HypermediaPipelineContributor
    {
        protected override List<Link> GetReflectedHypermedia(IHypermedia entity)
        {
            List<Link> results = null;
            switch (entity.GetType().Name)
            {
                case "agency":
                    results = new List<Link>();
                    results.Add(new Link(BaseURI, "members", String.Format(Configuration.agencyResource + "/{0}/" + Configuration.memberResource, ((agency)entity).agency_id), refType.GET));
                    results.Add(new Link(BaseURI, "sources", String.Format(Configuration.agencyResource + "/{0}/" + Configuration.sourceResource, ((agency)entity).agency_id), refType.GET));
                    
                    break;

          
                default:
                    break;
            }

            return results;
        }
        protected override List<Link> GetEnumeratedHypermedia(IHypermedia entity)
        {
            List<Link> results = null;
            switch (entity.GetType().Name)
            {
                case "agency":
                    results = new List<Link>();
                    results.Add(new Link(BaseURI, "self", Configuration.agencyResource + "/" + ((agency)entity).agency_id, refType.GET));
                    results.Add(new Link(BaseURI, "edit", Configuration.agencyResource + "/" + ((agency)entity).agency_id, refType.PUT));
                    results.Add(new Link(BaseURI, "delete", Configuration.agencyResource + "/" + ((agency)entity).agency_id, refType.DELETE));                      
                    break;
    
                default:
                    break;
            }

            return results;
        }
        
    }//end class
}//end namespace
