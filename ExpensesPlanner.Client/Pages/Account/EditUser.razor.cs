using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Radzen.Blazor;

namespace ExpensesPlanner.Client.Pages.Account
{
    public partial class EditUser
    {
        [Inject] private IUserService _userService { get; set; } = default!;
        private ApplicationUser editUserModel = new();
        private RadzenTemplateForm<ApplicationUser> form { get; set; } = default!;
        [Inject] private NavigationManager navigation { get; set; } = default!;
        [Parameter] public string Id { get; set; } = string.Empty;
        private string imagePreview;
        private bool busy;
        protected override async Task OnInitializedAsync()
        {
            var user = await LoadUserAsync(Id);

            editUserModel = new ApplicationUser
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            if(editUserModel.ProfilePictureUrl != null)
                imagePreview = $"data:image/png;base64,{Convert.ToBase64String(editUserModel.ProfilePictureUrl)}";
        }
        private async Task UpdateUser()
        {
            busy = true;
            var response = await _userService.UpdateUserAsync(editUserModel);
            if(response.IsSuccessStatusCode)
            {
                
                await Task.Delay(2000);
                busy = false;
                navigation.NavigateTo("/account/users");
            }

            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error updating user: {errorMessage}");
                busy = false;
            }
        }
        private async Task<ApplicationUser> LoadUserAsync(string id)
        {
            return await _userService.GetUserByIdAsync(id);
        }

        private async Task OnImageSelected(InputFileChangeEventArgs e)
        {
            const int maxBytes = 5 * 1024 * 1024; //5MB

            var file = e.File;

            if (file.Size > maxBytes)
            {
                Console.WriteLine("Image exceed the dimension of KBs.");
            }
            else
            {
                using var stream = file.OpenReadStream(maxBytes);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                
                editUserModel.ProfilePictureUrl = imageBytes;
                Console.WriteLine("Image uploaded with success.");
                imagePreview = $"data:image/png;base64,{Convert.ToBase64String(editUserModel.ProfilePictureUrl)}";
            }
        }

        private async Task TriggerFileInput()
        {
            await JS.InvokeVoidAsync("eval", "document.getElementById('fileUpload').click();");
        }

        private void Cancel()
        {
            navigation.NavigateTo("/account/users");
        }
    }
}
