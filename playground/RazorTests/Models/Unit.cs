using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RazorTests.Models
{
	[Table("Unit")]
	public class Unit
	{
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int SiteId { get; set; }					// FK to SiteId

		[ForeignKey("SiteId")]
		public SiteProfile SiteProfile { get; set; }

		[Required]
		[Display(Name = "Abbr.")]
		public string Abbr { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }
	}
}
