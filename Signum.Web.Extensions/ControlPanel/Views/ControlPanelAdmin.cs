﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
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
    using Signum.Entities.ControlPanel;
    using Signum.Web.ControlPanel;
    using System.Reflection;
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("MvcRazorClassGenerator", "1.0")]
    [System.Web.WebPages.PageVirtualPathAttribute("~/ControlPanel/Views/ControlPanelAdmin.cshtml")]
    public class _Page_ControlPanel_Views_ControlPanelAdmin_cshtml : System.Web.Mvc.WebViewPage<dynamic>
    {


        public _Page_ControlPanel_Views_ControlPanelAdmin_cshtml()
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



WriteLiteral(@"
<script type=""text/javascript"">
    var SF = SF || {};

    SF.ControlPanel = (function () {
        var toggleFillColumn = function (cbFillId, colInputId) {
            if ($(""#"" + cbFillId + "":checked"").length > 0) {
                $('#' + colInputId).val(1).attr('disabled', 'disabled');
            }
            else {
                $('#' + colInputId).removeAttr('disabled');
            }
        };

        return {
            toggleFillColumn: toggleFillColumn
        };
    })();
</script>

");


 using (var tc = Html.TypeContext<ControlPanelDN>())
{
    
Write(Html.EntityLine(tc, cp => cp.Related, el => el.Create = false));

                                                                   
    
Write(Html.ValueLine(tc, cp => cp.DisplayName));

                                             
    
Write(Html.ValueLine(tc, cp => cp.HomePage));

                                          
    
Write(Html.ValueLine(tc, cp => cp.NumberOfColumns));

                                                 
    
Write(Html.EntityRepeater(tc, cp => cp.Parts));

                                            
}

        }
    }
}
