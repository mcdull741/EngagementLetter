using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using EngagementLetter.Data;
using EngagementLetter.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EngagementLetter.Controllers
{
    public class QuestionnairesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionnairesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Questionnaires
        public async Task<IActionResult> Index()
        {
            return View(await _context.Questionnaires.ToListAsync());
        }

        // GET: Questionnaires/Create
        public IActionResult Create()
        {
            return View(new Questionnaire());
        }

        // POST: Questionnaires/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,IsActive")] Questionnaire questionnaire, string QuestionsJson)
        {
            if (ModelState.IsValid)
            {
                questionnaire.Id = Guid.NewGuid().ToString();
                questionnaire.CreatedDate = DateTime.Now;
                _context.Add(questionnaire);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(QuestionsJson))
                {
                    var questions = JsonConvert.DeserializeObject<List<Question>>(QuestionsJson);
                    foreach (var question in questions)
                    {
                        question.QuestionnaireId = questionnaire.Id;
                        question.Id = Guid.NewGuid().ToString();
                        _context.Questions.Add(question);
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(questionnaire);
        }

        // GET: Questionnaires/Preview/5
        public async Task<IActionResult> Preview(string id)
        {
            if (string.IsNullOrEmpty(id) || _context.Questionnaires == null)
            {
                return NotFound();
            }

            var questionnaire = await _context.Questionnaires
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionnaire == null)
            {
                return NotFound();
            }

            return View(questionnaire);
        }

        // GET: Questionnaires/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id) || _context.Questionnaires == null)
            {
                return NotFound();
            }

            var questionnaire = await _context.Questionnaires
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (questionnaire == null)
            {
                return NotFound();
            }
            return View(questionnaire);
        }

        // POST: Questionnaires/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Title,Description,IsActive,CreatedDate")] Questionnaire questionnaire, string QuestionsJson)
        {
            if (id != questionnaire.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(questionnaire);

                    // 删除现有问题
                    var existingQuestions = await _context.Questions.Where(q => q.QuestionnaireId == id).ToListAsync();
                    _context.Questions.RemoveRange(existingQuestions);

                    // 添加更新后的问题
                    if (!string.IsNullOrEmpty(QuestionsJson))
                    {
                        var questions = JsonConvert.DeserializeObject<List<Question>>(QuestionsJson);
                        foreach (var question in questions)
                        {
                            question.QuestionnaireId = questionnaire.Id;
                            question.Id = Guid.NewGuid().ToString();
                            _context.Questions.Add(question);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionnaireExists(questionnaire.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(questionnaire);
        }

        private bool QuestionnaireExists(string id)
        {
          return (_context.Questionnaires?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}