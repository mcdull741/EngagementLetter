using Microsoft.AspNetCore.Mvc;
using EngagementLetter.Data;
using EngagementLetter.Models;
using System.Linq;

namespace EngagementLetter.Controllers
{
    public class EngLettersController : Controller

    {
        private readonly ApplicationDbContext _context;

        public EngLettersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EngLetters
        public IActionResult Index()
        {
            // 获取所有Engagement Letter记录
            var engagementLetters = _context.EngLetters.ToList();
            return View(engagementLetters);
        }
    }
}