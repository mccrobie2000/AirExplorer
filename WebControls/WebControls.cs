using OrderBy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Antiforgery.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace WebControls
{
    public static class HtmlHelpers
    {
        public static IHtmlHelper<TModel> For<TModel>(this IHtmlHelper helper) where TModel : class, new()
        {
            TModel model = new TModel();
            return For<TModel>(helper.ViewContext, helper.ViewData, model);
        }

        public static IHtmlHelper<TModel> For<TModel>(ViewContext viewContext, ViewDataDictionary viewData, TModel model) where TModel : class, new()
        {
            // Spin up a new HtmlHelper<T> "by hand" since we need the parameterized version which can only be done at compile time.
            // Everything else can get had from the IOC container.
            var htmlGenerator = viewContext.HttpContext.RequestServices.GetRequiredService(typeof(IHtmlGenerator)) as IHtmlGenerator;
            var compositeViewEngine = viewContext.HttpContext.RequestServices.GetRequiredService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            var modelMetadataProvider = viewContext.HttpContext.RequestServices.GetRequiredService(typeof(IModelMetadataProvider)) as IModelMetadataProvider;
            var bufferScope = viewContext.HttpContext.RequestServices.GetRequiredService(typeof(IViewBufferScope)) as IViewBufferScope;
            var htmlEncoder = viewContext.HttpContext.RequestServices.GetRequiredService(typeof(HtmlEncoder)) as HtmlEncoder;
            var urlEncoder = UrlEncoder.Default;
            var expressionTextCache = viewContext.HttpContext.RequestServices.GetRequiredService(typeof(ExpressionTextCache)) as ExpressionTextCache;

            var htmlHelper = new HtmlHelper<TModel>(htmlGenerator,
                compositeViewEngine,
                modelMetadataProvider,
                bufferScope,
                htmlEncoder,
                urlEncoder,
                expressionTextCache);

            if (htmlHelper is IViewContextAware contextable)
            {
                // Must use this constructor since we're changing view model types
		        var newViewDataDictionary = new ViewDataDictionary<TModel>(viewData, model);

                var newViewContext = new ViewContext(viewContext, viewContext.View, newViewDataDictionary, viewContext.Writer);
                contextable.Contextualize(newViewContext);
            }

            return htmlHelper;
        }

        //public static HtmlHelper<TModel> ForEnumerable<TModel, T>(ViewContext viewContext, ViewDataDictionary viewData, RouteCollection routeCollection)
        //{
        //    var newViewData = new ViewDataDictionary(viewData);
        //    ViewContext newViewContext = new ViewContext(
        //        viewContext.Controller.ControllerContext,
        //        viewContext.View,
        //        newViewData,
        //        viewContext.TempData,
        //        viewContext.Writer);
        //    var viewDataContainer = new ViewDataContainer(newViewContext.ViewData);
        //    return new HtmlHelper<TModel>(newViewContext, viewDataContainer, routeCollection);
        //}

        //public static HtmlHelper<TModel> ForEnumerable<TModel, T>(ViewContext viewContext, ViewDataDictionary viewData, RouteCollection routeCollection, T model)
        //{
        //    var newViewData = new ViewDataDictionary(viewData) { Model = model };
        //    ViewContext newViewContext = new ViewContext(
        //        viewContext.Controller.ControllerContext,
        //        viewContext.View,
        //        newViewData,
        //        viewContext.TempData,
        //        viewContext.Writer);
        //    var viewDataContainer = new ViewDataContainer(newViewContext.ViewData);
        //    return new HtmlHelper<TModel>(newViewContext, viewDataContainer, routeCollection);
        //}

//        private class ViewDataContainer : System.Web.Mvc.IViewDataContainer
//        {
//            public System.Web.Mvc.ViewDataDictionary ViewData { get; set; }
//
//            public ViewDataContainer(System.Web.Mvc.ViewDataDictionary viewData)
//            {
//                ViewData = viewData;
//            }
//        }
    }

    public static class MvcWebControls
    {
        public static MvcWebControlsUI<T> WebControls<T>(this IHtmlHelper<T> helper)
        {
            return new MvcWebControlsUI<T>(helper);
        }
    }

    public class MvcWebControlsUI<T>
    {
        private IHtmlHelper<T> _htmlHelper;
        public MvcWebControlsUI(IHtmlHelper<T> htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        public Grid<T> Grid()
        {
            return new Grid<T>(_htmlHelper);
        }
        public Grid<T> Grid(IEnumerable<T> model)
        {
            return new Grid<T>(_htmlHelper, model);
        }

        public DropDownList<T, TProperty> DropDownListFor<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            return new DropDownList<T, TProperty>(_htmlHelper, expression);
        }

        public RadioButton<T, TProperty> RadioButtonFor<TProperty>(Expression<Func<T, TProperty>> expression, string display, string value)
        {
            return new RadioButton<T, TProperty>(_htmlHelper, expression, display, value);
        }

        public RadioButtonSet<T, TProperty> RadioButtonSet<TProperty>(Expression<Func<T, TProperty>> expression, Action<IList<SelectListItem>> items)
        {
            return new RadioButtonSet<T, TProperty>(_htmlHelper, expression, items);
        }
        public RadioButtonSet<T, TProperty> RadioButtonSet<TProperty>(Expression<Func<T, TProperty>> expression, IEnumerable<SelectListItem> items)
        {
            return new RadioButtonSet<T, TProperty>(_htmlHelper, expression, items);
        }

        public CheckBox<T, bool> CheckboxFor(Expression<Func<T, bool>> expression)
        {
            return new CheckBox<T, bool>(_htmlHelper, expression);
        }

        //public RadioButtonSet<T> RadioButtonSetFor()
        //{
        //    return new RadioButtonSet<T>(_htmlHelper);
        //}
    }

    public abstract class BaseWebControl<T> : IHtmlContent
    {
        protected IHtmlHelper<T> htmlHelper;

        protected BaseWebControl(IHtmlHelper<T> htmlHelper)
        {
            this.htmlHelper = htmlHelper;
        }

	public abstract void WriteTo(TextWriter textWriter, HtmlEncoder encoder);
    }

    #region Obsolete
    internal class xWebControlUtilities
    {
        public static PropertyInfo GetPropertyInfo<T>(Expression expression)
        {
            Type type = typeof(T);

            MemberExpression member = expression as MemberExpression;

            PropertyInfo propertyInfo = member.Member as PropertyInfo;

            return propertyInfo;
        }

        public static string GetDisplayName<T>(PropertyInfo propertyInfo)
        {
            var attribute = propertyInfo.GetCustomAttribute(typeof(DisplayNameAttribute), true) as DisplayNameAttribute;
            var displayName = attribute?.DisplayName ?? propertyInfo.Name;
            return displayName;
        }

        public static string GetOrderByName<T>(PropertyInfo propertyInfo)
        {
            var attribute = propertyInfo.GetCustomAttribute(typeof(OrderByNameAttribute), true) as OrderByNameAttribute;
            var orderByName = attribute?.Name ?? propertyInfo.Name;
            return orderByName;
        }

        public static void GetPropertyParameters<T>(Expression expression, out string propertyName, out string displayName, out Type type, out string orderByName)
        {
            propertyName = "";
            displayName = "";
            orderByName = "";
            type = typeof(Object);

            RecurseForProperty<T>(expression, out propertyName, out displayName, out type, out orderByName);
        }

        private static void RecurseForProperty<T>(Expression expression, out string propertyName, out string displayName, out Type type, out string orderByName)
        {
            propertyName = "";
            displayName = "";
            orderByName = "";
            type = typeof(Object);

            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpression = (MemberExpression)expression;
                    propertyName = memberExpression.Member is PropertyInfo ? memberExpression.Member.Name : null;
                    displayName = memberExpression.Member is PropertyInfo ? GetDisplayName<T>(memberExpression.Member as PropertyInfo) : null;
                    orderByName = memberExpression.Member is PropertyInfo ? GetOrderByName<T>(memberExpression.Member as PropertyInfo) : null;
                    type = memberExpression.Type;
                    break;
                case ExpressionType.Convert:
                    var unary = expression as UnaryExpression;
                    if (unary != null)
                    {
                        RecurseForProperty<T>(unary.Operand, out propertyName, out displayName, out type, out orderByName);
                    }
                    break;
            }
        }
    }
    #endregion

    #region RadioButtonSet

    public class RadioButtonSet<T, TProperty> : BaseWebControl<T>
    {
        private IList<SelectListItem> _items;
        private string _name;
        private string _id;

        public RadioButtonSet(IHtmlHelper<T> htmlHelper, Expression<Func<T, TProperty>> expression, Action<IList<SelectListItem>> items) : base(htmlHelper)
        {
            _items = new List<SelectListItem>();

            items(_items);

            _name = htmlHelper.NameFor(expression);
            _id = htmlHelper.IdFor(expression);
        }

        public RadioButtonSet(IHtmlHelper<T> htmlHelper, Expression<Func<T, TProperty>> expression, IEnumerable<SelectListItem> list)
            : this(htmlHelper, expression, items => { foreach (var i in list) items.Add(i); })
        {
        }

	public override void WriteTo(TextWriter textWriter, HtmlEncoder encoder)
        {
            var random = new Random();
            var divName = random.Next().ToString();

            StringBuilder b = new StringBuilder();

            b.Append($"<div id='{divName}'>");
            foreach (var item in _items)
            {
                b.Append($"<label for='{_id}'>{item.Text}</label>");
                b.Append($"<input id='{_id}' name='{_name}', type='radio' value='{item.Value}'>");
            }
            b.Append("</div>");
            b.Append($"<script>$('#{divName}').buttonset();</script>");

            textWriter.Write(b.ToString());
        }
    }

    #endregion

    #region RadioButton

    public class RadioButton<T, TProperty> : BaseWebControl<T>
    {
        protected bool _checked;
        protected bool _enabled;
        protected string _id;
        protected string _name;
        protected string _display;
        protected string _value;
        protected Expression<Func<T, TProperty>> _fieldAccessor;

        public RadioButton(IHtmlHelper<T> htmlHelper, Expression<Func<T, TProperty>> expression, string display, string value) : base(htmlHelper)
        {
            _fieldAccessor = expression;
            _display = display;
            _value = value;
            _name = htmlHelper.NameFor(expression);
            _id = htmlHelper.IdFor(expression);

            _enabled = true;
        }

        public RadioButton<T, TProperty> Checked(bool check)
        {
            _checked = check;
            return this;
        }

	public override void WriteTo(TextWriter textWriter, HtmlEncoder encoder)
        {
            StringBuilder b = new StringBuilder();

            var isChecked = _checked ? "checked" : string.Empty;

            b.Append($"<label for='{_id}'>{_display}</label>");
            b.Append($"<input type='radio' name='{_name}' id='{_id}' {isChecked} value='{_value}'>");
            b.Append($"<script>$('#{_id}').checkboxradio();</script>");

            textWriter.Write(b.ToString());
        }
    }

    #endregion

    #region Checkbox

    public class CheckBox<T, TProperty> : BaseWebControl<T>
    {
        protected bool _checked;
        protected bool _enabled;
        protected string _id;
        protected string _name;
        protected Expression<Func<T, TProperty>> _fieldAccessor;

        public CheckBox(IHtmlHelper<T> htmlHelper, Expression<Func<T, TProperty>> expression) : base(htmlHelper)
        {
            _fieldAccessor = expression;

            _enabled = true;

            _name = htmlHelper.NameFor(expression);
            _id = htmlHelper.IdFor(expression);
        }

        public CheckBox<T, TProperty> Enable(bool enabled)
        {
            _enabled = enabled;
            return this;
        }

        public CheckBox<T, TProperty> Checked(bool check)
        {
            _checked = check;
            return this;
        }


	public override void WriteTo(TextWriter textWriter, HtmlEncoder encoder)
        {
            StringBuilder b = new StringBuilder();

            var isChecked = _checked ? "checked" : string.Empty;

            b.Append($"<input type='checkbox' name='{_name}' id='{_id}' {isChecked} value='true'>");
            b.Append($"<script>$('#{_id}').checkboxradio();</script>");

            textWriter.Write(b.ToString());
        }
    }

    #endregion

    #region DropDownList

    public class DropDownList<T, TProperty> : BaseWebControl<T>
    {
        private string _name;
        private string _id;
        private DropDownListItemCollection _collection;
        private DropDownListEvent _events;

        public DropDownList(IHtmlHelper<T> htmlHelper) : base(htmlHelper)
        {
            var random = new Random();
            _name = $"_ddl{random.Next()}";
            _id = _name;
        }

        public DropDownList(IHtmlHelper<T> htmlHelper, Expression<Func<T, TProperty>> expression)
            : base(htmlHelper)
        {
            _id = htmlHelper.IdFor(expression).ToString();
            _name = htmlHelper.NameFor(expression).ToString();
        }

        public DropDownList<T, TProperty> Items(Action<DropDownListItemCollection> itemsAction)
        {
            _collection = new DropDownListItemCollection(htmlHelper, itemsAction);
            return this;
        }

        public DropDownList<T, TProperty> BindTo(IEnumerable<DropDownListItem> items)
        {
            _collection = new DropDownListItemCollection(htmlHelper, items);
            return this;
        }
        public DropDownList<T, TProperty> BindTo(IEnumerable<SelectListItem> items)
        {
            _collection = new DropDownListItemCollection(htmlHelper, items);
            return this;
        }
        public DropDownList<T, TProperty> BindTo(T model, Expression<Func<T, IEnumerable<SelectListItem>>> expression)
        {
            _name = htmlHelper.NameFor(expression).ToString();
            _id = htmlHelper.IdFor(expression).ToString();

            var compiled = expression.Compile();
            var list = compiled(model);

            _collection = new DropDownListItemCollection(htmlHelper, list);

            return this;
        }


        public DropDownList<T, TProperty> Name(string name)
        {
            _name = name;
            return this;
        }

        public DropDownList<T, TProperty> Events(Action<DropDownListEvent> eventAction)
        {
            _events = new DropDownListEvent(eventAction);
            return this;
        }

	public override void WriteTo(TextWriter textWriter, HtmlEncoder encoder)
        {
            StringBuilder b = new StringBuilder();

            b.Append($"<select name='{_name}' id='{_id}'>");
            b.Append(_collection.ToString());
            b.Append("</select>");
            b.Append($"<script>$('#{_id}').selectmenu({{");
            if (_events != null) b.Append(_events.ToString());
            b.Append("});</script>");

            textWriter.Write(b.ToString());
        }
    }

    public class DropDownListEvent
    {
        private string _clientChangeFunction;

        public DropDownListEvent(Action<DropDownListEvent> eventAction)
        {
            eventAction(this);
        }

        public DropDownListEvent Change(string clientFunction)
        {
            _clientChangeFunction = clientFunction;
            return this;
        }

	public override string ToString()
        {
            char comma = ' ';
            StringBuilder b = new StringBuilder();

            if (!string.IsNullOrEmpty(_clientChangeFunction))
            {
                b.Append(comma);
                b.Append($"change: {_clientChangeFunction}");
                comma = ',';
            }

            return b.ToString();
        }
    }

    public class DropDownListItemCollection
    {
        private List<DropDownListItem> _items;
        private IHtmlHelper _htmlHelper;

        public DropDownListItemCollection(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
            _items = new List<DropDownListItem>();
        }
        public DropDownListItemCollection(IHtmlHelper htmlHelper, Action<DropDownListItemCollection> itemsAction) : this(htmlHelper)
        {
            itemsAction(this);
        }
        public DropDownListItemCollection(IHtmlHelper htmlHelper, IEnumerable<DropDownListItem> items) : this(htmlHelper)
        {
            _items.AddRange(items);
        }
        public DropDownListItemCollection(IHtmlHelper htmlHelper, IEnumerable<SelectListItem> items)
        {
            _items = items.Select(s => new DropDownListItem(htmlHelper).Text(s.Text).Value(s.Value).Selected(s.Selected)).ToList();
        }

        public DropDownListItem Add()
        {
            DropDownListItem item = new DropDownListItem(_htmlHelper);
            _items.Add(item);
            return item;
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

            foreach (var item in _items)
            {
                b.Append(item.ToString());
            }

            return b.ToString();
        }
    }

    public class DropDownListItem
    {
        private string _text;
        private string _value;
        private bool _selected;
        private IHtmlHelper _htmlHelper;

        public DropDownListItem(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        public DropDownListItem Text(string text)
        {
            _text = text;
            return this;
        }

        public DropDownListItem Value(string value)
        {
            _value = value;
            return this;
        }

        public DropDownListItem Selected(bool selected = true)
        {
            _selected = selected;
            return this;
        }

        public override string ToString()
        {
            var value = _htmlHelper.Encode(_value);
            var text = _htmlHelper.Encode(_text);

            StringBuilder b = new StringBuilder();

            var selected = _selected ? "selected" : "";
            b.Append($"<option value='{value}' {selected}>{text}</option>");

            return b.ToString();
        }
    }

    #endregion

    #region Grid

    public class Grid<T> : BaseWebControl<T>
    {
        private IEnumerable<T> _model;
        private string _name;
        private GridColumnCollection<T> _columns;
        private DataSourceBuilder<T> _dataSourceBuilder;
        private int _width;
        private string _caption;
        private GridEvents<T> _events;
        private string _initialSort;

        public Grid(IHtmlHelper<T> htmlHelper, IEnumerable<T> model) : base(htmlHelper)
        {
            _model = model;
            _name = "A" + (new Random().Next().ToString());
            _events = new GridEvents<T>(htmlHelper);
        }
        public Grid(IHtmlHelper<T> htmlHelper) : base(htmlHelper)
        {
            _name = "A" + (new Random().Next().ToString());
            _events = new GridEvents<T>(htmlHelper);
        }

        public Grid<T> Caption(string caption)
        {
            _caption = caption;
            return this;
        }
        public Grid<T> BindTo(IEnumerable<T> model)
        {
            _model = model;
            return this;
        }
        public Grid<T> Name(string name)
        {
            _name = name;
            return this;
        }
        public Grid<T> Width(int width)
        {
            _width = width;
            return this;
        }
        public Grid<T> Sortable()
        {
            return this;
        }
        public Grid<T> Columns(Action<GridColumnCollection<T>> columnsAction)
        {
            _columns = new GridColumnCollection<T>(htmlHelper, columnsAction);
            return this;
        }
        public Grid<T> DataSource(Action<DataSourceBuilder<T>> dataSourceBuilder)
        {
            _dataSourceBuilder = new DataSourceBuilder<T>(htmlHelper);
            dataSourceBuilder(_dataSourceBuilder);
            return this;
        }
        public Grid<T> Events(Action<GridEvents<T>> events)
        {
            events(_events);
            return this;
        }
        public Grid<T> InitialSort<TProperty>(Expression<Func<T, TProperty>> initialSort)
        {
            var provider = new EmptyModelMetadataProvider();
            var metadata = ExpressionMetadataProvider.FromLambdaExpression(initialSort, htmlHelper.ViewData, provider);
            _initialSort = metadata.Metadata.PropertyName;

            return this;
        }

	public override void WriteTo(TextWriter textWriter, HtmlEncoder encoder)
        {
            try
            {
                bool isLocal = false;
                string localDataName = $"{_name}LocalData";
                string pagerName = $"{_name}Pager";

                StringBuilder b = new StringBuilder();

                // Use jqGrid as the grid control on the javascript side.

                b.Append($"<div><table id='{_name}'></table><div id='{pagerName}'></div></div>");
                b.Append("<script>");

                if (_model != null && _dataSourceBuilder == null)
                {
                    isLocal = true;
                    var m = _columns.BuildLocalData(localDataName, _model);
                    b.Append(m);
                }

                b.Append($"$('#{_name}').jqGrid({{");
                string comma = "";

                if (isLocal)
                {
                    b.Append(comma);
                    b.Append("datatype: 'local'");
                    b.Append(",");
                    b.Append($"data: {localDataName}");
                    comma = ",";
                }
                else if (_dataSourceBuilder != null)
                {
                    b.Append(comma);
                    b.Append(_dataSourceBuilder.ToString());
                    b.Append(",");
                    b.Append("mtype: 'POST'");
                    comma = ",";
                }

                if (!string.IsNullOrEmpty(_initialSort))
                {
                    b.Append(comma);
                    b.Append($"sortname: '{_initialSort}'");
                    comma = ",";
                }
                b.Append(comma);
                b.Append(_columns.ToString());
                comma = ",";
                b.Append(",");
                b.Append("scrollrows: true");
                b.Append(",");
                b.Append("viewrecords: true");
                b.Append(",");
                b.Append($"pager: '#{pagerName}'");
                b.Append(",");
                b.Append($"pagerpos: 'left'");
                b.Append(",");
                b.Append($"recordpos: 'center'");
                b.Append(",");
                b.Append($"width: {_width}");
                if (!string.IsNullOrEmpty(_caption))
                {
                    b.Append(comma);
                    b.Append($"caption: '{htmlHelper.Encode(_caption)}'");
                    comma = ",";
                }
                b.Append(comma);
                b.Append(_events.ToString());
                comma = ",";

                b.Append("});");
                b.Append("</script>");

                textWriter.Write(b.ToString());
            }
            catch
            {
            }
        }
    }

    public class GridEvents<T>
    {
        private IHtmlHelper<T> _htmlHelper;
        private string _selectRow;
        private string _loadComplete;
        private string _userDataHandler;

        public GridEvents(IHtmlHelper<T> htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        public GridEvents<T> SelectRow(string jsFunction)
        {
            _selectRow = jsFunction;
            return this;
        }
        public GridEvents<T> LoadComplete(string jsFunction)
        {
            _loadComplete = jsFunction;
            return this;
        }
        public GridEvents<T> UserDataHandler(string jsFunction)
        {
            _userDataHandler = jsFunction;
            return this;
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            string comma = "";

            if (!string.IsNullOrEmpty(_selectRow))
            {
                b.Append(comma);
                b.Append($"onSelectRow: {_selectRow}");
                comma = ",";
            }

            b.Append(comma);
            b.Append("loadComplete: function(data) {");
            if (!string.IsNullOrEmpty(_loadComplete))
            {
                if (!string.IsNullOrEmpty(_loadComplete))
                {
                    b.Append($"{_loadComplete}(data);");
                }
            }
            if (!string.IsNullOrEmpty(_userDataHandler))
            {
                b.Append("if (data != null && typeof(data.userdata) != 'undefined' && data.userdata != null) {");
                b.Append($"{_userDataHandler}(data.userdata);");
                b.Append("}");
            }
            b.Append("}");
            comma = ",";

            return b.ToString();
        }
    }

    public class DataSourceBuilder<T>
    {
        private IHtmlHelper<T> _htmlHelper;
        private AjaxDataSourceBuilder<T> _ajax;

        public string Url
        {
            get
            {
                if (_ajax != null)
                    return _ajax.Url;
                return string.Empty;
            }
        }
        public string DataType
        {
            get
            {
                if (_ajax != null)
                    return _ajax.DataType;
                return string.Empty;
            }
        }
        public int NumberRecordsPerPage
        {
            get
            {
                if (_ajax != null)
                    return _ajax.NumberRecordsPerPage;
                return 20;
            }
        }

        public DataSourceBuilder(IHtmlHelper<T> htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }
        public AjaxDataSourceBuilder<T> Ajax()
        {
            _ajax = new AjaxDataSourceBuilder<T>(_htmlHelper);
            return _ajax;
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

            b.Append($"url: '{Url}'");
            b.Append(", datatype: 'json'");
            b.Append($", rowNum: {NumberRecordsPerPage}");
            //b.Append("jsonReader : {");
            //b.Append("root: 'rows',");
            //b.Append("page: 'page',");
            //b.Append("total: 'total',");
            //b.Append("records: 'records',");
            //b.Append("cell: '',");
            //b.Append($"id: '${_ajax.Id}',");
            //b.Append("repeatitems: false");
            //b.Append("}");

            return b.ToString();
        }
    }

    public class AjaxDataSourceBuilder<T>
    {
        private IHtmlHelper<T> _htmlHelper;
        private int? _pageSize;
        private AjaxDataReader _dataReader;
        public string Url { get { return GenerateUrl(); } }
        public string DataType { get { return "json"; } }
        public int NumberRecordsPerPage { get { return _pageSize ?? 10; } }

        public AjaxDataSourceBuilder(IHtmlHelper<T> htmlHelper)
        {
            _htmlHelper = htmlHelper;

            // Extract out the public properties and store in array with their types
        }

        public AjaxDataSourceBuilder<T> Read(Action<AjaxDataReader> dataReaderAction)
        {
            _dataReader = new AjaxDataReader(_htmlHelper);
            dataReaderAction(_dataReader);
            return this;
        }

        public AjaxDataSourceBuilder<T> Key<TProperty>(Expression<Func<T, TProperty>> id)
        {
            return this;
        }

        public AjaxDataSourceBuilder<T> PageSize(int pageSize)
        {
            _pageSize = pageSize;
            return this;
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            return b.ToString();
        }

        public string GenerateUrl()
        {
            return _dataReader.Url;
        }
    }

    public class AjaxDataReader
    {
        private IHtmlHelper _htmlHelper;
        public string ActionMethod { get; set; }
        public string Controller { get; set; }
        public object RouteValues { get; set; }
        public string Url
        {
            get
            {
                var urlHelper = new UrlHelper(_htmlHelper.ViewContext);
                return urlHelper.Action(ActionMethod, Controller, RouteValues);
            }
        }
        public AjaxDataReader(IHtmlHelper htmlHelper)
        {
            _htmlHelper = htmlHelper;
        }

        public void Action(string action, string controller)
        {
            ActionMethod = action;
            Controller = controller;
        }

        public void Action(string action, string controller, object routeValues)
        {
            ActionMethod = action;
            Controller = controller;
            RouteValues = routeValues;
        }
    }

    public abstract class GridColumn<T>
    {
        public abstract string ToString(T item);
    }

    public class GridColumnCollection<T>
    {
        private IHtmlHelper<T> _htmlHelper;
        private IList<GridColumn<T>> _columns;

        public GridColumnCollection(IHtmlHelper<T> htmlHelper, Action<GridColumnCollection<T>> columnsAction)
        {
            _htmlHelper = htmlHelper;
            _columns = new List<GridColumn<T>>();
            columnsAction(this);
        }

        public GridColumn<T, TProperty> Bound<TProperty>(Expression<Func<T, TProperty>> bounding)
        {
            GridColumn<T, TProperty> column = new GridColumn<T, TProperty>(_htmlHelper, bounding);
            _columns.Add(column);
            return column;
        }

        public string BuildLocalData(string arrayName, IEnumerable<T> data)
        {
            StringBuilder b = new StringBuilder();

            b.Append($"var {arrayName} = [");
            string ocomma = "";
            foreach (var item in data)
            {
                b.Append(ocomma);
                b.Append("{");
                string icomma = "";
                foreach (var column in _columns)
                {
                    b.Append(icomma);
                    b.Append(column.ToString(item));
                    icomma = ",";
                }
                b.Append("}");
                ocomma = ",";
            }
            b.Append("];");

            return b.ToString();
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();

            string comma = "";
            b.Append("colModel: [");
            foreach (var column in _columns)
            {
                b.Append(comma);
                b.Append(column.ToString());
                comma = ",";
            }
            b.Append("]");

            return b.ToString();
        }
    }

    public class GridColumn<T, TProperty> : GridColumn<T>
    {
        private IHtmlHelper<T> _htmlHelper;
        private ModelMetadata _metadata;
        private Expression<Func<T, TProperty>> _bounding;
        private Func<T, TProperty> _compiled;
        private bool _hidden;
        private Type _type;
        private bool _isString;
        private string _sorttype;
        private bool _sortable;
        private string _sortName;
        private bool _isKey;

        public GridColumn(IHtmlHelper<T> htmlHelper, Expression<Func<T, TProperty>> bounding)
        {
            _htmlHelper = htmlHelper;
            _bounding = bounding;
            _compiled = bounding.Compile();

            if (_bounding == null) throw new ArgumentNullException(nameof(_bounding));
            if (_htmlHelper == null) throw new ArgumentNullException(nameof(htmlHelper));

            var provider = new EmptyModelMetadataProvider();

            var modelExplorer = ExpressionMetadataProvider.FromLambdaExpression(_bounding, _htmlHelper.ViewData, provider);
            _metadata = modelExplorer.Metadata;

            _isString = _metadata.ModelType == typeof(string);
            if (_metadata.ModelType == typeof(int)) _sorttype = "int";
            else if (_metadata.ModelType == typeof(float) || _metadata.ModelType == typeof(double)) _sorttype = "float";
            else if (_metadata.ModelType == typeof(DateTime)) _sorttype = "date";
            else _sorttype = "text";
        }

        public GridColumn<T, TProperty> Sortable()
        {
            string propertyName;
            string displayName;
            Type type;

            xWebControlUtilities.GetPropertyParameters<T>(_bounding.Body, out propertyName, out displayName, out type, out _sortName);

            _sortable = true;

            return this;
        }

        public GridColumn<T, TProperty> Hidden()
        {
            _hidden = true;
            return this;
        }
        public GridColumn<T, TProperty> Key()
        {
            _isKey = true;
            return this;
        }

        public override string ToString(T row)
        {
            StringBuilder b = new StringBuilder();

            b.Append($"{_metadata.PropertyName}");
            b.Append(":");

            if (_isString)
            {
                b.Append("'");
            }

            var o = _compiled(row).ToString();
            o = _htmlHelper.Encode(o);
            b.Append(o.ToString());

            if (_isString)
            {
                b.Append("'");
            }

            return b.ToString();
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_sortName))
            {
                _sortName = _metadata.PropertyName;
            }

            StringBuilder b = new StringBuilder();

            var label = _metadata.DisplayName ?? _metadata.PropertyName ?? "";

            b.Append("{");
            b.Append($"label: '{label}'");
            b.Append(",");
            b.Append($"name: '{_metadata.PropertyName}'");
            b.Append($", hidden: {_hidden.ToString().ToLower()}");
            b.Append($", sortable: {_sortable.ToString().ToLower()}");
            b.Append($", index: '{_sortName}'");
            b.Append($", sorttype: '{_sorttype}'");
            b.Append($", key: {_isKey.ToString().ToLower()}");
            b.Append("}");

            return b.ToString();
        }
    }

    #endregion

    public class jqGridResponse<T>
    {
        public int page { get; set; }
        public int records { get; set; }
        public int total { get; set; }
        public object userdata { get; set; }
        public IEnumerable<T> rows { get; set; }

        public jqGridResponse(int _page, int _records, int _total, IEnumerable<T> _rows, object _userdata = null)
        {
            page = _page;
            records = _records;
            total = _total;
            rows = _rows;
            userdata = _userdata;
        }
    }

    //[TestClass]
    //public class Test
    //{
    //    public TestContext TestContext { get; set; }

    //    [TestMethod]
    //    public void DoTest()
    //    {
    //        HtmlHelper<ModelFoo> htmlHelper = null;

    //        var p = htmlHelper.WebControls().DropDownListFor(m => m.Bar)
    //            .Name("MyFoo")
    //            .Items(items =>
    //            {
    //                items.Add().Text("Foo 1").Value("1").Selected();
    //                items.Add().Text("Foo 2").Value("2");
    //            });

    //        var ps = p.ToHtmlString();

    //        List<ModelFoo> modelFooList = new List<ModelFoo>();
    //        modelFooList.Add(new ModelFoo { Foo = "s1", Bar = 1 });
    //        modelFooList.Add(new ModelFoo { Foo = "s2", Bar = 2 });
    //        modelFooList.Add(new ModelFoo { Foo = "s3", Bar = 3 });

    //        var o = htmlHelper.WebControls().Grid(modelFooList)
    //            .Name("MyFooTable")
    //            .Columns(columns =>
    //        {
    //            columns.Bound(c => c.Bar);
    //            columns.Bound(c => c.Foo);
    //        })
    //        .Sortable()
    //        .DataSource(dataSource =>
    //        {
    //            dataSource.Ajax().PageSize(20).Read(read => read.Action("Foo", "Bar"));
    //        });

    //        var s = o.ToHtmlString();
    //        TestContext.WriteLine("{0}", s);
    //    }
    //}
}
