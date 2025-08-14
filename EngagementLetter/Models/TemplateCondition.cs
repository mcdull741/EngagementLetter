using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EngagementLetter.Models.Base;

namespace EngagementLetter.Models
{
    /// <summary>
    /// 模板条件实体类 - 定义模板的启用条件
    /// </summary>
    public class TemplateCondition : BaseEntity
    {

        /// <summary>
        /// 关联的问卷ID（用于条件验证时的上下文）
        /// </summary>
        [ForeignKey("Questionnaire")]
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// 关联的问卷对象
        /// </summary>
        public virtual Questionnaire Questionnaire { get; set; }

        /// <summary>
        /// 关联的问题ID
        /// </summary>
        [ForeignKey("Question")]
        public string QuestionId { get; set; }

        /// <summary>
        /// 关联的问题对象
        /// </summary>
        public virtual Question Question { get; set; }

        /// <summary>
        /// 关联的模板ID
        /// </summary>
        [ForeignKey("Template")]
        public string TemplateId { get; set; }

        /// <summary>
        /// 关联的模板对象
        /// </summary>
        public virtual Template Template { get; set; }

        /// <summary>
        /// 条件匹配的文本响应值
        /// </summary>
        [Column(TypeName = "TEXT")]
        public string TextResponse { get; set; }

        /// <summary>
        /// 条件类型（等于、包含、大于、小于等）
        /// </summary>
        [StringLength(50)]
        public string ConditionType { get; set; } = "Equals"; // Equals, Contains, GreaterThan, LessThan, NotEquals

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
        /// 条件描述（便于管理员理解）
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// 获取条件显示文本
        /// </summary>
        [NotMapped]
        public string DisplayText
        {
            get
            {
                var questionText = Question?.Content ?? "未知问题";
                var operatorText = GetOperatorText(ConditionType);
                return $"{questionText} {operatorText} \"{TextResponse}\"";
            }
        }

        /// <summary>
        /// 获取操作符显示文本
        /// </summary>
        private string GetOperatorText(string conditionType)
        {
            return conditionType switch
            {
                "Equals" => "等于",
                "Contains" => "包含",
                "GreaterThan" => "大于",
                "LessThan" => "小于",
                "NotEquals" => "不等于",
                _ => conditionType
            };
        }

        /// <summary>
        /// 验证响应是否满足此条件
        /// </summary>
        public bool IsConditionMet(string userResponse)
        {
            if (string.IsNullOrEmpty(userResponse))
                return false;

            return ConditionType switch
            {
                "Equals" => userResponse.Equals(TextResponse, StringComparison.OrdinalIgnoreCase),
                "Contains" => userResponse.Contains(TextResponse, StringComparison.OrdinalIgnoreCase),
                "GreaterThan" => decimal.TryParse(userResponse, out var userVal) && decimal.TryParse(TextResponse, out var targetVal) && userVal > targetVal,
                "LessThan" => decimal.TryParse(userResponse, out var userVal) && decimal.TryParse(TextResponse, out var targetVal) && userVal < targetVal,
                "NotEquals" => !userResponse.Equals(TextResponse, StringComparison.OrdinalIgnoreCase),
                _ => false
            };
        }
    }
}