using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ComputerServicing.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index
        {
            get
            {
                return View();
            }
        }

        public IActionResult About
        {
            get
            {
                ViewData["Message"] = "Your application description page.";

                return View();
            }
        }

        public IActionResult Contact
        {
            get
            {
                ViewData["Message"] = "Your contact page.";

                return View();
            }
        }

        public IActionResult Error
        {
            get
            {
                return View();
            }
        }

        public IActionResult ShoppingCart
        {
            get
            {
                ViewData["Message"] = "Shopping Cart";

                return View();
            }
        }

        public IActionResult Support
        {
            get
            {
                ViewData["Message"] = "Support";

                return View();
            }
        }

        public IActionResult Login
        {
            get
            {
                ViewData["Message"] = "Login";

                return View();
            }
        }

        public IActionResult CostofServices
        {
            get
            {
                ViewData["Message"] = "Message";

                return View();
            }
        }
    }
}
