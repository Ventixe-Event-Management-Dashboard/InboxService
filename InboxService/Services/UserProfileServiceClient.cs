using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace InboxService.Services
{
    public class UserProfileServiceClient
    {
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserProfileServiceClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _client = httpClientFactory.CreateClient("UserProfileService");
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt"];

            if (!string.IsNullOrWhiteSpace(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var users = await _client.GetFromJsonAsync<IEnumerable<UserDto>>("users");
            return users ?? new List<UserDto>();
        }
    }

    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public string FullName => $"{FirstName} {LastName}";
    }
}