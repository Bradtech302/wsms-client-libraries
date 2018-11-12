using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SessionLibrary;
using System.Net;
using System.Security.Claims;

namespace Testing
{
    public partial class testSession : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            int iid = 998;
            var claims = new List<Claim>();

            claims.Add(new Claim("iat", DateTime.Now.ToString("M/d/yyyy"), ClaimValueTypes.Integer64));
            claims.Add(new Claim("iss", "develop"));
            claims.Add(new Claim("userId", "111"));
            claims.Add(new Claim("userType", "true"));
            claims.Add(new Claim("productId", "101"));
            claims.Add(new Claim("orgName", "testOrg"));
            claims.Add(new Claim("jti", Guid.NewGuid().ToString()));

            int expHours = 24;

            WSMSClient libObj = new WSMSClient();
            txtResponse.Text = libObj.CreateSession(iid, @"D:\Training\C# code which can generate JWT\ClassLibraryProjects\SessionLibrary\bin\Debug\sandbox_key.pem", claims, expHours).ToString();

        }
    }
}