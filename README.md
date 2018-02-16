# JQuery-UI-MVC
ASP.Net MVC Template Wrappers for JQuery UI

AirExplorer
==========
Included is a sample ASP.Net MVC web application for Airport exploring based on NASA WorldWind and Virtual Radar Server / StandingData.
It uses both the JQuery UI MVC and OrderBy for demonstration.  The free-jqGrid is used for grid-type data and an MVC wrapper is included.

WorldWind
=========
NASA open source global with different layers, terrain, positioning, etc. etc.


JQuery UI MVC
=============
This GitHub Repository holds ASP.NET MVC Wrappers for some widgets in the JQuery UI widget set.

The wrappers are very similar to the stock Html wrappers and implement a Fluent design like Kendo MVC and FluentHtml.


OrderBy
=======
You may also find the "OrderBy" library very interesting.  Instead of hard-coding order by strings, you can mark properties in a
model with an attribute and "OrderBy" does the rest with DbQuery.OrderBy("property-name", "ASC"). [or "DESC"].

You can order by "joined" fields.  If your EF diagrams includes foreign key constraints, your entities will have a property for
the foreign entity.  In this case, you can specify the [OrderBy(Include=typeof(foreign-entity), JoinField = nameof(foreign-entity.Property)]

For example, AirportDTO includes a "CountryId" field that we want to alias as "County.Name" when sorting.  When EF queries for this, we
have to specify an .Include("Country") to get the foreign entity fetched.  We want to sort by the "Name" property within the Country.


AirportDTO.cs : order by a Property in a Foreign Entity
-------------------------------------------------------
	[OrderBy(typeof=(Country), Name="Country.Name")]
	public int CountryId { get; set; }

Getting names of properties that can be ordered by
--------------------------------------------------
string fieldToSort = OrderByExtensions.GetOrderByFields<Airport>()[0];


Using OrderBy - and .Include those Foreign Entities for sorting
---------------------------------------------------------------
var query = Airports.IncludeOrderByJoins(fieldToSort).Where(a => a.Name == "My Airport").OrderBy(fieldToSort, "ASC");

