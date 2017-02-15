using secimyolu.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace secimyolu
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected virtual void Application_BeginRequest()
        {
            HttpContext.Current.Items[Constants.EntityContextName] = new SECIMYOLUEntities();
        }

        protected virtual void Application_EndRequest()
        {
            var entityContext = HttpContext.Current.Items[Constants.EntityContextName] as SECIMYOLUEntities;
            if (entityContext != null)
                entityContext.Dispose();
        }
    }
}
