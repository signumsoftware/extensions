﻿using Signum.Engine;
using Signum.Engine.Basics;
using Signum.Entities;
using Signum.Entities.Basics;
using Signum.React;
using Signum.React.Json;
using Signum.React.TypeHelp;
using Signum.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;

namespace Signum.React.Dynamic
{
    public static class DynamicServer
    {
        public static void Start(IApplicationBuilder app)
        {
            TypeHelpServer.Start(app);
            SignumControllerFactory.RegisterArea(MethodInfo.GetCurrentMethod());

            EntityJsonConverter.AfterDeserilization.Register((PropertyRouteEntity wc) =>
            {
                var route = PropertyRouteLogic.TryGetPropertyRouteEntity(wc.RootType, wc.Path);
                if (route != null)
                {
                    wc.SetId(route.Id);
                    wc.SetIsNew(false);
                    wc.SetCleanModified(false);
                }
            });
        }
    }
}