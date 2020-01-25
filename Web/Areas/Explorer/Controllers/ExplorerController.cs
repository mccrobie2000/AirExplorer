using BusinessServices;
using BusinessServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Explorer.Models;
using Web.Utilities;
using WebControls;

namespace Web.Areas.Explorer.Controllers
{
    [Area("Explorer")]
    public class ExplorerController : Controller
    {
        protected AirportBusinessService AirportBusinessService { get; set; }

        public ExplorerController(AirportBusinessService airportBusinessService)
        {
            AirportBusinessService = airportBusinessService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var model = new ExplorerModel();
            model.Load(AirportBusinessService);

            return View("Explorer", model);
        }

        [HttpPost]
        public async Task<ActionResult> Airport(int airportId)
        {
            JsonResult result = null;

            try
            {
                var model = new AirportModel();
                await model.Load(AirportBusinessService, airportId);

                var view = await ControllerContext.RenderRazorViewToString("_AirportInformation", model);
                var data = new { Html = view, Airport = model.Airport };

                result = Json(data);
            }
            catch (Exception exception)
            {
                var data = new { Html = exception.Message };

                result = Json(data);
            }

            return result;
        }

        [HttpPost]
        public async Task<ActionResult> Airports(int page, int rows, string sidx, string sord)
        {
            jqGridResponse<AirportDTO> jqgridResponse = null;

            try
            {
                int offset = (page - 1) * rows;
                var task = AirportBusinessService.GetAirportsAsync(sidx, sord, offset, rows);

                await task;

                int totalPages = task.Result.TotalRecords / rows;
                jqgridResponse = new jqGridResponse<AirportDTO>(page, task.Result.TotalRecords, totalPages, task.Result.Airports);
            }
            catch (Exception exception)
            {
                jqgridResponse = new jqGridResponse<AirportDTO>(0, 0, 0, new List<AirportDTO>(), exception.Message);
            }

            return Json(jqgridResponse);
        }

        [HttpPost]
        public async Task<ActionResult> CountryAirports(int countryId, int page, int rows, string sidx, string sord)
        {
            jqGridResponse<AirportDTO> jqgridResponse = null;

            try
            {
                int offset = (page - 1) * rows;

                var airportList = await AirportBusinessService.GetAirports(countryId, sidx, sord, offset, rows);

                int totalPages = (airportList.TotalRecords + rows - 1) / rows;

                jqgridResponse = new jqGridResponse<AirportDTO>(page, airportList.TotalRecords, totalPages, airportList.Airports);
            }
            catch (Exception exception)
            {
                jqgridResponse = new jqGridResponse<AirportDTO>(0, 0, 0, new List<AirportDTO>(), exception.Message);
            }

            return Json(jqgridResponse);
        }

        [HttpPost]
        public async Task<ActionResult> Country(long countryId)
        {
            JsonResult result = null;

            try
            {
                var model = new CountryModel();
                await model.Load(AirportBusinessService, countryId);

                var view = await ControllerContext.RenderRazorViewToString("_CountryInformation", model);
                var data = new { Airports = model.Country.Airports, Html = view };

                result = Json(data);
            }
            catch (Exception exception)
            {
                var data = new { Html = exception.Message };
                result = Json(data);
            }

            return result;
        }

        [HttpPost]
        public async Task<ActionResult> Countries(int page, int rows, string sidx, string sord, int? countryId = null)
        {
            JsonResult result = null;

            jqGridResponse<CountryDTO> jqgridResponse = null;

            try
            {
                CountryDTOList countryList = null;
                IList<CountryDTO> countries = null;

                if (countryId.HasValue)
                {
                    // Find the page on which the countryId is located based on the sorting
                    countryList = await AirportBusinessService.GetCountries(sidx, sord);
                    int index = 0;
                    for (index = 0; index < countryList.Countries.Count; index++)
                    {
                        if (countryList.Countries[index].CountryId == countryId)
                        {
                            break;
                        }
                    }

                    int offset = 0;

                    if (index < countryList.Countries.Count)
                    {
                        page = index / rows + 1;
                        offset = (page - 1) * rows;
                    }

                    countries = countryList.Countries.Skip(offset).Take(rows).ToList();
                }
                else
                {
                    int offset = (page - 1) * rows;
                    countryList = await AirportBusinessService.GetCountries(sidx, sord, offset, rows);
                    countries = countryList.Countries;
                }

                int totalPages = (countryList.TotalRecords + rows - 1) / rows;
                jqgridResponse = new jqGridResponse<CountryDTO>(page, countryList.TotalRecords, totalPages, countries, countryId);
            }
            catch (Exception exception)
            {
                jqgridResponse = new jqGridResponse<CountryDTO>(0, 0, 0, new List<CountryDTO>(), exception.Message);
            }

            result = Json(jqgridResponse);
            return result;
        }

        [HttpPost]
        public async Task<ActionResult> NearBy(double latitude, double longitude)
        {
            JsonResult result = null;

            try
            {
                var model = new AirportsNearByModel();
                await model.Load(AirportBusinessService, latitude, longitude, 100);

                var view = await ControllerContext.RenderRazorViewToString("_AirportsNearBy", model);

                var data = new { Html = view, Airports = model.Airports };

                result = Json(data);
            }
            catch (Exception exception)
            {
                var data = new { Html = exception.Message };
                result = Json(data);
            }

            return result;
        }
    }
}
