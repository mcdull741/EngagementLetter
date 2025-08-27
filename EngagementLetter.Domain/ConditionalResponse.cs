using System.Diagnostics;
using EngagementLetter.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace EngagementLetter.Models
{
    public class ConditionalResponse : BaseEntity
    {
        [Display(Name = "绑定的问卷Id")]
        public string QuestionnaireId { get; set; }

        [Display(Name = "绑定的问题Id")]
        public string QuestionId { get; set; }

        [Display(Name = "绑定的条件")]
        public ICollection<ConditionalResponseCondition> Conditions { get; set; } = new List<ConditionalResponseCondition>();

        [Display(Name = "绑定的回答")]
        public string Response { get; set; } = "[]";

        //导航属性
        public virtual Questionnaire? Questionnaire { get; set; }
        public virtual Question Question { get; set; }

    }

}