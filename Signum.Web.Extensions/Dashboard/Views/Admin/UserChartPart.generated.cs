﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Signum.Web.Extensions.Dashboard.Views.Admin
{
    using System;
    using System.Collections.Generic;
    
    #line 1 "..\..\Dashboard\Views\Admin\UserChartPart.cshtml"
    using System.Configuration;
    
    #line default
    #line hidden
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
    
    #line 2 "..\..\Dashboard\Views\Admin\UserChartPart.cshtml"
    using Signum.Entities.Dashboard;
    
    #line default
    #line hidden
    
    #line 4 "..\..\Dashboard\Views\Admin\UserChartPart.cshtml"
    using Signum.Entities.DynamicQuery;
    
    #line default
    #line hidden
    using Signum.Utilities;
    using Signum.Web;
    
    #line 5 "..\..\Dashboard\Views\Admin\UserChartPart.cshtml"
    using Signum.Web.Chart;
    
    #line default
    #line hidden
    
    #line 3 "..\..\Dashboard\Views\Admin\UserChartPart.cshtml"
    using Signum.Web.Dashboard;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Dashboard/Views/Admin/UserChartPart.cshtml")]
    public partial class UserChartPart : System.Web.Mvc.WebViewPage<dynamic>
    {
        public UserChartPart()
        {
        }
        public override void Execute()
        {
WriteLiteral("\r\n");

            
            #line 7 "..\..\Dashboard\Views\Admin\UserChartPart.cshtml"
 using(var tc = Html.TypeContext<UserChartPartDN>())
{
    
            
            #line default
            #line hidden
            
            #line 9 "..\..\Dashboard\Views\Admin\UserChartPart.cshtml"
Write(Html.EntityLine(tc, ucp => ucp.UserChart, el => el.Create = false));

            
            #line default
            #line hidden
            
            #line 9 "..\..\Dashboard\Views\Admin\UserChartPart.cshtml"
                                                                       
    
            
            #line default
            #line hidden
            
            #line 10 "..\..\Dashboard\Views\Admin\UserChartPart.cshtml"
Write(Html.ValueLine(tc, ucp => ucp.ShowData));

            
            #line default
            #line hidden
            
            #line 10 "..\..\Dashboard\Views\Admin\UserChartPart.cshtml"
                                            
}
            
            #line default
            #line hidden
        }
    }
}
#pragma warning restore 1591
