using Flexlib.Domain;

namespace Flexlib.Application.Ports;


public interface IUserRepository
{
    bool Exists(string id);
    void Save(IUser user);
    IUser? Get(string id);
    void Delete(string id);
    IEnumerable<IUser> GetAll();
}


