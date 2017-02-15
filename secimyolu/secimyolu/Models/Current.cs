using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.SessionState;

namespace secimyolu.Models
{
    public class Current
    {
        public static HttpRequest Request
        {
            get { return HttpContext.Current.Request; }
        }

        public static HttpResponse Response
        {
            get { return HttpContext.Current.Response; }
        }

        public static HttpSessionState Session
        {
            get { return HttpContext.Current.Session; }
        }

        public static HttpServerUtility Server
        {
            get { return HttpContext.Current.Server; }
        }

        public static HttpApplicationState Application
        {
            get { return HttpContext.Current.Application; }
        }

        public static string ServerRootUrl
        {
            get { return Request.Url.GetLeftPart(UriPartial.Authority); }
        }

        public static string RequestUrl
        {
            get { return Request.Url.ToString(); }
        }

        public static string IpAddress
        {
            get
            {
                var remoteIp = Current.Request.UserHostAddress;
                return remoteIp;
            }
        }
        /*Sessionda bulunan verilerin okuma islemini gerceklestirir*/
        public static object getSessionItem(string param)
        {
            if (HttpContext.Current.Session != null)
                return HttpContext.Current.Session[param];
            else
                return null;
        }

        //Verileri sessiona atma islemini gerceklestirir
        public static void setSessionItem(string param, object value)
        {
            HttpContext.Current.Session[param] = value;
        }

        //Oturum acmis kullanici bilgilerini nesne seklinde gonderir
        public static User getUser
        {
            get { return (User)Current.getSessionItem("userInfo"); }
        }

        public static bool isLogon
        {
            get
            {
                if (getSessionItem("userInfo") != null)
                    return true;
                else
                    return false;
            }
        }

        public static bool hasPhone
        {
            get
            {
                if (getSessionItem("userInfo") != null)
                {
                    if(!string.IsNullOrEmpty(getUser.GSM))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                    return false;
            }
        }

        public static bool isAuthorized(string UserType)
        {
            // sessiondan right listesi alınır
            string rightObjectList = getUser.UserTypeId==Constants.USER_TYPE_YSKM?"YSKM":(getUser.UserTypeId==Constants.USER_TYPE_BSKM?"BSKM":"GUEST");

            // eğer kullanıcının right listesi boşsa false dönülür
            if (rightObjectList == null)
                return false;

            if (rightObjectList == "YSKM")
            {
                return true;
            }
            else if (rightObjectList == "BSKM")
            {
                if (rightObjectList == UserType)
                    return true;
                else
                    return false;
            }
            else if (rightObjectList == "GUEST")
            {
                if (rightObjectList == UserType)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public static bool isAuthorizedRole(string role)
        {
            List<string> roleObjectList = (List<string>)getSessionItem("roles");

            if (roleObjectList == null)
                return false;

            return roleObjectList.Contains(role);
        }


        public static int getUserId
        {
            get
            {
                User usr = (User)Current.getSessionItem("userInfo");
                return usr.Id;
            }
        }

       
       
      

        public static SECIMYOLUEntities Context
        {
            get { return HttpContext.Current.Items[Constants.EntityContextName] as SECIMYOLUEntities; }
        }
    }
}