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
		public int UserId { get; set; }					// FK to UserProfile.UserId

		[Required]
		public int SiteId { get; set; }					// FK to SiteId

		[ForeignKey("UserId")]
		public UserProfile UserProfile { get; set; }

		[ForeignKey("SiteId")]
		public SiteProfile SiteProfile { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		public string Email { get; set; }

		[Required]
		public string RegistrationToken { get; set; }

		[Required]
		public bool Activated { get; set; }
	}

	public class LocalPasswordModel
	{
		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Current password")]
		public string OldPassword { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
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

	/// <summary>
	/// Used by admins to register new users into the system.
	/// Once registered, a user will receive an email instructing them to go to a specific link to create their password.
	/// </summary>
	public class RegisterNewUserModel
	{
		[Required]
		[Display(Name = "Username:")]
		public string UserName { get; set; }

		[Required]
		[Display(Name = "First Name:")]
		public string FirstName { get; set; }

		[Required]
		[Display(Name = "Last Name:")]
		public string LastName { get; set; }

		[Required]
		[Display(Name = "Email:")]
		public string Email { get; set; }
	}
}
