using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace RazorTests.Models
{
	public class UsersContext : DbContext
	{
		public UsersContext()
			: base("DefaultConnection")
		{
		}

		public DbSet<UserProfile> UserProfiles { get; set; }
		public DbSet<SiteProfile> SiteProfiles { get; set; }
		public DbSet<Role> Roles { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }
		public DbSet<UserInfo> UserInfo { get; set; }
		public DbSet<Unit> Units { get; set; }
		public DbSet<LaborRate> LaborRates { get; set; }
		public DbSet<Equipment> Equipment { get; set; }
		public DbSet<Material> Materials { get; set; }
	}

	/// <summary>
	/// WebSecurity table.
	/// </summary>
	[Table("UserProfile")]
	public class UserProfile
	{
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int UserId { get; set; }
		public string UserName { get; set; }
	}

	/// <summary>
	/// WebSecurity table.
	/// </summary>
	[Table("webpages_Roles")]
	public class Role
	{
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int RoleId { get; set; }
		
		[MaxLength(256)]
		public string RoleName { get; set; }
	}

	[Table("webpages_UsersInRoles")]
	public class UserRole
	{
		[Key]
		public int UserId { get; set; }		// FK field

		[Required]
		public int RoleId { get; set; }		// FK field

		[ForeignKey("UserId")]
		public UserProfile UserProfile { get; set; }		// Relationship

		[ForeignKey("RoleId")]								// Relationship
		public Role Role { get; set; }
	}

	[Table("UserInfo")]
	public class UserInfo
	{
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		[Required]
		public int SiteId { get; set; }					// FK to SiteId

		[ForeignKey("SiteId")]
		public SiteProfile SiteProfile { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		public string Email { get; set; }

		[Required(AllowEmptyStrings=true)]
		public string RegistrationToken { get; set; }

		[Required]
		public bool Activated { get; set; }
	}

	/// <summary>
	/// Used by the user management UI.
	/// </summary>
	public class UserItem
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
	}

	public class PasswordModel
	{
		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password:")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password:")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}

	public class LoginModel
	{
		[Required]
		[Display(Name = "Username:")]
		public string UserName { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password:")]
		public string Password { get; set; }

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}

	/// <summary>
	/// !!! USED TO SEED THE DATABASE ON INITIAL LOAD !!!
	/// </summary>
	public class RegisterModel
	{
		[Required]
		[Display(Name = "Username:")]
		public string UserName { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password:")]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm password:")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}
}
