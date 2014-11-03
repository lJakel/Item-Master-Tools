using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tools.Controllers
{
    [RoutePrefix("Reports")]
    [Route("{action}")]
    public class ReportsController : Controller
    {
        [Route]
        public ActionResult Index()
        {
            return View();
        }
    }
}