using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EngagementLetter.Data;
using EngagementLetter.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace EngagementLetter.Controllers
{
    public class TemplatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public TemplatesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Templates
        public async Task<IActionResult> Index(string questionnaireId, string search, string sortBy = "createdDate", bool showConditionalOnly = false)
        {
            var query = _context.Templates
                .Include(t => t.Questionnaire)
                .Include(t => t.Conditions)
                .AsQueryable();

            // 问卷筛选
            if (!string.IsNullOrEmpty(questionnaireId))
            {
                query = query.Where(t => t.QuestionnaireId == questionnaireId);
            }

            // 搜索模板
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Name.Contains(search) || t.Description.Contains(search));
            }

            // 显示有条件模板筛选
            if (showConditionalOnly)
            {
                query = query.Where(t => t.Conditions.Any());
            }

            // 排序
            query = sortBy switch
            {
                "name" => query.OrderBy(t => t.Name),
                "priority" => query.OrderByDescending(t => t.Priority),
                "createdDate" => query.OrderByDescending(t => t.CreatedDate),
                _ => query.OrderByDescending(t => t.CreatedDate)
            };

            var model = new TemplateIndexViewModel
            {
                Templates = await query.ToListAsync(),
                Questionnaires = await _context.Questionnaires.ToListAsync(),
                SelectedQuestionnaireId = questionnaireId,
                SearchQuery = search,
                SortBy = sortBy,
                ShowConditionalOnly = showConditionalOnly
            };

            return View(model);
        }

        // GET: Templates/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var template = await _context.Templates
                .Include(t => t.Questionnaire)
                .Include(t => t.Conditions)
                .ThenInclude(c => c.Question)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            return View(template);
        }

        // 在现有的Create GET方法中添加questionnaireId参数支持
        [HttpGet]
        public async Task<IActionResult> Create(string? questionnaireId)
        {
            var viewModel = new TemplateCreateViewModel
            {
                Priority = 50,
                Questionnaires = await _context.Questionnaires
                    .Select(q => new SelectListItem { Value = q.Id.ToString(), Text = q.Title })
                    .ToListAsync(),
                Questions = new List<dynamic>() // 初始为空，将通过AJAX加载
            };
        
            // 如果传入了questionnaireId，预选择该问卷
            if (!string.IsNullOrWhiteSpace(questionnaireId))
            {
                viewModel.QuestionnaireId = questionnaireId;
            }
        
            return View(viewModel);
        }
        
        // 在现有的Create POST方法中确保条件处理逻辑正确
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] TemplateCreateViewModel model, [FromForm] string conditionsJson = null)
        {
            // 如果传入了JSON格式的条件，反序列化
            if (!string.IsNullOrEmpty(conditionsJson))
            {
                try
                {
                    model.Conditions = JsonSerializer.Deserialize<List<TemplateConditionCreateViewModel>>(conditionsJson, _jsonOptions);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("Conditions", "条件格式不正确");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 处理文件上传
                    string fileName = null;
                    if (model.TemplateFile != null && model.TemplateFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "Templates");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
        
                        fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.TemplateFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.TemplateFile.CopyToAsync(stream);
                        }
                    }
        
                    // 创建模板实体
                    var template = new Template
                    {
                        Name = model.Name,
                        Description = model.Description,
                        QuestionnaireId = model.QuestionnaireId,
                        Priority = model.Priority,
                        TemplatePath = fileName,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System",
                        UpdatedBy = User.Identity?.Name ?? "System"
                    };
        
                    _context.Templates.Add(template);
                    await _context.SaveChangesAsync(); // 先保存模板以获取ID
        
                    // 保存条件（如果有）
                    if (model.Conditions != null && model.Conditions.Any())
                    {
                        for (int i = 0; i < model.Conditions.Count; i++)
                        {
                            var conditionModel = model.Conditions[i];
                            var condition = new TemplateCondition
                            {
                                TemplateId = template.Id,
                                QuestionId = conditionModel.QuestionId,
                                QuestionnaireId = model.QuestionnaireId,
                                ConditionType = conditionModel.ConditionType,
                                TextResponse = conditionModel.ExpectedAnswer,
                                OrderIndex = conditionModel.OrderIndex ?? i
                            };
                            _context.TemplateConditions.Add(condition);
                        }
                        await _context.SaveChangesAsync();
                    }
        
                    TempData["Success"] = "模板创建成功！";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"创建模板时出错：{ex.Message}");
                }
            }
        
            // 如果模型验证失败，重新加载下拉列表
            model.Questionnaires = await _context.Questionnaires
                .Select(q => new SelectListItem { Value = q.Id.ToString(), Text = q.Title })
                .ToListAsync();
        
            return View(model);
        }
        
        // 添加获取问卷问题的方法（用于AJAX调用）
        [HttpGet]
        public async Task<IActionResult> GetQuestionsByQuestionnaire(string questionnaireId)
        {
            var questions = await _context.Questions
                .Where(q => q.QuestionnaireId == questionnaireId)
                .Select(q => new { id = q.Id, text = q.Content })
                .ToListAsync();

            return Json(questions);
        }

        // 下载模板文件
        [HttpGet]
        public async Task<IActionResult> Download(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var template = await _context.Templates
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null || string.IsNullOrEmpty(template.TemplatePath))
            {
                return NotFound();
            }

            var filePath = Path.Combine(_environment.WebRootPath, "Templates", template.TemplatePath);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileStream = System.IO.File.OpenRead(filePath);
            return File(fileStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", template.Name + ".docx");
        }

        // GET: Templates/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var template = await _context.Templates
                .Include(t => t.Conditions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            var viewModel = new TemplateEditViewModel
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                QuestionnaireId = template.QuestionnaireId,
                Priority = template.Priority,
                FileName = template.TemplatePath,
                Conditions = template.Conditions?.Select(c => new TemplateConditionCreateViewModel
                {
                    QuestionId = c.QuestionId,
                    ConditionType = c.ConditionType,
                    ExpectedAnswer = c.TextResponse,
                    OrderIndex = c.OrderIndex
                }).ToList() ?? new List<TemplateConditionCreateViewModel>(),
                Questionnaires = await _context.Questionnaires
                    .Select(q => new SelectListItem { Value = q.Id.ToString(), Text = q.Title })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // POST: Templates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TemplateEditViewModel model, [FromForm] string ConditionsData = null)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            // 清除TemplateFile的验证错误，因为它是可选的
            if (ModelState.ContainsKey("TemplateFile") || ModelState.ContainsKey("FileName"))
            {
                ModelState.Remove("TemplateFile");
                ModelState.Remove("FileName");
            }

            // 解析条件数据
            if (!string.IsNullOrEmpty(ConditionsData))
            {
                try
                {
                    var conditions = JsonSerializer.Deserialize<List<TemplateConditionCreateViewModel>>(ConditionsData, _jsonOptions);
                    model.Conditions = conditions ?? new List<TemplateConditionCreateViewModel>();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ConditionsData", $"条件数据格式错误：{ex.Message}");
                }
            }
            else
            {
                model.Conditions = new List<TemplateConditionCreateViewModel>();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var template = await _context.Templates
                        .Include(t => t.Conditions)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (template == null)
                    {
                        return NotFound();
                    }

                    // 更新模板基本信息
                    template.Name = model.Name;
                    template.Description = model.Description;
                    template.QuestionnaireId = model.QuestionnaireId;
                    template.Priority = model.Priority;
                    template.UpdatedDate = DateTime.Now;
                    template.UpdatedBy = User.Identity?.Name ?? "System";

                    // 处理文件上传（如果有新文件上传）
                    if (model.TemplateFile != null && model.TemplateFile.Length > 0)
                    {
                        // 删除旧文件
                        if (!string.IsNullOrEmpty(template.TemplatePath))
                        {
                            var oldFilePath = Path.Combine(_environment.WebRootPath, "Templates", template.TemplatePath);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // 保存新文件
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "Templates");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.TemplateFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.TemplateFile.CopyToAsync(stream);
                        }

                        template.TemplatePath = fileName;
                    }

                    // 更新条件：先删除旧条件，再添加新条件
                    _context.TemplateConditions.RemoveRange(template.Conditions);

                    if (model.Conditions != null && model.Conditions.Any())
                    {
                        for (int i = 0; i < model.Conditions.Count; i++)
                        {
                            var conditionModel = model.Conditions[i];
                            var condition = new TemplateCondition
                            {
                                TemplateId = template.Id,
                                QuestionId = conditionModel.QuestionId,
                                QuestionnaireId = model.QuestionnaireId,
                                ConditionType = conditionModel.ConditionType,
                                TextResponse = conditionModel.ExpectedAnswer,
                                OrderIndex = conditionModel.OrderIndex ?? i
                            };
                            _context.TemplateConditions.Add(condition);
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "模板更新成功！";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"更新模板时出错：{ex.Message}");
                }
            }

            // 如果模型验证失败，重新加载下拉列表
            model.Questionnaires = await _context.Questionnaires
                .Select(q => new SelectListItem { Value = q.Id.ToString(), Text = q.Title })
                .ToListAsync();

            return View(model);
        }

        // POST: Templates/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var template = await _context.Templates
                .Include(t => t.Conditions)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (template == null)
            {
                return NotFound();
            }

            try
            {
                // 删除关联的条件
                _context.TemplateConditions.RemoveRange(template.Conditions);
                
                // 删除模板文件
                if (!string.IsNullOrEmpty(template.TemplatePath))
                {
                    var filePath = Path.Combine(_environment.WebRootPath, "Templates", template.TemplatePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // 删除模板记录
                _context.Templates.Remove(template);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                // 记录错误日志
                Console.WriteLine($"Error deleting template: {ex.Message}");
                return StatusCode(500, "删除模板时发生错误");
            }
        }

        // GET: Templates/_ConditionsFrame
        [HttpGet]
        public IActionResult _ConditionsFrame()
        {
            return View();
        }
    }

    public class TemplateIndexViewModel
    {
        public List<Template> Templates { get; set; }
        public List<Questionnaire> Questionnaires { get; set; }
        public string SelectedQuestionnaireId { get; set; }
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }
        public bool ShowConditionalOnly { get; set; }
    }

    public class TemplateCreateViewModel
    {
        [Required(ErrorMessage = "模板名称不能为空")]
        [StringLength(100, ErrorMessage = "模板名称不能超过100个字符")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "描述不能超过500个字符")]
        public string Description { get; set; }

        [Required(ErrorMessage = "请选择关联问卷")]
        public string QuestionnaireId { get; set; }

        [Range(1, 100, ErrorMessage = "优先级必须在1-100之间")]
        public int Priority { get; set; } = 50;

        [Required(ErrorMessage = "请上传模板文件")]
        [DataType(DataType.Upload)]
        public IFormFile? TemplateFile { get; set; }

        public List<TemplateConditionCreateViewModel> Conditions { get; set; } = new List<TemplateConditionCreateViewModel>();

        public List<SelectListItem> Questionnaires { get; set; } = new List<SelectListItem>();
        public List<dynamic> Questions { get; set; } = new List<dynamic>();
    }

    public class TemplateConditionCreateViewModel
    {
        [Required(ErrorMessage = "请选择问题")]
        public string QuestionId { get; set; }

        [Required(ErrorMessage = "请选择条件类型")]
        public string ConditionType { get; set; } = "Equals";

        [Required(ErrorMessage = "请填写预期回答")]
        public string ExpectedAnswer { get; set; }

        public int? OrderIndex { get; set; }
    }

    public class TemplateEditViewModel
    {
        [Required(ErrorMessage = "模板ID不能为空")]
        public string Id { get; set; }

        [Required(ErrorMessage = "模板名称不能为空")]
        [StringLength(100, ErrorMessage = "模板名称不能超过100个字符")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "描述不能超过500个字符")]
        public string Description { get; set; }

        [Required(ErrorMessage = "请选择关联问卷")]
        public string QuestionnaireId { get; set; }

        [Range(1, 100, ErrorMessage = "优先级必须在1-100之间")]
        public int Priority { get; set; } = 50;

        [DataType(DataType.Upload)]
        public IFormFile? TemplateFile { get; set; }

        public string FileName { get; set; }

        public List<TemplateConditionCreateViewModel> Conditions { get; set; } = new List<TemplateConditionCreateViewModel>();

        public List<SelectListItem> Questionnaires { get; set; } = new List<SelectListItem>();
    }
}