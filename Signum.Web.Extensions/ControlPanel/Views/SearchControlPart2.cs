﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.237
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
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using System.Web.Mvc;
    using System.Web.Mvc.Ajax;
    using System.Web.Mvc.Html;
    using System.Web.Routing;
    using Signum.Utilities;
    using Signum.Entities;
    using Signum.Web;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.DataAnnotations;
    using System.Configuration;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.Caching;
    using System.Web.DynamicData;
    using System.Web.SessionState;
    using System.Web.Profile;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.UI.HtmlControls;
    using System.Xml.Linq;
    using Signum.Web.Properties;
    using Signum.Entities.ControlPanel;
    using Signum.Web.ControlPanel;
    using Signum.Entities.DynamicQuery;
    using Signum.Entities.Reports;
    using Signum.Web.UserQueries;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("MvcRazorClassGenerator", "1.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/ControlPanel/Views/SearchControlPart.cshtml")]
    public class _Page_ControlPanel_Views_SearchControlPart_cshtml : System.Web.Mvc.WebViewPage<PanelPart>
    {


        public _Page_ControlPanel_Views_SearchControlPart_cshtml()
        {
        }
        protected System.Web.HttpApplication ApplicationInstance
        {
            get
            {
                return ((System.Web.HttpApplication)(Context.ApplicationInstance));
            }
        }
        public override void Execute()
        {








WriteLiteral("\r\n");


   
    UserQueryDN uq = ((UserQueryDN)Model.Content);
    object queryName = Navigator.Manager.QuerySettings.Keys.First(k => QueryUtils.GetQueryUniqueKey(k) == uq.Query.Key);
    FindOptions fo = new FindOptions(queryName)
    {
        FilterMode = FilterMode.OnlyResults
    };

    
Write(Html.SearchControl(uq, fo, new Context(null, "r{0}c{1}".Formato(Model.Row, Model.Column))));

                                                                                               ;


        }
    }
}
