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
    
    #line 1 "..\..\Mailing\Views\EmailConfiguration.cshtml"
    using Signum.Entities.Mailing;
    
    #line default
    #line hidden
    using Signum.Utilities;
    using Signum.Web;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Mailing/Views/EmailConfiguration.cshtml")]
    public partial class _Mailing_Views_EmailConfiguration_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Mailing_Views_EmailConfiguration_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("\r\n");

            
            #line 3 "..\..\Mailing\Views\EmailConfiguration.cshtml"
 using (var sc = Html.TypeContext<EmailConfigurationEmbedded>())
{
    
            
            #line default
            #line hidden
            
            #line 5 "..\..\Mailing\Views\EmailConfiguration.cshtml"
Write(Html.ValueLine(sc, ca => ca.ReciveEmails));

            
            #line default
            #line hidden
            
            #line 5 "..\..\Mailing\Views\EmailConfiguration.cshtml"
                                              
    
            
            #line default
            #line hidden
            
            #line 6 "..\..\Mailing\Views\EmailConfiguration.cshtml"
Write(Html.ValueLine(sc, ca => ca.SendEmails));

            
            #line default
            #line hidden
            
            #line 6 "..\..\Mailing\Views\EmailConfiguration.cshtml"
                                            
    
            
            #line default
            #line hidden
            
            #line 7 "..\..\Mailing\Views\EmailConfiguration.cshtml"
Write(Html.ValueLine(sc, ca => ca.OverrideEmailAddress));

            
            #line default
            #line hidden
            
            #line 7 "..\..\Mailing\Views\EmailConfiguration.cshtml"
                                                      
    
            
            #line default
            #line hidden
            
            #line 8 "..\..\Mailing\Views\EmailConfiguration.cshtml"
Write(Html.EntityCombo(sc, ca => ca.DefaultCulture));

            
            #line default
            #line hidden
            
            #line 8 "..\..\Mailing\Views\EmailConfiguration.cshtml"
                                                  
    
            
            #line default
            #line hidden
            
            #line 9 "..\..\Mailing\Views\EmailConfiguration.cshtml"
Write(Html.ValueLine(sc, ca => ca.UrlLeft));

            
            #line default
            #line hidden
            
            #line 9 "..\..\Mailing\Views\EmailConfiguration.cshtml"
                                         

    using (var ac = sc.SubContext())
    {
        ac.FormGroupStyle = FormGroupStyle.Basic;

            
            #line default
            #line hidden
WriteLiteral("        <fieldset");

WriteLiteral(" class=\"form-vertical\"");

WriteLiteral(">\r\n            <legend>Async</legend>\r\n            <div");

WriteLiteral(" class=\"row\"");

WriteLiteral(">\r\n                <div");

WriteLiteral(" class=\"col-sm-6\"");

WriteLiteral(">\r\n");

WriteLiteral("                    ");

            
            #line 18 "..\..\Mailing\Views\EmailConfiguration.cshtml"
               Write(Html.ValueLine(ac, ca => ca.AvoidSendingEmailsOlderThan));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("                    ");

            
            #line 19 "..\..\Mailing\Views\EmailConfiguration.cshtml"
               Write(Html.ValueLine(ac, ca => ca.ChunkSizeSendingEmails));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </div>\r\n                <div");

WriteLiteral(" class=\"col-sm-6\"");

WriteLiteral(">\r\n");

WriteLiteral("                    ");

            
            #line 22 "..\..\Mailing\Views\EmailConfiguration.cshtml"
               Write(Html.ValueLine(ac, ca => ca.MaxEmailSendRetries));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("                    ");

            
            #line 23 "..\..\Mailing\Views\EmailConfiguration.cshtml"
               Write(Html.ValueLine(ac, ca => ca.AsyncSenderPeriod));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </div>\r\n            </div>\r\n        </fieldset>\r\n");

            
            #line 27 "..\..\Mailing\Views\EmailConfiguration.cshtml"
    }
}

            
            #line default
            #line hidden
        }
    }
}
#pragma warning restore 1591