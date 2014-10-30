using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RazorTests.Controllers
{
    public class SiteController : Controller
    {
        public ActionResult Manage()
        {
            return View();
        }
    }
}
