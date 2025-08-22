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
using DocumentFormat.OpenXml.Wordprocessing;
using System.Xml.Linq;


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
        // 获取匹配的模板
        private async Task<Template?> GetMatchingTemplateAsync(EngLetter engLetter)
        {
            var templates = await _context.Templates
                .Include(t => t.Conditions)
                    .ThenInclude(c => c.Question)
                .Where(t => t.QuestionnaireId == engLetter.QuestionnaireId)
                .OrderByDescending(t => t.Priority)
                .ToListAsync();

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
                    var expectedAnswer = condition.TextResponse;
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
                        try
                        {
                            var expectedArray = System.Text.Json.JsonSerializer.Deserialize<string[]>(expectedAnswer ?? "");
                            if (expectedArray != null && expectedArray.Length > 0)
                            {
                                var expectedValue = expectedArray[0];
                                if (actualAnswer != expectedValue)
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (actualAnswer != expectedAnswer)
                                {
                                    isMatch = false;
                                    break;
                                }
                            }
                        }
                        catch
                        {
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
                    return template;
                }
            }

            return null;
        }

        // 获取匹配的ReplaceContent字典
        private async Task<Dictionary<string, string>> GetMatchingReplaceContentsAsync(EngLetter engLetter)
        {
            var replaceContents = await _context.ReplaceContents
                .Include(rc => rc.Conditions)
                    .ThenInclude(c => c.Question)
                .Where(rc => rc.QuestionnaireId == engLetter.QuestionnaireId)
                .ToListAsync();

            var dicReplaceContent = new Dictionary<string, string>();
            
            foreach (var replaceContent in replaceContents)
            {
                bool conditionsMatch = true;
                
                // 检查所有条件是否匹配
                foreach (var condition in replaceContent.Conditions)
                {
                    var userResponse = engLetter.UserResponses
                        .FirstOrDefault(ur => ur.QuestionId == condition.QuestionId);

                    if (userResponse == null)
                    {
                        conditionsMatch = false;
                        break;
                    }

                    // 根据问题类型处理答案匹配
                    var questionType = condition.Question?.Type;
                    var expectedAnswer = condition.TextResponse;
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
                                    conditionsMatch = false;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            conditionsMatch = false;
                            break;
                        }
                    }
                    else
                    {
                        // 单选题和文本题处理
                        try
                        {
                            var expectedArray = System.Text.Json.JsonSerializer.Deserialize<string[]>(expectedAnswer ?? "");
                            if (expectedArray != null && expectedArray.Length > 0)
                            {
                                var expectedValue = expectedArray[0];
                                if (actualAnswer != expectedValue)
                                {
                                    conditionsMatch = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (actualAnswer != expectedAnswer)
                                {
                                    conditionsMatch = false;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            if (actualAnswer != expectedAnswer)
                            {
                                conditionsMatch = false;
                                break;
                            }
                        }
                    }

                    if (!conditionsMatch) break;
                }

                // 如果所有条件都匹配，将Key和Content添加到字典
                if (conditionsMatch)
                {
                    dicReplaceContent[replaceContent.Key] = replaceContent.Content;
                }
            }

            return dicReplaceContent;
        }

        private async Task<byte[]> ReplaceContentInTemplate(string templatePath, Dictionary<string, string> replaceContents)
        {
            var filePath = Path.Combine(_environment.WebRootPath, "Templates", templatePath);
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException("模板文件不存在", filePath);
            }

            // 读取模板文件内容
            byte[] templateBytes;
            using (var stream = new MemoryStream())
            {
                using (var fileStream = System.IO.File.OpenRead(filePath))
                {
                    await fileStream.CopyToAsync(stream);
                }
                templateBytes = stream.ToArray();
            }

            // 使用OpenXml替换内容
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(templateBytes, 0, templateBytes.Length);
                memoryStream.Position = 0;

                using (var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(memoryStream, true))
                {
                    var body = wordDoc.MainDocumentPart?.Document?.Body;
                    if (body == null)
                    {
                        throw new InvalidOperationException("无法打开Word文档");
                    }

                    // 替换段落中的关键字（跳过表格中的段落）
                    foreach (var paragraph in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                    {
                        // 检查段落是否在表格中，如果在表格中则跳过
                        var parent = paragraph.Parent;
                        while (parent != null)
                        {
                            if (parent is DocumentFormat.OpenXml.Wordprocessing.TableCell)
                            {
                                break; // 在表格中，跳过此段落
                            }
                            parent = parent.Parent;
                        }
                        
                        if (parent == null) // 不在表格中
                        {
                            ReplaceKeywordsInParagraph(paragraph, replaceContents);
                        }
                    }


                    foreach (var cell in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.TableCell>())
                    {
                        foreach (var paragraph in cell.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                        {
                            ReplaceKeywordsInParagraph(paragraph, replaceContents);
                        }
                    }

                    wordDoc.MainDocumentPart?.Document?.Save();
                }

                return memoryStream.ToArray();
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

            var dicReplaceContent = await GetMatchingReplaceContentsAsync(engLetter);

            var matchedTemplate = await GetMatchingTemplateAsync(engLetter);
        
            if (matchedTemplate == null || string.IsNullOrEmpty(matchedTemplate.TemplatePath))
            {
                return Json(new { success = false, message = "未找到匹配的模板或模板文件不存在" });
            }
        
            try
            {
                // 使用OpenXml替换模板中的关键字
                var processedBytes = await ReplaceContentInTemplate(matchedTemplate.TemplatePath, dicReplaceContent);
                
                // 返回处理后的文件
                var fileName = $"{engLetter.Title}_{matchedTemplate.Name}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                return File(processedBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }
            catch (FileNotFoundException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"处理模板时发生错误: {ex.Message}" });
            }
        }

        /// <summary>
        /// 在段落中替换关键字，支持跨多个run的关键字替换，支持多段落内容
        /// </summary>
        /// <param name="paragraph">OpenXml段落元素</param>
        /// <param name="replaceContents">要替换的关键字字典</param>
        private static void ReplaceKeywordsInParagraph(DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph, Dictionary<string, string> replaceContents)
        {
            var paragraphText = GetParagraphPlainText(paragraph);
            foreach (var kvp in replaceContents)
            {
                if (!paragraphText.Contains(kvp.Key)) { continue; }
                
                // 查找包含关键字的run序列
                var runs = paragraph.Descendants<DocumentFormat.OpenXml.Wordprocessing.Run>().ToList();
                var matchingRuns = new List<DocumentFormat.OpenXml.Wordprocessing.Run>();
                string combinedStr = "";
                
                // 收集所有包含关键字的run
                foreach (var run in runs)
                {
                    var runText = "";
                    foreach (var text in run.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
                    {
                        runText += text.Text;
                    }
                    
                    if (runText.Contains(kvp.Key) || combinedStr.Contains(kvp.Key) || (combinedStr + runText).Contains(kvp.Key))
                    {
                        matchingRuns.Add(run);
                        combinedStr += runText;
                    }
                }
                
                if (matchingRuns.Count > 0 && combinedStr.Contains(kvp.Key))
                {
                    // 替换合并后的字符串
                    string newCombinedStr = combinedStr.Replace(kvp.Key, kvp.Value);
                    
                    // 检查是否是多段落内容
                    var paragraphs = newCombinedStr.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    
                    // 第一段放入当前段落
                    var firstParagraphStr = paragraphs[0];
                    var firstRun = matchingRuns[0];
                    var firstRunTexts = firstRun.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().ToList();
                    if (firstRunTexts.Count > 0)
                    {
                        firstRunTexts[0].Text = firstParagraphStr;
                        for (int i = 1; i < firstRunTexts.Count; i++)
                        {
                            firstRunTexts[i].Text = "";
                        }
                    }
                    
                    // 清空其他run的text
                    for (int i = 1; i < matchingRuns.Count; i++)
                    {
                        var runTexts = matchingRuns[i].Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>().ToList();
                        foreach (var text in runTexts)
                        {
                            text.Text = "";
                        }
                    }

                    // 多段落处理
                    if (paragraphs.Length > 1)
                    {
                        var parent = paragraph.Parent;
                        if (parent != null)
                        {
                            // 复制当前段落格式并创建新段落
                            var currParagraph = paragraph;
                            int localNumberingCounter = 0;
                            for (int i = 1; i < paragraphs.Length; i++)
                            {
                                var newParagraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
                                
                                // 复制段落属性
                                if (paragraph.ParagraphProperties != null)
                                {
                                    newParagraph.ParagraphProperties = (DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties)paragraph.ParagraphProperties.CloneNode(true);
                                }
                                
                                // 创建新的run
                                var newRun = new DocumentFormat.OpenXml.Wordprocessing.Run();
                                
                                // 复制run属性（如果有第一个run）
                                if (matchingRuns.Count > 0 && matchingRuns[0].RunProperties != null)
                                {
                                    newRun.RunProperties = (DocumentFormat.OpenXml.Wordprocessing.RunProperties)matchingRuns[0].RunProperties.CloneNode(true);
                                }
                                
                                // 添加文本
                                var newText = new DocumentFormat.OpenXml.Wordprocessing.Text(paragraphs[i]);
                                newRun.AppendChild(newText);
                                newParagraph.AppendChild(newRun);

                                // 格式化新段落
                                FormatParagraph(newParagraph, ref localNumberingCounter);

                                // 插入到当前段落后面
                                parent.InsertAfter(newParagraph, currParagraph);
                                currParagraph = currParagraph.NextSibling() as DocumentFormat.OpenXml.Wordprocessing.Paragraph;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取Word段落的纯文本内容
        /// </summary>
        /// <param name="paragraph">OpenXml段落元素</param>
        /// <returns>纯文本字符串</returns>
        private static string GetParagraphPlainText(DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph)
        {
            if (paragraph == null)
                return string.Empty;

            // 使用StringBuilder高效构建文本
            var textBuilder = new System.Text.StringBuilder();

            // 遍历段落中的所有Run元素
            foreach (var run in paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
            {
                foreach (var text in run.Elements<DocumentFormat.OpenXml.Wordprocessing.Text>())
                {
                    textBuilder.Append(text.Text);
                }

                // 处理换行符
                foreach (var breakElement in run.Elements<DocumentFormat.OpenXml.Wordprocessing.Break>())
                {
                    textBuilder.Append("\n");
                }

                // 处理制表符
                foreach (var tab in run.Elements<DocumentFormat.OpenXml.Wordprocessing.TabChar>())
                {
                    textBuilder.Append("\t");
                }
            }

            return textBuilder.ToString().Trim();
        }

        /// <summary>
        /// 格式化段落，根据首字符添加bullet point或numbering
        /// </summary>
        /// <param name="paragraph">OpenXml段落元素</param>
        /// <param name="numberingCounter">计数器，用于numbering的序号</param>
        private static void FormatParagraph(DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph, ref int numberingCounter)
        {
            if (paragraph == null) return;

            var paragraphText = GetParagraphPlainText(paragraph);
            if (string.IsNullOrWhiteSpace(paragraphText)) return;

            var trimmedText = paragraphText.TrimStart();
            if (string.IsNullOrEmpty(trimmedText)) return;

            var firstChar = trimmedText[0];
            string actualText = trimmedText.Substring(1).TrimStart();

            // 确保段落有ParagraphProperties
            if (paragraph.ParagraphProperties == null)
            {
                paragraph.ParagraphProperties = new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties();
            }

            // 处理bullet point
            if (firstChar == '*')
            {
                // 创建bullet point的numbering属性
                var numberingProperties = new DocumentFormat.OpenXml.Wordprocessing.NumberingProperties(
                    new DocumentFormat.OpenXml.Wordprocessing.NumberingLevelReference() { Val = 0 },
                    new DocumentFormat.OpenXml.Wordprocessing.NumberingId() { Val = 1 }
                );

                // 清除现有的numbering属性
                var existingNumbering = paragraph.ParagraphProperties.Elements<DocumentFormat.OpenXml.Wordprocessing.NumberingProperties>().FirstOrDefault();
                if (existingNumbering != null)
                {
                    existingNumbering.Remove();
                }

                paragraph.ParagraphProperties.AppendChild(numberingProperties);

                // 更新段落文本
                UpdateParagraphText(paragraph, actualText);
            }
            // 处理numbering
            else if (firstChar == '#')
            {
                numberingCounter++;
                
                // 创建numbering属性
                var numberingProperties = new DocumentFormat.OpenXml.Wordprocessing.NumberingProperties(
                    new DocumentFormat.OpenXml.Wordprocessing.NumberingLevelReference() { Val = 0 },
                    new DocumentFormat.OpenXml.Wordprocessing.NumberingId() { Val = 2 }
                );

                // 清除现有的numbering属性
                var existingNumbering = paragraph.ParagraphProperties.Elements<DocumentFormat.OpenXml.Wordprocessing.NumberingProperties>().FirstOrDefault();
                if (existingNumbering != null)
                {
                    existingNumbering.Remove();
                }

                paragraph.ParagraphProperties.AppendChild(numberingProperties);

                // 更新段落文本为带序号格式
                string numberedText = $"{numberingCounter}. {actualText}";
                UpdateParagraphText(paragraph, numberedText);
            }
            else
            {
                // 如果段落已有numbering属性，提取并更新计数器
                var existingNumbering = paragraph.ParagraphProperties.Elements<DocumentFormat.OpenXml.Wordprocessing.NumberingProperties>().FirstOrDefault();
                if (existingNumbering != null)
                {
                    var numberingId = existingNumbering.Elements<DocumentFormat.OpenXml.Wordprocessing.NumberingId>().FirstOrDefault();
                    if (numberingId != null && numberingId.Val == 2)
                    {
                        // 这是一个已有的numbering段落，尝试提取当前序号
                        var match = System.Text.RegularExpressions.Regex.Match(paragraphText, @"^(\d+)\.");
                        if (match.Success && int.TryParse(match.Groups[1].Value, out int currentNumber))
                        {
                            numberingCounter = Math.Max(numberingCounter, currentNumber);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更新段落中的文本内容
        /// </summary>
        /// <param name="paragraph">OpenXml段落元素</param>
        /// <param name="newText">新的文本内容</param>
        private static void UpdateParagraphText(DocumentFormat.OpenXml.Wordprocessing.Paragraph paragraph, string newText)
        {
            if (paragraph == null) return;

            // 清除现有的所有run
            var runs = paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>().ToList();
            foreach (var run in runs)
            {
                run.Remove();
            }

            // 创建新的run和文本
            var newRun = new DocumentFormat.OpenXml.Wordprocessing.Run();
            var newRunText = new DocumentFormat.OpenXml.Wordprocessing.Text(newText);
            newRun.AppendChild(newRunText);
            paragraph.AppendChild(newRun);
        }

    }
}