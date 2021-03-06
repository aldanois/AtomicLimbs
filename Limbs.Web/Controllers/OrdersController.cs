﻿using Limbs.Web.Common.Extensions;
using Limbs.Web.Entities.Models;
using Limbs.Web.Repositories.Interfaces;
using Limbs.Web.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Limbs.Web.Controllers
{
    [DefaultAuthorize(Roles = AppRoles.Requester)]
    public class OrdersController : BaseController
    {
        private readonly IUserFiles _userFiles;
        private readonly IOrderNotificationService _ns;

        public OrdersController(IUserFiles userFiles, IOrderNotificationService notificationService)
        {
            _userFiles = userFiles;
            _ns = notificationService;
        }

        // GET: Orders/Index
        public ActionResult Index()
        {
            return RedirectToActionPermanent("Index", "Home");
        }

        // GET: Orders/Details/5
        [OverrideAuthorize(Roles = AppRoles.User + ", " + AppRoles.Administrator)]
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var orderModel = await Db.OrderModels.Include(x => x.OrderRequestor).FirstOrDefaultAsync(x => x.Id == id);
            if (orderModel == null)
            {
                return HttpNotFound();
            }
            if (!orderModel.CanView(User))
            {
                return RedirectToAction("RedirectUser", "Account");
            }
            return View(orderModel);
        }
        
        // GET: Orders/ManoPedir
        public ActionResult ManoPedir()
        {
            return View("ManoPedir", new OrderModel());
        }


        // POST: Orders/ManoPedir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManoPedir(OrderModel orderModel)
        {
            TempData["AmputationType"] = orderModel.AmputationType;
            TempData["ProductType"] = orderModel.ProductType;

            return RedirectToAction("ManoImagen");
        }

        // GET: Orders/ManoImagen
        public ActionResult ManoImagen()
        {
            var amputationType =  TempData["AmputationType"];
            var productType = TempData["ProductType"];
            if (amputationType == null || productType == null)
            {
                return RedirectToAction("ManoPedir");
            }

            return View("ManoImagen", new OrderModel
            {
                AmputationType = (AmputationType)amputationType,
                ProductType = (ProductType)productType,
            });
        }
        
        // POST: Orders/UploadImageUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadImageUser(OrderModel orderModel, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("nofile", @"Seleccione una foto.");
            }
            else
            {
                if (file.ContentLength > 1000000 * 5)
                {
                    ModelState.AddModelError("bigfile", @"La foto elegida es muy grande (max = 5 MB).");
                }
                if (!file.IsImage())
                {
                    ModelState.AddModelError("noimage", @"El archivo seleccionado no es una imagen.");
                }
            }
            if (!ModelState.IsValid) return View("ManoImagen", orderModel);

            var fileName = Guid.NewGuid().ToString("N") + ".jpg";
            var fileUrl = _userFiles.UploadOrderFile(file?.InputStream, fileName);

            TempData["fileUrl"] = fileUrl.AbsoluteUri;
            TempData["AmputationType"] = orderModel.AmputationType;
            TempData["ProductType"] = orderModel.ProductType;

            return Json(new { Action = "ManoMedidas" });
                //return RedirectToAction("ManoMedidas");
            }

        // GET: Orders/ManoMedidas
        public ActionResult ManoMedidas()
        {
            if (TempData["fileUrl"] == null)
            {
                return RedirectToAction("ManoPedir", "Orders");
            }
            
            return View("ManoMedidas", new OrderModel
            {
                IdImage = TempData["fileUrl"].ToString(),
                AmputationType = (AmputationType)TempData["AmputationType"],
                ProductType = (ProductType)TempData["ProductType"],
            });
        }

        // POST: Orders/ManoOrden
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManoOrden(OrderModel orderModel)
        {
            ModelState.Clear();
            if (string.IsNullOrWhiteSpace(orderModel.IdImage))
                ModelState.AddModelError("noimage", @"Error desconocido, vuelva a comenzar.");
            if (orderModel.Sizes.A <= 0 || orderModel.Sizes.B <= 0 || orderModel.Sizes.C <= 0)
                ModelState.AddModelError("nodistance", @"Seleccione las medidas.");

            if (!ModelState.IsValid) return View("ManoMedidas", orderModel);

            return View("ManoOrden", orderModel);
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(OrderModel orderModel)
        {
            if (!ModelState.IsValid) return View("ManoOrden", orderModel);
            
            var currentUserId = User.Identity.GetUserId();
            var userModel = await Db.UserModelsT.Where(c => c.UserId == currentUserId).SingleAsync();

            orderModel.OrderRequestor = userModel;
            orderModel.Status = OrderStatus.NotAssigned;
            orderModel.StatusLastUpdated = DateTime.UtcNow;
            orderModel.Date = DateTime.UtcNow;
            orderModel.LogMessage(User, "New order");

            Db.OrderModels.Add(orderModel);
            await Db.SaveChangesAsync();

            return RedirectToAction("Index", "Users");
        }

        // GET: Orders/GetUserImage
        [OverrideAuthorize(Roles = AppRoles.User + ", " + AppRoles.Administrator)]
        public ActionResult GetUserImage(string url)
        {
            var client = new HttpClient();

            var file = client.GetByteArrayAsync(url);

            return new FileContentResult(file.Result, "image/jpeg");
        }


        // POST: Orders/UploadProofOfDelivery
        [HttpPost]
        [OverrideAuthorize(Roles = AppRoles.User + ", " + AppRoles.Administrator)]
        public async Task<ActionResult> UploadProofOfDelivery(HttpPostedFileBase file, int orderId)
        {
            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("nofile", @"Seleccione una foto.");
            }
            else
            {
                if (file.ContentLength > 1000000 * 5)
                {
                    ModelState.AddModelError("bigfile", @"La foto elegida es muy grande (max = 5 MB).");
                }
                if (!file.IsImage())
                {
                    ModelState.AddModelError("noimage", @"El archivo seleccionado no es una imagen.");
                }
            }
            var orderModel = await Db.OrderModels.Include(x => x.OrderRequestor).FirstOrDefaultAsync(x => x.Id == orderId);
            if (orderModel == null) return new HttpStatusCodeResult(HttpStatusCode.Conflict);
            if (!orderModel.CanView(User))
            {
                return RedirectToAction("RedirectUser", "Account");
            }
            if (!ModelState.IsValid) return View("Details", orderModel);


            var fileName = Guid.NewGuid().ToString("N") + ".jpg";
            var fileUrl = _userFiles.UploadOrderFile(file?.InputStream, fileName);

            orderModel.ProofOfDelivery = fileUrl.AbsoluteUri;
            orderModel.LogMessage(User, "New proof of delivery at: " + fileUrl.AbsoluteUri);
            Db.SaveChanges();

            await _ns.SendProofOfDeliveryNotification(orderModel);

            return Redirect(Request.UrlReferrer?.PathAndQuery);
        }

        [HttpPost]
        [OverrideAuthorize(Roles = AppRoles.Ambassador + ", " + AppRoles.Administrator)]
        public async Task<ActionResult> PrintedPiecesUpdate(Pieces pieces, int orderId)
        {
            var order = await Db.OrderModels.FirstOrDefaultAsync(x => x.Id == orderId);
            if(!order.CanView(User)) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            order.Pieces = pieces;
            order.StatusLastUpdated = DateTime.UtcNow;
            Db.OrderModels.AddOrUpdate(order);
            await Db.SaveChangesAsync();

            if (Request.IsAjaxRequest())
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            return RedirectToAction("Details", "Orders", new { id = orderId });
        }
        
        [OverrideAuthorize(Roles = AppRoles.Ambassador + ", " + AppRoles.Administrator)]
        public async Task<ActionResult> GetPartial(int orderId, string partialName)
        {
            var order = await Db.OrderModels.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order.CanView(User))
            {
                return PartialView("Details/_" + partialName, order);
            }
            return new HttpUnauthorizedResult();
        }
        
        [AllowAnonymous]
        public ActionResult PublicOrders()
        {
            var orders = Db.OrderModels.Include(c => c.OrderRequestor).Include(c => c.OrderAmbassador).OrderByDescending(x => x.Date).ToList();
            return View(orders);
        }
    }
}