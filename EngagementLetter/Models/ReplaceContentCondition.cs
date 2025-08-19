using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EngagementLetter.Models.Base;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EngagementLetter.Models
{
    /// <summary>
    /// 模板实体类 - 用于管理Engagement Letter的Word文档模板
    /// </summary>
    public class ReplaceContentCondition : BaseEntity
    {
        /// <summary>
        /// 关联的问卷ID
        /// </summary>
        [ForeignKey("Questionnaire")]
        public required string QuestionnaireId { get; set; }

        /// <summary>
        /// 关联的问题ID
        /// </summary>
        [ForeignKey("Question")]
        public required string QuestionId { get; set; }

        /// <summary>
        /// 关联的替换内容ID
        /// </summary>
        [ForeignKey("ReplaceContent")]
        public required string ReplaceContentId { get; set; }

        /// <summary>
        /// 条件匹配的文本响应值, JArray格式字符串，
        /// 如果QuestionType == Radio / Text时，只有一个元素
        /// 如果QuestionType == Checkbox时，允许有多个元素
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string TextResponse { get; set; } = "[]";

        /// <summary>
        /// 条件逻辑关系（AND、OR）
        /// </summary>
        [StringLength(10)]
        public string LogicOperator { get; set; } = "AND"; // AND, OR

        /// <summary>
        /// 条件顺序（用于复杂条件的执行顺序）
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        /// <summary>
        /// 关联的问卷对象
        /// </summary>
        public required virtual Questionnaire Questionnaire { get; set; }

        /// <summary>
        /// 关联的问题对象
        /// </summary>  
        public required virtual Question Question { get; set; }

        /// <summary>
        /// 关联的替换内容对象
        /// </summary>
        public required virtual ReplaceContent ReplaceContent { get; set; }

    }
}
