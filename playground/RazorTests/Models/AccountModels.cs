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
	}

	/// <summary>
	/// WebSecurity table.  This is all that it can contain.
	/// </summary>
	[Table("UserProfile")]
	public class UserProfile
	{
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int UserId { get; set; }
		public string UserName { get; set; }
	}

	[Table("UserInfo")]
	public class UserInfo
	{
		[Key]
		[DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
		public int UserInfoId { get; set; }
		public int UserId { get; set; }					// FK to UserProfile.UserId
		public int SiteId { get; set; }					// FK to SiteId
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string RegistrationToken { get; set; }
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
