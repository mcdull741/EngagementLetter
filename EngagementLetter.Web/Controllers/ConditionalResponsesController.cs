using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EngagementLetter.Data;
using EngagementLetter.Models;

namespace EngagementLetter.Web.Controllers
{
    public class ConditionalResponsesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConditionalResponsesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ConditionalResponses
        public async Task<IActionResult> Index(string questionnaireId)
        {
            if (string.IsNullOrEmpty(questionnaireId))
            {
                return NotFound();
            }

            var questionnaire = await _context.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == questionnaireId);

            if (questionnaire == null)
            {
                return NotFound();
            }

            ViewBag.Questionnaire = questionnaire;

            var conditionalResponses = await _context.ConditionalResponses
                .Include(cr => cr.Conditions)
                .ThenInclude(c => c.Question)
                .Where(cr => cr.QuestionnaireId == questionnaireId)
                .OrderBy(cr => cr.QuestionId)
                .ToListAsync();

            return View(conditionalResponses);
        }
    }
}