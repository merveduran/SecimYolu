using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace secimyolu.Models
{
    public class Constants
    {

        public static string EntityContextName { get { return "_EntityContext"; } }


        public const int CAR_STATUS_ACTIVE = 1;
        public const int CAR_STATUS_DELETED = 2;

        public const int USER_TYPE_GUEST = 1;
        public const int USER_TYPE_BSKM = 2;
        public const int USER_TYPE_YSKM = 3;
        public const int USER_TYPE_POLLING_CLERK = 4;

        public const int MAIL_TYPE_RESET_PASSWORD = 1;


        public const int MAIL_STATUS_SEND = 1;
        public const int MAIL_STATUS_WAITING = 2;
        public const int MAIL_STATUS_FAIL = 3;
    }
}