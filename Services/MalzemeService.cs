using coreAden.Core.Interfaces;
using coreAden.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace coreAden.Services
{
    public class MalzemeService : IMalzemeServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public MalzemeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public string getMalzemeNameWithMalzemeID(int id)
        {
            var repository = _unitOfWork.Repository<Malzemeler>();
            return repository.Find(x => x.MalzemeID == id).FirstOrDefault().MalzemeAdı;
            
        }


       

    }
}