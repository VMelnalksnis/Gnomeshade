// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using NodaTime;

using VMelnalksnis.NordigenDotNet;
using VMelnalksnis.NordigenDotNet.Accounts;
using VMelnalksnis.NordigenDotNet.Agreements;
using VMelnalksnis.NordigenDotNet.Institutions;
using VMelnalksnis.NordigenDotNet.Requisitions;

namespace Gnomeshade.WebApi.Tests.Integration.Fixtures;

internal sealed class MockNordigenClient : INordigenClient
{
	public MockNordigenClient(IInstitutionClient institutions, IAgreementClient agreements)
	{
		Institutions = institutions;
		Agreements = agreements;
	}

	public IAccountClient Accounts { get; } = new MockAccountClient();

	public IAgreementClient Agreements { get; }

	public IInstitutionClient Institutions { get; }

	public IRequisitionClient Requisitions { get; } = new MockRequisitionClient();

	private sealed class MockRequisitionClient : IRequisitionClient
	{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public async IAsyncEnumerable<Requisition> Get(int pageSize = 100, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			yield return new()
			{
				Id = Guid.ParseExact("4d66e13c-9043-4169-86d2-0ffdf66731ec", "D"),
				Created = Instant.FromUtc(2024, 08, 19, 13, 15, 31),
				Redirect = new("https://gnomeshade.org/"),
				Status = RequisitionStatus.Ln,
				InstitutionId = "SANDBOXFINANCE_SFIN0000",
				Reference = "4d66e13c-9043-4169-86d2-0ffdf66731ec",
				Accounts =
				[
					Guid.ParseExact("1cf4bf3a-5073-42e7-b5da-f51615bad966", "D"),
					Guid.ParseExact("5243ec05-1bba-4f86-b49a-c29df3536cb1", "D"),
				],
				Link = new("https://ob.gocardless.com/ob-psd2/start/6b7b88fb-c279-40f2-9378-6bf8c17f08d5/SANDBOXFINANCE_SFIN0000"),
				AccountSelection = false,
				RedirectImmediate = false,
				AgreementValue = "088bce07-22f2-4bed-8f29-bcd61429f36f",
				UserLanguage = null,
				Ssn = null,
			};
		}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

		public Task<Requisition> Get(Guid id, CancellationToken cancellationToken = default) =>
			throw new NotImplementedException();

		public Task<Requisition> Post(RequisitionCreation requisitionCreation) =>
			throw new NotImplementedException();

		public Task Delete(Guid id) =>
			throw new NotImplementedException();
	}

	private sealed class MockAccountClient : IAccountClient
	{
		public Task<Account> Get(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Account>(id.ToString("D") switch
		{
			"1cf4bf3a-5073-42e7-b5da-f51615bad966" => new()
			{
				Id = id,
				Created = Instant.FromUtc(2024, 08, 19, 13, 16, 42),
				LastAccessed = Instant.FromUtc(2024, 08, 19, 13, 19, 06),
				Iban = "GL4263460000063464",
				InstitutionId = "SANDBOXFINANCE_SFIN0000",
				Status = AccountStatus.Ready,
			},

			"5243ec05-1bba-4f86-b49a-c29df3536cb1" => new()
			{
				Id = id,
				Created = Instant.FromUtc(2024, 08, 19, 13, 16, 42),
				LastAccessed = Instant.FromUtc(2024, 08, 19, 13, 19, 06),
				Iban = "GL9852800000052804",
				InstitutionId = "SANDBOXFINANCE_SFIN0000",
				Status = AccountStatus.Ready,
			},

			_ => throw new NotImplementedException(),
		});

		public Task<AccountDetails> GetDetails(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<AccountDetails>(id.ToString("D") switch
		{
			"1cf4bf3a-5073-42e7-b5da-f51615bad966" => new()
			{
				ResourceId = "01F3NS5ASCNMVCTEJDT0G215YE",
				Iban = "GL4263460000063464",
				Currency = "EUR",
				OwnerName = "Jane Doe",
				Name = "Main Account",
				Details = null,
				Product = "Checkings",
				CashAccountType = "CACC",
				MaskedPan = null,
			},

			"5243ec05-1bba-4f86-b49a-c29df3536cb1" => new()
			{
				ResourceId = "01F3NS4YV94RA29YCH8R0F6BMF",
				Iban = "GL9852800000052804",
				Currency = "EUR",
				OwnerName = "John Doe",
				Name = "Main Account",
				Details = null,
				Product = "Checkings",
				CashAccountType = "CACC",
				MaskedPan = null,
			},

			_ => throw new NotImplementedException(),
		});

		public Task<List<Balance>> GetBalances(Guid id, CancellationToken cancellationToken = default) =>
			throw new NotImplementedException();

		public Task<Transactions> GetTransactions(Guid id, Interval? interval = null, CancellationToken cancellationToken = default)
		{
			Transactions transactions = id.ToString("D") switch
			{
				"1cf4bf3a-5073-42e7-b5da-f51615bad966" => new()
				{
					Booked =
					[
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = null,
							DebtorName = null,
							TransactionAmount = new() { Currency = "EUR", Amount = -15.00m },
							TransactionId = "2024082001645708-1",
							UnstructuredInformation = "PAYMENT Alderaan Coffe",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = new() { Iban = "GL8240830000040838" },
							DebtorName = "MON MOTHMA",
							TransactionAmount = new() { Currency = "EUR", Amount = 45.00m },
							TransactionId = "2024082001645707-1",
							UnstructuredInformation = "For the support of Restoration of the Republic foundation",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = null,
							DebtorName = null,
							TransactionAmount = new() { Currency = "EUR", Amount = -15.00m },
							TransactionId = "2024082001645705-1",
							UnstructuredInformation = "PAYMENT Alderaan Coffe",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = new() { Iban = "GL8240830000040838" },
							DebtorName = "MON MOTHMA",
							TransactionAmount = new() { Currency = "EUR", Amount = 45.00m },
							TransactionId = "2024082001645704-1",
							UnstructuredInformation = "For the support of Restoration of the Republic foundation",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = null,
							DebtorName = null,
							TransactionAmount = new() { Currency = "EUR", Amount = -15.00m },
							TransactionId = "2024082001645702-1",
							UnstructuredInformation = "PAYMENT Alderaan Coffe",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = new() { Iban = "GL8240830000040838" },
							DebtorName = "MON MOTHMA",
							TransactionAmount = new() { Currency = "EUR", Amount = 45.00m },
							TransactionId = "2024082001645701-1",
							UnstructuredInformation = "For the support of Restoration of the Republic foundation",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 18),
							DebtorAccount = null,
							DebtorName = null,
							TransactionAmount = new() { Currency = "EUR", Amount = -15.00m },
							TransactionId = "2024081801645708-1",
							UnstructuredInformation = "PAYMENT Alderaan Coffe",
							ValueDate = new LocalDate(2024, 08, 18),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 18),
							DebtorAccount = new() { Iban = "GL8240830000040838" },
							DebtorName = "MON MOTHMA",
							TransactionAmount = new() { Currency = "EUR", Amount = 45.00m },
							TransactionId = "2024081801645707-1",
							UnstructuredInformation = "For the support of Restoration of the Republic foundation",
							ValueDate = new LocalDate(2024, 08, 18),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 18),
							DebtorAccount = null,
							DebtorName = null,
							TransactionAmount = new() { Currency = "EUR", Amount = -15.00m },
							TransactionId = "2024081801645705-1",
							UnstructuredInformation = "PAYMENT Alderaan Coffe",
							ValueDate = new LocalDate(2024, 08, 18),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 18),
							DebtorAccount = new() { Iban = "GL8240830000040838" },
							DebtorName = "MON MOTHMA",
							TransactionAmount = new() { Currency = "EUR", Amount = 45.00m },
							TransactionId = "2024081801645704-1",
							UnstructuredInformation = "For the support of Restoration of the Republic foundation",
							ValueDate = new LocalDate(2024, 08, 18),
						},
					],
					Pending =
					[
						new()
						{
							TransactionAmount = new() { Currency = "EUR", Amount = 10.00m },
							UnstructuredInformation = "Reserved PAYMENT Emperor's Burgers",
							ValueDate = new(2024, 08, 19),
						},
						new()
						{
							TransactionAmount = new() { Currency = "EUR", Amount = 10.00m },
							UnstructuredInformation = "Reserved PAYMENT Emperor's Burgers",
							ValueDate = new(2024, 08, 19),
						},
						new()
						{
							TransactionAmount = new() { Currency = "EUR", Amount = 10.00m },
							UnstructuredInformation = "Reserved PAYMENT Emperor's Burgers",
							ValueDate = new(2024, 08, 19),
						},
					],
				},

				"5243ec05-1bba-4f86-b49a-c29df3536cb1" => new()
				{
					Booked =
					[
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = null,
							DebtorName = null,
							TransactionAmount = new() { Currency = "EUR", Amount = -15.00m },
							TransactionId = "2024082001773508-1",
							UnstructuredInformation = "PAYMENT Alderaan Coffe",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = new() { Iban = "GL4888530000088535" },
							DebtorName = "MON MOTHMA",
							TransactionAmount = new() { Currency = "EUR", Amount = 45.00m },
							TransactionId = "2024082001773507-1",
							UnstructuredInformation = "For the support of Restoration of the Republic foundation",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = null,
							DebtorName = null,
							TransactionAmount = new() { Currency = "EUR", Amount = -15.00m },
							TransactionId = "2024082001773505-1",
							UnstructuredInformation = "PAYMENT Alderaan Coffe",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = new() { Iban = "GL4888530000088535" },
							DebtorName = "MON MOTHMA",
							TransactionAmount = new() { Currency = "EUR", Amount = 45.00m },
							TransactionId = "2024082001773504-1",
							UnstructuredInformation = "For the support of Restoration of the Republic foundation",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = null,
							DebtorName = null,
							TransactionAmount = new() { Currency = "EUR", Amount = -15.00m },
							TransactionId = "2024082001773502-1",
							UnstructuredInformation = "PAYMENT Alderaan Coffe",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 20),
							DebtorAccount = new() { Iban = "GL4888530000088535" },
							DebtorName = "MON MOTHMA",
							TransactionAmount = new() { Currency = "EUR", Amount = 45.00m },
							TransactionId = "2024082001773501-1",
							UnstructuredInformation = "For the support of Restoration of the Republic foundation",
							ValueDate = new LocalDate(2024, 08, 20),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 18),
							DebtorAccount = null,
							DebtorName = null,
							TransactionAmount = new() { Currency = "EUR", Amount = -15.00m },
							TransactionId = "2024081801773508-1",
							UnstructuredInformation = "PAYMENT Alderaan Coffe",
							ValueDate = new LocalDate(2024, 08, 18),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 18),
							DebtorAccount = new() { Iban = "GL4888530000088535" },
							DebtorName = "MON MOTHMA",
							TransactionAmount = new() { Currency = "EUR", Amount = 45.00m },
							TransactionId = "2024081801773507-1",
							UnstructuredInformation = "For the support of Restoration of the Republic foundation",
							ValueDate = new LocalDate(2024, 08, 18),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 18),
							DebtorAccount = null,
							DebtorName = null,
							TransactionAmount = new() { Currency = "EUR", Amount = -15.00m },
							TransactionId = "2024081801773505-1",
							UnstructuredInformation = "PAYMENT Alderaan Coffe",
							ValueDate = new LocalDate(2024, 08, 18),
						},
						new()
						{
							BankTransactionCode = "PMNT",
							BookingDate = new LocalDate(2024, 08, 18),
							DebtorAccount = new() { Iban = "GL4888530000088535" },
							DebtorName = "MON MOTHMA",
							TransactionAmount = new() { Currency = "EUR", Amount = 45.00m },
							TransactionId = "2024081801773504-1",
							UnstructuredInformation = "For the support of Restoration of the Republic foundation",
							ValueDate = new LocalDate(2024, 08, 18),
						},
					],
					Pending =
					[
						new()
						{
							TransactionAmount = new() { Currency = "EUR", Amount = 10.00m },
							UnstructuredInformation = "Reserved PAYMENT Emperor's Burgers",
							ValueDate = new(2024, 08, 19),
						},
						new()
						{
							TransactionAmount = new() { Currency = "EUR", Amount = 10.00m },
							UnstructuredInformation = "Reserved PAYMENT Emperor's Burgers",
							ValueDate = new(2024, 08, 19),
						},
						new()
						{
							TransactionAmount = new() { Currency = "EUR", Amount = 10.00m },
							UnstructuredInformation = "Reserved PAYMENT Emperor's Burgers",
							ValueDate = new(2024, 08, 19),
						},
					],
				},

				_ => throw new ArgumentOutOfRangeException(),
			};

			return Task.FromResult(transactions);
		}
	}
}
