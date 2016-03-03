using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.ServiceModel.DomainServices.EntityFramework;
using System.ServiceModel.DomainServices.Hosting;
using System.ServiceModel.DomainServices.Server;
using Dashboard.Web;
using System.Web.Security;


namespace Dashboard.Web.Services
{

	// Implements application logic using the HubEntities context.
	// TODO: Add your application logic to these methods or in additional methods.
	// TODO: Wire up authentication (Windows/ASP.NET Forms) and uncomment the following to disable anonymous access
	// Also consider adding roles to restrict access as appropriate.
	// [RequiresAuthentication]
	[EnableClientAccess()]
	public class HubService : LinqToEntitiesDomainService<HubEntities>
	{

		// TODO:
		// Consider constraining the results of your query method.  If you need additional input you can
		// add parameters to this method or create additional query methods with different names.
		// To support paging you will need to add ordering to the 'Clients' query.
		public IQueryable<Client> GetClients()
		{
			return this.ObjectContext.Clients;
		}

		public Client GetCurrentClient()
		{
			Guid id = (Guid)Membership.GetUser().ProviderUserKey;
			
			return GetUsers().Where(uc => uc.ID == id).Select(e => e.Client).FirstOrDefault();
		}

		// TODO:
		// Consider constraining the results of your query method.  If you need additional input you can
		// add parameters to this method or create additional query methods with different names.
		// To support paging you will need to add ordering to the 'UserClients' query.
		public IQueryable<HubUser> GetUsers()
		{
			return this.ObjectContext.HubUsers;
		}
	}
}


