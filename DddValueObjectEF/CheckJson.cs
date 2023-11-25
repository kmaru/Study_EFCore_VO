using DddValueObjectEF.Infra;
using Domain;
using System.Text.Json;

namespace DddValueObjectEF
{
    public record UserDto(UserId Id, UserName Name, Email Email, UserAge Age);
        
    internal static class CheckJson
    {
        public static async Task Run()
        {
            var user = User.Create(
                UserId.Create(Guid.NewGuid()),
                UserName.Create("John Doe"),
                Email.Create("a@example.com"),
                UserAge.Create(42)
            );

            var userJson = JsonSerializer.Serialize(user, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new UserIdJsonConverter(),
                    new UserNameJsonConverter(),
                    new EmailJsonConverter(),
                    new UserAgeJsonConverter()
                }
            });

            Console.WriteLine(userJson);

            var user2 = JsonSerializer.Deserialize<UserDto>(userJson, new JsonSerializerOptions()
            {
                Converters =
                {
                    new UserIdJsonConverter(),
                    new UserNameJsonConverter(),
                    new EmailJsonConverter(),
                    new UserAgeJsonConverter()
                }
            });

            Console.WriteLine($"{user2.Id} {user2.Name} {user2.Email} {user2.Age}");

        }
    }
}
