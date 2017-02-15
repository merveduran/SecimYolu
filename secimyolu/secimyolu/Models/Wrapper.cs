using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace secimyolu.Models
{
  public class QueryParameter
  {
      public string PCode { get; set; }
      public int CountryId { get; set; }
      public int Dest { get; set; }
      public bool AllDate { get; set; }
      public DateTime depDate { get; set; }
  }

  public class ResetPasswordMail
  {
      public string UserName { get; set; }
      public string ActivationLink { get; set; }
  }

    public class ReservationInfo
    {
        public string Name { get; set; }
        public string DateString { get; set; }
        public string TimeString { get; set; }
        public string Address { get; set; }
        public string Destination { get; set; }
    }

    public class ArrivalAddresses
    {
        public string Description { get; set; }
        public double Lng { get; set; }
        public double Lat { get; set; }
    }
}