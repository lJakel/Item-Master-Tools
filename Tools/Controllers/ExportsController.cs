using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Tools.Controllers
{
    [RoutePrefix("Exports")]
    [Route("{action}")]
    public class ExportsController : Controller
    {
        [Route]
        public ActionResult Index()
        {
            return View();
        }
    }
}