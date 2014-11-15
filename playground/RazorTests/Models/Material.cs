using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RazorTests.Models
{
	[Table("Material")]
	public class Material
	{
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int SiteId { get; set; }					// FK to SiteId

		[ForeignKey("SiteId")]
		public SiteProfile SiteProfile { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public int UnitId { get; set; }

		[ForeignKey("UnitId")]
		public virtual Unit Unit { get; set; }

		[Required]
		public decimal UnitCost { get; set; }
	}
}
