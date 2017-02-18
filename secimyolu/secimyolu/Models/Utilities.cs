using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace secimyolu.Models
{
    public class Utilities
    {
        public static string getDestinationOptionList(int selVal)
        {
            string optionStr = "<option value=\"-1\">Seçiniz</option>";
            string selStr = "selected=\"selected\"";
            List<Destination> destList = Current.Context.Destination.ToList();
            foreach(var item in destList.OrderBy(d=>d.Box))
            {
                optionStr += "<option value=\"" + item.Id + "\" " + (item.Id == selVal ? selStr : "") + ">" + item.Box + "</option>";
            }
            return optionStr;
        }

        public static string getCountryOptionList(int selVal,bool ? isMultipleSelect=false)
        {
            string optionStr = "";
            if (isMultipleSelect == false)
            {
                optionStr = "<option value=\"-1\">Seçiniz</option>"; 
            }
          
            string selStr = "selected=\"selected\"";
            List<Country> countryList = Current.Context.Country.ToList();
            foreach (var item in countryList.OrderBy(f => f.Name).ToList())
            {
                optionStr += "<option value=\"" + item.Id + "\" " + (selVal == item.Id ? selStr : " ") + ">" + item.TrName + "</option>";
            }
            return optionStr;
        }

        public static string getCountryOptionListForMultipleSelect(List<int> selVal)
        {
            string optionStr = "";
            string selStr = "selected=\"selected\"";
            List<Country> countryList = Current.Context.Country.ToList();
            foreach (var item in countryList.OrderBy(f => f.Name).ToList())
            {
                optionStr += "<option value=\"" + item.Id + "\" " + (selVal.Contains(item.Id) ? selStr : " ") + ">" + item.Name + "</option>";
            }
            return optionStr;
        }

        /// <summary>
        /// girilen string değerini md5 ile şifreleyip döndürür
        /// </summary>
        /// <param name="sPASSWORD"></param>
        /// <returns></returns>
        public static string ConvertToMD5(string sPASSWORD)
        {
            MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
            byte[] bs = Encoding.UTF8.GetBytes(sPASSWORD);
            bs = x.ComputeHash(bs);
            StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }

        public static string getUserTypeList(int selVal)
        {
            string optionStr = "<option value=\"-1\">Seçiniz</option>";
            string selStr = "selected=\"selected\"";
            List<UserType> destList = Current.Context.UserType.ToList();
            foreach (var item in destList.OrderBy(d => d.Name))
            {
                optionStr += "<option value=\"" + item.Id + "\" " + (item.Id == selVal ? selStr : "") + ">" + item.Name + "</option>";
            }
            return optionStr;
        }

        public static string GetCarTypeList(int selVal)
        {
            string optionStr = "<option value=\"-1\">Seçiniz</option>";
            string selStr = "selected=\"selected\"";
            List<CarType> typeList = Current.Context.CarType.ToList();
            foreach (var item in typeList.OrderBy(d => d.Name))
            {
                optionStr += "<option value=\"" + item.Id + "\" " + (item.Id == selVal ? selStr : "") + ">" + item.Name + "</option>";
            }
            return optionStr;
        }
        public static string getPassengerCountOption(int maxCount, int selVal = -1)
        {
            string optionStr = "";
            string selStr = "selected=\"selected\"";
            for (int i = 1; i <= maxCount; i++)
            {
                optionStr += "<option value=\"" + i + "\" " + (selVal == i ? selStr : "") + ">" + i + "</option>";
            }
            return optionStr;
        }
    }
}