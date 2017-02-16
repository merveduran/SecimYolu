using secimyolu.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GoogleMaps.LocationServices;

namespace secimyolu.Controllers
{
    public class HomeController : BaseController
    {
        private DateTime startDate = new DateTime(2015, 10, 8);
        // GET: Home
        public ActionResult Index()
        {
            try
            {
                QueryParameter qParam = new QueryParameter();
                if (Current.getSessionItem("queryParameter") == null)
                {
                    qParam = new QueryParameter { PCode = "", CountryId = -1, Dest = -1, depDate = (DateTime.Now <= startDate ? startDate : DateTime.Now), AllDate = false };
                    ViewBag.QueryParameters = qParam;
                }
                else
                {
                    qParam = (QueryParameter)Current.getSessionItem("queryParameter");
                    string postalCode = qParam.PCode.Substring(0, 1);
                    ViewBag.QueryParameters = qParam;
                    if (qParam.AllDate)
                    {
                        ViewBag.QueryResult = Current.Context.vwCarList.Where(d => d.DepartureDate >= DateTime.Now && d.PostalCode.StartsWith(postalCode) && d.Destination == qParam.Dest && d.DepartureDate >= qParam.depDate).OrderByDescending(d => d.ServiceStatus).ThenBy(d => d.DepartureDate).ToList();
                    }
                    else
                    {
                        ViewBag.QueryResult = Current.Context.vwCarList.Where(d => d.PostalCode.StartsWith(postalCode) && d.Destination == qParam.Dest && d.DepartureDate == qParam.depDate).OrderByDescending(d => d.ServiceStatus).ToList();
                    }
                }
                if (Current.isLogon && Current.getUser.UserTypeId == Constants.USER_TYPE_GUEST)
                {
                    ViewBag.MyReservation = Current.Context.vwServicePassenger.Where(d => d.AddUserId == Current.getUserId).ToList();
                }
            }
            catch(Exception)
            {
                Current.setSessionItem("queryParameter", null);
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(QueryParameter qParam)
        {
            Current.setSessionItem("queryParameter", qParam);
            return Redirect("/#sonuc");
        }

        public ActionResult Reservation(int ServiceId)
        {
            if(Current.isLogon)
            {
                vwCarList car = Current.Context.vwCarList.FirstOrDefault(d => d.DepartureDate >= DateTime.Now && d.Id == ServiceId);
                ServicePassenger sP = Current.Context.ServicePassenger.FirstOrDefault(d => d.ServiceId == ServiceId && d.AddUserId == Current.getUserId);
                int pCount = sP != null ? sP.PassengerCount : 0;
                if (car.Qutoa > (car.PassengerCount-pCount))
                {
                    int curQuota = (int)car.Qutoa - car.PassengerCount + pCount;
                    ViewBag.CurQuota = curQuota > 10 ? 10 : curQuota;
                    ViewBag.PassengerCount = pCount;
                    return View(car);
                }
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        [SessionControlFilter]
        public ActionResult Reservation(int ServiceId,int PassengerCount)
        {
            try
            {
                CarService service = Current.Context.CarService.FirstOrDefault(d => d.Id == ServiceId);
                if (service != null)
                {
                    ServicePassenger cService = Current.Context.ServicePassenger.FirstOrDefault(d => d.AddUserId == Current.getUserId && d.ServiceId == ServiceId);
                    secimyolu.Models.User curUser = Current.getUser;
                    if (cService == null)
                    {
                        cService = new ServicePassenger();
                        cService.ServiceId = ServiceId;
                        cService.AddTime = DateTime.Now;
                        cService.AddUserId = curUser.Id;
                        cService.Name = curUser.Name;
                        cService.Surname = curUser.Surname;
                        cService.TCK = curUser.TCNo;
                        cService.Phone = curUser.GSM;
                        cService.PassengerCount = PassengerCount;
                        Current.Context.ServicePassenger.Add(cService);
                    }
                    else
                    {
                        cService.PassengerCount = PassengerCount;
                        cService.AddTime = DateTime.Now;
                    }
                    Current.Context.SaveChanges();
                    sendReservationMail(cService.Id);
                    return Redirect("/#rezervasyonlar");
                }
            }
            catch(Exception)
            { }
            return Redirect("/#sonuc");
        }

        [SessionControlFilter]
        public ActionResult CancelReservation(int ServiceId)
        {
            ServicePassenger sP = Current.Context.ServicePassenger.FirstOrDefault(d => d.AddUserId == Current.getUserId && d.ServiceId == ServiceId);
            CarService cS = Current.Context.CarService.FirstOrDefault(d => d.Id == ServiceId && d.DepartureDate >= DateTime.Now);
            if(cS!=null && sP!=null)
            {
                Current.Context.ServicePassenger.Remove(sP);
                Current.Context.SaveChanges();
            }
            return Redirect("/#rezervasyonlar");
        }

        public string getDestinationList(int CountryId,int selVal)
        {
            string optionList = "<option value=\"-1\">Seçiniz</option>";
            string selStr="selected=\"selected\"";
            List<Destination> destList = Current.Context.Destination.Where(d => d.CountryId == CountryId).ToList();
            if(destList.Count<=0)
            {
                optionList = "<option value=\"-1\">Konsolosluk bulunamadı!</option>";
            }
            else if(destList.Count==1)
            {
                optionList = "<option value=\"" + destList.FirstOrDefault().Id + "\">" + destList.FirstOrDefault().Box + "</option>";
            }
            else
            {
                foreach(var item in destList.OrderBy(d=>d.Box))
                {
                    optionList += "<option value=\"" + item.Id + "\" " + (item.Id == selVal ? selStr : "") + ">" + item.Box + "</option>";
                }
            }
            return optionList;
        }


        private void sendReservationMail(int servicePId)
        {
            ServicePassenger sP=Current.Context.ServicePassenger.FirstOrDefault(d=>d.Id==servicePId);
            User mailToUser = Current.Context.User.FirstOrDefault(f => f.Id == sP.AddUserId);
            vwCarList carDetail=Current.Context.vwCarList.FirstOrDefault(d=>d.Id==sP.ServiceId);
            if (!string.IsNullOrEmpty(mailToUser.Email))
            {
                ReservationInfo newLayout = new ReservationInfo
                {
                    Name = mailToUser.Name + " " + mailToUser.Surname,
                    DateString = ((DateTime)carDetail.DepartureDate).ToString("dd/MM/yyyy"),
                    TimeString = carDetail.DepartureHour,
                    Destination = carDetail.DestinationBox,
                    Address = carDetail.Address
                };

                string ContentMessage = ViewRenderer.RenderView("~/Views/Shared/MailLayouts/Reservation.cshtml",
                    newLayout, ControllerContext);

                Mail newMail = new Mail
                {
                    MailTo = mailToUser.Id,
                    MailFrom = 1,
                    MailContent = ContentMessage,
                    Status = Constants.MAIL_STATUS_WAITING,
                    MailSubject = "Rezervasyon Bilgisi"
                };
                Current.Context.Mail.Add(newMail);
                Current.Context.SaveChanges();
                SecimMail.MailGonder(mailToUser.Id, "Rezervasyon Bilgisi", ContentMessage, newMail.id);
            }
        }

        #region google map
        public string GetLanLogGoogleMap(string fullAddress, double mapLatitude = 0, double mapLongitude = 0)
        {
                ArrivalAddresses MapAddress = new ArrivalAddresses();
            try
            {
                MapAddress.Description = fullAddress;
                if ((mapLatitude != 0.0 && mapLongitude != 0.0) || !string.IsNullOrEmpty(fullAddress))
                {
                    var locationService = new GoogleLocationService();
                    var point = locationService.GetLatLongFromAddress(fullAddress);
                    if (mapLatitude != 0.0 && mapLongitude != 0.0)
                    {
                        MapAddress.Lat = mapLatitude;
                        MapAddress.Lng = mapLongitude;
                    }
                    else
                    {
                        MapAddress.Lat = point.Latitude;
                        MapAddress.Lng = point.Longitude;
                    }
                }
                else
                {
                    MapAddress.Lat = 50.611132;
                    MapAddress.Lng = 27.07031;
                }
                return (MapAddress.Description + "_" + MapAddress.Lat + "_" + MapAddress.Lng);

            }
            catch (DbEntityValidationException dbEx)
            {
                
            }
            catch (Exception ex)
            {
                MapAddress.Lat = 51.124799;
                MapAddress.Lng = 10.630131;
                return (MapAddress.Description + "_" + MapAddress.Lat + "_" + MapAddress.Lng);
            }
            return null;
        }
        public string GetDestinations(string countryCode, int selVal= -1)
        {
            Country selectedCountry = Current.Context.Country.FirstOrDefault(f => f.Iso == countryCode) ?? new Country();
            List<Destination> destinationList = Current.Context.Destination.Where(f => f.CountryId == selectedCountry.Id).ToList();
            string optionStr = "<option value=\"-1\">Seçiniz</option>";
            string selStr = "selected=\"selected\"";
            foreach (var item in destinationList.OrderBy(f => f.Box))
            {
                optionStr += "<option value=\"" + item.Id + "\" " + (selVal == item.Id ? selStr : "") + ">" + item.Box + "</option>";
            }
            return optionStr;

        }
        #endregion

        #region Araç Ekle
        public ActionResult AddVoluntaryCar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddVoluntaryCar(CarService newCar, string countryCode)
        {
            try
            {
                Country country = Current.Context.Country.FirstOrDefault(f => f.Iso == countryCode) ?? new Country();
                if (newCar.Id != 0) // düzenle
                {
                    CarService currCarService = Current.Context.CarService.FirstOrDefault(f => f.Id == newCar.Id);
                    if (currCarService != null)
                    {
                        currCarService.CarTypeId = newCar.CarTypeId;
                        currCarService.PostalCode = newCar.PostalCode;
                        currCarService.CityName = newCar.CityName;
                        currCarService.Address = newCar.PostalCode;
                        currCarService.CountryId = country.Id;
                        currCarService.Destination = newCar.Destination;
                        currCarService.DepartureDate = newCar.DepartureDate;
                        currCarService.DepartureHour = newCar.DepartureHour;
                        currCarService.Qutoa = newCar.Qutoa;
                        currCarService.ResponsibleName = newCar.ResponsibleName;
                        currCarService.ResponsiblePhone = newCar.ResponsiblePhone;
                        currCarService.Description = newCar.Description;
                        currCarService.MapLongitude = newCar.MapLongitude;
                        currCarService.MapLatitude = newCar.MapLatitude;
                        Current.Context.SaveChanges();

                        TempData["ErrorType"] = "Success";
                        TempData["Message"] = "Araç güncelleme işlemi başarı ile sonuçlanmıştır!";
                    }
                }
                else
                {
                    newCar.CreationDate = DateTime.Now;
                    newCar.Status = Constants.CAR_STATUS_ACTIVE;
                    newCar.AddUserId = Current.getUserId;
                    newCar.CountryId = country.Id;
                    newCar.IsVoluntaryService = true;
                    newCar.Status = Constants.CAR_STATUS_ACTIVE;
                    Current.Context.CarService.Add(newCar);
                    Current.Context.SaveChanges();

                    TempData["ErrorType"] = "Success";
                    TempData["Message"] = "Araç ekleme işlemi başarı ile sonuçlanmıştır!";
                }
    
             
                // return RedirectToAction("CarList");
            }
            catch (Exception)
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "İşlem başarısız olmuştur!";
            }
            TempData["IsDefaultTab"] = 0;
            return Redirect("/#araclarim");
        }

        public ActionResult EditVoluntaryCar(int Id = -1)
        {
            TempData["voluntaryCar"] = Current.Context.CarService.FirstOrDefault(f=> f.Id == Id);
            TempData["IsDefaultTab"] = 0;
            return Redirect("/#aracGir");
        }

        public ActionResult GetPassengerInfo(int Id)
        {
            List<vwServicePassenger> list = Current.Context.vwServicePassenger.Where(w => w.ServiceId == Id).ToList();
            return PartialView(list);
        }
        #endregion
    }
}