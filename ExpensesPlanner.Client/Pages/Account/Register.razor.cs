using Blazored.LocalStorage;
using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.RootPages;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Pages.Account
{
    public partial class Register
    {     
        [Inject] private IUserService userService { get; set; } = default!;  
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] private AuthService AuthService { get; set; } = default!;
        [Inject] private HttpClient HttpClient { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;

        private readonly RegisterUser registerModel = new();
        private RadzenTemplateForm<RegisterUser>? form;
        
        private string? imagePreview;
        private string? message;
        private bool busy;
        private async Task SubmitForm()
        {
            try
            {
                busy = true;
                var response = await userService.CreateUserAsync(registerModel);

                if (response.IsSuccessStatusCode)
                {
                    await Task.Delay(2000);
                    busy = false;

                    var loginDto = new LoginDto
                    {
                        Email = registerModel.Email!,
                        Password = registerModel.Password
                    };


                    response = await AuthService.LoginAsync(loginDto);

                    if (response.IsSuccessStatusCode)
                    {
                        var user = await response.Content.ReadFromJsonAsync<TokenResponse>();

                        await LocalStorage.SetItemAsync("authToken", user?.Token);
                        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user?.Token);

                        await ((JwtAuthenticationStateProvider)AuthStateProvider).MarkUserAsAuthenticatedAsync(user!.Token);

                        Navigation.NavigateTo(PagesRoutes.AllExpenses);
                    }
                    else
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Error,
                            Summary = "Error",
                            Detail = "Attempt to login without success.",
                            Duration = 4000,
                            ShowProgress = true,
                            CloseOnClick = true,
                            Payload = DateTime.Now
                        });
                    }
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error creating expense: " + errorMessage);
                    busy = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception error: " + ex.Message);
            }
        }

        private async Task OnImageSelected(InputFileChangeEventArgs e)
        {
            const int maxBytes = 2 * 1024 * 1024; //2MB

            var file = e.File;

            if (file.Size > maxBytes)
            {
                message = "imagem excede os bytes";
                Console.WriteLine(message);
            }
            else
            {
                using var stream = file.OpenReadStream(maxBytes);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var imageBytes = ms.ToArray();

                message = "imagem carregada.";
                registerModel.ProfilePictureUrl = imageBytes;
                Console.WriteLine("imagem carregada.");
                imagePreview = $"data:image/png;base64,{Convert.ToBase64String(registerModel.ProfilePictureUrl)}";
            }
        }

        private async Task ValidateAndSubmit()
        {
            if (form!.EditContext.Validate())
            {
                await form.Submit.InvokeAsync(null);
            }
        }

        private void Cancel()
        {
            Navigation.NavigateTo("/account/users");
        }

        private async Task TriggerFileInput()
        {
            await JS.InvokeVoidAsync("eval", "document.getElementById('fileUpload').click();");
        }
    }
}
