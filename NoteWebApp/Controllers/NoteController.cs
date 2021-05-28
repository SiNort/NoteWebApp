using NoteWebApp.Data;
using NoteWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace NoteWebApp.Controllers
{
    [Authorize]
    public class NoteController : Controller
    {
        private ApplicationDbContext db;
        private UserManager<IdentityUser> userManager;

        public NoteController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            db = context;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            var notes = await db.Notes.Where(x => x.User == user).ToListAsync();
            return View(notes);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Display(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var note = await db.Notes.FirstOrDefaultAsync(x => x.Id == id);
            if (note is null || note.User != user)
            {
                return RedirectToAction("Index");
            }
            return View(note);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Note note)
        {
            if (ModelState.IsValid)
            {
                note.User = await userManager.GetUserAsync(User);
                db.Notes.Add(note);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                return View(note);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Note note)
        {
            var user = await userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(note.UserId) || note.UserId != user.Id)
            {
                ModelState.AddModelError("User", "Authentication problems");
            }

            if (ModelState.IsValid)
            {
                db.Notes.Update(note);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                return View("Display", note);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Note note)
        {
            var user = await userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(note.UserId) || note.UserId != user.Id)
            {
                ModelState.AddModelError("User", "Authentication problems");
            }

            db.Notes.Remove(note);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
