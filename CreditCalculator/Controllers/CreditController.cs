using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CreditCalculator.Models;

namespace CreditCalculator.Controllers
{
    public class CreditController : Controller
    {
        // GET: Credit
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(Credit credit)
        {
            if (!ModelState.IsValid)
                return View();

            credit.Configure();
            return RedirectToAction("GetPayments", credit);
        }

        public ActionResult GetPayments(Credit credit)
        {
            return View();
        }
    }
}