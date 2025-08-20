using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EngagementLetter.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EngagementLetter.Models
{
    /// <summary>
    /// ReplaceContent实体类 - 用于管理Engagement Letter的Word文档模板的替换内容
    /// </summary>
    public class ReplaceContent : BaseEntity
    {
        /// <summary>
        /// 关联的问卷ID
        /// </summary>
        [ForeignKey("Questionnaire")]
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// 替换关键字
        /// </summary>
        [Required(ErrorMessage = "替换关键字不能为空")]
        [StringLength(255)]
        public string Key { get; set; }

        /// <summary>
        /// 替换关键字的描述
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// 替换内容
        /// </summary>
        [Required(ErrorMessage = "替换内容不能为空")]
        public string? Content { get; set; }

        /// <summary>
        /// 关联的条件集合
        /// </summary>
        public virtual ICollection<ReplaceContentCondition> Conditions { get; set; } = new List<ReplaceContentCondition>();
    
        /// <summary>
        /// 关联的问卷对象（导航属性）
        /// </summary>
        public virtual Questionnaire? Questionnaire { get; set; }
    }
}