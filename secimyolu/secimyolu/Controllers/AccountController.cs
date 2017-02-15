using Facebook;
using secimyolu.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;

namespace secimyolu.Controllers
{
    public class AccountController : BaseController
    {
        public ActionResult Index(string ReturnUrl, bool isPopup = false)
        {
            if (Current.isLogon)
                return null;
            if (isPopup)
                return View();
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public JsonResult LoginAction(string Email, string Password)
        {
            User lgnUser = Current.Context.User.FirstOrDefault(d => d.Email == Email);

            if (lgnUser != null)
            {
                Current.setSessionItem("userInfo", lgnUser);
                if (lgnUser.Password != Utilities.ConvertToMD5(Password))
                {
                    return
                        Json(
                            new
                            {
                                IsLogin = 0,
                                Message = "Kullanıcı adı ve ya şifre hatalı."
                            }, JsonRequestBehavior.AllowGet);
                }
                if (lgnUser.UserTypeId == Constants.USER_TYPE_BSKM || lgnUser.UserTypeId == Constants.USER_TYPE_YSKM)
                {
                    return
                        Json(
                            new
                            {
                                IsLogin = 1,
                                ReturnUrl = Url.Action("CarList", "Management"),
                                Message = ""
                            }, JsonRequestBehavior.AllowGet);
                }
                else if (lgnUser.UserTypeId == Constants.USER_TYPE_GUEST)
                {
                    return
                        Json(
                            new
                            {
                                IsLogin = 1,
                                ReturnUrl = "",
                                Message = ""
                            }, JsonRequestBehavior.AllowGet);
                }
            }
            return
                        Json(
                            new
                            {
                                IsLogin = 0,
                                Message = "Kullanıcı adı ve ya şifre hatalı."
                            }, JsonRequestBehavior.AllowGet);
        }

        [SessionControlFilter]
        public ActionResult Edit()
        {
            return View(Current.Context.User.FirstOrDefault(d => d.Id == Current.getUserId));
        }

        [HttpPost]
        public ActionResult Edit(User user)
        {
            User oldUser = Current.Context.User.FirstOrDefault(d => d.Id == Current.getUserId);
            if (oldUser != null)
            {
                oldUser.Name = user.Name;
                oldUser.Surname = user.Surname;
                oldUser.GSM = user.GSM;
                oldUser.TCNo = user.TCNo;
                if (user.Password != null)
                {
                    oldUser.Password = user.Password;
                }
                Current.Context.SaveChanges();
            }
            Current.setSessionItem("userInfo", user);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            if (Session["accessToken"] != null)
            {
                var fb = new FacebookClient();
                string accessToken = Session["accessToken"] as string;
                var logoutUrl = fb.GetLogoutUrl(new { access_token = accessToken, next = "http://www.secimyolu.com/" });

                Session.RemoveAll();
            }
            Current.setSessionItem("userInfo", null);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(User user, string CaptchaCode)
        {
            string DogrulamaKodu = Session["DogrulamaKodu"] != null ? Session["DogrulamaKodu"].ToString() : "";

            /*Captca Kontrolü koyuluyor*/
            if (DogrulamaKodu != CaptchaCode)
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "Doğrulama kodu geçersiz!";
                return View("Register", user);
            }
            if (user != null)
            {
                user.UserTypeId = Constants.USER_TYPE_GUEST;
                user.Password = Utilities.ConvertToMD5(user.Password);
                user.Status = true;
                Current.Context.User.Add(user);
                Current.Context.SaveChanges();
                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Üyelik işleminiz başarıyla gerçekleşmiştir!";
            }
            else
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "Üyelik işlemi sırasında bir hata oluştu lütfen tekrar deneyiniz!";
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult ResetPasswordRequest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPasswordRequest(string Email)
        {

            if (checkEmailExist(Email))
            {
                User resetWho = Current.Context.User.FirstOrDefault(f => f.Email == Email.Trim());
                if (resetWho != null)
                {
                    /*Gönderilen link hala aktifse ve kullanıcı tekrardan istekte bulunursa kullanılabilir bir key gönderilir*/

                    ResetPassword isUseable =
                        Current.Context.ResetPassword.FirstOrDefault(
                            f => f.UserId == resetWho.Id && f.IsValid == true && DateTime.Now < f.ExpirationTime);
                    if (isUseable != null)
                    {
                        string link = Current.ServerRootUrl + "/Account/ResetPasswordUser?UniqueKey=" + isUseable.Guid;
                        CreateMailToUser(resetWho.Id, Constants.MAIL_TYPE_RESET_PASSWORD, link);
                    }
                    else
                    {
                        ResetPassword newResetRequest = new ResetPassword
                        {
                            UserId = resetWho.Id,
                            IsValid = true,
                            Guid = Guid.NewGuid().ToString(),
                            CreationTime = DateTime.Now,
                            ExpirationTime = DateTime.Now.AddDays(2)
                        };
                        Current.Context.ResetPassword.Add(newResetRequest);
                        Current.Context.SaveChanges();

                        string link = "http://www.secimyolu.com/Account/ResetPasswordUser?UniqueKey=" +
                                      newResetRequest.Guid; // DEĞİŞECEK && TEMPLATE Değişecek
                        CreateMailToUser(resetWho.Id, Constants.MAIL_TYPE_RESET_PASSWORD, link);
                    }
                }


                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Şifre sıfırlama bağlanıtısı mail adresinize gönderilmiştir!";
                return RedirectToAction("Index", "Home");

            }
            else
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "Geçersiz mail adresi!";
                return View();
            }


        }

        /*Sifre talebini basari ile olusturan kullanici sifresini sifirlayabilecektir*/

        public ActionResult ResetPasswordUser(string UniqueKey)
        {
            ResetPassword isValid =
                Current.Context.ResetPassword.FirstOrDefault(
                    f => f.Guid == UniqueKey && f.IsValid == true && DateTime.Now < f.ExpirationTime);

            if (isValid != null)
            {
                ViewBag.UCode = isValid.Guid;
                return View(Current.Context.User.FirstOrDefault(f => f.Id == isValid.UserId));
            }
            else
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "Gönderilen bağlantı geçerliliğini kaybetmiştir! Yeniden şifre sıfırlama talebinde bulununuz!";
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        public ActionResult ResetPasswordUser(string UniqueKey, int UserId, string newPassword)
        {

            User userWho = Current.Context.User.FirstOrDefault(f => f.Id == UserId);

            /*Sifrenin standartlara uygunluk fonksiyonu koyulacaktir*/


            ResetPassword resetWho =
                Current.Context.ResetPassword.FirstOrDefault(
                    f => f.UserId == UserId && f.Guid == UniqueKey && f.IsValid == true);
            if (resetWho != null)
            {
                /*Şifre sıfırlanır*/
                User resetUserPassword = Current.Context.User.FirstOrDefault(f => f.Id == UserId);
                resetUserPassword.Password = Utilities.ConvertToMD5(newPassword);
                Current.Context.SaveChanges();

                /*Şifre sıfırlandıktan sonra link pasif yapılır*/
                resetWho.IsValid = false;
                Current.Context.SaveChanges();
                TempData["ErrorType"] = "Success";
                TempData["Message"] = "Şifre sıfırlama işlemi başarılıdır!";
                return RedirectToAction("ResetPasswordSuccess",
                    new { Who = userWho.Name + " " + userWho.SecondName + " " + userWho.Surname });
            }
            else
            {
                TempData["ErrorType"] = "Fail";
                TempData["Message"] = "Şifre sıfırlama işlemi başarısızdır!";
                ViewBag.UCode = UniqueKey;
                return View(userWho);
            }
        }


        public ActionResult ResetPasswordSuccess(string Who)
        {
            ViewBag.Who = Who;
            return View();
        }





        public void CreateMailToUser(int UserId, int mType, string parameter)
        {
            User mailToUser = Current.Context.User.FirstOrDefault(f => f.Id == UserId);

            /*Şifre sıfırlama maili*/
            if (mType == Constants.MAIL_TYPE_RESET_PASSWORD)
            {
                ResetPasswordMail newLayout = new ResetPasswordMail
                {
                    UserName = mailToUser.Name + " " + mailToUser.Surname,
                    ActivationLink = parameter
                };

                string ContentMessage = ViewRenderer.RenderView("~/Views/Shared/MailLayouts/ResetPassword.cshtml",
                    newLayout, ControllerContext);

                Mail newMail = new Mail
                {
                    MailTo = UserId,
                    MailFrom = 1,
                    MailContent = ContentMessage,
                    Status = Constants.MAIL_STATUS_WAITING,
                    MailSubject = "Şifre Sıfırlama"
                };
                Current.Context.Mail.Add(newMail);
                Current.Context.SaveChanges();
                SecimMail.MailGonder(UserId, "Şifre Sıfırlama", ContentMessage, newMail.id);
            }
        }


        #region facebook 

        [HttpPost]
        public JsonResult UserDetail(FacebookLoginModel model)
        {
            Session["uid"] = model.uid;
            Session["accessToken"] = model.accessToken;
            var client = new FacebookClient(model.accessToken);
            dynamic fbresult = client.Get("me?fields=id,email,first_name,last_name,gender");
            FacebookUserModel facebookUser =
                Newtonsoft.Json.JsonConvert.DeserializeObject<FacebookUserModel>(fbresult.ToString());
            User lgnUser = Current.Context.User.FirstOrDefault(d => d.FacebookId == facebookUser.id);
            if (lgnUser != null)
            {
                Current.setSessionItem("userInfo", lgnUser);
            }
            else
            {
                var newUser = new User();
                var oldUserInfo = Current.Context.User.FirstOrDefault(a => a.Email == facebookUser.email);
                if (oldUserInfo!=null)
                {
                    //eğer adam kendi mail adresiyle daha önceden kaydolmuşsa bu hesapla facebooku eşleştirelim
                    oldUserInfo.FacebookId = facebookUser.id;
                    Current.Context.SaveChanges();
                    Current.setSessionItem("userInfo", oldUserInfo);
                }
                else
                {
                    newUser = new User
                    {
                        Name = facebookUser.first_name,
                        Surname = facebookUser.last_name,
                        Email = facebookUser.email,
                        FacebookId = facebookUser.id,
                        UserTypeId = Constants.USER_TYPE_GUEST,
                        Status = true
                    };
                    Current.Context.User.Add(newUser);
                    Current.Context.SaveChanges();
                    Current.setSessionItem("userInfo", newUser);
                }
               

            }

            return Json(new { success = true });

        }

        [HttpPost]
        public JsonResult FacebookLogin(FacebookLoginModel model)
        {
            Session["uid"] = model.uid;
            Session["accessToken"] = model.accessToken;

            return Json(new { success = true });
        }

        [HttpGet]
        public ActionResult UserDetails()
        {
            var client = new FacebookClient(Session["accessToken"].ToString());
            dynamic fbresult =
                client.Get(
                    "me?fields=id,email,first_name,last_name,gender,locale,link,username,timezone,location,picture");
            FacebookUserModel facebookUser =
                Newtonsoft.Json.JsonConvert.DeserializeObject<FacebookUserModel>(fbresult.ToString());

            return View(facebookUser);
        }

        #endregion

        #region google

        [HttpPost]
        public async Task<ActionResult> GoogleLogin(string tokenID, string ReturnURL)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync("https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + tokenID);// https://www.googleapis.com/oauth2/v1/userinfo?access_token
                string content = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var googleUser = JsonConvert.DeserializeObject<GoogleTokenInfo>(content);
                    if (googleUser.email_verified)
                    {
                        //başarılı
                        var lgnUser = Current.Context.User.FirstOrDefault(d => d.Email == googleUser.email);
                        if (lgnUser != null) {
                            Current.setSessionItem("userInfo", lgnUser);
                            if (lgnUser.UserTypeId == Constants.USER_TYPE_BSKM || lgnUser.UserTypeId == Constants.USER_TYPE_YSKM)
                            {
                                return RedirectToAction("CarList", "Management");
                            }
                        }
                        else
                        {
                            var newUser = new User
                            {
                                Name = googleUser.given_name,
                                Surname = googleUser.family_name,
                                Email = googleUser.email,
                                UserTypeId = Constants.USER_TYPE_GUEST,
                                Status = true
                            };
                            Current.Context.User.Add(newUser);
                            Current.Context.SaveChanges();
                            Current.setSessionItem("userInfo", newUser);
                        } 
                    }
                }
                
            }
            return RedirectToAction("Index", "Home");
        }
        #endregion




        public ActionResult Show(string key)
        {
            var randomText = GenerateRandomText(6);
            Session["DogrulamaKodu"] = randomText;
            var hash = ComputeMd5Hash(randomText + GetSalt());
            //Session["CaptchaHash"] = hash;

            var rnd = new Random();
            var fonts = new[] { "Oswald", "sans-serif" };
            float orientationAngle = rnd.Next(0, 359);
            const int height = 80;
            const int width = 190;
            var index0 = rnd.Next(0, fonts.Length);
            var familyName = fonts[index0];
            /*Eski yapıya ekleme*/
            int[] aFontEmSizes = new int[] { 18, 22, 26, 30, 34 };
            string[] aFontNames = new string[]
            {
             "Comic Sans MS",
             "Arial",
             "Times New Roman",
             "Georgia",
             "Verdana",
             "Geneva"
            };
            FontStyle[] aFontStyles = new FontStyle[]
            {
             FontStyle.Bold,
             FontStyle.Italic,
             FontStyle.Regular,
             FontStyle.Strikeout,
             FontStyle.Underline
            };

            HatchStyle[] aHatchStyles = new HatchStyle[]
            {
             HatchStyle.BackwardDiagonal, HatchStyle.Cross,
                HatchStyle.DashedDownwardDiagonal, HatchStyle.DashedHorizontal,
             HatchStyle.DashedUpwardDiagonal, HatchStyle.DashedVertical,
                HatchStyle.DiagonalBrick, HatchStyle.DiagonalCross,
             HatchStyle.Divot, HatchStyle.DottedDiamond, HatchStyle.DottedGrid,
                HatchStyle.ForwardDiagonal, HatchStyle.Horizontal,
             HatchStyle.HorizontalBrick, HatchStyle.LargeCheckerBoard,
                HatchStyle.LargeConfetti, HatchStyle.LargeGrid,
             HatchStyle.LightDownwardDiagonal, HatchStyle.LightHorizontal,
                HatchStyle.LightUpwardDiagonal, HatchStyle.LightVertical,
             HatchStyle.Max, HatchStyle.Min, HatchStyle.NarrowHorizontal,
                HatchStyle.NarrowVertical, HatchStyle.OutlinedDiamond,
             HatchStyle.Plaid, HatchStyle.Shingle, HatchStyle.SmallCheckerBoard,
                HatchStyle.SmallConfetti, HatchStyle.SmallGrid,
             HatchStyle.SolidDiamond, HatchStyle.Sphere, HatchStyle.Trellis,
                HatchStyle.Vertical, HatchStyle.Wave, HatchStyle.Weave,
             HatchStyle.WideDownwardDiagonal, HatchStyle.WideUpwardDiagonal, HatchStyle.ZigZag
            };

            using (var bmpOut = new Bitmap(width, height))
            {
                var g = Graphics.FromImage(bmpOut);

                var gradientBrush = new LinearGradientBrush(new Rectangle(0, 0, width, height),
                                                           Color.Transparent, Color.Transparent,
                                                           orientationAngle);
                g.FillRectangle(gradientBrush, 0, 0, width, height);
                DrawRandomLines(ref g, width, height);
                System.Drawing.Drawing2D.Matrix oMatrix = new System.Drawing.Drawing2D.Matrix();
                int i = 0;
                for (i = 0; i <= randomText.Length - 1; i++)
                {
                    oMatrix.Reset();
                    int iChars = randomText.Length;
                    int x = width / (iChars + 1) * i;
                    int y = height / 2;

                    //Rotate text Random
                    oMatrix.RotateAt(rnd.Next(-40, 40), new PointF(x, y));
                    g.Transform = oMatrix;

                    //Draw the letters with Random Font Type, Size and Color
                    g.DrawString
                    (
                    //Text
                    randomText.Substring(i, 1),
                    //Random Font Name and Style
                    new Font(aFontNames[rnd.Next(aFontNames.Length - 1)],
                       aFontEmSizes[rnd.Next(aFontEmSizes.Length - 1)],
                       aFontStyles[rnd.Next(aFontStyles.Length - 1)]),
                    //Random Color (Darker colors RGB 0 to 100)
                    new SolidBrush(Color.FromArgb(52, 158, 209)),
                    x,
                    rnd.Next(10, 40)
                    );
                    g.ResetTransform();
                }
                /*Ekleme Sonrası*/
                MemoryStream oMemoryStream = new MemoryStream();
                bmpOut.Save(oMemoryStream, System.Drawing.Imaging.ImageFormat.Png);
                byte[] oBytes = oMemoryStream.GetBuffer();
                var bmpBytes = oMemoryStream.GetBuffer();
                bmpOut.Dispose();
                oMemoryStream.Close();
                return new FileContentResult(bmpBytes, "image/png");
            }
        }

        private string GetSalt()
        {
            return typeof(HomeController).Assembly.FullName;
        }


        private string GenerateRandomText(int textLength)
        {
            const string chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ1234567890";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, textLength)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
            return result;
        }

        private void DrawRandomLines(ref Graphics g, int width, int height)
        {
            var rnd = new Random();
            var pen = new Pen(Color.Gray);
            for (var i = 0; i < 0; i++)
            {
                g.DrawLine(pen, rnd.Next(0, width), rnd.Next(0, height),
                                rnd.Next(0, width), rnd.Next(0, height));
            }
        }

        private string ComputeMd5Hash(string input)
        {
            var encoding = new ASCIIEncoding();
            var bytes = encoding.GetBytes(input);
            HashAlgorithm md5Hasher = MD5.Create();
            return BitConverter.ToString(md5Hasher.ComputeHash(bytes));
        }


        public bool checkEmailExist(String Email)
        {
            if (Current.Context.User.Count(i => i.Email == Email) > 0)
            {
                return true;
            }
            return false;
        }
    }
}