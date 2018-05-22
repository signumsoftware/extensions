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
    
    #line 1 "..\..\Calendar\Views\Calendar.cshtml"
    using System.Globalization;
    
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
    using Signum.Utilities;
    
    #line 2 "..\..\Calendar\Views\Calendar.cshtml"
    using Signum.Web;
    
    #line default
    #line hidden
    
    #line 3 "..\..\Calendar\Views\Calendar.cshtml"
    using Signum.Web.Calendar;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Calendar/Views/Calendar.cshtml")]
    public partial class _Calendar_Views_Calendar_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Calendar_Views_Calendar_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("\r\n\r\n");

            
            #line 6 "..\..\Calendar\Views\Calendar.cshtml"
Write(Html.ScriptCss("~/calendar/content/calendar.css"));

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n");

            
            #line 8 "..\..\Calendar\Views\Calendar.cshtml"
  
    DateTime startDate = ViewBag.StartDate;

    int daysToMove = ((int)startDate.DayOfWeek + 6) % 7;
    startDate = startDate.AddDays(-daysToMove);
    
    DateTime endDate = ViewBag.EndDate;
    Func<DateTime, int, HelperResult> cellTemplate = ViewBag.CellTemplate;

            
            #line default
            #line hidden
WriteLiteral("\r\n\r\n");

            
            #line 18 "..\..\Calendar\Views\Calendar.cshtml"
 if (!ViewData.ContainsKey("ShowSlider") || ViewBag.ShowSlider)
{ 

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" class=\"sf-annual-calendar-slider\"");

WriteLiteral(">\r\n    </div>\r\n");

            
            #line 22 "..\..\Calendar\Views\Calendar.cshtml"
}

            
            #line default
            #line hidden
WriteLiteral("\r\n<table");

WriteLiteral(" class=\"sf-annual-calendar\"");

WriteLiteral(@">
    <thead>
        <tr>
            <th></th>
            <th>L</th>
            <th>M</th>
            <th>X</th>
            <th>J</th>
            <th>V</th>
            <th>S</th>
            <th>D</th>
        </tr>
    </thead>
    <tbody>
");

            
            #line 38 "..\..\Calendar\Views\Calendar.cshtml"
        
            
            #line default
            #line hidden
            
            #line 38 "..\..\Calendar\Views\Calendar.cshtml"
           int max = (int)(endDate - startDate).TotalDays; 
            
            #line default
            #line hidden
WriteLiteral("\r\n");

            
            #line 39 "..\..\Calendar\Views\Calendar.cshtml"
        
            
            #line default
            #line hidden
            
            #line 39 "..\..\Calendar\Views\Calendar.cshtml"
         for (int d = 0; d < max; d++)
        {

            
            #line default
            #line hidden
WriteLiteral("            <tr>\r\n                <td");

WriteLiteral(" class=\"sf-annual-calendar-month\"");

WriteLiteral(">\r\n");

            
            #line 43 "..\..\Calendar\Views\Calendar.cshtml"
                    
            
            #line default
            #line hidden
            
            #line 43 "..\..\Calendar\Views\Calendar.cshtml"
                      
            string month = CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedMonthNames[startDate.AddDays(d).Month - 1];
            if (startDate.AddDays(d).Month != startDate.AddDays(d + 6).Month)
            {
                month += " " + CultureInfo.CurrentUICulture.DateTimeFormat.AbbreviatedMonthNames[startDate.AddDays(d).Month];
            }
                    
            
            #line default
            #line hidden
WriteLiteral("\r\n");

WriteLiteral("                    ");

            
            #line 50 "..\..\Calendar\Views\Calendar.cshtml"
               Write(month);

            
            #line default
            #line hidden
WriteLiteral("\r\n                </td>\r\n");

            
            #line 52 "..\..\Calendar\Views\Calendar.cshtml"
                
            
            #line default
            #line hidden
            
            #line 52 "..\..\Calendar\Views\Calendar.cshtml"
                 for (int dow = 0; dow < 7; dow++)
                {
                    DateTime day = startDate.AddDays(d + dow);

            
            #line default
            #line hidden
WriteLiteral("                    <td");

WriteLiteral(" data-date=\"");

            
            #line 55 "..\..\Calendar\Views\Calendar.cshtml"
                              Write(day.ToShortDateString());

            
            #line default
            #line hidden
WriteLiteral("\"");

WriteLiteral(">\r\n");

WriteLiteral("                        ");

            
            #line 56 "..\..\Calendar\Views\Calendar.cshtml"
                   Write(cellTemplate(day, d + dow));

            
            #line default
            #line hidden
WriteLiteral("\r\n                    </td>\r\n");

            
            #line 58 "..\..\Calendar\Views\Calendar.cshtml"
                }

            
            #line default
            #line hidden
WriteLiteral("                ");

            
            #line 59 "..\..\Calendar\Views\Calendar.cshtml"
                   d += 6; 
            
            #line default
            #line hidden
WriteLiteral("\r\n            </tr>    \r\n");

            
            #line 61 "..\..\Calendar\Views\Calendar.cshtml"
        }

            
            #line default
            #line hidden
WriteLiteral("    </tbody>\r\n</table>\r\n\r\n<script>\r\n    $(function () {\r\n");

WriteLiteral("        ");

            
            #line 67 "..\..\Calendar\Views\Calendar.cshtml"
    Write(CalendarClient.Modules["init"]());

            
            #line default
            #line hidden
WriteLiteral("\r\n    })\r\n</script>\r\n\r\n");

        }
    }
}
#pragma warning restore 1591