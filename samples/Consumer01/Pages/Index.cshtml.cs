using Event;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Pluto.EventBus.Abstract.Interfaces;

namespace Consumer01.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IEventBus _eventBus;

        public IndexModel(ILogger<IndexModel> logger,IEventBus eventBus)
        {
            _logger = logger;
            _eventBus= eventBus;
        }

        public void OnGet()
        {
            _eventBus?.Subscribe<DemoEvent, DemoEventHandler>();
        }
    }
}