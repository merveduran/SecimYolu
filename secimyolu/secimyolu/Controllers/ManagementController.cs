using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using secimyolu.Models;

namespace secimyolu.Controllers
{
    [SessionControlFilter]
    public class ManagementController : BaseController
    {
        // GET: Management
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public ActionResult Login()
        {
            return View();
        }

        #region Araç Listesi

        [AuthenticationFilter(UserType = "BSKM")]
        public ActionResult CarList()
        {
            return View();
        }

        [AuthenticationFilter(UserType = "BSKM")]
        public JsonResult getCarList(jQueryDataTableParamModel param)
        {

            var carType = Convert.ToString(Request["sSearch_0"]);
            var postalCode = Convert.ToString(Request["sSearch_1"]);
            var city = Convert.ToString(Request["sSearch_2"]);
            var country = Convert.ToString(Request["sSearch_3"]);
            var destination = Convert.ToString(Request["sSearch_5"]);
            var status = Convert.ToString(Request["sSearch_6"]);
            var addUserType = Convert.ToString(Request["sSearch_8"]);


            int UserType = (int)Current.getUser.UserTypeId;
            IQueryable<secimyolu.Models.vwCarList> regList = Current.Context.vwCarList;

            if (UserType == Constants.USER_TYPE_BSKM)
            {
                List<int> avaibleCountryList = Current.Context.UserCountry.Where(f => f.UserId == Current.getUserId).Select(f => (int)f.CountryId).ToList();
                regList = regList.Where(f => avaibleCountryList.Contains((int)f.CountryId));
            }



            if (!String.IsNullOrEmpty(carType))
                regList = regList.Where(d => d.CarType.Contains(carType));

            if (!String.IsNullOrEmpty(postalCode))
                regList = regList.Where(d => d.PostalCode.Contains(postalCode));

            if (!String.IsNullOrEmpty(city))
                regList = regList.Where(d => d.CityName.Contains(city));

            if (!String.IsNullOrEmpty(country))
                regList = regList.Where(d => d.CountryName.Contains(country));

            if (!String.IsNullOrEmpty(destination))
                regList = regList.Where(d => d.DestinationBox.Contains(destination));



            if (!String.IsNullOrEmpty(status))
            {
                if (status == "1")
                {
                    regList = regList.Where(f => f.ServiceStatus == 1);
                }
                if (status == "3")
                {
                    regList = regList.Where(f => f.ServiceStatus == 2);
                }
            }

            if (!String.IsNullOrEmpty(addUserType))
            {
                if (addUserType == "1")
                {
                    regList = regList.Where(f => f.IsVoluntaryService == true);
                }
                if (addUserType == "3")
                {
                    regList = regList.Where(f => f.IsVoluntaryService == false);
                }
            }
            int count = regList.Count();
            //order
            //-------------------------------------------------------------------
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            string orderingFunction = sortColumnIndex == 1 || sortColumnIndex == 0 ? "CarType" : sortColumnIndex == 2 ? "PostalCode" : sortColumnIndex == 3 ? "CityName" : sortColumnIndex == 4 ? "CountryName" : sortColumnIndex == 5 ? "DepartureDate" : "DestinationBox";
            var sortDirection = Request["sSortDir_0"];
            regList = OrderClass.OrderBy<vwCarList>(regList, orderingFunction, sortDirection);
            //-------------------------------------------------------------------
            var data = regList.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList().Select(r => new[] { r.Id.ToString(), r.CarType, r.PostalCode, r.CityName, r.CountryName, (r.DepartureDate != null ? String.Format("{0:dd/MM/yyyy}", r.DepartureDate) : "") + (r.DepartureHour != null ? "-" + r.DepartureHour : ""), TinyGridItem(r.DestinationBox), r.ServiceStatus.ToString(), (r.PassengerCount + "/" + (r.Qutoa != null ? r.Qutoa.ToString() : "")), r.IsVoluntaryService == true ? "Gönüllü" : "Yönetici" });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = count,
                iTotalDisplayRecords = count,
                aaData = data
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Araç Ekle
        [AuthenticationFilter(UserType = "BSKM")]
        public ActionResult AddCar()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddCar(CarService newCar)
        {
            try
            {
                newCar.CreationDate = DateTime.Now;
                newCar.Status = Constants.CAR_STATUS_ACTIVE;
                newCar.AddUserId = Current.getUserId;
                Current.Context.CarService.Add(newCar);
                Current.Context.SaveChanges();

                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Araç ekleme işlemi başarı ile sonuçlanmıştır!";
                return RedirectToAction("CarList");
            }
            catch (Exception)
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "İşlem başarısız olmuştur!";
                return View();
            }
        }
        #endregion

        #region Araç Düzenle
        [AuthenticationFilter(UserType = "BSKM")]
        public ActionResult EditCar(int Id)
        {
            int UserType = (int)Current.getUser.UserTypeId;
            if (UserType == Constants.USER_TYPE_BSKM)
            {
                List<int> avaibleCountryList = Current.Context.UserCountry.Where(f => f.UserId == Current.getUserId).Select(f => (int)f.CountryId).ToList();
                return View(Current.Context.CarService.FirstOrDefault(f => f.Id == Id && avaibleCountryList.Contains((int)f.CountryId)));
            }
            else
            {
                return View(Current.Context.CarService.FirstOrDefault(f => f.Id == Id));
            }


        }

        [HttpPost]
        public ActionResult EditCar(CarService editCar)
        {
            try
            {
                CarService oldCar = Current.Context.CarService.FirstOrDefault(f => f.Id == editCar.Id);

                oldCar.CarTypeId = editCar.CarTypeId;
                oldCar.PostalCode = editCar.PostalCode;
                oldCar.CityName = editCar.CityName;
                oldCar.Address = editCar.Address;
                oldCar.CountryId = editCar.CountryId;
                oldCar.Destination = editCar.Destination;
                oldCar.DepartureDate = editCar.DepartureDate;
                oldCar.DepartureHour = editCar.DepartureHour;
                oldCar.Qutoa = editCar.Qutoa;
                oldCar.ResponsibleName = editCar.ResponsibleName;
                oldCar.ResponsiblePhone = editCar.ResponsiblePhone;
                oldCar.Description = editCar.Description;
                Current.Context.SaveChanges();

                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Araç başarı ile güncellenmiştir!";
                return RedirectToAction("CarList");
            }
            catch (Exception)
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "Araç güncelleme işlemi başarısız olmuştur!";
                return View(editCar);
            }

        }
        #endregion

        #region Araç Sil
        [AuthenticationFilter(UserType = "BSKM")]
        public ActionResult DeleteCar(int Id)
        {
            try
            {

                CarService deletedItem = Current.Context.CarService.FirstOrDefault(f => f.Id == Id);
                deletedItem.Status = Constants.CAR_STATUS_DELETED;
                deletedItem.DeleteDate = DateTime.Now;
                deletedItem.DeletedUserId = Current.getUserId;
                Current.Context.SaveChanges();
                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Araç başarı ile silinmiştir!";
            }
            catch (Exception)
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "İşlem başarısız olmuştur!";
            }
            return RedirectToAction("CarList");
        }
        #endregion

        #region Araç Yeniden Ekleme
        [AuthenticationFilter(UserType = "BSKM")]
        public ActionResult AddCarAgain(int Id)
        {
            try
            {

                CarService addAgain = Current.Context.CarService.FirstOrDefault(f => f.Id == Id);
                addAgain.Status = Constants.CAR_STATUS_ACTIVE;
                //addAgain.CreationDate = DateTime.Now;
                //addAgain.AddUserId = Current.getUserId;
                Current.Context.SaveChanges();
                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Araç başarı ile yeniden eklenmiştir!";
            }
            catch (Exception)
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "İşlem başarısız olmuştur!";
            }
            return RedirectToAction("CarList");
        }
        #endregion

        #region Araç  Detay
        [AuthenticationFilter(UserType = "BSKM")]
        public ActionResult CarDetail(int CarId)
        {
            int UserType = (int)Current.getUser.UserTypeId;
            if (UserType == Constants.USER_TYPE_BSKM)
            {
                List<int> avaibleCountryList = Current.Context.UserCountry.Where(f => f.UserId == Current.getUserId).Select(f => (int)f.CountryId).ToList();
                return View(Current.Context.vwCarList.FirstOrDefault(f => f.Id == CarId && avaibleCountryList.Contains((int)f.CountryId)));
            }
            else
            {
                return View(Current.Context.vwCarList.FirstOrDefault(f => f.Id == CarId));
            }

        }
        #endregion

        #region Yolcu Ekleme
        [AuthenticationFilter(UserType = "BSKM")]
        public ActionResult AddPassenger(int CarId)
        {
            ViewBag.CarId = CarId;
            return View();
        }

        [HttpPost]
        public ActionResult AddPassenger(ServicePassenger service)
        {
            try
            {
                service.AddUserId = Current.getUserId;
                service.AddTime = DateTime.Now;
                Current.Context.ServicePassenger.Add(service);
                Current.Context.SaveChanges();

                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Yolcu başarı ile eklenmiştir!";
            }
            catch (Exception)
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "Yolcu ekleme işlemi başarısız olmuştur!";
            }
            return RedirectToAction("CarDetail", new { CarId = service.ServiceId });
        }
        #endregion

        #region Yolcu Düzenle
        [AuthenticationFilter(UserType = "BSKM")]
        public ActionResult EditPassenger(int SpId)
        {
            return View(Current.Context.ServicePassenger.FirstOrDefault(f => f.Id == SpId));
        }
        [HttpPost]
        public ActionResult EditPassenger(ServicePassenger service)
        {
            ServicePassenger oldServicePassenger = Current.Context.ServicePassenger.FirstOrDefault(f => f.Id == service.Id);
            try
            {

                oldServicePassenger.Name = service.Name;
                oldServicePassenger.Surname = service.Surname;
                oldServicePassenger.TCK = service.TCK;
                oldServicePassenger.Phone = service.Phone;
                oldServicePassenger.PassengerCount = service.PassengerCount;
                Current.Context.SaveChanges();

                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Yolcu güncelleme işlemi başarılı olmuştur!";

            }
            catch (Exception)
            {

                TempData["ErrprType"] = "Fail";
                TempData["Message"] = "İşlem başarısız olmuştur!";
            }
            return RedirectToAction("CarDetail", new { CarId = oldServicePassenger.ServiceId });
        }
        #endregion

        #region Yolcu Sil
        [AuthenticationFilter(UserType = "BSKM")]
        public ActionResult DeletePassenger(int SpId)
        {
            ServicePassenger deleteService = Current.Context.ServicePassenger.FirstOrDefault(f => f.Id == SpId);
            try
            {
                Current.Context.ServicePassenger.Remove(deleteService);
                Current.Context.SaveChanges();

                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Yolcu silme işlemi başarılı olmuştur!";

            }
            catch (Exception)
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "İşlem başarısız olmuştur!";
            }
            return RedirectToAction("CarDetail", new { CarId = deleteService.ServiceId });
        }
        #endregion

        #region Sandık Yönetimi

        public ActionResult BoxList()
        {
            return View();
        }

        public ActionResult BoxDetail(int destinationId)
        {
            List<vwPollingBox> boxList = Current.Context.vwPollingBox.Where(w => w.DestinationId == destinationId).ToList();
            return PartialView(boxList);
        }

        public ActionResult AddAssociateMember(string tck)
        {
            PollingList pollingList = Current.Context.PollingList.FirstOrDefault(f => f.TCKimlikNo == tck);
            return PartialView(pollingList);
        }
      
        public bool SaveAssociateMember(int boxId, int memberId, int memberType = 0)
        {
            try
            {
                var box = Current.Context.PollingClerk.FirstOrDefault(f => f.BoxId == boxId);
                if (box == null)
                {
                    box = new PollingClerk();
                    box.BoxId = boxId;
                }
                if (memberType == 0) // asıl üye = 0, yedek = 1
                    box.AssociateMember = memberId;
                else
                    box.AssociateMemberSecondary = memberId;

                if (box.Id == 0)
                    Current.Context.PollingClerk.Add(box);
                Current.Context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion


        public string GetDestinations(int CountryId, int Selval)
        {

            List<Destination> destinationList = Current.Context.Destination.Where(f => f.CountryId == CountryId).ToList();
            string optionStr = "<option value=\"-1\">Seçiniz</option>";
            string selStr = "selected=\"selected\"";
            foreach (var item in destinationList.OrderBy(f => f.Box))
            {
                optionStr += "<option value=\"" + item.Id + "\" " + (Selval == item.Id ? selStr : "") + ">" + item.Box + "</option>";
            }
            return optionStr;

        }


        public static string TinyGridItem(string oldAttibute)
        {
            string newAttribute = "";
            if ((oldAttibute.Length >= 10) && (oldAttibute != null))
            {
                newAttribute = oldAttibute.Substring(0, 10).ToString() + "...";
                return newAttribute;
            }
            else
            {
                return oldAttibute;
            }
        }

    }
}