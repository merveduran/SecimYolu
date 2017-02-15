using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace secimyolu.Models
{
    public class FacebookLoginModel
    {
        public string uid { get; set; }
        public string accessToken { get; set; }
    }

    public class FacebookLocation
    {
        public string id { get; set; }
        public string name { get; set; }
    }


    //https://www.googleapis.com/oauth2/v2/tokeninfo?id_token=xxxxx
    /*
        {
        issued_to: "284541259428-5eqtvev7p5i7knoqt58c9ho831buaruo.apps.googleusercontent.com",
        audience: "284541259428-5eqtvev7p5i7knoqt58c9ho831buaruo.apps.googleusercontent.com",
        user_id: "105575205869597017160",
        expires_in: 3577,
        email: "umut@tiga.com.tr",
        verified_email: true
        }               
     */
    /*
           {
           error_description: "either access_token, id_token, or token_handle required"
           }
        */
    public class GoogleTokenInfo
    {
        public string email { get; set; }
        public bool email_verified { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string picture { get; set; }
    }

    public class Picture
    {
        public PicureData data { get; set; }
    }

    public class PicureData
    {
        public string url { get; set; }
        public bool is_silhouette { get; set; }
    }

    public class FacebookUserModel
    {
        public string id { get; set; }
        public string email { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string gender { get; set; }
        public string locale { get; set; }
        public string link { get; set; }
        public string username { get; set; }
        public int timezone { get; set; }
        public FacebookLocation location { get; set; }
        public Picture picture { get; set; }
    }
}