﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RazorTests.Views.Home
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Optimization;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Home/RegisterNewUser.cshtml")]
    public partial class RegisterNewUser : System.Web.Mvc.WebViewPage<RazorTests.Models.PasswordModel>
    {
        public RegisterNewUser()
        {
        }
        public override void Execute()
        {
            
            #line 3 "..\..\Views\Home\RegisterNewUser.cshtml"
  
    ViewBag.Title = "Register New User";

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n");

            
            #line 7 "..\..\Views\Home\RegisterNewUser.cshtml"
 using (Html.BeginForm()) {
    
            
            #line default
            #line hidden
            
            #line 8 "..\..\Views\Home\RegisterNewUser.cshtml"
Write(Html.ValidationSummary(true));

            
            #line default
            #line hidden
            
            #line 8 "..\..\Views\Home\RegisterNewUser.cshtml"
                                 


            
            #line default
            #line hidden
WriteLiteral("    <fieldset>\r\n        <legend>Register New User</legend>\r\n\r\n        <p>Hello ");

            
            #line 13 "..\..\Views\Home\RegisterNewUser.cshtml"
            Write(ViewBag.UserInfo.FirstName);

            
            #line default
            #line hidden
WriteLiteral(".  Please create a password to complete your registration:</p>\r\n\r\n        <li>\r\n");

WriteLiteral("            ");

            
            #line 16 "..\..\Views\Home\RegisterNewUser.cshtml"
       Write(Html.LabelFor(m => m.Password));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("            ");

            
            #line 17 "..\..\Views\Home\RegisterNewUser.cshtml"
       Write(Html.PasswordFor(m => m.Password));

            
            #line default
            #line hidden
WriteLiteral("\r\n        </li>\r\n        <li>\r\n");

WriteLiteral("            ");

            
            #line 20 "..\..\Views\Home\RegisterNewUser.cshtml"
       Write(Html.LabelFor(m => m.ConfirmPassword));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("            ");

            
            #line 21 "..\..\Views\Home\RegisterNewUser.cshtml"
       Write(Html.PasswordFor(m => m.ConfirmPassword));

            
            #line default
            #line hidden
WriteLiteral("\r\n        </li>\r\n        <input");

WriteLiteral(" name=\"token\"");

WriteAttribute("value", Tuple.Create(" value=\"", 616), Tuple.Create("\"", 638)
            
            #line 23 "..\..\Views\Home\RegisterNewUser.cshtml"
, Tuple.Create(Tuple.Create("", 624), Tuple.Create<System.Object, System.Int32>(ViewBag.Token
            
            #line default
            #line hidden
, 624), false)
);

WriteLiteral(" type=\"hidden\"");

WriteLiteral("/>\r\n        <p>\r\n            <input");

WriteLiteral(" type=\"submit\"");

WriteLiteral(" value=\"Register\"");

WriteLiteral(" />\r\n        </p>\r\n    </fieldset>\r\n");

            
            #line 28 "..\..\Views\Home\RegisterNewUser.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("\r\n");

DefineSection("Scripts", () => {

WriteLiteral("\r\n");

WriteLiteral("    ");

            
            #line 31 "..\..\Views\Home\RegisterNewUser.cshtml"
Write(Scripts.Render("~/bundles/jqueryval"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

});

        }
    }
}
#pragma warning restore 1591