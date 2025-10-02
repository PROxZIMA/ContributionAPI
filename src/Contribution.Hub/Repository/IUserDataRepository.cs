using Contribution.Hub.Models;

namespace Contribution.Hub.Repository;

public interface IUserDataRepository
{
    Task<UserData?> GetUserDataAsync(string userId);
}