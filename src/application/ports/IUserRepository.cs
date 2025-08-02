using Flexlib.Domain;
using Flexlib.Infrastructure.Interop;
using Flexlib.Infrastructure.Persistence;

namespace Flexlib.Application.Ports;


public interface IUserRepository
{
    bool                Exists(string id);
    void                Save(IUser user);
    IUser?              Get(string id);
    IUser?              GetByHashedId(string id);
    void                Delete(string id);
    IEnumerable<IUser>  GetAll();
    Session             GetSession();
    Result              CloseSession();
    Result              SaveSession(string id);
}


