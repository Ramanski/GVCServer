using System.Threading.Tasks;
using ModelsLibrary;

namespace StationAssistant.Auth
{
    interface IAccountsRepository
    {
        public Task<UserToken> Regiser(User userInfo);
        public Task<UserToken> Login(User userInfo);
    }
}
