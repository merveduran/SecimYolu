using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace secimyolu.Models
{
    public class AuthenticationFilter : ActionFilterAttribute
    {
        public string UserType { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            //redirect if the user is not authenticated
            if (!String.IsNullOrEmpty(UserType))
            {
                //use the current url for the redirect
                string redirectOnSuccess = filterContext.HttpContext.Request.Url.AbsolutePath + filterContext.HttpContext.Request.Url.Query;

                //send them off to the login page
                string redirectUrl = string.Format("?ReturnUrl={0}", redirectOnSuccess);

                if (!Current.isLogon)
                {
                    filterContext.Result = new RedirectResult("/Account" + redirectUrl);
                }
                else if (!Current.isAuthorized(UserType))
                {
                    filterContext.Result = new RedirectResult("/Home/PermissionDenied");
                    //filterContext.HttpContext.Response.Redirect("/Main/PermissionDenied", true);
                }
            }
            else
            {
                throw new InvalidOperationException("No Role Specified");
            }
        }
    }

    public class SessionControlFilter : ActionFilterAttribute, IActionFilter
    {

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Current.isLogon)
            {
            }
            else
            {
                //use the current url for the redirect
                string redirectOnSuccess = filterContext.HttpContext.Request.Url.AbsolutePath + filterContext.HttpContext.Request.Url.Query;

                //send them off to the login page
                string redirectUrl = string.Format("/Account?ReturnUrl={0}", redirectOnSuccess);

                try
                {
                    filterContext.HttpContext.Response.Redirect(redirectUrl);
                    // filterContext.Result = new RedirectResult(redirectUrl);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}