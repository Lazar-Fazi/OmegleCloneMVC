using Microsoft.AspNetCore.Mvc;

namespace OmegleCloneMVC.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Video() => View("Index"); // ili druga View stranica ako želiš

        public IActionResult Text()
        {
            return View("TextChat");
        }






    }



}
