using Microsoft.AspNetCore.Mvc;
using System.Data;
using EHS_2023.Models;

namespace EHS_2023.Controllers
{
    public class CertificateController : Controller
    {
        public IActionResult CertificateIndex()
        {
            var x = new DataModel()
            {
                //
                // Store session variables in the model
                //
                CredentialsOutput = HttpContext.Session.GetString("SessionVariable"),
                FullName = HttpContext.Session.GetString("FullName")
            };
            //
            // Pass model into the CertificateIndex view
            //
            return View(x);
        }
    }
}