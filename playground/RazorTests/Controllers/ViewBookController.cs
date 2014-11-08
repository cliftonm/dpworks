using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public class ViewBookController : Controller
    {
		public ActionResult GenericGrid()
		{
			var books = InitVars();
			HtmlHelper helper = this.GetHtmlHelper();

			WebGrid grid = new WebGrid(books, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");
			
			WebGridColumn col1 = grid.Column("", "", (item) => GetEditButtons(item), "col1", true);
			WebGridColumn col2 = grid.Column("Title", "Title", (item) => GetTextControl(helper, "Title", "title", item.Title), "col2", true);
			WebGridColumn col3 = grid.Column("AuthorId", "Author", (item) => GetDropDownList(helper, ViewBag.Authors, "AuthorId", "authorname", item.AuthorID.ToString(), item.AuthorName), "col3", true);
			WebGridColumn col4 = grid.Column("CategoryId", "Category", (item) => GetDropDownList(helper, ViewBag.Categories, "CategoryId", "category", item.CategoryID.ToString(), item.Category), "col4", true);
			WebGridColumn col5 = grid.Column("ISBN", "ISBN", (item) => GetTextControl(helper, "ISBN", "isbn", item.ISBN), "col5", true);
			WebGridColumn col6 = grid.Column("Harback", "Harback", (item) => GetCheckBox(helper, "Hardback", item.Hardback), "col6", true);
			List<WebGridColumn> columnSet = new List<WebGridColumn>() {col1, col2, col3, col4, col5, col6};

			ViewBag.Grid = grid;
			ViewBag.ColumnSet = columnSet;
/*
			ViewBag.fncPageInit = new HtmlString("$(function () {"+
                "$('.edit-mode').hide();"+
                "initializeFormEvents();"+
                "initializeGridItemEvents();"+
				"});");

			ViewBag.fncAddItem = new HtmlString("            function initializeFormEvents() {
                $('#add').on('click', function () {addBook($(this));});
            }
*/

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

