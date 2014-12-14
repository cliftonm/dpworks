﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
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
	using WebMatrix.WebData;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Views/Home/Index.cshtml")]
    public partial class Index : System.Web.Mvc.WebViewPage<dynamic>
    {
        public Index()
        {
        }
        public override void Execute()
        {
            
            #line 1 "..\..\Views\Home\Index.cshtml"
  
    ViewBag.Title = "Home Page";

            
            #line default
            #line hidden
WriteLiteral("\r\n\n<link");

WriteLiteral(" rel=\"stylesheet\"");

WriteAttribute("href", Tuple.Create(" href=\"", 64), Tuple.Create("\"", 89)
, Tuple.Create(Tuple.Create("", 71), Tuple.Create<System.Object, System.Int32>(Href("~/Styles/Login.css")
, 71), false)
);

WriteLiteral(" type=\"text/css\"");

WriteLiteral(" />\n<link");

WriteLiteral(" rel=\"stylesheet\"");

WriteAttribute("href", Tuple.Create(" href=\"", 132), Tuple.Create("\"", 162)
, Tuple.Create(Tuple.Create("", 139), Tuple.Create<System.Object, System.Int32>(Href("~/Styles/StyleSheet.css")
, 139), false)
);

WriteLiteral(" type=\"text/css\"");

WriteLiteral(@" />

<script>
    $(function () {
        $('#RememberMe').hide();
        $('#selectSite').on('click', function () { selectSite(); });
        initializeSpinny();
    });

    function rememberMe() {
        $('.checkbox').toggleClass('show');
        $('#RememberMe').prop(""checked"", !$('#RememberMe').prop(""checked""));
    }

    function selectSite() {
        var siteId = $('#siteList').val();
        $.post(
            '/Home/SelectSite',
            { SiteId: siteId },
            function (site) {
                $('#siteName').text(site.Name);
                // Redirect, because the UI of the home page (like the sidebar) may change.
                window.location.replace(""/Home/Index"");
            });
    }

    function initializeSpinny() {
        var loading = $('#loadingDiv');
        loading.hide();
        $(document)
          .ajaxStart(function () {
              loading.show();
          })
          .ajaxStop(function () {
              loading.hide();
          });
    }
</script>

");

            
            #line 45 "..\..\Views\Home\Index.cshtml"
 if (!WebSecurity.IsAuthenticated)
{
    
            
            #line default
            #line hidden
            
            #line 47 "..\..\Views\Home\Index.cshtml"
Write(Html.ValidationSummary(true));

            
            #line default
            #line hidden
            
            #line 47 "..\..\Views\Home\Index.cshtml"
                                 
    
            
            #line default
            #line hidden
            
            #line 48 "..\..\Views\Home\Index.cshtml"
Write(Html.Partial("_BootstrapLogin"));

            
            #line default
            #line hidden
            
            #line 48 "..\..\Views\Home\Index.cshtml"
                                    
    
            
            #line default
            #line hidden
            
            #line 49 "..\..\Views\Home\Index.cshtml"
Write(Html.Partial("_ForgotPasswordFlyout"));

            
            #line default
            #line hidden
            
            #line 49 "..\..\Views\Home\Index.cshtml"
                                          
}
else
{

            
            #line default
            #line hidden
WriteLiteral("    <p>Welcome [TODO: User]</p>\r\n");

            
            #line 54 "..\..\Views\Home\Index.cshtml"
    
    if (User.IsInRole("Site-Wide Administrator"))
    {

            
            #line default
            #line hidden
WriteLiteral("        <p>Select Site:</p>\r\n");

            
            #line 58 "..\..\Views\Home\Index.cshtml"
        
            
            #line default
            #line hidden
            
            #line 58 "..\..\Views\Home\Index.cshtml"
   Write(Html.DropDownList("siteList", new SelectList(ViewBag.Sites, "Id", "Name")));

            
            #line default
            #line hidden
            
            #line 58 "..\..\Views\Home\Index.cshtml"
                                                                                   ;

            
            #line default
            #line hidden
WriteLiteral("        <button");

WriteLiteral(" id=\"selectSite\"");

WriteLiteral(">Select</button>\r\n");

            
            #line 60 "..\..\Views\Home\Index.cshtml"
    }
    

            
            #line default
            #line hidden
WriteLiteral("    <p>TODO: If user is admin or other, show site name.</p>\r\n");

            
            #line 63 "..\..\Views\Home\Index.cshtml"
    

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" id=\'loadingDiv\'");

WriteLiteral("><img");

WriteAttribute("src", Tuple.Create(" src=\"", 1760), Tuple.Create("\"", 1786)
, Tuple.Create(Tuple.Create("", 1766), Tuple.Create<System.Object, System.Int32>(Href("~/Images/spinner.gif")
, 1766), false)
);

WriteLiteral("/></div> \n");

            
            #line 65 "..\..\Views\Home\Index.cshtml"

}

            
            #line default
            #line hidden
        }
    }
}
#pragma warning restore 1591