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

namespace Signum.Web.Extensions.Processes.Views
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
    
    #line 1 "..\..\Processes\Views\PackageLine.cshtml"
    using Signum.Entities.Processes;
    
    #line default
    #line hidden
    using Signum.Utilities;
    using Signum.Web;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Processes/Views/PackageLine.cshtml")]
    public partial class PackageLine : System.Web.Mvc.WebViewPage<dynamic>
    {
        public PackageLine()
        {
        }
        public override void Execute()
        {
WriteLiteral("\r\n");

            
            #line 3 "..\..\Processes\Views\PackageLine.cshtml"
 using (var e = Html.TypeContext<PackageLineDN>())
{
    e.ReadOnly = true;
    
    
            
            #line default
            #line hidden
            
            #line 7 "..\..\Processes\Views\PackageLine.cshtml"
Write(Html.EntityLine(e, f => f.Package));

            
            #line default
            #line hidden
            
            #line 7 "..\..\Processes\Views\PackageLine.cshtml"
                                       
    
            
            #line default
            #line hidden
            
            #line 8 "..\..\Processes\Views\PackageLine.cshtml"
Write(Html.EntityLine(e, f => f.Target));

            
            #line default
            #line hidden
            
            #line 8 "..\..\Processes\Views\PackageLine.cshtml"
                                      
    
            
            #line default
            #line hidden
            
            #line 9 "..\..\Processes\Views\PackageLine.cshtml"
Write(Html.EntityLine(e, f => f.Result));

            
            #line default
            #line hidden
            
            #line 9 "..\..\Processes\Views\PackageLine.cshtml"
                                      
    
            
            #line default
            #line hidden
            
            #line 10 "..\..\Processes\Views\PackageLine.cshtml"
Write(Html.ValueLine(e, f => f.FinishTime));

            
            #line default
            #line hidden
            
            #line 10 "..\..\Processes\Views\PackageLine.cshtml"
                                         

    
            
            #line default
            #line hidden
            
            #line 12 "..\..\Processes\Views\PackageLine.cshtml"
Write(Html.SearchControl(new FindOptions(typeof(ProcessExceptionLineDN), "Line", e.Value), new Context(e, "Exceptions")));

            
            #line default
            #line hidden
            
            #line 12 "..\..\Processes\Views\PackageLine.cshtml"
                                                                                                                       
}
            
            #line default
            #line hidden
WriteLiteral(" ");

        }
    }
}
#pragma warning restore 1591
