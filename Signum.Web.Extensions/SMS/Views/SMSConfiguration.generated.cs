﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASP
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
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using Signum.Entities;
    
    #line 1 "..\..\SMS\Views\SMSConfiguration.cshtml"
    using Signum.Entities.SMS;
    
    #line default
    #line hidden
    using Signum.Utilities;
    
    #line 2 "..\..\SMS\Views\SMSConfiguration.cshtml"
    using Signum.Web;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/SMS/Views/SMSConfiguration.cshtml")]
    public partial class _SMS_Views_SMSConfiguration_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _SMS_Views_SMSConfiguration_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("\r\n");

            
            #line 4 "..\..\SMS\Views\SMSConfiguration.cshtml"
 using (var tc = Html.TypeContext<SMSConfigurationEmbedded>())
{   
    
            
            #line default
            #line hidden
            
            #line 6 "..\..\SMS\Views\SMSConfiguration.cshtml"
Write(Html.EntityCombo(tc, s => s.DefaultCulture));

            
            #line default
            #line hidden
            
            #line 6 "..\..\SMS\Views\SMSConfiguration.cshtml"
                                                
}

            
            #line default
            #line hidden
        }
    }
}
#pragma warning restore 1591