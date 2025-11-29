using coreAden.Core.Interfaces;
using coreAden.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace coreAden.Services
{
    public class UserService : IUsers
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Users getUserByID(int id)
        {
            var repository = _unitOfWork.Repository<Users>();
            var usr = repository.GetAll().FirstOrDefault(x=>x.UserID ==id);
            return usr;
        }

        public List<SelectListItem> getUsers()
        {
            var repository = _unitOfWork.Repository<Users>();

            return repository.GetAll()
                .Select(x => new SelectListItem
                {
                    Text = x.username,
                    Value = x.UserID.ToString()
                }).ToList();

        }
    }
}