﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace Limbs.Web.Entities.Models
{
    public class OrderModel
    {
        public OrderModel()
        {
            DeliveryCourier = Courier.NoCourier;
            Log = new List<OrderLogItem>();
            Pieces = new Pieces();
        }

        [Key]
        public int Id { get; set; }

        [Display(Name = "Creado", Description = "")]
        public DateTime Date { get; set; }

        /// <summary>
        ///     Usuario que solicita la orden de protesis
        /// </summary>
        public UserModel OrderRequestor { get; set; }

        /// <summary>
        ///     Embajador que procesara la orden
        /// </summary>
        public virtual AmbassadorModel OrderAmbassador { get; set; }

        public OrderColor Color { get; set; }

        [Display(Name = "Cuál", Description = "")]
        [Required(ErrorMessage = "Campo requerido")]
        public ProductType ProductType { get; set; }

        public virtual ICollection<AccessoryModel> Extras { get; set; }

        [NotMapped]
        public virtual OrderSizesModel Sizes { get; set; }

        public string SizesData
        {
            get => JsonConvert.SerializeObject(Sizes);
            set => Sizes = value != null
                ? JsonConvert.DeserializeObject<OrderSizesModel>(value)
                : new OrderSizesModel();
        }

        [Display(Name = "Amputación", Description = "")]
        [Required(ErrorMessage = "Campo requerido")]
        public AmputationType AmputationType { get; set; }

        [Display(Name = "Comentarios", Description = "")]
        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }

        public string IdImage { get; set; }

        [Display(Name = "Estado", Description = "")]
        public OrderStatus Status { get; set; }

        [Display(Name = "Última modificación", Description = "")]
        public DateTime StatusLastUpdated { get; set; }

        [Display(Name = "Prueba de entrega", Description = "")]
        public string ProofOfDelivery { get; set; }

        [Display(Name = "Courier", Description = "")]
        public Courier DeliveryCourier { get; set; }

        [Display(Name = "Tracking", Description = "")]
        public string DeliveryTrackingCode { get; set; }

        [Display(Name = "Etiqueta postal", Description = "")]
        public string DeliveryPostalLabel { get; set; }

        [NotMapped]
        [Display(Name = "Historial de cambios", Description = "")]
        public List<OrderLogItem> Log { get; private set; }

        public virtual ICollection<MessageModel> Messages { get; set; }

        public string OrderLog
        {
            get => JsonConvert.SerializeObject(Log);
            set => Log = value != null
                ? JsonConvert.DeserializeObject<List<OrderLogItem>>(value)
                : new List<OrderLogItem>();
        }

        public void LogMessage(IPrincipal user, string message)
        {
            Log.Add(new OrderLogItem
            {
                User = user.Identity.GetUserName(),
                Message = message
            });
        }

        public Pieces Pieces { get; set; }

    }

    public class Pieces
    {
        public bool AtomicLabCover { get; set; }
        public bool FingerMechanismHolder { get; set; }
        public bool Fingers { get; set; }
        public bool FingerStopper { get; set; }
        public bool FingersX1 { get; set; }
        public bool FingersX2P { get; set; }
        public bool Palm { get; set; }
        public bool ThumbConnector { get; set; }
        public bool ThumbThumbClip { get; set; }
        public bool ThumbScrew { get; set; }
        public bool UpperArm_FingerConnector { get; set; }
        public bool UpperArm_PalmConnector { get; set; }
        public bool UpperArm_ThumbShortConnector { get; set; }
        public bool UpperArm_FingerSlider { get; set; }
        public bool UpperArm { get; set; }
    }

    public class OrderLogItem
    {
        public OrderLogItem()
        {
            Date = DateTime.UtcNow;
        }

        public DateTime Date { get; set; }

        public string User { get; set; }

        public string Message { get; set; }
    }

    public enum OrderStatus
    {
        [Description("No asignado")] NotAssigned,
        [Description("Pre-asignado")] PreAssigned,
        [Description("Imprimiendo")] Pending,
        [Description("Lista")] Ready,
        [Description("Entregada")] Delivered
    }

    public enum Courier
    {
        [Description("Sin envio")] NoCourier,
        [Description("Andreani")] Andreani
    }

    public enum OrderColor
    {
        [Description("Blanco y rojo")] A,
        [Description("Azul y rojo")] B,
        [Description("Rosa y blanco")] C,
        [Description("Azul y amarillo")] D,
        [Description("Azul blanco y rojo")] E,
        [Description("Rojo y amarillo")] F
    }

    public enum ProductType
    {
        [Description("Derecha")] Right,
        [Description("Izquierda")] Left
    }

    

}