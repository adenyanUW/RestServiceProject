using Microsoft.AspNetCore.Identity;
using UserService.Models;

namespace UserService.Data
{
    public interface IUserRepository
    {
        public List<User> Users { get; }
        public User? GetUser(int id);
        public int Add(User newUser);
        public User? Update(int updatedUsersid, User updatedUserInfo);
        public bool Remove(int id);
    }

    public class UserRepository : IUserRepository
    {
        private static int currentId = 1;

        private List<User> _users;
        public List<User> Users
        {
            get => _users;
        }

        public UserRepository()
        {
            _users = new List<User>();

            // Put some default users in here for ease of testing.
            AddDefaultUsers();
        }

        //===========================================================================================================//
        // Public Methods
        //===========================================================================================================//

        public User? GetUser(int id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        public int Add(User newUser)
        {
            int newId = currentId++;
            newUser.Id = newId;
            newUser.CreatedDate = DateTime.UtcNow;
            _users.Add(newUser);
            return newId;
        }

        public User? Update(int updatedUsersid, User updatedUserInfo)
        {
            User? userToUpdate = _users.FirstOrDefault(u => u.Id == updatedUsersid);

            if (userToUpdate != null)
            {
                userToUpdate.Email = updatedUserInfo.Email;
                userToUpdate.Password = updatedUserInfo.Password;
            }

            return userToUpdate;
        }

        public bool Remove(int id)
        {
            int usersRemoved = _users.RemoveAll(u => u.Id == id);
            return (usersRemoved == 1);
        }

        //===========================================================================================================//
        // PRivate Methods
        //===========================================================================================================//

        private void AddDefaultUsers()
        {
            Add(new User { Email = "My1stEmail@gmail.com", Password = "My1stPassword" });
            Add(new User { Email = "My2ndEmail@gmail.com", Password = "My2ndPassword" });
            Add(new User { Email = "My3rdEmail@gmail.com", Password = "My3rdPassword" });
            Add(new User { Email = "My4thEmail@gmail.com", Password = "My4thPassword" });
            Add(new User { Email = "My5thEmail@gmail.com", Password = "My5thPassword" });
        }
    }
}
