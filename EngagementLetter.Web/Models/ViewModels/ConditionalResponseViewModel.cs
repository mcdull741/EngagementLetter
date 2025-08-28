using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EngagementLetter.Models.ViewModels
{
    /// <summary>
    /// 条件响应视图模型 - 用于表单提交和显示
    /// </summary>
    public class ConditionalResponseViewModel
    {
        /// <summary>
        /// 条件响应ID
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// 关联的问卷ID
        /// </summary>
        [Required(ErrorMessage = "请选择关联问卷")]
        public string QuestionnaireId { get; set; } = string.Empty;

        /// <summary>
        /// 关联的问题ID
        /// </summary>
        [Required(ErrorMessage = "请选择关联问题")]
        public string QuestionId { get; set; } = string.Empty;

        /// <summary>
        /// 绑定的回答
        /// </summary>
        [Required(ErrorMessage = "请输入回答内容")]
        public string Response { get; set; } = "[]";
    }
}