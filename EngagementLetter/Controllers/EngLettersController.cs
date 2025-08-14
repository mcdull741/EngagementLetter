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
        public async Task<IActionResult> Create([FromForm] string title, [FromForm] string questionnaireId, IFormCollection formData)
        {
            if (string.IsNullOrEmpty(title))
            {
                return Json(new { success = false, message = "标题不能为空" });
            }

            if (string.IsNullOrEmpty(questionnaireId))
            {
                return Json(new { success = false, message = "请选择关联问卷" });
            }

            try
            {
                // 创建新的Engagement Letter
                var engLetterId = Guid.NewGuid().ToString();
                var engLetter = new EngLetter
                {
                    Id = engLetterId,
                    Title = title,
                    QuestionnaireId = questionnaireId,
                    CreatedDate = DateTime.Now
                };

                _context.EngLetters.Add(engLetter);

                // 获取问卷的所有问题
                var questions = await _context.Questions
                    .Where(q => q.QuestionnaireId == questionnaireId)
                    .ToListAsync();

                // 创建用户响应
                var userResponses = new List<UserResponse>();
                foreach (var question in questions)
                {
                    var key = $"question_{question.Id}";
                    if (formData.ContainsKey(key))
                    {
                        var values = formData[key];
                        string textResponse;
                        
                        if (values.Count > 1)
                        {
                            // 多选答案（CheckBox）
                            textResponse = System.Text.Json.JsonSerializer.Serialize(values.ToArray());
                        }
                        else
                        {
                            // 单选或文本答案
                            textResponse = values.FirstOrDefault();
                        }

                        if (!string.IsNullOrEmpty(textResponse))
                        {
                            userResponses.Add(new UserResponse
                            {
                                Id = Guid.NewGuid().ToString(),
                                QuestionId = question.Id,
                                EngLetterId = engLetterId,
                                TextResponse = textResponse,
                                ResponseDate = DateTime.Now
                            });
                        }
                    }
                }

                _context.UserResponses.AddRange(userResponses);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Engagement Letter 创建成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "创建失败：" + ex.Message });
            }
        }

        // GET: EngLetters/Create
        public async Task<IActionResult> Create()
        {
            // 加载已发布的问卷
            var publishedQuestionnaire = await _context.Questionnaires
                .Where(q => q.Status == QuestionnaireStatus.Published)
                .OrderBy(q => q.Title)
                .FirstOrDefaultAsync();
            
            ViewBag.Questionnaire = publishedQuestionnaire;
            return View();
        }

        // GET: EngLetters/Preview/5
        public async Task<IActionResult> Preview(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var engLetter = await _context.EngLetters
                .Include(e => e.Questionnaire)
                .Include(e => e.UserResponses)
                    .ThenInclude(ur => ur.Question)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (engLetter == null)
            {
                return NotFound();
            }

            return View(engLetter);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var engLetter = await _context.EngLetters
                .Include(e => e.Questionnaire)
                    .ThenInclude(q => q.Questions)
                .Include(e => e.UserResponses)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (engLetter == null)
            {
                return NotFound();
            }
            ViewBag.IsEditMode = true;
            return View(engLetter);
        }

        [HttpGet]
        public async Task<IActionResult> RenderContent(string id, string mode = "preview")
        {
            var engLetter = await _context.EngLetters
                .Include(e => e.Questionnaire)
                    .ThenInclude(q => q.Questions)
                .Include(e => e.UserResponses)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (engLetter == null)
            {
                return NotFound();
            }

            ViewBag.IsEditMode = mode == "edit";
            return View(engLetter);
        }

        [HttpGet]
        public async Task<IActionResult> RenderCreateContent(string questionnaireId)
        {
            if (string.IsNullOrEmpty(questionnaireId))
            {
                return NotFound();
            }

            var questionnaire = await _context.Questionnaires
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == questionnaireId);

            if (questionnaire == null)
            {
                return NotFound();
            }

            // 创建一个临时的EngLetter对象用于渲染
            var tempEngLetter = new EngLetter
            {
                Id = "temp",
                Title = "新建Engagement Letter",
                Questionnaire = questionnaire,
                UserResponses = new List<UserResponse>()
            };

            ViewBag.IsEditMode = true;
            return View("RenderContent", tempEngLetter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [FromForm] string title, IFormCollection formData)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "无效的ID" });
            }

            var engLetter = await _context.EngLetters
                .Include(e => e.UserResponses)
                    .ThenInclude(ur => ur.Question)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (engLetter == null)
            {
                return Json(new { success = false, message = "未找到该Engagement Letter" });
            }

            try
            {
                // 更新标题
                if (!string.IsNullOrEmpty(title))
                {
                    engLetter.Title = title;
                    engLetter.UpdatedDate = DateTime.Now;
                }

                // 更新用户响应
                foreach (var response in engLetter.UserResponses)
                {
                    var key = $"question_{response.QuestionId}";
                    if (formData.ContainsKey(key))
                    {
                        var values = formData[key];
                        if (values.Count > 1)
                        {
                            // 多选答案（CheckBox）
                            response.TextResponse = System.Text.Json.JsonSerializer.Serialize(values.ToArray());
                        }
                        else
                        {
                            // 单选或文本答案
                            response.TextResponse = values.FirstOrDefault();
                        }
                        response.ResponseDate = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "保存成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "保存失败：" + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "无效的ID" });
            }

            var engLetter = await _context.EngLetters
                .Include(e => e.UserResponses)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (engLetter == null)
            {
                return Json(new { success = false, message = "未找到该Engagement Letter" });
            }

            try
            {
                // 删除关联的用户响应
                _context.UserResponses.RemoveRange(engLetter.UserResponses);
                
                // 删除Engagement Letter
                _context.EngLetters.Remove(engLetter);
                
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "删除成功" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "删除失败：" + ex.Message });
            }
        }
    }
}