﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VDebugLib
{
    public class VDebug
    {

        static private readonly bool isOn = true;
        static private readonly Uri baseAddress = new Uri("http://localhost:7000/vdebug/in");

        static VDebug()
        {
        }

        public static void Init()
        {
            if (!isOn) return;
            Log("New session");
        }

        public static void Log(object obj)
        {
            if (!isOn) return;
            var wait = LogAsync(obj).Result;
        }

        private static async Task<string> LogAsync(object obj)
        {
            // TODO: warp any data in valid VDebug-log object

            // if logged data is string, warp it as an object
            // This is done because VDebug server expect only JSON content type
            if (obj is string)
            {
                obj = new { stringValue = obj };
            }

            // create HTTP Client and send HTTP post request
            var client = new HttpClient();
            var result = client.PostAsJsonAsync(baseAddress.ToString(), obj).Result;

            string resultContent = result.Content.ReadAsStringAsync().Result;

            // TODO: Check that response was successful or throw exception
            //response.EnsureSuccessStatusCode();

            // Read result as Contact
            //Contact result = await response.Content.ReadAsAsync<Contact>();

            return resultContent;
        }
    }
}
