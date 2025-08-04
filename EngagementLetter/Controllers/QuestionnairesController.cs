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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var questionnaire = await _context.Questionnaires
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (questionnaire == null)
            {
                return NotFound();
            }
            
            // 删除关联的问题
            _context.Questions.RemoveRange(questionnaire.Questions);
            _context.Questionnaires.Remove(questionnaire);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Questionnaires
        public async Task<IActionResult> Index()
        {
            return View(await _context.Questionnaires.ToListAsync());
        }

        // GET: Questionnaires/HasActiveQuestionnaire
        public async Task<IActionResult> HasActiveQuestionnaire(string excludeId = null)
        {
            var query = _context.Questionnaires.Where(q => q.IsActive);
            if (string.IsNullOrEmpty(excludeId))
            {
                query = query.Where(q => q.Id != excludeId);
            }
            // 编辑问卷时，检查除当前问卷外是否有其他启用的问卷
            var activeIds = await query.Select(q => q.Id).ToListAsync();
            var hasActive = activeIds != null && activeIds.Count() > 0;

            return Json(new { 
                hasActive = hasActive,
                activeIds = activeIds
            });
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

                //update active to inactive
                if (questionnaire.IsActive)
                {
                    await _context.Questionnaires
                        .Where(q => q.Id != questionnaire.Id && q.IsActive)
                        .ExecuteUpdateAsync(q => q.SetProperty(q => q.IsActive, false));
                }

                if (!string.IsNullOrEmpty(QuestionsJson))
                {
                    var questions = JsonConvert.DeserializeObject<List<Question>>(QuestionsJson);
                    foreach (var question in questions)
                    {
                        question.QuestionnaireId = questionnaire.Id;
                        question.Id = Guid.NewGuid().ToString();
                        if(question.Type == QuestionType.Text)
                        {
                            question.OptionsJson = "[]";
                        }
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
                    // 获取数据库中的原始问卷，以保留创建时间
                    var originalQuestionnaire = await _context.Questionnaires.FindAsync(id);
                    if (originalQuestionnaire != null)
                    {
                        // 只更新需要修改的字段
                        originalQuestionnaire.Title = questionnaire.Title;
                        originalQuestionnaire.Description = questionnaire.Description;
                        originalQuestionnaire.IsActive = questionnaire.IsActive;
                        originalQuestionnaire.LastModifiedDate = DateTime.Now; // 更新修改时间

                        _context.Update(originalQuestionnaire);
                    }

                    //update active to inactive
                    if (questionnaire.IsActive)
                    {
                        await _context.Questionnaires
                            .Where(q => q.Id != questionnaire.Id && q.IsActive)
                            .ExecuteUpdateAsync(q => q.SetProperty(q => q.IsActive, false));
                    }

                    // 处理问题更新或插入
                    if (!string.IsNullOrEmpty(QuestionsJson))
                    {
                        var questions = JsonConvert.DeserializeObject<List<Question>>(QuestionsJson);
                        var existingQuestions = await _context.Questions
                            .Where(q => q.QuestionnaireId == id)
                            .ToListAsync();

                        foreach (var question in questions)
                        {
                            question.QuestionnaireId = questionnaire.Id;

                            if (string.IsNullOrEmpty(question.Id))
                            {
                                // 新问题，生成ID并添加
                                question.Id = Guid.NewGuid().ToString();
                                _context.Questions.Add(question);
                            }
                            else
                            {
                                // 现有问题，查找并更新
                                var existingQuestion = existingQuestions
                                    .FirstOrDefault(q => q.Id == question.Id);
                                if (existingQuestion != null)
                                {
                                    // 更新问题属性
                                    existingQuestion.Content = question.Content;
                                    existingQuestion.Type = question.Type;
                                    existingQuestion.OptionsJson = question.OptionsJson;
                                    existingQuestion.SortOrder = question.SortOrder;
                                    // 更新其他需要更新的字段
                                    _context.Questions.Update(existingQuestion);
                                }
                                else
                                {
                                    // 如果数据库中不存在该ID的问题，则添加为新问题
                                    question.Id = Guid.NewGuid().ToString();
                                    if(question.Type == QuestionType.Text)
                                    {
                                        question.OptionsJson = "[]";
                                    }
                                    _context.Questions.Add(question);
                                }
                            }
                        }

                        // 删除不在提交列表中的问题
                        foreach (var existingQuestion in existingQuestions)
                        {
                            if (!questions.Any(q => q.Id == existingQuestion.Id))
                            {
                                _context.Questions.Remove(existingQuestion);
                            }
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