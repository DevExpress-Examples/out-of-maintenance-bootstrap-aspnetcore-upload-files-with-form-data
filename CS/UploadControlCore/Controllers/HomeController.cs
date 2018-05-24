using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UploadControlCore.Models;
using DevExpress.AspNetCore.Bootstrap;

namespace UploadControlCore.Controllers {
    public class HomeController : Controller {

        public HomeController(IFileStorage storage) {
            Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            Storage.RemoveOldFiles();
        }

        public IFileStorage Storage { get; }

        public IActionResult Index() {
            var newPortfolio = new Portfolio { Id = Guid.NewGuid() };
            return View(newPortfolio);
        }

        public IActionResult CreatePortfolio(Portfolio portfolio) {
            if (ModelState.IsValid)
                return View("ViewPortfolio", portfolio);

            return View("Index", portfolio);
        }

        public IActionResult ViewImage(Guid portfolioId, string name) {
            byte[] fileData = Storage.GetFilesByPortfolioId(portfolioId).Where(f => f.FileName == name).FirstOrDefault().Bytes;
            return File(fileData, "image/jpg");
        }

        public IActionResult UploadFile(BootstrapUploadedFilesCollection files, Guid id) {
            files.ForEach(file => {
                using (var fileStream = file.OpenReadStream()) {
                    byte[] data = new byte[fileStream.Length];
                    fileStream.Read(data, 0, data.Length);
                    Storage.SaveFile(id, data, file.FileName);
                }
                file.CustomData["name"] = file.FileName;
            });
            return Ok(files);
        }
    }
}