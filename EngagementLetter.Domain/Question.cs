using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using EngagementLetter.Models.Base;

namespace EngagementLetter.Models
{
    public enum QuestionType
    {
        Radio, //单选
        CheckBox, //多选
        Text
    }

    public class Question : BaseEntity
    {
        [Display(Name = "问卷ID")]
        [Required(ErrorMessage = "问卷ID不能为空")]
        public string QuestionnaireId { get; set; } = string.Empty;

        [Required(ErrorMessage = "问题内容不能为空")]
        [Display(Name = "问题内容")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "问题类型")]
        public QuestionType Type { get; set; }

        [Display(Name = "排序号")]
        public int SortOrder { get; set; }

        //Valid when OptionType is QuestionType.Radio or QuestionType.CheckBox
        //JArray格式，代表单选或多选的选项列表
        [Display(Name = "选项列表")]
        public string OptionsJson { get; set; } = string.Empty;

        //导航属性
        public virtual Questionnaire? Questionnaire { get; set; }
    }
}