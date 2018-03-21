using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using System.Globalization;


namespace EoS
{
    public class HelpFunctions
    {
        public static string GetShortCode()
        {

            
            Random random = new Random();
            string x = null;

            //get 5 digits random number
            for (int i = 0; i < 5 ; i++) { 
                x = String.Concat(x, random.Next(10).ToString());
            }

           
            //get current year & month 
            string date = DateTime.Now.ToString("yyMM");
            string code =  date + x;
            return code;
     }
    }
}


