using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HD.Station.MediaManagement.Abstractions.Abstractions;
using HD.Station.MediaManagement.Mvc.Features.MediaFile.Models;
using HD.Station.MediaManagement.Mvc.Mapping;
using System.Linq;

namespace HD.Station.MediaManagement.Mvc.Features.MediaFile.Controllers
{
    [Route("[controller]")]
    public class MediaFilesController : Controller
    {
        private readonly IMediaFileManager _manager;

        public MediaFilesController(IMediaFileManager manager)
        {
            _manager = manager;
        }

        // GET: /MediaFiles
        [HttpGet("")]
        public async Task<IActionResult> Index(string filter = "", int page = 1)
        {
            const int pageSize = 10;
            var dtos = await _manager.ListAsync(filter, page, pageSize);
            var vms = dtos.Select(d => d.ToViewModel());
            ViewBag.Filter = filter;
            ViewBag.Page = page;
            return View(vms);
        }

        // GET: /MediaFiles/Details/{id}
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var dto = await _manager.GetAsync(id);
            if (dto == null) return NotFound();
            return View(dto.ToViewModel());
        }

        // GET: /MediaFiles/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new MediaFileViewModel { Status = StatusEnum.Active });
        }

        // POST: /MediaFiles/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MediaFileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Tính Hash, MediaInfo + lưu file
            // (để service xử lý; giả sử manager nhận IFormFile + vm.ToDto())
            var newId = await _manager.CreateAsync(vm.FileUpload, vm.ToDto());
            return RedirectToAction(nameof(Details), new { id = newId });
        }

        // GET: /MediaFiles/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _manager.GetAsync(id);
            if (dto == null) return NotFound();
            return View(dto.ToViewModel());
        }

        // POST: /MediaFiles/Edit/{id}
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, MediaFileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            vm.Id = id;
            await _manager.UpdateAsync(vm.ToDto());
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /MediaFiles/Delete/{id}
        [HttpPost("Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _manager.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
