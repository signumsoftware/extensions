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
    
    #line 2 "..\..\Processes\Views\ProcessPanel.cshtml"
    using Signum.Engine.Processes;
    
    #line default
    #line hidden
    using Signum.Entities;
    using Signum.Utilities;
    
    #line 1 "..\..\Processes\Views\ProcessPanel.cshtml"
    using Signum.Utilities.ExpressionTrees;
    
    #line default
    #line hidden
    using Signum.Web;
    
    #line 3 "..\..\Processes\Views\ProcessPanel.cshtml"
    using Signum.Web.Processes;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Processes/Views/ProcessPanel.cshtml")]
    public partial class _Processes_Views_ProcessPanel_cshtml : System.Web.Mvc.WebViewPage<ProcessLogicState>
    {
        public _Processes_Views_ProcessPanel_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("<h2>ProcessLogic state</h2>\r\n<div>\r\n    <a");

WriteAttribute("href", Tuple.Create(" href=\"", 172), Tuple.Create("\"", 227)
            
            #line 8 "..\..\Processes\Views\ProcessPanel.cshtml"
, Tuple.Create(Tuple.Create("", 179), Tuple.Create<System.Object, System.Int32>(Url.Action((ProcessController pc) => pc.Stop())
            
            #line default
            #line hidden
, 179), false)
);

WriteLiteral(" class=\"sf-button btn btn-default active\"");

WriteAttribute("style", Tuple.Create(" style=\"", 269), Tuple.Create("\"", 325)
            
            #line 8 "..\..\Processes\Views\ProcessPanel.cshtml"
                                 , Tuple.Create(Tuple.Create("", 277), Tuple.Create<System.Object, System.Int32>(Model.Running ? "" : "display:none"
            
            #line default
            #line hidden
, 277), false)
, Tuple.Create(Tuple.Create("", 315), Tuple.Create(";color:red", 315), true)
);

WriteLiteral(" id=\"sfProcessDisable\"");

WriteLiteral(">Stop </a>\r\n    <a");

WriteAttribute("href", Tuple.Create(" href=\"", 366), Tuple.Create("\"", 422)
            
            #line 9 "..\..\Processes\Views\ProcessPanel.cshtml"
, Tuple.Create(Tuple.Create("", 373), Tuple.Create<System.Object, System.Int32>(Url.Action((ProcessController pc) => pc.Start())
            
            #line default
            #line hidden
, 373), false)
);

WriteLiteral(" class=\"sf-button btn btn-default\"");

WriteAttribute("style", Tuple.Create(" style=\"", 457), Tuple.Create("\"", 504)
            
            #line 9 "..\..\Processes\Views\ProcessPanel.cshtml"
                           , Tuple.Create(Tuple.Create("", 465), Tuple.Create<System.Object, System.Int32>(!Model.Running ? "" : "display:none"
            
            #line default
            #line hidden
, 465), false)
);

WriteLiteral(" id=\"sfProcessEnable\"");

WriteLiteral(">Start </a>\r\n</div>\r\n<div");

WriteLiteral(" id=\"processMainDiv\"");

WriteLiteral(">\r\n");

WriteLiteral("    ");

            
            #line 12 "..\..\Processes\Views\ProcessPanel.cshtml"
Write(Html.Partial(ProcessClient.ViewPrefix.FormatWith("ProcessPanelTable")));

            
            #line default
            #line hidden
WriteLiteral("\r\n</div>\r\n<script>\r\n    $(function () {\r\n");

WriteLiteral("        ");

            
            #line 16 "..\..\Processes\Views\ProcessPanel.cshtml"
    Write(ProcessClient.Module["initDashboard"](Url.Action((ProcessController p) => p.View())));

            
            #line default
            #line hidden
WriteLiteral("\r\n    });\r\n</script>\r\n");

        }
    }
}
#pragma warning restore 1591