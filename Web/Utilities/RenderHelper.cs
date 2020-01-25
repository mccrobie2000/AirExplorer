using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Web.Utilities
{
    public static class RenderHelper
    {
        public static async Task<string> RenderRazorViewToString(this ControllerContext ControllerContext, string viewName, object model)
        {
            var tempDataProvider = (ITempDataProvider) ControllerContext.HttpContext.RequestServices.GetService(typeof(ITempDataProvider));

            ViewDataDictionary viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary());
            viewData.Model = model;

            TempDataDictionary tempData = new TempDataDictionary(ControllerContext.HttpContext, tempDataProvider);

            using (var sw = new StringWriter())
            {
                var viewEngine = (IRazorViewEngine) ControllerContext.HttpContext.RequestServices.GetService(typeof(IRazorViewEngine));
                var viewResult = viewEngine.FindView(ControllerContext, viewName, false);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, viewData, tempData, sw, new HtmlHelperOptions());
                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}
