using Microsoft.AspNetCore.Mvc;
using EngagementLetter.Data;
using EngagementLetter.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EngagementLetter.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Questions/Create
        [HttpGet]
        public IActionResult Create(string questionnaireId)
        {
            var question = new Question
            {
                QuestionnaireId = questionnaireId
            };
            return PartialView(question);
        }

        // POST: Questions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,QuestionnaireId,Content,Type,SortOrder,OptionsJson")] Question question)
        {
            if (ModelState.IsValid)
            {
                question.Id = Guid.NewGuid().ToString();
                _context.Add(question);
                await _context.SaveChangesAsync();
                return Json(new { success = true, question });
            }
            return PartialView(question);
        }

        // GET: Questions/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id, string questionnaireId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            // 确保问题属于指定的问卷
            if (question.QuestionnaireId != questionnaireId)
            {
                return BadRequest("问题不属于指定的问卷");
            }

            return PartialView(question);
        }

        // POST: Questions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,QuestionnaireId,Content,Type,SortOrder,OptionsJson")] Question question)
        {
            if (id != question.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Json(new { success = true, question });
            }
            return PartialView(question);
        }

        [HttpGet("GetByQuestionnaire/{questionnaireId}")]
        public async Task<IActionResult> GetByQuestionnaire(string questionnaireId)
        {
            if (string.IsNullOrEmpty(questionnaireId))
            {
                return BadRequest("问卷ID不能为空");
            }

            var questions = await _context.Questions
                .Where(q => q.QuestionnaireId == questionnaireId)
                .OrderBy(q => q.SortOrder)
                .Select(q => new
                {
                    id = q.Id,
                    text = q.Content,
                    type = q.Type
                })
                .ToListAsync();

            return Json(questions);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("问题ID不能为空");
            }

            var question = await _context.Questions
                .Where(q => q.Id == id)
                .Select(q => new
                {
                    id = q.Id,
                    text = q.Content,
                    type = q.Type,
                    optionsJson = q.OptionsJson
                })
                .FirstOrDefaultAsync();

            if (question == null)
            {
                return NotFound();
            }

            return Json(question);
        }

        private bool QuestionExists(string id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}