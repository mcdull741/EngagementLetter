using EngagementLetter.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace EngagementLetter.Models
{
    public class CommonCondition : BaseEntity
    {
        [Display(Name = "绑定的问题")]
        public string QuestionId { get;  set; }
        
        [Display(Name = "绑定的问题")]
        public string QuestionnaireId { get;  set; }

        /// <summary>
        /// JArray格式字符串，用于存储条件答案
        /// Radio / Text是，Array长度为1，
        /// Checkbox是，Array长度大于等于1
        /// </summary>
        [Display(Name = "条件答案")]
        public string TextResponse { get; set; } = "[]";

        /// <summary>
        /// 条件类型（等于、包含、大于、小于等）
        /// </summary>
        [StringLength(50)]
        public string ConditionType 
        { 
            get => GetOperatorText(_conditionType); 
            set => _conditionType = SetOperator(value); 
        }
        private string _conditionType;

        /// <summary>
        /// 条件逻辑关系（AND、OR）
        /// </summary>
        [StringLength(10)]
        public string LogicOperator 
        { 
            get => GetLogicOperatorText(_logicOperator); 
            set => _logicOperator = SetLogicOperator(value); 
        }
        private string _logicOperator;

        public CommonCondition()
        {
            // 设置默认值
            ConditionType = "Equals"; // Equals, Contains, GreaterThan, LessThan, NotEquals
            LogicOperator = "AND"; // AND, OR
        }

        /// <summary>
        /// 条件顺序（用于复杂条件的执行顺序）
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        private static string GetOperatorText(string conditionType)
        {
            return conditionType switch
            {
                "Equals" => "等于",
                "Contains" => "包含",
                "GreaterThan" => "大于",
                "LessThan" => "小于",
                "NotEquals" => "不等于",
                _ => throw new ArgumentException(message:"Invalid Condition Type.")
            };
        }

        private static string SetOperator(string conditionType)
        {
            return conditionType switch
            {
                "等于" => "Equals",
                "包含" => "Contains",
                "大于" => "GreaterThan",
                "小于" => "LessThan",
                "不等于" => "NotEquals",
                "Equals" => "Equals",
                "Contains" => "Contains",
                "GreaterThan" => "GreaterThan",
                "LessThan" => "LessThan",
                "NotEquals" => "NotEquals",
                _ => throw new ArgumentException(message:"Invalid Condition Type.")
            };
        }

        private static string SetLogicOperator(string logicOperator)
        {
            return logicOperator switch
            {
                "AND" => "AND",
                "OR" => "OR",
                "且" => "AND",
                "或" => "OR",
                _ => throw new ArgumentException(message:"Invalid Logic Operator.")
            };
        }

        private static string GetLogicOperatorText(string logicOperator)
        {
            return logicOperator switch
            {
                "AND" => "且",
                "OR" => "或",
                _ => throw new ArgumentException(message:"Invalid Logic Operator.")
            };
        }

        //导航属性
        public virtual Question? Question { get; set; }
        public virtual Questionnaire? Questionnaire { get; set; }
    }

}