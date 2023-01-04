using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SessionLibrary
{

    public class SessionLibrary
    {
        private readonly string API = "https://develop-clickforms.appraisalworld.com/wsms/sessions";

        public async Task<object> CreateSession(int iid, string keyPath, List<Claim> claims, int expHours, DataDTO data = null)
        {

            //Read the key
            string privateKey = File.ReadAllText(keyPath);

            //Add expiration to the claims
            int timestamp = expHours * 60 * 60;

            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
            double result = Math.Floor(diff.TotalSeconds) + timestamp;

            claims.Add(new Claim("exp", result.ToString(), ClaimValueTypes.Integer));

            //Generate the token
            var token = CreateToken(claims, privateKey, iid);

            //Call the API
            var httpClient = new HttpClient(new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                PreAuthenticate = true,
                Credentials = CredentialCache.DefaultCredentials
            });

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            string content = data == null ? "" :
                JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }).ToString();

            var stringContent = new StringContent(content, Encoding.UTF8, "application/json");

            var _response = await httpClient.PostAsync(API, stringContent);

            var streamResponse = await _response.Content.ReadAsStreamAsync();

            string response;

            using (var streamReader = new StreamReader(streamResponse))
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

                Dictionary<string, dynamic> _payload = new Dictionary<string, dynamic>();

                foreach (Claim claim in claims)
                {
                    var type = claim.ValueType;
                    dynamic value = claim.Value;

                    if (type.Contains("integer"))
                    {
                        value = Convert.ChangeType(claim.Value, System.Type.GetType("System.Int32"));
                    }

                    _payload.Add(claim.Type, value);
                }

                var headers = new Dictionary<string, object>()
                {
                     { "kid", iid}
                };

                return Jose.JWT.Encode(_payload, rsa, Jose.JwsAlgorithm.RS256, headers);
            }
        }
    }
    public class DataDTO
    {
        public bool Validity { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public StartupOptions Startup { get; set; }
    }
    public class StartupOptions
    {
        public bool OpenFormsLibrary { get; set; }
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