﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18051
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Signum.Web.Extensions.Mailing.Views
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
    
    #line 1 "..\..\Mailing\Views\EmailAttachment.cshtml"
    using Signum.Entities.Mailing;
    
    #line default
    #line hidden
    using Signum.Utilities;
    using Signum.Web;
    
    #line 2 "..\..\Mailing\Views\EmailAttachment.cshtml"
    using Signum.Web.Files;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Mailing/Views/EmailAttachment.cshtml")]
    public partial class EmailAttachment : System.Web.Mvc.WebViewPage<dynamic>
    {
        public EmailAttachment()
        {
        }
        public override void Execute()
        {


WriteLiteral("\r\n");


            
            #line 4 "..\..\Mailing\Views\EmailAttachment.cshtml"
 using (var sc = Html.TypeContext<EmailAttachmentDN>())
{
    
            
            #line default
            #line hidden
            
            #line 6 "..\..\Mailing\Views\EmailAttachment.cshtml"
Write(Html.FileLine(sc, ea => ea.File, fl => { fl.FileType = EmailFileType.Attachment; fl.LabelVisible = false; }));

            
            #line default
            #line hidden
            
            #line 6 "..\..\Mailing\Views\EmailAttachment.cshtml"
                                                                                                                 
    
            
            #line default
            #line hidden
            
            #line 7 "..\..\Mailing\Views\EmailAttachment.cshtml"
Write(Html.ValueLine(sc, c => c.Type, vl => { vl.ReadOnly = true; vl.WriteHiddenOnReadonly = true; }));

            
            #line default
            #line hidden
            
            #line 7 "..\..\Mailing\Views\EmailAttachment.cshtml"
                                                                                                    
}

            
            #line default
            #line hidden

        }
    }
}
#pragma warning restore 1591
