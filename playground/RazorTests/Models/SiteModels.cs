using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace RazorTests.Models
{
	public class SitesContext : DbContext
	{
		public SitesContext()
			: base("DefaultConnection")
		{
		}

		public DbSet<SiteProfile> SiteProfiles { get; set; }
	}

	[Table("SiteProfile")]
	public class SiteProfile
	{
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int SiteId { get; set; }
		public string Name { get; set; }
		public string Municipality { get; set; }
		public string State { get; set; }
		public string ContactName { get; set; }
		public string ContactPhone { get; set; }
		public string ContactEmail { get; set; }
		public string LogoUrl { get; set; }
	}
}
