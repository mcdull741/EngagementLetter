using Microsoft.AspNetCore.Mvc;
using EngagementLetter.Models;
using EngagementLetter.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace EngagementLetter.Controllers
{
    public class EngLettersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EngLettersController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
        // 导出报告
        [HttpGet]
        public async Task<IActionResult> ExportReport(string id)
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

            var replaceContents = await _context.ReplaceContents
                .Include(rc => rc.Conditions)
                .Where(rc => rc.QuestionnaireId == engLetter.QuestionnaireId)
                .ToListAsync();

            // 获取关联的模板
            var templates = await _context.Templates
                .Include(t => t.Conditions)
                    .ThenInclude(c => c.Question)
                .Where(t => t.QuestionnaireId == engLetter.QuestionnaireId)
                .OrderByDescending(t => t.Priority)
                .ToListAsync();
        
            // 找到匹配的模板
            Template? matchedTemplate = null;
            foreach (var template in templates)
            {
                bool isMatch = true;
                foreach (var condition in template.Conditions)
                {
                    var userResponse = engLetter.UserResponses
                        .FirstOrDefault(ur => ur.QuestionId == condition.QuestionId);
        
                    if (userResponse == null)
                    {
                        isMatch = false;
                        break;
                    }
        
                    // 根据问题类型处理答案匹配
                    var questionType = condition.Question?.Type;
                    var expectedAnswer = condition.TextResponse;  // 修正属性名称
                    var actualAnswer = userResponse.TextResponse;
        
                    if (questionType == QuestionType.CheckBox)
                    {
                        // 多选题处理
                        try
                        {
                            var actualOptions = System.Text.Json.JsonSerializer.Deserialize<string[]>(actualAnswer ?? "[]") ?? Array.Empty<string>();
                            var expectedOptions = System.Text.Json.JsonSerializer.Deserialize<string[]>(expectedAnswer ?? "[]") ?? Array.Empty<string>();
                            
                            // 检查是否包含所有期望的选项
                            foreach (var expected in expectedOptions)
                            {
                                if (!actualOptions.Contains(expected))
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else
                    {
                        // 单选题和文本题处理
                        // 检查expectedAnswer是否为JArray格式
                        try
                        {
                            // 尝试反序列化expectedAnswer
                            var expectedArray = System.Text.Json.JsonSerializer.Deserialize<string[]>(expectedAnswer ?? "");
                            if (expectedArray != null && expectedArray.Length > 0)
                            {
                                // 如果是JArray，使用第一个元素进行比较
                                var expectedValue = expectedArray[0];
                                if (actualAnswer != expectedValue)
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                            else
                            {
                                // 如果不是JArray，直接比较字符串
                                if (actualAnswer != expectedAnswer)
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            // 反序列化失败，直接比较字符串
                            if (actualAnswer != expectedAnswer)
                            {
                                isMatch = false;
                                break;
                            }
                        }
                    }
        
                    if (!isMatch) break;
                }
        
                if (isMatch)
                {
                    matchedTemplate = template;
                    break;
                }
            }
        
            if (matchedTemplate == null || string.IsNullOrEmpty(matchedTemplate.TemplatePath))
            {
                return Json(new { success = false, message = "未找到匹配的模板或模板文件不存在" });
            }
        
            // 检查模板文件是否存在
            var filePath = Path.Combine(_environment.WebRootPath, "Templates", matchedTemplate.TemplatePath);
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { success = false, message = "模板文件不存在" });
            }
        
            // 返回文件下载
            var fileStream = System.IO.File.OpenRead(filePath);
            var fileName = $"{engLetter.Title}_{matchedTemplate.Name}_{DateTime.Now:yyyyMMddHHmmss}.docx";
            return File(fileStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
        }
    }
}