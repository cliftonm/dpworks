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
    public class ViewBookController : Controller
    {
		public ActionResult GenericGrid()
		{
			var books = InitVars();
			HtmlHelper helper = this.GetHtmlHelper();
			
			// Initialize grid metadata
			GridMetadata gridMetadata = new GridMetadata();
			gridMetadata.AddColumn("col2", "title", ColumnMetadata.Control.TextBox, "Title", "Title", "45");
			gridMetadata.AddColumn("col3", "AuthorId", ColumnMetadata.Control.DropDownList, "AuthorId", "Author", ViewBag.Authors, "AuthorId", "authorname", "AuthorId", "AuthorName", "-- Select Author --", "/ViewBook/GetAuthors");
			gridMetadata.AddColumn("col4", "CategoryId", ColumnMetadata.Control.DropDownList, "CategoryId", "Category", ViewBag.Categories, "CategoryId", "category", "CategoryId", "Category", "-- Select Category --", "/ViewBook/GetCategories");
			gridMetadata.AddColumn("col5", "isbn", ColumnMetadata.Control.TextBox, "ISBN", "ISBN", "20");
			gridMetadata.AddColumn("col6", "Harback", ColumnMetadata.Control.CheckBox, "Hardback", "Hardback", "2");

			// Initialize grid
			WebGrid grid = new WebGrid(books, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");

			List<WebGridColumn> columnSet = gridMetadata.GetColumnSet(helper, grid);

			// Look at EngineContext.Current.Resolve<GenericController>() to be able to call the controller methods directly.
			// See http://stackoverflow.com/questions/5960664/calling-a-method-in-the-controller

			ViewBag.Grid = grid;
			ViewBag.ColumnSet = columnSet;
			ViewBag.InlineNewRow = gridMetadata.GetInlineNewRow();
			ViewBag.EditGetters = gridMetadata.EditGetters();
			ViewBag.DisplaySetters = gridMetadata.DisplaySetters();
			ViewBag.PostPath = "/ViewBook/SaveChanges";
			ViewBag.DeletePath = "/ViewBook/DeleteBook";
			ViewBag.PostbackParams = gridMetadata.PostbackParams();
			ViewBag.PopulateDropDownLists = gridMetadata.PopulateDropDownLists();

			return View();
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

			// ViewBag.Categories = categories;
			// ViewBag.Authors = authors;

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
			var bookId = Request["ItemId"];
			var db = Database.Open("Books");
			var data = db.Execute(@"delete from Books where BookID=@0", bookId);

			return new EmptyResult();
		}


		[HttpPost]
		public ActionResult SaveChanges()
		{
			var bookId = Request["ItemId"];
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

			return Json(new { ItemId = bookId });
		}
    }
}

