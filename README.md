# AirExplorer
A sandbox for ASP.Net MVC / ASP.Net Core

This repository holds a sandbox application called AirExplorer.  This application is for exploring different technologies
and techniques within the ASP.Net MVC and ASP.Net Core stacks.

A web application called AirExplorer is used for the test bed.  The AirExplorer project allows the user to explorer
the location of different airports around the world.  The Virtual Radar Server project's StandingData database is used
as the backing datastore.

This repository holds two interesting ideas.  The first, wrappers for JQuery UI in MVC Fluent parlance can be found
in the "WebControls.cs" file.  Controls are provided for Checkbox, RadioButton, DropDownList, and Grid - the grid
wraps the popular free-jqGrid control.

The second is the OrderBy library found in the "OrderByExtensions.cs" file.  It attempts to get rid of the a priori
knowledge of database sort fields.  Basically, the model properties that can be used to order by are marked with
an attribute.  The library takes care of the rest, including providing strings for the sort fields.  These strings
can then be used to mark sortable columns, etc.  No longer do you have to just assume the data layer will take
"Name" as a sort field.  Now you'll know it because the data model dictates it.

Building
========
```
dotnet build
```

Running
=======
Use ``dotnet run --project Web/Web.csproj``

The dotnet core webserver will start, then use a browser to navigate to the listed URL.

AirExplorer
==========
Included is a sample ASP.Net MVC web application for Airport exploring based on NASA WorldWind and Virtual Radar Server / StandingData.
It uses both the JQuery UI MVC and OrderBy for demonstration.  The free-jqGrid is used for grid-type data and an MVC wrapper is included.


WorldWind
=========
NASA open source global with different layers, terrain, positioning, etc. etc. See: https://worldwind.arc.nasa.gov/

WebControls
===========
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

	This is called an "OrderByJoin".  The caller specifies "CountryId" as the sort field, but the actual sorting column is
	Country.Name - a property in a foreign entity.

Getting names of properties that can be ordered by
--------------------------------------------------
string fieldToSort = OrderByExtensions.GetOrderByFields<Airport>()[0];


Using OrderBy - and .Include those Foreign Entities for sorting
---------------------------------------------------------------
var query = Airports.IncludeOrderByJoins(fieldToSort).Where(a => a.Name == "My Airport").OrderBy(fieldToSort, "ASC");

