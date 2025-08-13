using Microsoft.AspNetCore.Mvc;
using EngagementLetter.Models;
using EngagementLetter.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            // 获取所有Engagement Letter记录，包含关联的Questionnaire
            var engagementLetters = _context.EngLetters
                .Include(e => e.Questionnaire)
                .ToList();
            return View(engagementLetters);
        }

        // POST: EngLetters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] EngLetter model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 创建新的Engagement Letter
            var engLetterId = Guid.NewGuid().ToString();
            var engLetter = new EngLetter
            {
                Id = engLetterId,
                Title = model.Title,
                QuestionnaireId = model.QuestionnaireId,
                CreatedDate = DateTime.Now,
                UserResponses = model.UserResponses.Select(r => new UserResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    QuestionId = r.QuestionId,
                    EngLetterId = engLetterId,
                    TextResponse = r.TextResponse,
                    ResponseDate = DateTime.Now
                }).ToList()
            };

            _context.EngLetters.Add(engLetter);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Engagement Letter created successfully" });
        }

        // GET: EngLetters/Create
        public IActionResult Create()
        {
            return View();
        }
    }
}