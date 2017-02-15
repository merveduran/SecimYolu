using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace secimyolu.Controllers
{
    public class BaseController : Controller
    {

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (TempData["ErrorType"] != null)
            {
                var cookie = new HttpCookie("ShowError");
                cookie.Value = "1";
                Response.Cookies.Add(cookie);
            }
        }
    }
}