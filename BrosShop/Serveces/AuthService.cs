using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BrosShop.Serveces
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService()
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
            _httpClient = new HttpClient();
        }

        public async Task<AuthToken> AuthenticateAsync(string username, string password)
        {
            using (var client = new HttpClient())
            {
                var adminDto = new AdminDto {Login = username, Password = password };
                var apiString = _configuration["ApiSettings:AuthUrl"];
                var content = new StringContent(JsonConvert.SerializeObject(adminDto), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiString}login", content);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AuthToken>(json);
                }
                else
                {
                    throw new Exception("Authentication failed");
                }
            }
        }

        public void SaveToken(AuthToken token)
        {
            Properties.Settings.Default.Token = token.Token;
            Properties.Settings.Default.Save();
        }

        public AuthToken LoadToken()
        {
            var token = Properties.Settings.Default.Token;

            if (!string.IsNullOrEmpty(token))
            {
                return new AuthToken { Token = token};
            }

            return null;
        }
    }
}
