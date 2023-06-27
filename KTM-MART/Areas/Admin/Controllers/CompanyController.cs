using KTM_MART.DataAccess.Repository.IRepository;
using KTM_MART.Models;
using KTM_MART.Models.ViewModels;
using KTM_MART.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KTM_MART.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {

            return View();
        }

        //Get Request for Create/Edit Product
        public IActionResult Upsert(int? id)
        {
            Company company = new();          
            if (id == null || id == 0)
            {
                //This means we want to create a new product
                return View(company);
            }
            else
            {
                //This section deals with updating product
                company = _unitOfWork.Company.GetFirstOrDefault(u => u.Id == id);
                return View(company);
            }


        }

        //Post Request for Create/Edit product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Company.Add(obj);
                    TempData["success"] = "Company Created Successfully";

                }
                else
                {

                    _unitOfWork.Company.Update(obj);
                    TempData["success"] = "Company Updated Successfully";
                }

                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Details(int id)
        {
            Product productDetials = _unitOfWork.Product.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category");

            return View(productDetials);
        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll();
            return Json(new { data = companyList });
        }


        //Post Request for Delete Product using API CAll
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Company.GetFirstOrDefault(c => c.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
         
            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Company Deleted" });

        }
        #endregion

    }
}
