using BookStoreAggregateLoader;
using BookStoreAggregateLoader.LegacyDb;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Net;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(Startup))]

namespace BookStoreAggregateLoader
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            builder.Services.AddSingleton(configuration);
			var bookStoreConnectionString = "";

			builder.Services.AddDbContext<BookStoreContext>(options =>
				options.UseSqlServer(bookStoreConnectionString), ServiceLifetime.Singleton);

			var transientFailurePolicy = Policy.HandleResult<HttpResponseMessage>(r =>
					r.StatusCode == HttpStatusCode.NotFound ||
					r.StatusCode == HttpStatusCode.RequestTimeout ||
					r.StatusCode == HttpStatusCode.InternalServerError ||
					r.StatusCode == HttpStatusCode.NotImplemented ||
					r.StatusCode == HttpStatusCode.BadGateway ||
					r.StatusCode == HttpStatusCode.GatewayTimeout ||
					r.StatusCode == HttpStatusCode.ServiceUnavailable)
				.Or<WebException>()
				.WaitAndRetryAsync(new[]
				{
					TimeSpan.FromSeconds(0.25),
					TimeSpan.FromSeconds(0.5),
					TimeSpan.FromSeconds(1)
				});

			builder.Services.AddHttpClient<IBookStoreApiClient, BookStoreApiClient>()
			.AddPolicyHandler(transientFailurePolicy);
		}
    }
}
