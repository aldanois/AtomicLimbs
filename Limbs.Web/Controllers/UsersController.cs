﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using Limbs.Web.Models;
using Limbs.Web.Services;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using Limbs.Web.Helpers;
using Limbs.Web.ViewModels;
using Microsoft.AspNet.Identity.EntityFramework;



namespace Limbs.Web.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Users
        public async Task<ActionResult> Index()
        {
            return View(await db.UserModelsT.ToListAsync());
        }
        
        // GET: Users/Create
        [Authorize(Roles = "Unassigned")]
        public ActionResult Create()
        {
            return View();
        }

        private static IEnumerable<SelectListItem> GetCountryList()
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(x => new SelectListItem
                {
                    Text = new RegionInfo(x.LCID).DisplayName,
                    Value = new RegionInfo(x.LCID).DisplayName,
                }).OrderBy(x => x.Value).DistinctBy(x => x.Value);
        }

        // POST: Users/Create
        [HttpPost]
        [Authorize(Roles = "Unassigned")]
        public async Task<ActionResult> Create(UserModel userModel)
        {
            userModel.Email = User.Identity.GetUserName();
            userModel.UserId = User.Identity.GetUserId();
            // userModel.OrderModelId = 0;

            if (ModelState.IsValid)
            {
                //   userModel.OrderModelId = new List<int>();
                //   userModel.OrderModel = new List<OrderModel>();
                var pointAddress = userModel.Country + ", " + userModel.City + ", " + userModel.Address;
                
                var point = Helpers.Geolocalization.GetPointGoogle(pointAddress).Split(',');
                var lat = Convert.ToDouble(point[0].Replace('.',','));
                var lng = Convert.ToDouble(point[1].Replace('.', ','));
                userModel.Lat = lat;
                userModel.Long = lng;

                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
                await userManager.RemoveFromRoleAsync(userModel.UserId, "Unassigned");
                await userManager.AddToRoleAsync(userModel.UserId, "Requester");


                db.UserModelsT.Add(userModel);
                await db.SaveChangesAsync();
                return RedirectToAction("UserPanel");
            }

           // ViewBag.CountryList = GetCountryList();

            return View("Create", userModel);
        }

        #region AdminActions
        
        // GET: Users/Details/5
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserModel userModel = await db.UserModelsT.FindAsync(id);
            if (userModel == null)
            {
                return HttpNotFound();
            }
            return View(userModel);
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserModel userModel = await db.UserModelsT.FindAsync(id);
            if (userModel == null)
            {
                return HttpNotFound();
            }
            return View(userModel);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Edit([Bind(Include = "Id,ResponsableName,UserName,Email,Dni,Phone,Birth,Gender,Address,ProthesisType,ProductType,AmputationType,Lat,Long")] UserModel userModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userModel).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(userModel);
        }

        // GET: Users/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userModel = await db.UserModelsT.FindAsync(id);
            if (userModel == null)
            {
                return HttpNotFound();
            }
            return View(userModel);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var userModel = await db.UserModelsT.FindAsync(id);

            if (userModel == null)
            {
                return HttpNotFound();
            }

            db.UserModelsT.Remove(userModel);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        #endregion

        [Authorize(Roles = "Requester")]
        public ActionResult UserPanel(string message)
        {
            var userId = User.Identity.GetUserId();
            var user = db.UserModelsT.Single(c => c.UserId == userId);

            var orderList = db.OrderModels.Where(c => c.OrderRequestor.UserId == userId).ToList();

            var lat = user.Lat;
            var lng = user.Long;
            //TODO (ale): porque se valida esto aca?
            var pointIsValid = Geolocalization.PointIsValid(lat, lng);

            var viewModel = new UserPanelViewModel
            {
                Order = orderList.ToList(),
                PointIsValid = pointIsValid,
                Message = message

            };

            return View(viewModel); 
     
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult GetUserImage(string url)
        {
            var client = new HttpClient();

            var file = client.GetByteArrayAsync(url);

            return new FileContentResult(file.Result, "image/jpeg");
        }
    }

}
