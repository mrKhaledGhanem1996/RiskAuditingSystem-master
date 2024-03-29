﻿using AuditingSystem.Entities.AuditProcess;
using AuditingSystem.Entities.Lockups;
using AuditingSystem.Entities.RiskAssessments;
using AuditingSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AuditingSystem.Web.Controllers.RiskAssessments
{
    public class ControlController : Controller
    {
        private readonly IBaseRepository<Control, int> _controlRepository;
        private readonly IBaseRepository<ControlType, int> _controlTypeRepository;
        private readonly IBaseRepository<ControlProcess, int> _controlProcessRepository;
        private readonly IBaseRepository<ControlFrequency, int> _controlFrequencyRepository;
        private readonly IBaseRepository<ControlEffectiveness, int> _controlEffectivenessRepository;
        private readonly IBaseRepository<RiskIdentification, int> _riskIdentificationRepository;

        public ControlController(
            IBaseRepository<Control, int> controlRepository,
            IBaseRepository<ControlEffectiveness, int> controlEffectivenessRepository,
            IBaseRepository<ControlFrequency, int> controlFrequencyRepository,
            IBaseRepository<ControlProcess, int> controlProcessRepository,
            IBaseRepository<ControlType, int> controlTypeRepository,
            IBaseRepository<RiskIdentification, int> riskIdentificationRepository)
        {
            _controlRepository = controlRepository;
            _controlEffectivenessRepository = controlEffectivenessRepository;
            _controlFrequencyRepository = controlFrequencyRepository;
            _controlProcessRepository = controlProcessRepository;
            _controlTypeRepository = controlTypeRepository;
            _riskIdentificationRepository = riskIdentificationRepository;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var controlRepository = await _controlRepository.ListAsync(
                new Expression<Func<Control, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                c => c.ControlType, c => c.ControlProcess, c => c.ControlFrequency, c => c.ControlEffectiveness, c => c.RiskIdentification);
            var model = controlRepository.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            ViewBag.TotalRow = controlRepository.Count();
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(controlRepository.Count() / (double)pageSize);

            return View(model);
        }

        public async Task<IActionResult> Add(int? riskIdentificationId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var controlType = await _controlTypeRepository.ListAsync(
                new Expression<Func<ControlType, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            var controlProcess = await _controlProcessRepository.ListAsync(
                new Expression<Func<ControlProcess, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            var controlFrequency = await _controlFrequencyRepository.ListAsync(
                new Expression<Func<ControlFrequency, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            var controlEffectivenesss = await _controlEffectivenessRepository.ListAsync(
                new Expression<Func<ControlEffectiveness, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Rate),
                null);

            var riskIdentification = await _riskIdentificationRepository.ListAsync(
                new Expression<Func<RiskIdentification, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            // Filter Risk Identification based on the provided ID
            if (riskIdentificationId.HasValue)
            {
                riskIdentification = riskIdentification.Where(r => r.Id == riskIdentificationId.Value);
            }

            ViewBag.ControlTypeId = new SelectList(controlType, "Id", "Name");
            ViewBag.ControlProcessId = new SelectList(controlProcess, "Id", "Name");
            ViewBag.ControlFrequencyId = new SelectList(controlFrequency, "Id", "Name");
            ViewBag.ControlEffectivenessId = new SelectList(controlEffectivenesss.Select(r => new { Id = r.Id, Name = $"{r.Rate} - {r.Name}" }), "Id", "Name");
            var riskIdentificationList = riskIdentification.Select(r => new
            {
                Id = r.Id,
                DisplayText = RemoveHtmlTags(r.Name), // إزالة العناصر HTML
            }).ToList();

            ViewBag.RiskIdentificationId = new SelectList(
                riskIdentificationList,
                "Id",
                "DisplayText"
            );

            return View();
        }

        public async Task<IActionResult> Edit(int id, int? riskIdentificationId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (riskIdentificationId.HasValue)
            {
                var controls = await _controlRepository.FindByAsync(x => x.RiskIdentificationId == riskIdentificationId);

                if (controls != null)
                {
                    var controlTypes = await _controlTypeRepository.ListAsync(
                       new Expression<Func<ControlType, bool>>[] { u => u.IsDeleted == false },
                       q => q.OrderBy(u => u.Name),
                       null);

                    var controlProcesses = await _controlProcessRepository.ListAsync(
                        new Expression<Func<ControlProcess, bool>>[] { u => u.IsDeleted == false },
                        q => q.OrderBy(u => u.Name),
                        null);

                    var controlFrequencies = await _controlFrequencyRepository.ListAsync(
                        new Expression<Func<ControlFrequency, bool>>[] { u => u.IsDeleted == false },
                        q => q.OrderBy(u => u.Name),
                        null);

                    var controlEffectivenesss = await _controlEffectivenessRepository.ListAsync(
                      new Expression<Func<ControlEffectiveness, bool>>[] { u => u.IsDeleted == false },
                       q => q.OrderBy(u => u.Rate),
                       null);

                    var riskIdentifications = await _riskIdentificationRepository.ListAsync(
                        new Expression<Func<RiskIdentification, bool>>[] { u => u.IsDeleted == false },
                        q => q.OrderBy(u => u.Name),
                        null);

                    ViewBag.ControlTypeId = new SelectList(controlTypes, "Id", "Name", controls.ControlTypeId);
                    ViewBag.ControlProcessId = new SelectList(controlProcesses, "Id", "Name", controls.ControlProcessId);
                    ViewBag.ControlFrequencyId = new SelectList(controlFrequencies, "Id", "Name", controls.ControlFrequencyId);
                    ViewBag.ControlEffectivenessId = new SelectList(controlEffectivenesss.Select(r => new { Id = r.Id, Name = $"{r.Rate} - {r.Name}" }), "Id", "Name", controls.ControlEffectivenessId);
                    var riskIdentificationLista = riskIdentifications.Select(r => new
                    {
                        Id = r.Id,
                        DisplayText = RemoveHtmlTags(r.Name), 
                    }).ToList();

                    ViewBag.RiskIdentificationId = new SelectList(
                        riskIdentificationLista,
                        "Id",
                        "DisplayText"
                    );
                    return View(controls);

                }
            }

            var control = await _controlRepository.FindByAsync(id);

            var controlType = await _controlTypeRepository.ListAsync(
                new Expression<Func<ControlType, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            var controlProcess = await _controlProcessRepository.ListAsync(
                new Expression<Func<ControlProcess, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            var controlFrequency = await _controlFrequencyRepository.ListAsync(
                new Expression<Func<ControlFrequency, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            var controlEffectiveness = await _controlEffectivenessRepository.ListAsync(
                new Expression<Func<ControlEffectiveness, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Rate),
                null);

            var riskIdentification = await _riskIdentificationRepository.ListAsync(
                new Expression<Func<RiskIdentification, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            ViewBag.ControlTypeId = new SelectList(controlType, "Id", "Name", control.ControlTypeId);
            ViewBag.ControlProcessId = new SelectList(controlProcess, "Id", "Name", control.ControlProcessId);
            ViewBag.ControlFrequencyId = new SelectList(controlFrequency, "Id", "Name", control.ControlFrequencyId);
            ViewBag.ControlEffectivenessId = new SelectList(controlEffectiveness.Select(r => new { Id = r.Id, Name = $"{r.Rate} - {r.Name}" }), "Id", "Name", control.ControlEffectivenessId);
            //ViewBag.RiskIdentificationId = new SelectList(riskIdentification, "Id", "Name", control.RiskIdentificationId);
            var riskIdentificationList = riskIdentification.Select(r => new
            {
                Id = r.Id,
                DisplayText = RemoveHtmlTags(r.Name),
            }).ToList();

            ViewBag.RiskIdentificationId = new SelectList(
                riskIdentificationList,
                "Id",
                "DisplayText"
            );
            return View(control);
        }


        public async Task<IActionResult> View(int id, int? riskIdentificationId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            if (riskIdentificationId.HasValue)
            {
                var controls = await _controlRepository.FindByAsync(x => x.RiskIdentificationId == riskIdentificationId);

                if (controls != null)
                {
                    var controlTypes = await _controlTypeRepository.ListAsync(
               new Expression<Func<ControlType, bool>>[] { u => u.IsDeleted == false },
               q => q.OrderBy(u => u.Name),
               null);

                    var controlProcesses = await _controlProcessRepository.ListAsync(
                        new Expression<Func<ControlProcess, bool>>[] { u => u.IsDeleted == false },
                        q => q.OrderBy(u => u.Name),
                        null);

                    var controlFrequencies = await _controlFrequencyRepository.ListAsync(
                        new Expression<Func<ControlFrequency, bool>>[] { u => u.IsDeleted == false },
                        q => q.OrderBy(u => u.Name),
                        null);

                    var controlEffectivenesss = await _controlEffectivenessRepository.ListAsync(
                      new Expression<Func<ControlEffectiveness, bool>>[] { u => u.IsDeleted == false },
                       q => q.OrderBy(u => u.Rate),
                       null);

                    var riskIdentifications = await _riskIdentificationRepository.ListAsync(
                        new Expression<Func<RiskIdentification, bool>>[] { u => u.IsDeleted == false },
                        q => q.OrderBy(u => u.Name),
                        null);

                    ViewBag.ControlTypeId = new SelectList(controlTypes, "Id", "Name", controls.ControlTypeId);
                    ViewBag.ControlProcessId = new SelectList(controlProcesses, "Id", "Name", controls.ControlProcessId);
                    ViewBag.ControlFrequencyId = new SelectList(controlFrequencies, "Id", "Name", controls.ControlFrequencyId);
                    ViewBag.ControlEffectivenessId = new SelectList(controlEffectivenesss.Select(r => new { Id = r.Id, Name = $"{r.Rate} - {r.Name}" }), "Id", "Name", controls.ControlEffectivenessId);
                    var riskIdentificationLists = riskIdentifications.Select(r => new
                    {
                        Id = r.Id,
                        DisplayText = RemoveHtmlTags(r.Name),
                    }).ToList();

                    ViewBag.RiskIdentificationId = new SelectList(
                        riskIdentificationLists,
                        "Id",
                        "DisplayText"
                    );
                    return View(controls);

                }
            }

            var control = await _controlRepository.FindByAsync(id);

            var controlType = await _controlTypeRepository.ListAsync(
                new Expression<Func<ControlType, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            var controlProcess = await _controlProcessRepository.ListAsync(
                new Expression<Func<ControlProcess, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            var controlFrequency = await _controlFrequencyRepository.ListAsync(
                new Expression<Func<ControlFrequency, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            var controlEffectiveness = await _controlEffectivenessRepository.ListAsync(
                new Expression<Func<ControlEffectiveness, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Rate),
                null);

            var riskIdentification = await _riskIdentificationRepository.ListAsync(
                new Expression<Func<RiskIdentification, bool>>[] { u => u.IsDeleted == false },
                q => q.OrderBy(u => u.Id),
                null);

            ViewBag.ControlTypeId = new SelectList(controlType, "Id", "Name", control.ControlTypeId);
            ViewBag.ControlProcessId = new SelectList(controlProcess, "Id", "Name", control.ControlProcessId);
            ViewBag.ControlFrequencyId = new SelectList(controlFrequency, "Id", "Name", control.ControlFrequencyId);
            ViewBag.ControlEffectivenessId = new SelectList(controlEffectiveness.Select(r => new { Id = r.Id, Name = $"{r.Rate} - {r.Name}" }), "Id", "Name", control.ControlEffectivenessId);
            ViewBag.RiskIdentificationId = new SelectList(riskIdentification, "Id", "Name", control.RiskIdentificationId);

            return View(control);
        }

        private string RemoveHtmlTags(string input)
        {
            // Replace &nbsp; with a space
            string cleanedText = Regex.Replace(input, "&nbsp;", " ");

            // Remove other HTML tags
            cleanedText = Regex.Replace(cleanedText, "<.*?>", "");

            return cleanedText;
        }

    }
}
