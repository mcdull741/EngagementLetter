using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EngagementLetter.Data;
using EngagementLetter.Models;

namespace EngagementLetter.Web.Controllers
{
    public class ReplaceContentConditionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReplaceContentConditionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ReplaceContentConditions
        public async Task<IActionResult> Index(string replaceContentId)
        {
            if (string.IsNullOrEmpty(replaceContentId))
            {
                return NotFound();
            }

            var replaceContent = await _context.ReplaceContents
                .Include(rc => rc.Questionnaire)
                .FirstOrDefaultAsync(rc => rc.Id == replaceContentId);

            if (replaceContent == null)
            {
                return NotFound();
            }

            ViewBag.ReplaceContent = replaceContent;

            var conditions = await _context.ReplaceContentConditions
                .Include(c => c.Question)
                .Where(c => c.ReplaceContentId == replaceContentId)
                .OrderBy(c => c.Question.SortOrder)
                .ToListAsync();

            return View(conditions);
        }

        // GET: ReplaceContentConditions/Create
        public IActionResult Create(string replaceContentId)
        {
            if (string.IsNullOrEmpty(replaceContentId))
            {
                return NotFound();
            }

            ViewBag.ReplaceContentId = replaceContentId;
            
            // 获取替换内容对应的问卷的问题
            var replaceContent = _context.ReplaceContents
                .Include(rc => rc.Questionnaire)
                .ThenInclude(q => q.Questions)
                .FirstOrDefault(rc => rc.Id == replaceContentId);

            if (replaceContent?.Questionnaire?.Questions != null)
            {
                ViewBag.Questions = replaceContent.Questionnaire.Questions
                    .OrderBy(q => q.SortOrder)
                    .ToList();
            }

            return View();
        }

        // POST: ReplaceContentConditions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReplaceContentId,QuestionId,ExpectedAnswer")] ReplaceContentCondition replaceContentCondition)
        {
            if (ModelState.IsValid)
            {
                replaceContentCondition.Id = System.Guid.NewGuid().ToString();
                _context.Add(replaceContentCondition);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { replaceContentId = replaceContentCondition.ReplaceContentId });
            }

            ViewBag.ReplaceContentId = replaceContentCondition.ReplaceContentId;
            return View(replaceContentCondition);
        }

        // GET: ReplaceContentConditions/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var replaceContentCondition = await _context.ReplaceContentConditions
                .Include(c => c.ReplaceContent)
                .Include(c => c.Question)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (replaceContentCondition == null)
            {
                return NotFound();
            }

            // 获取替换内容对应的问卷的问题
            var replaceContent = _context.ReplaceContents
                .Include(rc => rc.Questionnaire)
                    .ThenInclude(q => q.Questions)
                .FirstOrDefault(rc => rc.Id == replaceContentCondition.ReplaceContentId);

            if (replaceContent?.Questionnaire?.Questions != null)
            {
                ViewBag.Questionnaire = replaceContent.Questionnaire;
                ViewBag.Questions = replaceContent.Questionnaire.Questions
                    .OrderBy(q => q.SortOrder)
                    .ToList();
            }

            return View(replaceContentCondition);
        }

        // POST: ReplaceContentConditions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,ReplaceContentId,QuestionId,ExpectedAnswer")] ReplaceContentCondition replaceContentCondition)
        {
            if (id != replaceContentCondition.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(replaceContentCondition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReplaceContentConditionExists(replaceContentCondition.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { replaceContentId = replaceContentCondition.ReplaceContentId });
            }
            return View(replaceContentCondition);
        }

        // GET: ReplaceContentConditions/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var replaceContentCondition = await _context.ReplaceContentConditions
                .Include(c => c.ReplaceContent)
                .Include(c => c.Question)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (replaceContentCondition == null)
            {
                return NotFound();
            }

            return View(replaceContentCondition);
        }

        // POST: ReplaceContentConditions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var replaceContentCondition = await _context.ReplaceContentConditions
                .FirstOrDefaultAsync(c => c.Id == id);

            if (replaceContentCondition != null)
            {
                _context.ReplaceContentConditions.Remove(replaceContentCondition);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { replaceContentId = replaceContentCondition?.ReplaceContentId });
        }

        private bool ReplaceContentConditionExists(string id)
        {
            return _context.ReplaceContentConditions.Any(e => e.Id == id);
        }
    }
}