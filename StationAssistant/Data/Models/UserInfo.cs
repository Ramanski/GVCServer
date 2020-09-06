using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StationAssistant.Data
{
    public class UserInfo
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Не указан пароль")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Не указан логин")]
        public string Login { get; set; }

        public string Name { get; set; }
        
        public string Role { get; set; }
    }
}
