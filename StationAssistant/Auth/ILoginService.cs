using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationAssistant.Auth
{
    interface ILoginService
    {
        Task Login(string token);
        Task LogOut();
    }
}
