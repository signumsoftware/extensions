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
    
    #line 1 "..\..\Chart\Views\UserChartOrder.cshtml"
    using Signum.Engine;
    
    #line default
    #line hidden
    using Signum.Entities;
    
    #line 5 "..\..\Chart\Views\UserChartOrder.cshtml"
    using Signum.Entities.Chart;
    
    #line default
    #line hidden
    
    #line 6 "..\..\Chart\Views\UserChartOrder.cshtml"
    using Signum.Entities.DynamicQuery;
    
    #line default
    #line hidden
    
    #line 2 "..\..\Chart\Views\UserChartOrder.cshtml"
    using Signum.Entities.UserQueries;
    
    #line default
    #line hidden
    using Signum.Utilities;
    
    #line 3 "..\..\Chart\Views\UserChartOrder.cshtml"
    using Signum.Web;
    
    #line default
    #line hidden
    
    #line 4 "..\..\Chart\Views\UserChartOrder.cshtml"
    using Signum.Web.Chart;
    
    #line default
    #line hidden
    
    #line 7 "..\..\Chart\Views\UserChartOrder.cshtml"
    using Signum.Web.UserAssets;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Chart/Views/UserChartOrder.cshtml")]
    public partial class _Chart_Views_UserChartOrder_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Chart_Views_UserChartOrder_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("\r\n");

            
            #line 9 "..\..\Chart\Views\UserChartOrder.cshtml"
 using (var e = Html.TypeContext<QueryOrderEmbedded>())
{
    var userChart = ((TypeContext<UserChartEntity>)e.Parent.Parent).Value;

    e.FormGroupStyle = FormGroupStyle.None;

    
            
            #line default
            #line hidden
            
            #line 15 "..\..\Chart\Views\UserChartOrder.cshtml"
Write(Html.QueryTokenDNBuilder(e.SubContext(a => a.Token), ChartClient.GetQueryTokenBuilderSettings(
    (QueryDescription)ViewData[ViewDataKeys.QueryDescription], SubTokensOptions.CanElement | (userChart.GroupResults ? SubTokensOptions.CanAggregate :  0))));

            
            #line default
            #line hidden
            
            #line 16 "..\..\Chart\Views\UserChartOrder.cshtml"
                                                                                                                                                            

    
            
            #line default
            #line hidden
            
            #line 18 "..\..\Chart\Views\UserChartOrder.cshtml"
Write(Html.ValueLine(e, f => f.OrderType));

            
            #line default
            #line hidden
            
            #line 18 "..\..\Chart\Views\UserChartOrder.cshtml"
                                        
}
            
            #line default
            #line hidden
        }
    }
}
#pragma warning restore 1591