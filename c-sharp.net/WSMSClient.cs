using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SessionLibrary
{
    public class WSMSClient
    {

       
        public object CreateSession(int iid, string keyPath, List<Claim> claims, int expHours)
        {

            //Read the key
            string privateKey = File.ReadAllText(keyPath);

            //Add expiration to the claims
            int timestamp = expHours * 60 * 60;

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
            double result = Math.Floor(diff.TotalSeconds) + timestamp;

            claims.Add(new Claim("exp", result.ToString(), ClaimValueTypes.Integer64));


            //Generate the token
            var token = CreateToken(claims, privateKey ,iid);

            //Call the API
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://clickforms.appraisalworld.com/wsms/sessions");
           
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + token);
            httpWebRequest.UseDefaultCredentials = true;
            httpWebRequest.PreAuthenticate = true;
            httpWebRequest.Credentials = CredentialCache.DefaultCredentials;

            WebResponse httpResponse = httpWebRequest.GetResponse();

            string response;

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
            }

            var sessionId = JsonConvert.DeserializeObject<ApiError>(response).sessionId;
            var url = JsonConvert.DeserializeObject<ApiError>(response).url;

            var res = new { Session_Id = sessionId, Url = url };

            return res;
           
        }

   

        public string CreateToken(List<Claim> claims, string privateRsaKey, int iid)
        {
            RSAParameters rsaParams;
            using (var tr = new StringReader(privateRsaKey))
            {
                var pemReader = new PemReader(tr);
                var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;

                if (keyPair == null)
                {
                    throw new Exception("Could not read RSA private key");
                }

                var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
            }

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaParams);
                Dictionary<string, object> payload = claims.ToDictionary(k => k.Type, v => (object)v.Value);

                var headers = new Dictionary<string, object>()
                {
                     { "kid", iid}
                };

                return Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256, headers);
            }
        }

    }
    [JsonConverter(typeof(ApiErrorConverter))]
    public class ApiError
    {
        [JsonProperty("iid")]
        public int iid { get; set; }

        [JsonProperty("sessionId")]
        public string sessionId { get; set; }

        [JsonProperty("url")]
        public string url { get; set; }

    }

    public class ApiErrorConverter : JsonConverter
    {
        private readonly Dictionary<string, string> _propertyMappings = new Dictionary<string, string>
        {
            {"iid", "iid"},
            {"sessionId", "sessionId"},
            {"url", "url"}
        };

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsClass;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object instance = Activator.CreateInstance(objectType);
            var props = objectType.GetTypeInfo().DeclaredProperties.ToList();

            JObject jo = JObject.Load(reader);
            foreach (JProperty jp in jo.Properties())
            {
                if (!_propertyMappings.TryGetValue(jp.Name, out var name))
                    name = jp.Name;

                PropertyInfo prop = props.FirstOrDefault(pi =>
                    pi.CanWrite && pi.GetCustomAttribute<JsonPropertyAttribute>().PropertyName == name);

                prop?.SetValue(instance, jp.Value.ToObject(prop.PropertyType, serializer));
            }

            return instance;
        }
    }
}
