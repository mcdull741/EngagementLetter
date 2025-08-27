using EngagementLetter.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace EngagementLetter.Models
{
    public class ConditionalResponseCondition : CommonCondition
    {
        [Display(Name = "绑定的条件响应")]
        public string ConditionalResponseId { get; set; }

        //导航属性
        public virtual ConditionalResponse? ConditionalResponse { get; set; }
    }

}