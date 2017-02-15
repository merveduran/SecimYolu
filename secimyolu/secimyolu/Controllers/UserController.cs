using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using secimyolu.Models;

namespace secimyolu.Controllers
{
    [SessionControlFilter]
    [AuthenticationFilter(UserType = "YSKM")]
    public class UserController : BaseController
    {
        //
        // GET: /User/

        #region KULLANICI LİSTESİ


        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetUserList(jQueryDataTableParamModel param)
        {
            var name = Convert.ToString(Request["sSearch_0"]);
            var gsm = Convert.ToString(Request["sSearch_1"]);
            var tip = Convert.ToString(Request["sSearch_2"]);
            IQueryable<vwUserList> regList = Current.Context.vwUserList;


            if (!String.IsNullOrEmpty(name))
                regList = regList.Where(d => d.Name.Contains(name));

            if (!String.IsNullOrEmpty(gsm))
                regList = regList.Where(d => d.GSM.Contains(gsm));

            if (!String.IsNullOrEmpty(tip))
                regList = regList.Where(d => d.UserTypeName.Contains(tip));

            int count = regList.Count();
            //order
            //-------------------------------------------------------------------
            var sortColumnIndex = Convert.ToInt32(Request["iSortCol_0"]);
            string orderingFunction = sortColumnIndex == 1 ? "Name" : sortColumnIndex == 2 ? "GSM" : sortColumnIndex == 3 ?"UserTypeName" : "Id";
            var sortDirection = Request["sSortDir_0"];
            regList = OrderClass.OrderBy<vwUserList>(regList, orderingFunction, sortDirection);
            //-------------------------------------------------------------------
            var data = regList.Skip(param.iDisplayStart).Take(param.iDisplayLength).ToList().Select(r => new[] { r.Id.ToString(), r.Name + " " + r.Surname, r.GSM,r.UserTypeName ,r.Status.ToString() });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = count,
                iTotalDisplayRecords = count,
                aaData = data
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region KULLANICI EKLEME
        public ActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateUser(User user, List<int> Country)
        {
          
            if (user != null)
            {
                if (Current.Context.User.Where(d => d.Email == user.Email).Count() > 0)
                {
                    TempData["ErrorType"] = "Fail";
                    TempData["Message"] = "Bu eposta adresine ait kullanıcı mevcuttur!";
                    return View("Index");
                }
                if (user.Email == null)
                {
                    TempData["ErrorType"] = "Fail";
                    TempData["Message"] = "Eposta adresi boş geçilemez!";
                    return View("Index");
                }
                User newUser = new User();
                newUser.Email = user.Email;
                newUser.Password = Utilities.ConvertToMD5(user.Password);
                newUser.Name = user.Name;
                newUser.UserTypeId = user.UserTypeId;
                newUser.Surname = user.Surname;
                newUser.Status = true;

                Current.Context.User.Add(newUser);
                Current.Context.SaveChanges();



                if (Country!=null && Country.Count() > 0)
                {
                    foreach (var item in Country)
                    {
                        if (item != -1)
                        {
                            UserCountry newUserCountry = new UserCountry
                            {
                                UserId = newUser.Id,
                                CountryId = item
                            };
                            Current.Context.UserCountry.Add(newUserCountry);
                            Current.Context.SaveChanges();
                        }
                    }
                }

                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Kullanıcı ekleme işlemi başarı ile sonuçlanmıştır!";
            }
            else
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "İşlem başarısız olmuştur!";
            }


            return View("Index");
        }
        #endregion

        #region KULLANICI SİLME

        public ActionResult DeleteUser(int Id)
        {
            User currUser = Current.Context.User.FirstOrDefault(d => d.Id == Id);
            if (currUser != null)
            {
                if (currUser.Status == false)
                {
                    currUser.Status = true;
                    TempData["ErrorType"] = "Success";
                    TempData["Message"] = "Kullanıcı başarıyla aktifleştirilmiştir!";
                }
                else
                {
                    currUser.Status = false;
                    TempData["ErrorType"] = "Success";
                    TempData["Message"] = "Kullanıcı başarıyla pasif hale getirilmiştir!";
                }

                Current.Context.SaveChanges();
               
            }
            else
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "İşlem başarısız olmuştur!";
            }
            return View("Index");
        }
        #endregion

        #region KULLANICI GÜNCELLEME

        public ActionResult EditUser(int Id)
        {
            return View(Current.Context.User.FirstOrDefault(d => d.Id == Id));
        }

        [HttpPost]
        public ActionResult EditUser(User user, List<int> Country)
        {
            if (user != null && user.Id != -1)
            {
                User usr = Current.Context.User.FirstOrDefault(d => d.Id == user.Id);
                if (usr != null)
                {
                    usr.Name = user.Name;
                    usr.Surname = user.Surname;
                    usr.GSM = user.GSM;
                    usr.UserTypeId = user.UserTypeId;
                }
                Current.Context.SaveChanges();
                List<UserCountry> oldCountryList = Current.Context.UserCountry.Where(d => d.UserId == user.Id).ToList();
                Current.Context.UserCountry.RemoveRange(oldCountryList);
                if (Country.Count() > 0)
                {
                    foreach (var item in Country)
                    {
                        if (item != -1)
                        {
                            UserCountry newUserCountry = new UserCountry
                            {
                                UserId = user.Id,
                                CountryId = item
                            };
                            Current.Context.UserCountry.Add(newUserCountry);
                            Current.Context.SaveChanges();
                        }
                    }
                }
                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Kullanıcı güncelleme işlemi başarı ile sonuçlanmıştır!";
            }
            else
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "İşlem başarısız olmuştur!";
            }

            return View("Index");
        }
        #endregion

        #region YETKİ DÜZENLEME

        public ActionResult UserRight(int UserId)
        {
            return View(UserId);
        }

        [HttpPost]
        public ActionResult UserRight(List<int> RightIds, int UserId)
        {
            List<UserRight> oldUserRightList = Current.Context.UserRight.Where(f => f.UserId == UserId).ToList();

            Current.Context.UserRight.RemoveRange(oldUserRightList);
            Current.Context.SaveChanges();

            foreach (var rightId in RightIds)
            {
                UserRight newRole = new UserRight { UserId = UserId, RightId = rightId };
                Current.Context.UserRight.Add(newRole);
                Current.Context.SaveChanges();
            }
            return View("Index");
        }
        #endregion
    }
}