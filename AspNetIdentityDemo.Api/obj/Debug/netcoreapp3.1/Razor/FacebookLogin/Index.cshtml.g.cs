#pragma checksum "C:\Users\Microsoft\Source\Repos\AsmaaMa7moud\LastAuthDemoUpdated\AspNetIdentityDemo.Api\FacebookLogin\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "1e11588c3149c5172f4bb8ccf399460bc9c16212"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.FacebookLogin_Index), @"mvc.1.0.razor-page", @"/FacebookLogin/Index.cshtml")]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"1e11588c3149c5172f4bb8ccf399460bc9c16212", @"/FacebookLogin/Index.cshtml")]
    public class FacebookLogin_Index : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("<h1>Welcome</h1>\r\n<br />\r\n\r\n<fb:login-button scope=\"public_profile,email\"\r\n                 onlogin=\"checkLoginState();\">\r\n</fb:login-button>\r\n<br>\r\n<div id=\"Auth\"></div>\r\n\r\n");
            DefineSection("Scripts", async() => {
                WriteLiteral(@"
    <script>
        window.fbAsyncInit = function () {
            FB.init({
                appId: '1087185511677446',
                cookie: true,
                xfbml: true,
                version: 'v8.0'
            });

            FB.AppEvents.logPageView();

        };

        (function (d, s, id) {
            var js, fjs = d.getElementsByTagName(s)[0];
            if (d.getElementById(id)) { return; }
            js = d.createElement(s); js.id = id;
            js.src = ""https://connect.facebook.net/en_US/sdk.js"";
            fjs.parentNode.insertBefore(js, fjs);
        }(document, 'script', 'facebook-jssdk'));
        function checkLoginState() {
            FB.getLoginStatus(function (response) {
                $(""#Auth"").html(""<code>"" + JSON.stringify(response, null, 2) + ""</code>"")
            });
        }
    </script>
");
            }
            );
            WriteLiteral("\r\n");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<AspNetIdentityDemo.Api.FacebookLogin.IndexModel> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<AspNetIdentityDemo.Api.FacebookLogin.IndexModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<AspNetIdentityDemo.Api.FacebookLogin.IndexModel>)PageContext?.ViewData;
        public AspNetIdentityDemo.Api.FacebookLogin.IndexModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
