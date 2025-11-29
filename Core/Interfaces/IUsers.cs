using coreAden.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace coreAden.Core.Interfaces
{
    internal interface IUsers
    {

        List<SelectListItem> getUsers();

        Users getUserByID(int id);
    }
}
