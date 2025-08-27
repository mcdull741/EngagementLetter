using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EngagementLetter.Models.Base;

namespace EngagementLetter.Models
{
    /// <summary>
    /// 模板实体类 - 用于管理Engagement Letter的Word文档模板
    /// </summary>
    public class Template : BaseEntity
    {

        /// <summary>
        /// 关联的问卷ID
        /// </summary>
        [ForeignKey("Questionnaire")]
        public string QuestionnaireId { get; set; }

        /// <summary>
        /// 关联的问卷对象
        /// </summary>
        public virtual Questionnaire Questionnaire { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// 模板描述
        /// </summary>
        [StringLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// 模板文件路径（存储Word文档的相对路径）
        /// </summary>
        public string TemplatePath { get; set; }

        /// <summary>
        /// 模板优先级（1-100，数值越大优先级越高）
        /// </summary>
        [Range(0, 100)]
        public int Priority { get; set; } = 50;

        /// <summary>
        /// 创建人用户名
        /// </summary>
        [StringLength(255)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最后修改人用户名
        /// </summary>
        [StringLength(255)]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// 关联的模板条件集合
        /// </summary>
        public virtual ICollection<TemplateCondition> Conditions { get; set; } = new List<TemplateCondition>();

        /// <summary>
        /// 获取模板文件名（不含路径）
        /// </summary>
        [NotMapped]
        public string FileName => System.IO.Path.GetFileName(TemplatePath);

        /// <summary>
        /// 获取优先级显示文本
        /// </summary>
        [NotMapped]
        public string PriorityDisplay => $"🔥 {Priority}";

        /// <summary>
        /// 获取优先级等级描述
        /// </summary>
        [NotMapped]
        public string PriorityLevel
        {
            get
            {
                return Priority switch
                {
                    0 => "已禁用",
                    >= 90 => "最高优先级",
                    >= 75 => "高优先级",
                    >= 50 => "中优先级",
                    >= 25 => "低优先级",
                    _ => "最低优先级"
                };
            }
        }
    }
}