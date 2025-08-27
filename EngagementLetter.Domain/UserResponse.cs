using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EngagementLetter.Models.Base;

namespace EngagementLetter.Models
{
    public class UserResponse : BaseEntity
    {
        [Required(ErrorMessage = "问题ID不能为空")]
        [Display(Name = "问题ID")]
        public string QuestionId { get; set; }

        [Required(ErrorMessage = "EngLetter ID不能为空")]
        [Display(Name = "EngLetter ID")]
        public string EngLetterId { get; set; }

        [Display(Name = "文本回答")]
        [MaxLength(1000, ErrorMessage = "回答长度不能超过1000个字符")]
        public string? TextResponse { get; set; }

        [Display(Name = "回答日期")]
        [DataType(DataType.DateTime)]
        public DateTime ResponseDate { get; set; } = DateTime.Now;

        // 导航属性
        public virtual Question? Question { get; set; }
        public virtual EngLetter? EngLetter { get; set; }
    }
}