using System.Threading.Tasks;
using ModelsLibrary;

namespace StationAssistant.Auth
{
    interface IAccountsRepository
    {
        public Task<UserToken> Regiser(UserInfo userInfo);
        public Task<UserToken> Login(UserInfo userInfo);
    }
}
