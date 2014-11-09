using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Mvc.Html;			// needed for HtmlHelper class extensions.
using WebMatrix.Data;

namespace RazorTests.Controllers
{
	public static class MarcHelpers
	{
		// http://stackoverflow.com/questions/621235/using-htmlhelper-in-a-controller
		public static HtmlHelper GetHtmlHelper(this Controller controller)
		{
			// If we don't fully initialize the view context (in order words, we can't just do "new ViewContext") then the call to, say, "htmlHelper.TextBox(...)" fails with a null reference exception.
			var viewContext = new ViewContext(controller.ControllerContext, new FakeView(), controller.ViewData, controller.TempData, TextWriter.Null);
			return new HtmlHelper(viewContext, new ViewPage());
		}
	}

	public class FakeView : IView
	{
		public void Render(ViewContext viewContext, TextWriter writer)
		{
			throw new InvalidOperationException();
		}
	}

	public class ColumnMetadata
	{
		public enum Control
		{
			Unknown,
			TextBox,
			DropDownList,
			CheckBox,
			RadioButton,
		}

		public string ColumnName { get; set; }
		public string HeaderText { get; set; }
		public Control ControlType { get; set; }
		public string ID { get; set; }
		public string ColumnID { get; set; }

		// For collection controls:
		public IEnumerable Items { get; set; }
		public string IdFieldName { get; set; }
		public string TextFieldName { get; set; }
		public string SelectedID { get; set; }
		public string SelectedText { get; set; }
	}

	public class GridMetadata
	{
		public List<ColumnMetadata> Columns { get; protected set; }

		public GridMetadata()
		{
			Columns = new List<ColumnMetadata>();
		}

		public void AddColumn(string columnID, string id, ColumnMetadata.Control ctrlType, string colName, string headerText)
		{
			Columns.Add(new ColumnMetadata() { ColumnID = columnID, ID = id, ControlType = ctrlType, ColumnName = colName, HeaderText = headerText });
		}

		public void AddColumn(string columnID, string id, ColumnMetadata.Control ctrlType, string colName, string headerText, IEnumerable items, string idFieldName, string textFieldName, string selectedID, string selectedText)
		{
			Columns.Add(new ColumnMetadata() { ColumnID = columnID, ID = id, ControlType = ctrlType, ColumnName = colName, HeaderText = headerText, Items=items, IdFieldName=idFieldName, TextFieldName=textFieldName, SelectedID=selectedID, SelectedText=selectedText });
		}

		/*
			"<td class='col1'><button class='delete-item display-mode' id='-1'>Delete</button> <button class='edit-item display-mode' id='-1'>Edit</button> <button class='save-new-item edit-mode' id='-1'>Save</button>" +
			"<td class='col2'><span id='title' class='display-mode'/> <input name='Title' id='Title' class='edit-mode'/></td>" +
			"<td class='col3'><span id='authorname' class='display-mode'/> <select name='AuthorId' id='AuthorId' class='edit-mode'></select></td>" +
			"<td class='col4'><span id='category' class='display-mode'/> <select name='CategoryId' id='CategoryId' class='edit-mode'></select></td>" +
			"<td class='col5'><span id='isbn' class='display-mode'/> <input name='ISBN' id='ISBN' class='edit-mode'/></td>" +
			"<td class='col6'><input id='hardback-display' class='display-mode' type='checkbox' name='hardback-display'/> <input name='HardBack' id='Hardback' type='checkbox' class='edit-mode'/></td>");

		 */

		public HtmlString GetInlineNewRow()
		{
			StringBuilder sb = new StringBuilder();

			// Buttons
			sb.Append("<td class='col1'><button class='delete-item display-mode' id='-1'>Delete</button> <button class='edit-item display-mode' id='-1'>Edit</button> <button class='save-new-item edit-mode' id='-1'>Save</button>");

			foreach (ColumnMetadata cm in Columns)
			{
				switch (cm.ControlType)
				{
					case ColumnMetadata.Control.TextBox:
						sb.Append("<td class='" + cm.ColumnID + "'><span id='" + cm.ID + "' class='display-mode'/> <input name='" + cm.ColumnName + "' id='" + cm.ColumnName + "' class='edit-mode'/></td>");
						break;

					case ColumnMetadata.Control.DropDownList:
						sb.Append("<td class='" + cm.ColumnID + "'><span id='" + cm.TextFieldName + "' class='display-mode'/> <select name='" + cm.SelectedID + "' id='" + cm.SelectedID + "' class='edit-mode'></select></td>");
						break;

					case ColumnMetadata.Control.CheckBox:
						sb.Append("<td class='" + cm.ColumnID + "'><input id='" + cm.ColumnName.ToLower() + "-display' class='display-mode' type='checkbox' name='" + cm.ColumnName.ToLower() + "-display'/> <input name='" + cm.ColumnName + "' id='" + cm.ColumnName + "' type='checkbox' class='edit-mode'/></td>");
						break;
				}
			}

			return new HtmlString("\""+sb.ToString()+"\"");
		}
	}

    public class ViewBookController : Controller
    {
		public ActionResult GenericGrid()
		{
			var books = InitVars();
			HtmlHelper helper = this.GetHtmlHelper();
			
			// Initialize grid metadata
			GridMetadata gridMetadata = new GridMetadata();
			gridMetadata.AddColumn("col2", "title", ColumnMetadata.Control.TextBox, "Title", "Title");
			gridMetadata.AddColumn("col3", "AuthorId", ColumnMetadata.Control.DropDownList, "AuthorId", "Author", ViewBag.Authors, "AuthorId", "authorname", "AuthorId", "AuthorName");
			gridMetadata.AddColumn("col4", "CategoryId", ColumnMetadata.Control.DropDownList, "CategoryId", "CategoryId", ViewBag.Categories, "CategoryId", "category", "CategoryId", "Category");
			gridMetadata.AddColumn("col5", "isbn", ColumnMetadata.Control.TextBox, "ISBN", "ISBN");
			gridMetadata.AddColumn("col6", "Harback", ColumnMetadata.Control.CheckBox, "Hardback", "Hardback");

			// Initialize grid
			WebGrid grid = new WebGrid(books, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");

			List<WebGridColumn> columnSet = new List<WebGridColumn>();
			columnSet.Add(grid.Column("", "", (item) => GetEditButtons(item), "col1", true));

			foreach (ColumnMetadata cm in gridMetadata.Columns)
			{
				WebGridColumn wgc = null;

				switch (cm.ControlType)
				{
					case ColumnMetadata.Control.TextBox:
						wgc = grid.Column(cm.ColumnName, cm.HeaderText, (item) =>
							{
								// PropertyInfo pi = ((Type)item.GetType()).GetProperty(cm.ColumnName);
								return GetTextControl(helper, cm.ColumnName, cm.ID, item[cm.ColumnName].ToString());
							}, cm.ColumnID, true);
						break;

					case ColumnMetadata.Control.DropDownList:
						wgc = grid.Column(cm.ID, cm.HeaderText, (item) =>
							{
								// PropertyInfo piID = ((Type)item.GetType()).GetProperty(cm.SelectedID);
								// PropertyInfo piText = ((Type)item.GetType()).GetProperty(cm.SelectedText);
								return GetDropDownList(helper, cm.Items, cm.IdFieldName, cm.TextFieldName, item[cm.SelectedID].ToString(), item[cm.SelectedText].ToString());
							}, cm.ColumnID, true);
						break;

					case ColumnMetadata.Control.CheckBox:
						wgc = grid.Column(cm.ColumnName, cm.HeaderText, (item) =>
							{
								// PropertyInfo pi = ((Type)item.GetType()).GetProperty(cm.ColumnName);
								return GetCheckBox(helper, cm.ColumnName, item[cm.ColumnName]);
							}, cm.ColumnID, true);
						break;
				}

				columnSet.Add(wgc);
			}
			
			ViewBag.Grid = grid;
			ViewBag.ColumnSet = columnSet;
			ViewBag.InlineNewRow = gridMetadata.GetInlineNewRow();

			return View();
		}

		public object GetEditButtons(dynamic item)
		{
			int bookID = item.BookId;

			return new HtmlString("<button class='delete-item display-mode' id='"+bookID+"'>Delete</button>" +
                   "<button class='edit-item display-mode' id='"+bookID+"'>Edit</button>" +
                   "<button class='save-item edit-mode edit-width' id='"+bookID+"'>Save</button>");
		}

		public object GetTextControl(HtmlHelper helper, string name, string id, string value)
		{
			var tb = helper.TextBox(name, value, new { @class = "edit-mode", size = "45" });

			return new HtmlString("<span id='"+id+"' class='display-mode'>" + value + "</span>" + tb.ToString());
		}

		public object GetDropDownList(HtmlHelper helper, IEnumerable items, string id, string name, string idValue, string textValue)
		{
			//<span id="authorname" class="display-mode">@item.AuthorName</span>
			//@Html.DropDownList("AuthorId", new SelectList(authors, "Value", "Text", @item.AuthorId), new {@class="edit-mode"})
			string text1 = "<span id='" + name + "' class='display-mode'>" + textValue + "</span>";
			string text2 = helper.DropDownList(id, new SelectList(items, "Value", "Text", idValue), new { @class = "edit-mode" }).ToString();

			return new HtmlString(text1 + text2);
		}

		public object GetCheckBox(HtmlHelper helper, string name, bool value)
		{
			var cb1 = helper.CheckBox(name, value, new { @class = "edit-mode" });
			var cb2 = helper.CheckBox(name.ToLower() + "-display", value, new { disabled = "disabled" });
			string text1 = "<span class='display-mode'>" + cb2.ToString() + "</span>";
			string text2 = cb1.ToString();

			return new HtmlString(text1 + text2);
		}

        public ActionResult Index()
        {
			var books = InitVars();

			var grid = new WebGrid(books, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");

			ViewBag.Grid = grid;

            return View();
        }

		protected IEnumerable<dynamic> InitVars()
		{
			var db = Database.Open("Books");

			var books = db.Query(@"SELECT b.BookId, b.Title, b.ISBN, b.AuthorId, b.CategoryId, 
                            a.FirstName + ' ' + a.LastName AS AuthorName, c.Category, b.Hardback  
                            FROM Books b INNER JOIN Authors a 
                            ON b.AuthorId = a.AuthorId  
                            INNER JOIN Categories c 
                            ON b.CategoryId = c.CategoryId 
                            ORDER BY b.BookId DESC");

			var categories = db.Query("SELECT CategoryId, Category FROM Categories")
								.Select(category => new SelectListItem
								{
									Value = category.CategoryId.ToString(),
									Text = category.Category
								});

			var authors = db.Query("SELECT AuthorId, FirstName + ' ' + LastName AS AuthorName FROM Authors")
								.Select(author => new SelectListItem
								{
									Value = author.AuthorId.ToString(),
									Text = author.AuthorName
								});

			ViewBag.Categories = categories;
			ViewBag.Authors = authors;

			return books;
		}

		public ActionResult GetAuthors()
		{
			var db = Database.Open("Books");
			var data = db.Query(@"SELECT AuthorId, FirstName + ' ' + LastName As Author FROM Authors");
			var jsonResult = System.Web.Helpers.Json.Encode(data);
			return Json(jsonResult, JsonRequestBehavior.AllowGet);
		}

		public ActionResult GetCategories()
		{
			var db = Database.Open("Books");
			var data = db.Query(@"SELECT CategoryId, Category FROM Categories order by Category");
			var jsonResult = System.Web.Helpers.Json.Encode(data);
			return Json(jsonResult, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult DeleteBook()
		{
			var bookId = Request["BookId"];
			var db = Database.Open("Books");
			var data = db.Execute(@"delete from Books where BookID=@0", bookId);

			return new EmptyResult();
		}


		[HttpPost]
		public ActionResult SaveChanges()
		{
			var bookId = Request["BookId"];
			var title = Request["Title"];
			var authorId = Request["AuthorId"];
			var categoryId = Request["CategoryId"];
			var isbn = Request["ISBN"];
			var hardback = Request["Hardback"];
 
			var db = Database.Open("Books");

			// Update or insert?
			if (bookId == "-1")
			{
				var sql = @"INSERT INTO Books (Title, ISBN, AuthorId, CategoryId, Hardback) VALUES (@0, @1, @2, @3, @4)";
				db.Execute(sql, title, isbn, authorId, categoryId, hardback);
				bookId = db.GetLastInsertId().ToString();
			}
			else
			{
				var sql = "UPDATE Books SET Title = @0, AuthorId = @1, CategoryId = @2, ISBN = @3, Hardback = @4 WHERE BookId = @5";
				db.Execute(sql, title, authorId, categoryId, isbn, hardback, bookId);
			}

			// Not used, as the client updates itself.

			/*
			sql = @"SELECT b.Title, b.ISBN,  a.FirstName + ' ' + a.LastName AS AuthorName, c.Category, b.Hardback  
            FROM Books b INNER JOIN Authors a ON b.AuthorId = a.AuthorId  
            INNER JOIN Categories c ON b.CategoryId = c.CategoryId 
            WHERE BookId = @0";
			var result = db.QuerySingle(sql, bookId);
			// Json.Write(result, Response.Output);
			var jsonResult = System.Web.Helpers.Json.Encode(result);
			return Json(jsonResult, JsonRequestBehavior.AllowGet);
			*/

			// return new EmptyResult();
			return Json(new { BookId = bookId });
		}
    }
}

