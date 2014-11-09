using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;
using System.Web.Mvc;
using WebMatrix.WebData;
using RazorTests.Models;

namespace RazorTests.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class InitializeSimpleMembershipAttribute : ActionFilterAttribute
	{
		private static SimpleMembershipInitializer _initializer;
		private static object _initializerLock = new object();
		private static bool _isInitialized;

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			// Ensure ASP.NET Simple Membership is initialized only once per app start
			LazyInitializer.EnsureInitialized(ref _initializer, ref _isInitialized, ref _initializerLock);
		}

		private class SimpleMembershipInitializer
		{
			public SimpleMembershipInitializer()
			{
				Database.SetInitializer<UsersContext>(null);

				try
				{
					using (var context = new UsersContext())
					{
						if (!context.Database.Exists())
						{
							// Create the SimpleMembership database without Entity Framework migration schema
							((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
						}
					}

					// See: http://stackoverflow.com/questions/13073247/websecurity-initializedatabaseconnection-method-can-be-called-only-once
					// This call is being made in _AppStart.  I bet we don't need this here at all now.
					// The reason it's being called in _AppStart is because I enabled "do migration on startup."
					// The more I work with Razor, the more I am beginning to despise it -- it gets IN THE WAY, rather than getting OUT OF THE WAY.
					// I added this test, then commented out the whole piece of shyte.
					//if (!WebSecurity.Initialized)
					//{
					//	WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
					//}
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException("The ASP.NET Simple Membership database could not be initialized. For more information, please see http://go.microsoft.com/fwlink/?LinkId=256588", ex);
				}
			}
		}
	}
}
