using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationAssistant.Data
{
    public class UsersContext : IdentityDbContext
    {
        public UsersContext(DbContextOptions<UsersContext> options) : base(options)
        {

        }
    }
}
