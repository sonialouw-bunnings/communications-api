using System;
using Microsoft.Extensions.DependencyInjection;

namespace Location.Domain.Repositories
{
	/// <summary>
	///     Base repository class
	/// </summary>
	/// <typeparam name="TCtx">type of db context</typeparam>
	public abstract class BaseRepository<TCtx>
	{
		// context provider
		private readonly Func<TCtx> _provider;

		protected BaseRepository(IServiceProvider serviceProvider)
			=> _provider = serviceProvider.GetRequiredService<TCtx>;

		protected BaseRepository(Func<TCtx> ctxProvider) => _provider = ctxProvider;

		/// <summary>
		///     Creates a new database context for access to the database.
		/// </summary>
		public TCtx Db() => _provider();
	}
}