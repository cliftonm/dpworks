﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

using WebMatrix.Data;

namespace RazorTests.Controllers
{
    public class ViewBookController : Controller
    {
        //
        // GET: /EditBook/

        public ActionResult Index()
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

			var grid = new WebGrid(books, ajaxUpdateContainerId: "grid", ajaxUpdateCallback: "setArrows");

			ViewBag.Grid = grid;
			ViewBag.Categories = categories;
			ViewBag.Authors = authors;

            return View();
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

			var sql = "UPDATE Books SET Title = @0, AuthorId = @1, CategoryId = @2, ISBN = @3, Hardback = @4 WHERE BookId = @5";
			db.Execute(sql, title, authorId, categoryId, isbn, hardback, bookId);

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

			return new EmptyResult();
		}
    }
}
