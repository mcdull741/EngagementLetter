using System;
using System.ComponentModel.DataAnnotations;

namespace EngagementLetter.Models.DTO
{
    /// <summary>
    /// 替换内容条件数据传输对象
    /// </summary>
    public class ReplaceContentConditionDto
    {
        /// <summary>
        /// 条件ID（用于编辑时标识）
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 关联的问题ID
        /// </summary>
        [Required(ErrorMessage = "请选择问题")]
        public string QuestionId { get; set; } = string.Empty;

        /// <summary>
        /// 条件匹配的文本响应值
        /// </summary>
        public string ExpectedAnswer { get; set; } = string.Empty;

        /// <summary>
        /// 条件顺序（用于复杂条件的执行顺序）
        /// </summary>
        public int OrderIndex { get; set; } = 0;
    }
}