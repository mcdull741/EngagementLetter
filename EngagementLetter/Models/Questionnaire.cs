using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EngagementLetter.Models.Base;

namespace EngagementLetter.Models
{
    public class Questionnaire : BaseEntity
    {
        [Required(ErrorMessage = "问卷标题不能为空")]
        [Display(Name = "问卷标题")]
        [MaxLength(100, ErrorMessage = "标题长度不能超过100个字符")]
        public string? Title { get; set; }

        [Display(Name = "问卷描述")]
        [MaxLength(500, ErrorMessage = "描述长度不能超过500个字符")]
        public string? Description { get; set; }

        [Display(Name = "创建日期")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "修改时间")]
        [DataType(DataType.DateTime)]
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        [Display(Name = "是否启用")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "问题列表")]
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}