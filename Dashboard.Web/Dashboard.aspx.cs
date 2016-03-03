using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Configuration;
namespace Dashboard.Web
{
	public partial class Dashboard : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Authenticate();
		}

		void Authenticate()
        {
            if (Request.IsLocal)
            {
                FormsAuthentication.SetAuthCookie("abrubaker@troyeradvisors.com", true);
                return;
            }
			if (!HttpContext.Current.User.Identity.IsAuthenticated)
                Response.Redirect(ConfigurationManager.AppSettings["LoginLink"] + "?ReturnUrl="+ Request.Url);
            //Response.Redirect(ConfigurationManager.AppSettings["LoginLink"] + "?ReturnUrl=http://troyeradvisorsdashboards.com/services/dashboards/maryland/dashboard.aspx");
		}
	}
}