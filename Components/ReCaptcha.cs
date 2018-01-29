using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json; 

namespace DotNetNuke.Modules.UserDefinedTable.Components
{
    public class ReCaptcha
    {
        public static bool Validate(string encodedResponse, string privateKey)
        {
            var client = new System.Net.WebClient();
            var googleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", privateKey, encodedResponse));
            var captchaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ReCaptcha>(googleReply);
            return captchaResponse.Success;
        }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}