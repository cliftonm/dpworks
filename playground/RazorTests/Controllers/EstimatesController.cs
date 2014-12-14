using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RazorTests.Controllers
{
    public class EstimatesController : Controller
    {
        public ActionResult NewEstimate()
        {
            return View();
        }

		public ActionResult ViewEstimates()
		{
			return View();
		}
    }
}
