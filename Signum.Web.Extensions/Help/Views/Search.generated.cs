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
    
    #line 2 "..\..\Help\Views\Search.cshtml"
    using Signum.Engine.Help;
    
    #line default
    #line hidden
    
    #line 3 "..\..\Help\Views\Search.cshtml"
    using Signum.Engine.WikiMarkup;
    
    #line default
    #line hidden
    using Signum.Entities;
    
    #line 6 "..\..\Help\Views\Search.cshtml"
    using Signum.Entities.Help;
    
    #line default
    #line hidden
    
    #line 1 "..\..\Help\Views\Search.cshtml"
    using Signum.Entities.Reflection;
    
    #line default
    #line hidden
    using Signum.Utilities;
    using Signum.Web;
    
    #line 5 "..\..\Help\Views\Search.cshtml"
    using Signum.Web.Extensions;
    
    #line default
    #line hidden
    
    #line 4 "..\..\Help\Views\Search.cshtml"
    using Signum.Web.Help;
    
    #line default
    #line hidden
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorGenerator", "2.0.0.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/Help/Views/Search.cshtml")]
    public partial class _Help_Views_Search_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {
        public _Help_Views_Search_cshtml()
        {
        }
        public override void Execute()
        {
WriteLiteral("\r\n");

DefineSection("head", () => {

WriteLiteral("\r\n");

WriteLiteral("    ");

            
            #line 10 "..\..\Help\Views\Search.cshtml"
Write(Html.ScriptCss("~/help/Content/help.css"));

            
            #line default
            #line hidden
WriteLiteral("\r\n");

});

WriteLiteral("\r\n<script");

WriteLiteral(" type=\"text/javascript\"");

WriteLiteral(">\r\n    function ShowMore(elem) {\r\n        $(elem).siblings(\"ul\").children(\".show-" +
"on-more\").show();\r\n        $(elem).hide();\r\n    }\r\n</script>\r\n\r\n<div");

WriteLiteral(" id=\"entityContent\"");

WriteLiteral(">\r\n    <h1");

WriteLiteral(" class=\"centered\"");

WriteLiteral("><a");

WriteAttribute("href", Tuple.Create(" href=\"", 479), Tuple.Create("\"", 530)
            
            #line 21 "..\..\Help\Views\Search.cshtml"
, Tuple.Create(Tuple.Create("", 486), Tuple.Create<System.Object, System.Int32>(Url.Action((HelpController hc)=>hc.Index())
            
            #line default
            #line hidden
, 486), false)
);

WriteLiteral(">");

            
            #line 21 "..\..\Help\Views\Search.cshtml"
                                                                           Write(HelpMessage.Help.NiceToString());

            
            #line default
            #line hidden
WriteLiteral("</a></h1>\r\n");

            
            #line 22 "..\..\Help\Views\Search.cshtml"
 using (Html.BeginForm("Search", "Help", FormMethod.Get, new { id = "form-search-big" }))
{

            
            #line default
            #line hidden
WriteLiteral("    <div");

WriteLiteral(" class=\"input-group\"");

WriteLiteral(">\r\n        <input");

WriteLiteral(" type=\"text\"");

WriteLiteral(" class=\"form-control\"");

WriteAttribute("placeholder", Tuple.Create(" placeholder=\"", 747), Tuple.Create("\"", 801)
            
            #line 25 "..\..\Help\Views\Search.cshtml"
, Tuple.Create(Tuple.Create("", 761), Tuple.Create<System.Object, System.Int32>(HelpSearchMessage.Search.NiceToString()
            
            #line default
            #line hidden
, 761), false)
);

WriteLiteral(" name=\"q\"");

WriteAttribute("value", Tuple.Create(" value=\"", 811), Tuple.Create("\"", 839)
            
            #line 25 "..\..\Help\Views\Search.cshtml"
                                        , Tuple.Create(Tuple.Create("", 819), Tuple.Create<System.Object, System.Int32>(Request.Params["q"]
            
            #line default
            #line hidden
, 819), false)
);

WriteLiteral(" />\r\n        <div");

WriteLiteral(" class=\"input-group-btn\"");

WriteLiteral(">\r\n            <button");

WriteLiteral(" class=\"btn btn-default\"");

WriteLiteral(" type=\"submit\"");

WriteLiteral("><i");

WriteLiteral(" class=\"glyphicon glyphicon-search\"");

WriteLiteral("></i></button>\r\n        </div>\r\n    </div>\r\n");

            
            #line 30 "..\..\Help\Views\Search.cshtml"
}

            
            #line default
            #line hidden
            
            #line 31 "..\..\Help\Views\Search.cshtml"
    
            
            #line default
            #line hidden
            
            #line 31 "..\..\Help\Views\Search.cshtml"
       List<List<SearchResult>> results = (List<List<SearchResult>>)Model;
       string q = Request.Params["q"];
       int count = results.Count;
    
            
            #line default
            #line hidden
WriteLiteral("\r\n    <p");

WriteLiteral(" id=\"title\"");

WriteLiteral(">\r\n");

WriteLiteral("        ");

            
            #line 36 "..\..\Help\Views\Search.cshtml"
   Write(HelpSearchMessage._0ResultsFor1In2.NiceToString().ForGenderAndNumber(number: count).FormatHtml(count, new HtmlTag("b").SetInnerText(q), ViewData["time"]));

            
            #line default
            #line hidden
WriteLiteral("\r\n    </p>\r\n    <hr />\r\n");

            
            #line 39 "..\..\Help\Views\Search.cshtml"
    
            
            #line default
            #line hidden
            
            #line 39 "..\..\Help\Views\Search.cshtml"
     foreach (var v in results)
    {
        int currentResults = 0;

        var first = v.First();
        

            
            #line default
            #line hidden
WriteLiteral("        <h4><a");

WriteAttribute("href", Tuple.Create(" href=\"", 1521), Tuple.Create("\"", 1539)
            
            #line 45 "..\..\Help\Views\Search.cshtml"
, Tuple.Create(Tuple.Create("", 1528), Tuple.Create<System.Object, System.Int32>(first.Link
            
            #line default
            #line hidden
, 1528), false)
);

WriteLiteral(">");

            
            #line 45 "..\..\Help\Views\Search.cshtml"
                              Write(first.Type?.NiceName() ?? first.ObjectName);

            
            #line default
            #line hidden
WriteLiteral("</a> <small>(");

            
            #line 45 "..\..\Help\Views\Search.cshtml"
                                                                                       Write(first.TypeSearchResult.NiceToString());

            
            #line default
            #line hidden
WriteLiteral(")</small></h4>\r\n");

            
            #line 46 "..\..\Help\Views\Search.cshtml"
            

            
            #line default
            #line hidden
WriteLiteral("        <ul>\r\n");

            
            #line 48 "..\..\Help\Views\Search.cshtml"
            
            
            #line default
            #line hidden
            
            #line 48 "..\..\Help\Views\Search.cshtml"
             foreach (var sr in v)
            {
                currentResults++;               


            
            #line default
            #line hidden
WriteLiteral("                <li>\r\n");

            
            #line 53 "..\..\Help\Views\Search.cshtml"
                    
            
            #line default
            #line hidden
            
            #line 53 "..\..\Help\Views\Search.cshtml"
                     if (first.TypeSearchResult == TypeSearchResult.Type &&
                        (sr.TypeSearchResult == TypeSearchResult.Operation ||
                         sr.TypeSearchResult == TypeSearchResult.Property ||
                         sr.TypeSearchResult == TypeSearchResult.Query))
                    {

            
            #line default
            #line hidden
WriteLiteral("                        <a");

WriteAttribute("href", Tuple.Create(" href=\"", 2163), Tuple.Create("\"", 2178)
            
            #line 58 "..\..\Help\Views\Search.cshtml"
, Tuple.Create(Tuple.Create("", 2170), Tuple.Create<System.Object, System.Int32>(sr.Link
            
            #line default
            #line hidden
, 2170), false)
);

WriteLiteral(">");

            
            #line 58 "..\..\Help\Views\Search.cshtml"
                                      Write(Html.WikiParse(sr.ObjectName, HelpWiki.NoLinkWikiSettings));

            
            #line default
            #line hidden
WriteLiteral("</a>\r\n");

            
            #line 59 "..\..\Help\Views\Search.cshtml"
                    }

            
            #line default
            #line hidden
WriteLiteral("                    ");

            
            #line 60 "..\..\Help\Views\Search.cshtml"
               Write(Html.WikiParse(sr.Description, HelpWiki.NoLinkWikiSettings));

            
            #line default
            #line hidden
WriteLiteral("\r\n                </li>\r\n");

            
            #line 62 "..\..\Help\Views\Search.cshtml"
            }

            
            #line default
            #line hidden
WriteLiteral("        </ul>\r\n");

WriteLiteral("        <hr />\r\n");

            
            #line 65 "..\..\Help\Views\Search.cshtml"
    }

            
            #line default
            #line hidden
WriteLiteral("</div>\r\n");

        }
    }
}
#pragma warning restore 1591