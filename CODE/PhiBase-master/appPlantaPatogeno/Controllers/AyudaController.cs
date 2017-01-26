using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace appPlantaPatogeno.Controllers
{
    public class AyudaController : Controller
    {
        // GET: Ayuda
        public ActionResult Ayuda()
        {
            string url = Request.QueryString["urlHome"];
            ViewData["url"] = url;
            return View();
        }
    }
}