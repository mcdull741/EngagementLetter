using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EngagementLetter.Data;
using EngagementLetter.Models;
using EngagementLetter.Models.ViewModels;
using System.Text.Json;

namespace EngagementLetter.Web.Controllers
{
    public class ConditionalResponsesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

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
                .Include(cr => cr.Question)
                .Include(cr => cr.Conditions)
                    .ThenInclude(c => c.Question)
                .Where(cr => cr.QuestionnaireId == questionnaireId)
                .OrderBy(cr => cr.Question.SortOrder)
                .ToListAsync();

            return View(conditionalResponses);
        }

        // GET: ConditionalResponses/Create
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
            
            // 获取问卷的问题列表
            var questions = _context.Questions
                .Where(q => q.QuestionnaireId == questionnaireId)
                .OrderBy(q => q.SortOrder)
                .Select(q => new 
                {
                    Id = q.Id,
                    Content = q.Content,
                    Type = q.Type,
                    OptionsJson = q.OptionsJson,
                    SortOrder = q.SortOrder
                })
                .ToList();
                
            ViewBag.Questions = questions;

            return View();
        }

        // POST: ConditionalResponses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConditionalResponseViewModel viewModel, string conditionsJson)
        {
            if (ModelState.IsValid)
            {
                // 优先从表单中获取ResponseHidden字段的值（用于处理动态生成的表单控件）
                string responseValue;
                if(Request.Form.ContainsKey("ResponseHidden"))
                    responseValue = Request.Form["ResponseHidden"].ToString();
                else
                    responseValue = viewModel.Response;
                // 创建条件响应实体
                var conditionalResponse = new ConditionalResponse
                {
                    Id = System.Guid.NewGuid().ToString(),
                    QuestionnaireId = viewModel.QuestionnaireId,
                    QuestionId = viewModel.QuestionId,
                    Response = responseValue
                };
                
                // 开始事务
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 保存条件响应
                    _context.Add(conditionalResponse);
                    await _context.SaveChangesAsync();

                    // 处理条件数据
                    if (!string.IsNullOrEmpty(conditionsJson))
                    {
                        try
                        {
                            var conditions = JsonSerializer.Deserialize<List<CommonCondition>>(conditionsJson, _jsonOptions);
                            
                            if (conditions != null && conditions.Count > 0)
                            {
                                int orderIndex = 0;
                                
                                foreach (var conditionData in conditions)
                                {
                                    var condition = new ConditionalResponseCondition
                                    {
                                        Id = System.Guid.NewGuid().ToString(),
                                        ConditionalResponseId = conditionalResponse.Id,
                                        QuestionnaireId = conditionalResponse.QuestionnaireId,
                                        QuestionId = conditionData.QuestionId?.ToString() ?? string.Empty,
                                        TextResponse = conditionData.TextResponse?.ToString() ?? "[]",
                                        ConditionType = conditionData.ConditionType?.ToString() ?? "Equals",
                                        LogicOperator = conditionData.LogicOperator?.ToString() ?? "AND",
                                        OrderIndex = orderIndex++
                                    };
                                    
                                    _context.Add(condition);
                                }
                                
                                await _context.SaveChangesAsync();
                            }
                        }
                        catch (JsonException ex)
                        {
                            ModelState.AddModelError("", "条件数据格式错误：" + ex.Message);
                            await transaction.RollbackAsync();
                            return await PrepareCreateView(viewModel);
                        }
                    }
                    
                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Index), new { questionnaireId = conditionalResponse.QuestionnaireId });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "保存条件响应时发生错误：" + ex.Message);
                    return await PrepareCreateView(viewModel);
                }
            }

            // 如果模型验证失败，重新显示表单
            return await PrepareCreateView(viewModel);
        }

        private async Task<IActionResult> PrepareCreateView(ConditionalResponseViewModel viewModel)
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

        // GET: ConditionalResponses/_ConditionsFrame
        [HttpGet]
        public IActionResult _ConditionsFrame()
        {
            return View();
        }

        // GET: ConditionalResponses/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // 获取条件响应
            var conditionalResponse = await _context.ConditionalResponses
                .Include(cr => cr.Conditions)
                .FirstOrDefaultAsync(cr => cr.Id == id);

            if (conditionalResponse == null)
            {
                return NotFound();
            }

            ViewBag.QuestionnaireId = conditionalResponse.QuestionnaireId;

            // 获取问卷信息
            var questionnaire = await _context.Questionnaires
                .FirstOrDefaultAsync(q => q.Id == conditionalResponse.QuestionnaireId);

            if (questionnaire == null)
            {
                return NotFound();
            }

            ViewBag.QuestionnaireTitle = questionnaire.Title;

            // 获取问卷的问题列表
            var questions = await _context.Questions
                .Where(q => q.QuestionnaireId == conditionalResponse.QuestionnaireId)
                .OrderBy(q => q.SortOrder)
                .Select(q => new 
                {
                    Id = q.Id,
                    Content = q.Content,
                    Type = q.Type,
                    OptionsJson = q.OptionsJson,
                    SortOrder = q.SortOrder
                })
                .ToListAsync();

            ViewBag.Questions = questions;

            // 准备条件数据用于JavaScript
            var conditionsData = conditionalResponse.Conditions
                .OrderBy(c => c.OrderIndex)
                .Select(c => new
                {
                    Id = c.Id,
                    QuestionId = c.QuestionId,
                    TextResponse = c.TextResponse,
                    ConditionType = c.ConditionType,
                    LogicOperator = c.LogicOperator,
                    OrderIndex = c.OrderIndex
                })
                .ToList();

            ViewBag.ConditionsData = conditionsData;

            // 准备视图模型
            var viewModel = new ConditionalResponseViewModel
            {
                Id = conditionalResponse.Id,
                QuestionnaireId = conditionalResponse.QuestionnaireId,
                QuestionId = conditionalResponse.QuestionId,
                Response = conditionalResponse.Response
            };

            // 将ConditionsData转换为JSON字符串并设置到隐藏字段
            ViewBag.ConditionsJson = JsonSerializer.Serialize(conditionsData);

            return View(viewModel);
        }

        // POST: ConditionalResponses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ConditionalResponseViewModel viewModel, string conditionsJson)
        {
            if (ModelState.IsValid)
            {
                // 获取现有的条件响应
                var conditionalResponse = await _context.ConditionalResponses
                    .Include(cr => cr.Conditions)
                    .FirstOrDefaultAsync(cr => cr.Id == viewModel.Id);

                if (conditionalResponse == null)
                {
                    return NotFound();
                }

                // 优先从表单中获取ResponseHidden字段的值
                string responseValue;
                if(Request.Form.ContainsKey("ResponseHidden"))
                    responseValue = Request.Form["ResponseHidden"].ToString();
                else
                    responseValue = viewModel.Response;

                // 更新条件响应实体
                conditionalResponse.QuestionnaireId = viewModel.QuestionnaireId;
                conditionalResponse.QuestionId = viewModel.QuestionId;
                conditionalResponse.Response = responseValue;

                // 开始事务
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 先删除现有的条件
                    _context.ConditionalResponseConditions
                        .RemoveRange(conditionalResponse.Conditions);
                    
                    // 保存条件响应更新
                    _context.Update(conditionalResponse);
                    await _context.SaveChangesAsync();

                    // 处理条件数据
                    if (!string.IsNullOrEmpty(conditionsJson))
                    {
                        try
                        {
                            var conditions = JsonSerializer.Deserialize<List<CommonCondition>>(conditionsJson, _jsonOptions);
                            
                            if (conditions != null && conditions.Count > 0)
                            {
                                int orderIndex = 0;
                                
                                foreach (var conditionData in conditions)
                                {
                                    var condition = new ConditionalResponseCondition
                                    {
                                        Id = System.Guid.NewGuid().ToString(),
                                        ConditionalResponseId = conditionalResponse.Id,
                                        QuestionnaireId = conditionalResponse.QuestionnaireId,
                                        QuestionId = conditionData.QuestionId?.ToString() ?? string.Empty,
                                        TextResponse = conditionData.TextResponse?.ToString() ?? "[]",
                                        ConditionType = conditionData.ConditionType?.ToString() ?? "Equals",
                                        LogicOperator = conditionData.LogicOperator?.ToString() ?? "AND",
                                        OrderIndex = orderIndex++
                                    };
                                    
                                    _context.Add(condition);
                                }
                                
                                await _context.SaveChangesAsync();
                            }
                        }
                        catch (JsonException ex)
                        {
                            ModelState.AddModelError("", "条件数据格式错误：" + ex.Message);
                            await transaction.RollbackAsync();
                            return await PrepareEditView(viewModel);
                        }
                    }
                    
                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Index), new { questionnaireId = conditionalResponse.QuestionnaireId });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "保存条件响应时发生错误：" + ex.Message);
                    return await PrepareEditView(viewModel);
                }
            }

            // 如果模型验证失败，重新显示表单
            return await PrepareEditView(viewModel);
        }

        private async Task<IActionResult> PrepareEditView(ConditionalResponseViewModel viewModel)
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
                        OptionsJson = q.OptionsJson,
                        SortOrder = q.SortOrder
                    })
                    .ToListAsync();
                    
                ViewBag.Questions = questions;
            }

            // 获取条件数据
            if (!string.IsNullOrEmpty(viewModel?.Id))
            {
                var conditionalResponse = await _context.ConditionalResponses
                    .Include(cr => cr.Conditions)
                    .FirstOrDefaultAsync(cr => cr.Id == viewModel.Id);

                if (conditionalResponse != null)
                {
                    // 准备条件数据用于JavaScript
                    var conditionsData = conditionalResponse.Conditions
                        .OrderBy(c => c.OrderIndex)
                        .Select(c => new
                        {
                            Id = c.Id,
                            QuestionId = c.QuestionId,
                            TextResponse = c.TextResponse,
                            ConditionType = c.ConditionType,
                            LogicOperator = c.LogicOperator,
                            OrderIndex = c.OrderIndex
                        })
                        .ToList();

                    ViewBag.ConditionsData = conditionsData;
                }
            }

            return View(viewModel);
        }

        // GET: ConditionalResponses/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var conditionalResponse = await _context.ConditionalResponses
                .Include(cr => cr.Question)
                .Include(cr => cr.Conditions)
                .ThenInclude(c => c.Question)
                .FirstOrDefaultAsync(cr => cr.Id == id);

            if (conditionalResponse == null)
            {
                return NotFound();
            }

            return View(conditionalResponse);
        }

        // POST: ConditionalResponses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var conditionalResponse = await _context.ConditionalResponses
                .Include(cr => cr.Conditions)
                .FirstOrDefaultAsync(cr => cr.Id == id);

            if (conditionalResponse == null)
            {
                return NotFound();
            }

            // 保存问卷ID以便重定向
            var questionnaireId = conditionalResponse.QuestionnaireId;

            // 开始事务
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 删除所有关联的条件
                _context.ConditionalResponseConditions
                    .RemoveRange(conditionalResponse.Conditions);

                // 删除条件响应
                _context.ConditionalResponses.Remove(conditionalResponse);

                // 保存更改
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // 设置成功消息
                TempData["SuccessMessage"] = "条件响应已成功删除。";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "删除条件响应时发生错误：" + ex.Message;
            }

            // 重定向回列表页面
            return RedirectToAction(nameof(Index), new { questionnaireId });
        }

        // Get: ConditionalResponses/GetConditionalResponses
        [HttpGet]
        public async Task<IActionResult> GetConditionalResponses(string questionnaireId)
        {
            try
            {
                var conditionalResponses = await _context.ConditionalResponses
                    .Where(cr => cr.QuestionnaireId == questionnaireId)
                    .Include(cr => cr.Conditions)
                    .Select(cr => new
                    {
                        cr.Id,
                        cr.QuestionnaireId,
                        cr.QuestionId,
                        cr.Response,
                        Conditions = cr.Conditions.Select(c => new
                        {
                            c.Id,
                            c.QuestionId,
                            c.ConditionType,
                            c.TextResponse,
                            c.LogicOperator
                        }).ToList()
                    })
                    .ToListAsync();
                
                return Json(conditionalResponses);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}