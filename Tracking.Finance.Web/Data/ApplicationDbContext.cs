using System;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Data
{
	/// <summary>
	/// Application specific implementation of the <see cref="IdentityDbContext"/> class used for identity.
	/// </summary>
	public class ApplicationDbContext : IdentityDbContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
		/// </summary>
		/// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
		/// </summary>
		public ApplicationDbContext()
		{
		}

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Accounts.
		/// </summary>
		public DbSet<Account> Accounts => Set<Account>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Accounts in Currencies.
		/// </summary>
		public DbSet<AccountInCurrency> AccountsInCurrencies => Set<AccountInCurrency>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Counterparties.
		/// </summary>
		public DbSet<Counterparty> Counterparties => Set<Counterparty>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Currencies.
		/// </summary>
		public DbSet<Currency> Currencies => Set<Currency>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Finance users.
		/// </summary>
		public DbSet<FinanceUser> FinanceUsers => Set<FinanceUser>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Products.
		/// </summary>
		public DbSet<Product> Products => Set<Product>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Product Categories.
		/// </summary>
		public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Product Category Closures.
		/// </summary>
		public DbSet<ProductCategoryClosure> ProductCategoryClosures => Set<ProductCategoryClosure>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Transactions.
		/// </summary>
		public DbSet<Transaction> Transactions => Set<Transaction>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Transaction Categories.
		/// </summary>
		public DbSet<TransactionCategory> TransactionCategories => Set<TransactionCategory>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Transaction Items.
		/// </summary>
		public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Units.
		/// </summary>
		public DbSet<Unit> Units => Set<Unit>();

		/// <summary>
		/// Gets the <see cref="DbSet{TEntity}"/> of Unit Closures.
		/// </summary>
		public DbSet<UnitClosure> UnitClosures => Set<UnitClosure>();

		/// <inheritdoc/>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = "TestDatabase.db" };
			var connectionString = connectionStringBuilder.ToString();
			var connection = new SqliteConnection(connectionString);

			optionsBuilder
				.LogTo(Console.WriteLine)
				.EnableSensitiveDataLogging()
				.UseSqlite(connection);
		}

		/// <inheritdoc/>
		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			var euro = new Currency
			{
				Id = 1,
				AlphabeticCode = "EUR",
				Crypto = false,
				NumericCode = 978,
				Name = "Euro",
				NormalizedName = "EURO",
				From = new DateTimeOffset(new DateTime(1999, 01, 01)),
				Historical = false,
				MinorUnit = 2,
				Official = true,
				Until = null,
			};

			builder.Entity<Currency>().HasData(euro);

			var dollars = new Currency
			{
				Id = 2,
				AlphabeticCode = "USD",
				Crypto = false,
				NumericCode = 0,
				Name = "United States Dollar",
				NormalizedName = "UNITED STATES DOLLAR",
				From = new DateTimeOffset(DateTime.Today),
				Historical = false,
				MinorUnit = 2,
				Official = true,
				Until = null,
			};

			builder.Entity<Currency>().HasData(dollars);
		}
	}
}
