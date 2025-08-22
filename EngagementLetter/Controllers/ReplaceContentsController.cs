using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EngagementLetter.Data;
using EngagementLetter.Models;
using EngagementLetter.Models.DTO;
using EngagementLetter.Models.ViewModels;

namespace EngagementLetter.Controllers
{
    public class ReplaceContentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static readonly System.Text.Json.JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ReplaceContentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ReplaceContents
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

            var replaceContents = await _context.ReplaceContents
                .Include(rc => rc.Conditions)
                .ThenInclude(c => c.Question)
                .Where(rc => rc.QuestionnaireId == questionnaireId)
                .OrderBy(rc => rc.Key)
                .ToListAsync();

            return View(replaceContents);
        }

        // GET: ReplaceContents/Create
        public IActionResult Create(string questionnaireId)
        {
            if (string.IsNullOrEmpty(questionnaireId))
            {
                return NotFound();
            }

            ViewBag.QuestionnaireId = questionnaireId;
            
            // 获取问卷信息
            var questionnaire = _context.Questionnaires
                .FirstOrDefault(q => q.Id == questionnaireId);
            
            if (questionnaire == null)
            {
                return NotFound();
            }
            
            ViewBag.QuestionnaireTitle = questionnaire.Title;
            
            // 获取问卷的问题列表，只选择需要的属性，避免循环引用
            var questions = _context.Questions
                .Where(q => q.QuestionnaireId == questionnaireId)
                .OrderBy(q => q.SortOrder)
                .Select(q => new 
                {
                    Id = q.Id,
                    Content = q.Content,
                    Type = q.Type,
                    SortOrder = q.SortOrder
                })
                .ToList();
                
            ViewBag.Questions = questions;

            return View();
        }

        // POST: ReplaceContents/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReplaceContentViewModel viewModel, string conditionsJson)
        {
            if (ModelState.IsValid)
            {
                // 确保Key使用中括号格式
                var key = viewModel.Key;
                if (!string.IsNullOrEmpty(key) && !key.StartsWith("[") && !key.EndsWith("]"))
                {
                    key = $"[{key}]";
                }

                var replaceContent = new ReplaceContent
                {
                    Id = System.Guid.NewGuid().ToString(),
                    QuestionnaireId = viewModel.QuestionnaireId,
                    Key = key,
                    Description = viewModel.Description,
                    Content = viewModel.Content
                };
                
                // 开始事务
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 保存替换内容
                    _context.Add(replaceContent);
                    await _context.SaveChangesAsync();

                    // 处理条件数据
                    if (!string.IsNullOrEmpty(conditionsJson))
                    {
                        try
                        {
                            var conditions = System.Text.Json.JsonSerializer.Deserialize<List<ReplaceContentConditionDto>>(conditionsJson, _jsonOptions);
                            
                            foreach (var condition in conditions)
                            {
                                if (!string.IsNullOrEmpty(condition.QuestionId))
                                {
                                    var replaceContentCondition = new ReplaceContentCondition
                                    {
                                        Id = System.Guid.NewGuid().ToString(),
                                        ReplaceContentId = replaceContent.Id,
                                        QuestionnaireId = replaceContent.QuestionnaireId,
                                        QuestionId = condition.QuestionId,
                                        TextResponse = condition.ExpectedAnswer ?? "[]",
                                        OrderIndex = condition.OrderIndex
                                    };
                                    
                                    _context.ReplaceContentConditions.Add(replaceContentCondition);
                                }
                            }
                            
                            await _context.SaveChangesAsync();
                        }
                        catch (System.Text.Json.JsonException ex)
                        {
                            ModelState.AddModelError("", "条件数据格式错误：" + ex.Message);
                            await transaction.RollbackAsync();
                            return await PrepareCreateView(viewModel);
                        }
                    }
                    
                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Index), new { questionnaireId = replaceContent.QuestionnaireId });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "保存替换内容时发生错误：" + ex.Message);
                    return await PrepareCreateView(viewModel);
                }
            }

            // 如果模型验证失败，重新显示表单
            return await PrepareCreateView(viewModel);
        }

        private async Task<IActionResult> PrepareCreateView(ReplaceContentViewModel viewModel)
        {
            ViewBag.QuestionnaireId = viewModel?.QuestionnaireId;
            
            // 获取问卷信息
            var questionnaire = await _context.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == viewModel.QuestionnaireId);
            
            if (questionnaire != null)
            {
                ViewBag.QuestionnaireTitle = questionnaire.Title;
            }
            
            // 获取问卷的问题列表
            if (!string.IsNullOrEmpty(viewModel?.QuestionnaireId))
            {
                var questions = await _context.Questions
                    .Where(q => q.QuestionnaireId == viewModel.QuestionnaireId)
                    .OrderBy(q => q.SortOrder)
                    .Select(q => new 
                    {
                        Id = q.Id,
                        Content = q.Content,
                        Type = q.Type,
                        SortOrder = q.SortOrder
                    })
                    .ToListAsync();
                    
                ViewBag.Questions = questions;
            }

            // 获取替换内容的所有现有条件
            if (!string.IsNullOrEmpty(viewModel?.Id))
            {
                var existingConditions = await _context.ReplaceContentConditions
                    .Include(c => c.Question)
                    .Where(c => c.ReplaceContentId == viewModel.Id)
                    .OrderBy(c => c.OrderIndex)
                    .Select(c => new 
                    {
                        QuestionId = c.QuestionId,
                        ExpectedAnswer = c.TextResponse,
                        OrderIndex = c.OrderIndex,
                        LogicOperator = c.LogicOperator ?? "AND",
                        QuestionContent = c.Question.Content
                    })
                    .ToListAsync();

                ViewBag.ExistingConditionsJson = System.Text.Json.JsonSerializer.Serialize(existingConditions);
            }
            
            return View(viewModel);
        }

        // GET: ReplaceContents/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var replaceContent = await _context.ReplaceContents
                .Include(rc => rc.Questionnaire)
                .Include(rc => rc.Conditions)
                .ThenInclude(c => c.Question)
                .FirstOrDefaultAsync(rc => rc.Id == id);

            if (replaceContent == null)
            {
                return NotFound();
            }
            ViewBag.QuestionnaireTitle = replaceContent.Questionnaire.Title;
            ViewBag.QuestionnaireId = replaceContent.Questionnaire.Id;

            // 获取问卷的问题列表，只选择需要的属性，避免循环引用
            var questions = _context.Questions
                .Where(q => q.QuestionnaireId == replaceContent.QuestionnaireId)
                .OrderBy(q => q.SortOrder)
                .Select(q => new 
                {
                    Id = q.Id,
                    Content = q.Content,
                    Type = q.Type,
                    SortOrder = q.SortOrder
                })
                .ToList();
                
            ViewBag.Questions = questions;
            
            // 序列化现有条件数据
            object existingConditions;
            if(null != replaceContent){
                existingConditions = replaceContent.Conditions.Select(c => new 
                {
                    questionId = c.QuestionId,
                    expectedAnswer = c.TextResponse,
                    orderIndex = c.OrderIndex,
                    logicOperator = c.LogicOperator ?? "AND",
                    questionContent = c.Question?.Content ?? "",
                    conditionType = c.ConditionType ?? "Euqal"
                }).ToList();
            }
            else
            {
                existingConditions = new List<object>();
            }
            
            ViewBag.ExistingConditionsJson = System.Text.Json.JsonSerializer.Serialize(existingConditions);

            var viewModel = new ReplaceContentViewModel
            {
                Id = replaceContent.Id,
                QuestionnaireId = replaceContent.QuestionnaireId,
                Key = replaceContent.Key,
                Description = replaceContent.Description,
                Content = replaceContent.Content
            };

            return View(viewModel);
        }

        // POST: ReplaceContents/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ReplaceContentViewModel viewModel, string conditionsJson)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // 验证并修正Key格式，确保为中括号包裹的格式
                var key = viewModel.Key?.Trim() ?? "";
                if (!string.IsNullOrEmpty(key) && !key.StartsWith("[") && !key.EndsWith("]"))
                {
                    key = $"[{key}]";
                }

                var replaceContent = new ReplaceContent
                {
                    Id = viewModel.Id,
                    QuestionnaireId = viewModel.QuestionnaireId,
                    Key = key,
                    Description = viewModel.Description,
                    Content = viewModel.Content
                };

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 更新替换内容
                    _context.Update(replaceContent);
                    await _context.SaveChangesAsync();

                    // 删除旧的条件
                    var oldConditions = _context.ReplaceContentConditions
                        .Where(c => c.ReplaceContentId == id);
                    _context.ReplaceContentConditions.RemoveRange(oldConditions);
                    await _context.SaveChangesAsync();

                    // 添加新的条件
                    if (!string.IsNullOrEmpty(conditionsJson))
                    {
                        try
                        {
                            var conditions = System.Text.Json.JsonSerializer.Deserialize<List<ReplaceContentConditionDto>>(conditionsJson, _jsonOptions);
                            
                            foreach (var condition in conditions)
                            {
                                if (!string.IsNullOrEmpty(condition.QuestionId))
                                {
                                    var replaceContentCondition = new ReplaceContentCondition
                                    {
                                        Id = System.Guid.NewGuid().ToString(),
                                        ReplaceContentId = replaceContent.Id,
                                        QuestionnaireId = replaceContent.QuestionnaireId,
                                        QuestionId = condition.QuestionId,
                                        TextResponse = condition.ExpectedAnswer ?? "[]",
                                        OrderIndex = condition.OrderIndex
                                    };
                                    
                                    _context.ReplaceContentConditions.Add(replaceContentCondition);
                                }
                            }
                            
                            await _context.SaveChangesAsync();
                        }
                        catch (System.Text.Json.JsonException ex)
                        {
                            ModelState.AddModelError("", "条件数据格式错误：" + ex.Message);
                            await transaction.RollbackAsync();
                            return await PrepareEditView(viewModel);
                        }
                    }
                    
                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Index), new { questionnaireId = replaceContent.QuestionnaireId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    if (!ReplaceContentExists(replaceContent.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "更新替换内容时发生错误：" + ex.Message);
                    return await PrepareEditView(viewModel);
                }
            }

            // 如果模型验证失败，重新显示表单
            return await PrepareEditView(viewModel);
        }

        private async Task<IActionResult> PrepareEditView(ReplaceContentViewModel viewModel)
        {
            ViewBag.QuestionnaireId = viewModel?.QuestionnaireId;
            
            // 获取问卷信息
            var questionnaire = await _context.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == viewModel.QuestionnaireId);
            
            if (questionnaire != null)
            {
                ViewBag.QuestionnaireTitle = questionnaire.Title;
            }
            
            // 获取问卷的问题列表
            if (!string.IsNullOrEmpty(viewModel?.QuestionnaireId))
            {
                var questions = await _context.Questions
                    .Where(q => q.QuestionnaireId == viewModel.QuestionnaireId)
                    .OrderBy(q => q.SortOrder)
                    .Select(q => new 
                    {
                        Id = q.Id,
                        Content = q.Content,
                        Type = q.Type,
                        SortOrder = q.SortOrder
                    })
                    .ToListAsync();
                    
                ViewBag.Questions = questions;
            }
            
            return View(viewModel);
        }

        // GET: ReplaceContents/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var replaceContent = await _context.ReplaceContents
                .Include(rc => rc.Questionnaire)
                .FirstOrDefaultAsync(rc => rc.Id == id);

            if (replaceContent == null)
            {
                return NotFound();
            }

            return View(replaceContent);
        }

        // POST: ReplaceContents/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var replaceContent = await _context.ReplaceContents
                .Include(rc => rc.Conditions)
                .FirstOrDefaultAsync(rc => rc.Id == id);

            if (replaceContent != null)
            {
                // 删除相关的条件
                _context.ReplaceContentConditions.RemoveRange(replaceContent.Conditions);
                _context.ReplaceContents.Remove(replaceContent);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { questionnaireId = replaceContent?.QuestionnaireId });
        }

        private bool ReplaceContentExists(string id)
        {
            return _context.ReplaceContents.Any(e => e.Id == id);
        }

        // GET: ReplaceContents/_ConditionsFrame
        [HttpGet]
        public IActionResult _ConditionsFrame()
        {
            return View();
        }
    }
}