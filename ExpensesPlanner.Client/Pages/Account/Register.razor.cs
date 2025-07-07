using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System.Net.Http.Json;

namespace ExpensesPlanner.Client.Pages.Account
{
    public partial class Register
    {       
        [Inject] NavigationManager Navigation { get; set; } = default!;

        private readonly RegisterUser registerModel = new();
        private RadzenTemplateForm<RegisterUser> form;
        [Inject] private IUserService userService { get; set; } = default!;
        private string imagePreview;
        private string message;
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
                    Navigation.NavigateTo("/account/users");
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
            if (form.EditContext.Validate())
            {
                await form.Submit.InvokeAsync(null);
            }
        }

        private async Task Cancel()
        {
            Navigation.NavigateTo("/account/users");
        }

        private async Task TriggerFileInput()
        {
            await JS.InvokeVoidAsync("eval", "document.getElementById('fileUpload').click();");
        }
    }
}
