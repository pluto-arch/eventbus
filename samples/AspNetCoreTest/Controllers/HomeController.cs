using AspNetCoreTest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using AspNetCoreTest.Data;
using Event;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using AspNetCoreTest.EventbUSS;
using System.Linq;
using Dncy.EventBus.Abstract.Interfaces;

namespace AspNetCoreTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        //private readonly UserEventBus _userEventBus;

        //private readonly AdminEventBus _adminEventBus;

        private readonly IEventBus _eventBus;

        public HomeController(IEventBus eventBus, ILogger<HomeController> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        ////private readonly DemoDbContext _context;
        //public HomeController(ILogger<HomeController> logger, IEnumerable<IEventBus> eventBus)
        //{
        //    _logger = logger;
        //    _userEventBus = eventBus.FirstOrDefault(x=>x.Name==nameof(UserEventBus)) as UserEventBus;
        //    _adminEventBus = eventBus.FirstOrDefault(x=>x.Name==nameof(AdminEventBus)) as AdminEventBus;
        //}


        //private readonly DemoDbContext _context;
        //public HomeController(ILogger<HomeController> logger, UserEventBus aeventBus,AdminEventBus adminEvent)
        //{
        //    _logger = logger;
        //    _userEventBus = aeventBus;
        //    _adminEventBus = adminEvent;
        //}




        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Sub()
        {
            _eventBus.Subscribe<UserEvent, UserEventHandler>();
            _eventBus.Subscribe<DemoEvent, DemoEventHandler>();
            return View("Index");
        }

        public async Task<IActionResult> Privacy()
        {
            await Task.Delay(500);
            return View("Index");
        }


        public async Task<IActionResult> User()
        {
            await _eventBus.PublishAsync(new UserEvent {Code = "UserEvent"});
            return View("Index");
        }


        public async Task<IActionResult> Admin()
        {
            await _eventBus.PublishAsync(new DemoEvent {Name = "DemoEvent"});
            return View("Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
