using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EngagementLetter.Models.DTO;

namespace EngagementLetter.Models.ViewModels
{
    /// <summary>
    /// 替换内容视图模型 - 用于表单提交和显示
    /// </summary>
    public class ReplaceContentViewModel
    {
        /// <summary>
        /// 替换内容ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 关联的问卷ID
        /// </summary>
        [Required(ErrorMessage = "请选择关联问卷")]
        public string QuestionnaireId { get; set; } = string.Empty;

        /// <summary>
        /// 替换关键字
        /// </summary>
        [Required(ErrorMessage = "替换关键字不能为空")]
        [StringLength(255, ErrorMessage = "关键字长度不能超过255个字符")]
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// 替换关键字的描述
        /// </summary>
        [StringLength(1000, ErrorMessage = "描述长度不能超过1000个字符")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 替换内容
        /// </summary>
        [Required(ErrorMessage = "替换内容不能为空")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 关联的条件集合
        /// </summary>
        public List<ReplaceContentConditionDto> Conditions { get; set; } = new List<ReplaceContentConditionDto>();
    }
}