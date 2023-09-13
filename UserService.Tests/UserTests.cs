using System.Net;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using UserService.Tests.Models;
using static System.Net.Mime.MediaTypeNames;

namespace UserService.Tests
{
    public class Token
    {
        [JsonPropertyName("token")]
        public string TokenString { get; set; }
    }

    [TestClass]
    public class UserTests
    {
        public TestContext TestContext { get; set; }
        public static HttpClient client;

        //=======================================================================================================//
        // Class Initialization
        //=======================================================================================================//

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5223/api/");


        }

        [TestMethod]
        public void AddUser()
        {
            // Testing add user success and failure and requesting all users without a token.

            // ======================================================================================//

            // Arrange - setup bad user
            var newUser = new UserService.Tests.Models.User
            {
                Email = "Email without password",
            };

            var newUserJson = JsonSerializer.Serialize(newUser);
            var postNewUser = new StringContent(newUserJson, Encoding.UTF8, "application/json");

            // Act - add bad user
            var postAddUserResult = client.PostAsync("users", postNewUser).Result;

            // Assert - check result status bad user
            Assert.AreEqual(HttpStatusCode.BadRequest, postAddUserResult.StatusCode);

            // ======================================================================================//

            // Arrange - setup good user
            newUser = new UserService.Tests.Models.User
            {
                Email = "Test AddUser - Email",
                Password = "Test AddUser - Password",
            };
            postNewUser = SetupGoodUser(newUser.Email, newUser.Password);
            newUserJson = JsonSerializer.Serialize(newUser);

            // Act - add good user
            postAddUserResult = client.PostAsync("users", postNewUser).Result;

            // Assert - check result status good user
            Assert.AreEqual(HttpStatusCode.Created, postAddUserResult.StatusCode);

            // ======================================================================================//
            // Request user without getting token 

            // Act - Get all users without token in header
            var resultAllUsers = client.GetAsync("users").Result;

            // Assert - check result status unauthorized
            Assert.AreEqual(HttpStatusCode.Unauthorized, resultAllUsers.StatusCode);

            // ======================================================================================//

            // Arrange - setup and get token
            var postGetToken = new StringContent(newUserJson, Encoding.UTF8, "application/json");

            var postTokenResult = client.PostAsync("token", postGetToken).Result;
            var tokenJson = postTokenResult.Content.ReadAsStringAsync().Result;
            var token = JsonSerializer.Deserialize<UserService.Tests.Models.Token>(tokenJson);

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.TokenString);

            // Act - Get all users with token in header
            resultAllUsers = client.GetAsync("users").Result;

            // Assert - check result status ok and check result for a user/password combination that does not exist
            Assert.AreEqual(HttpStatusCode.OK, resultAllUsers.StatusCode);
            var tokenAllUsersJson = resultAllUsers.Content.ReadAsStringAsync().Result;
            var allUsers = JsonSerializer.Deserialize<List<UserService.Tests.Models.User>>(tokenAllUsersJson);
            Assert.AreNotEqual(null, allUsers.Where(u => u.Email == newUser.Email && u.Password == newUser.Password).FirstOrDefault());
            Assert.AreEqual(null, allUsers.Where(u => u.Email == newUser.Email && u.Password == newUser.Password + "a").FirstOrDefault());
        }

        [TestMethod]
        public void UpdateUser()
        {
            // Testing update user success and failure, gets specific success and failure.

            // ======================================================================================//

            // Arrange - setup good user
            var newUser = new UserService.Tests.Models.User
            {
                Email = "Test UpdateUser - Email",
                Password = "Test UpdateUser - Password",
            };

            StringContent postNewUser = SetupGoodUser(newUser.Email, newUser.Password);

            // Act - add good user
            var postAddUserResult = client.PostAsync("users", postNewUser).Result;
            var PostResultjson = postAddUserResult.Content.ReadAsStringAsync().Result;
            var returnedContact = JsonSerializer.Deserialize<User>(PostResultjson);

            // ======================================================================================//

            // Act - get specific new user success
            var GetSpecificResult = client.GetAsync("users/" + returnedContact.Id).Result;

            // Assert - check result status good user
            Assert.AreEqual(HttpStatusCode.OK, GetSpecificResult.StatusCode);

            var SpecificUserJson = GetSpecificResult.Content.ReadAsStringAsync().Result;
            var specificUser = JsonSerializer.Deserialize<UserService.Tests.Models.User>(SpecificUserJson);

            Assert.AreEqual(newUser.Email, specificUser.Email);
            Assert.AreEqual(newUser.Password, specificUser.Password);

            // ======================================================================================//

            // Act - get specific new user failure
            GetSpecificResult = client.GetAsync("users/" + "985").Result;

            // Assert - check result status bad user
            Assert.AreEqual(HttpStatusCode.NotFound, GetSpecificResult.StatusCode);

            // ======================================================================================//

            // Arrange - update user (change email)
            newUser.Email = "***Test UpdateUser - Email***";
            var newUserJson = JsonSerializer.Serialize(newUser);
            postNewUser = new StringContent(newUserJson, Encoding.UTF8, "application/json");

            // Act - update user
            var updatedUserResult = client.PutAsync("users/" + returnedContact.Id, postNewUser).Result;

            // Assert - check result status updated user
            Assert.AreEqual(HttpStatusCode.OK, updatedUserResult.StatusCode);

            SpecificUserJson = updatedUserResult.Content.ReadAsStringAsync().Result;
            specificUser = JsonSerializer.Deserialize<UserService.Tests.Models.User>(SpecificUserJson);

            Assert.AreEqual(newUser.Email, specificUser.Email);
            Assert.AreEqual(newUser.Password, specificUser.Password);

            // ======================================================================================//

            // Arrange - update user failure 1 (change email to null)
            newUser.Email = null;
            newUserJson = JsonSerializer.Serialize(newUser);
            postNewUser = new StringContent(newUserJson, Encoding.UTF8, "application/json");

            // Act - update user
            updatedUserResult = client.PutAsync("users/" + returnedContact.Id, postNewUser).Result;

            // Assert - check result status updated user
            Assert.AreEqual(HttpStatusCode.BadRequest, updatedUserResult.StatusCode);

            // ======================================================================================//

            // Arrange - update user failure 2 (invalid user id to update)
            newUser.Email = "===Test UpdateUser - Email===";
            newUserJson = JsonSerializer.Serialize(newUser);
            postNewUser = new StringContent(newUserJson, Encoding.UTF8, "application/json");

            // Act - update user
            updatedUserResult = client.PutAsync("users/" + "985", postNewUser).Result;

            // Assert - check result status updated user
            Assert.AreEqual(HttpStatusCode.NotFound, updatedUserResult.StatusCode);
        }

        [TestMethod]
        public void DeleteUser()
        {
            // Testing delete user success and failure, gets specific success and failure.

            // ======================================================================================//

            // Arrange add new user
            var newUser = new UserService.Tests.Models.User
            {
                Email = "Test DeleteUser - Email",
                Password = "Test DeleteUser - Password",
            };

            StringContent postNewUser = SetupGoodUser(newUser.Email, newUser.Password);

            // Act - add good user
            var postAddUserResult = client.PostAsync("users", postNewUser).Result;
            var PostResultjson = postAddUserResult.Content.ReadAsStringAsync().Result;
            var returnedContact = JsonSerializer.Deserialize<User>(PostResultjson);

            // ======================================================================================//

            // Act - get specific new user
            var GetSpecificResult = client.GetAsync("users/" + returnedContact.Id).Result;

            // Assert - check result status good user
            Assert.AreEqual(HttpStatusCode.OK, GetSpecificResult.StatusCode);

            var SpecificUserJson = GetSpecificResult.Content.ReadAsStringAsync().Result;
            var specificUser = JsonSerializer.Deserialize<UserService.Tests.Models.User>(SpecificUserJson);

            Assert.AreEqual(newUser.Email, specificUser.Email);
            Assert.AreEqual(newUser.Password, specificUser.Password);

            // ======================================================================================//

            // Act - delete new user
            var DeleteResult = client.DeleteAsync("users/" + returnedContact.Id).Result;

            // Assert - check result status good user
            Assert.AreEqual(HttpStatusCode.OK, DeleteResult.StatusCode);

            // ======================================================================================//

            // Act - get specific deleted user (failure)
            GetSpecificResult = client.GetAsync("users/" + returnedContact.Id).Result;

            // Assert - check result status good user
            Assert.AreEqual(HttpStatusCode.NotFound, GetSpecificResult.StatusCode);

            // ======================================================================================//

            // Act - try to delete previously deleted user (failure)
            DeleteResult = client.DeleteAsync("users/" + returnedContact.Id).Result;

            // Assert - check result status good user
            Assert.AreEqual(HttpStatusCode.NotFound, DeleteResult.StatusCode);
        }

        public StringContent SetupGoodUser(string email, string password)
        {
            var newUser = new UserService.Tests.Models.User
            {
                Email = email,
                Password = password,
            };

            var newUserJson = JsonSerializer.Serialize(newUser);
            return new StringContent(newUserJson, Encoding.UTF8, "application/json");
        }
    }
}