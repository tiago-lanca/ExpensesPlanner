using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

namespace ExpensesPlanner.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddRadzenComponents();
            builder.Services.AddScoped<DialogService>();

            builder.Services.AddHttpClient("Api", client =>
            {
                client.BaseAddress = new Uri("https://localhost:8081/");
            });

            builder.Services.AddScoped<IExpenseService, ExpenseService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ListExpensesService>();
            builder.Services.AddScoped<DialogService>();


            await builder.Build().RunAsync();
        }
    }
}
