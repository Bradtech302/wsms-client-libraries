using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SessionLibrary
{
    public class WSMSClient
    {

       
        public string createSession(int iid, string keyPath, List<Claim> claims, int expHours)
        {

            //Read the key
            string privateKey = File.ReadAllText(keyPath);

            //Add expiration to the claims
            //int exp = (expHours * 60 * 60);
            //claims.Add(new Claim("exp", exp));


            //Generate the token
            var token = CreateToken(claims, privateKey ,iid);

           
        
            //Call the API
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://develop-clickforms.appraisalworld.com/wsms/sessions");

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

            return response;
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
}
