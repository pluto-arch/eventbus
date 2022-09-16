using AspNetCoreTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using AspNetCoreTest.Data;
using Event;
using Microsoft.EntityFrameworkCore;
using Pluto.EventBus.Abstract.Interfaces;
using System.Collections.Generic;
using AspNetCoreTest.EventbUSS;
using System.Linq;

namespace AspNetCoreTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly UserEventBus _userEventBus;

        private readonly AdminEventBus _adminEventBus;

        ////private readonly DemoDbContext _context;
        //public HomeController(ILogger<HomeController> logger, IEnumerable<IEventBus> eventBus)
        //{
        //    _logger = logger;
        //    _userEventBus = eventBus.FirstOrDefault(x=>x.Name==nameof(UserEventBus)) as UserEventBus;
        //    _adminEventBus = eventBus.FirstOrDefault(x=>x.Name==nameof(AdminEventBus)) as AdminEventBus;
        //}


        //private readonly DemoDbContext _context;
        public HomeController(ILogger<HomeController> logger, UserEventBus aeventBus,AdminEventBus adminEvent)
        {
            _logger = logger;
            _userEventBus = aeventBus;
            _adminEventBus = adminEvent;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Sub()
        {
            _userEventBus.Subscribe<UserEvent, UserEventHandler>();
            _adminEventBus.Subscribe<DemoEvent, DemoEventHandler>();
            return View("Index");
        }

        public async Task<IActionResult> Privacy()
        {
            await Task.Delay(500);
            return View("Index");
        }


        public async Task<IActionResult> User()
        {
            await _userEventBus.PublishAsync(new UserEvent {Code = "UserEvent"});
            return View("Index");
        }


        public async Task<IActionResult> Admin()
        {
            await _adminEventBus.PublishAsync(new DemoEvent {Name = "DemoEvent"});
            return View("Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
