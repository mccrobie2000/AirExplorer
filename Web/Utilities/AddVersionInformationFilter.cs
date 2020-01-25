using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Web.Utilities
{
    public class AddVersionInformationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            var controller = filterContext.Controller as Controller;
            if (controller != null)
            {
                controller.ViewData["_Air_Version_"] = "1.0";
            }
        }
    }
}
