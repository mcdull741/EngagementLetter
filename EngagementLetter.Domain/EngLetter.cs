using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EngagementLetter.Models.Base;

namespace EngagementLetter.Models
{
    public class EngLetter : BaseEntity
    {
        [Required(ErrorMessage = "标题不能为空")]
        [Display(Name = "记录标题")]
        [MaxLength(100, ErrorMessage = "标题长度不能超过100个字符")]
        public required string Title { get; set; }

        [Display(Name = "记录描述")]
        [MaxLength(500, ErrorMessage = "描述长度不能超过500个字符")]
        public string? Description { get; set; }

        [Display(Name = "创建日期")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "最后修改日期")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }

        [Display(Name = "是否删除")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "关联问卷ID")]
        [Required(ErrorMessage = "关联问卷ID不能为空")]
        public string QuestionnaireId { get; set; } = string.Empty;
        
        [Display(Name = "用户回答集合")]
        public ICollection<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
        
        //导航属性
        public virtual Questionnaire? Questionnaire { get; set; }
    }
}