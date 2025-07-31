using Microsoft.AspNetCore.Mvc;
using EngagementLetter.Data;
using EngagementLetter.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EngagementLetter.Controllers
{
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

        private bool QuestionExists(string id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}