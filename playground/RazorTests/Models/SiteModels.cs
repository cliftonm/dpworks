using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace RazorTests.Models
{
	[Table("SiteProfile")]
	public class SiteProfile
	{
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public string Municipality { get; set; }

		[Required]
		[MaxLength(2)]		// Contrast with StringLength, which does client-side validation.
		public string State { get; set; }

		[Required]
		public string ContactName { get; set; }

		[Required]
		public string ContactPhone { get; set; }

		[Required]
		public string ContactEmail { get; set; }

		public string LogoUrl { get; set; }
	}
}
