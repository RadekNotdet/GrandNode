﻿using Grand.Business.Core.Commands.Checkout.Shipping;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Checkout.Shipping;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Common.Pdf;
using Grand.Domain.Permissions;
using Grand.Domain.Orders;
using Grand.Domain.Shipping;
using Grand.Infrastructure;
using Grand.Web.Admin.Interfaces;
using Grand.Web.Admin.Models.Orders;
using Grand.Web.Common.DataSource;
using Grand.Web.Common.Extensions;
using Grand.Web.Common.Filters;
using Grand.Web.Common.Security.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shipping.DHL.CQRS.Commands.CreateDelivery;

namespace Grand.Web.Admin.Controllers;

[PermissionAuthorize(PermissionSystemName.Shipments)]
public class ShipmentController : BaseAdminController
{
    public ShipmentController(
        IShipmentViewModelService shipmentViewModelService,
        IOrderService orderService,
        ITranslationService translationService,
        IContextAccessor contextAccessor,
        IGroupService groupService,
        IPdfService pdfService,
        IShipmentService shipmentService,
        IDateTimeService dateTimeService,
        IMediator mediator)
    {
        _shipmentViewModelService = shipmentViewModelService;
        _orderService = orderService;
        _translationService = translationService;
        _contextAccessor = contextAccessor;
        _groupService = groupService;
        _pdfService = pdfService;
        _shipmentService = shipmentService;
        _dateTimeService = dateTimeService;
        _mediator = mediator;
    }

    #region Fields

    private readonly IShipmentViewModelService _shipmentViewModelService;
    private readonly IOrderService _orderService;
    private readonly ITranslationService _translationService;
    private readonly IContextAccessor _contextAccessor;
    private readonly IGroupService _groupService;
    private readonly IPdfService _pdfService;
    private readonly IShipmentService _shipmentService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IMediator _mediator;

    #endregion

    #region Shipments

    public async Task<IActionResult> List()
    {
        var model = await _shipmentViewModelService.PrepareShipmentListModel();
        return View(model);
    }

    [PermissionAuthorizeAction(PermissionActionName.List)]
    [HttpPost]
    public async Task<IActionResult> ShipmentListSelect(DataSourceRequest command, ShipmentListModel model)
    {
        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer))
            model.StoreId = _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId;

        var shipments = await _shipmentViewModelService.PrepareShipments(model, command.Page, command.PageSize);
        var items = new List<ShipmentModel>();
        foreach (var item in shipments.shipments)
            items.Add(await _shipmentViewModelService.PrepareShipmentModel(item, false));

        var gridModel = new DataSourceResult {
            Data = items,
            Total = shipments.totalCount
        };
        return Json(gridModel);
    }

    [PermissionAuthorizeAction(PermissionActionName.List)]
    [HttpPost]
    public async Task<IActionResult> ShipmentsByOrder(string orderId, DataSourceRequest command)
    {
        var order = await _orderService.GetOrderById(orderId);
        if (order == null)
            throw new ArgumentException("No order found with the specified id");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            order.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return Content("");

        //shipments
        var shipmentModels = new List<ShipmentModel>();
        var shipments = (await _shipmentService.GetShipmentsByOrder(orderId))
            .OrderBy(s => s.CreatedOnUtc)
            .ToList();
        foreach (var shipment in shipments)
            shipmentModels.Add(await _shipmentViewModelService.PrepareShipmentModel(shipment, false));

        var gridModel = new DataSourceResult {
            Data = shipmentModels,
            Total = shipmentModels.Count
        };
        return Json(gridModel);
    }

    [PermissionAuthorizeAction(PermissionActionName.List)]
    [HttpPost]
    public async Task<IActionResult> ShipmentsItemsByShipmentId(string shipmentId, DataSourceRequest command)
    {
        var shipment = await _shipmentService.GetShipmentById(shipmentId);
        if (shipment == null)
            throw new ArgumentException("No shipment found with the specified id");

        var order = await _orderService.GetOrderById(shipment.OrderId);
        if (order == null)
            throw new ArgumentException("No order found with the specified id");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return Content("");

        //shipments
        var shipmentModel = await _shipmentViewModelService.PrepareShipmentModel(shipment, true);
        var gridModel = new DataSourceResult {
            Data = shipmentModel.Items,
            Total = shipmentModel.Items.Count
        };

        return Json(gridModel);
    }

    [PermissionAuthorizeAction(PermissionActionName.Create)]
    public async Task<IActionResult> AddShipment(string orderId)
    {
        var order = await _orderService.GetOrderById(orderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            order.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var model = await _shipmentViewModelService.PrepareShipmentModel(order);

        return View(model);
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    [ArgumentNameFilter(KeyName = "save-continue", Argument = "continueEditing")]
    public async Task<IActionResult> AddShipment(AddShipmentModel model, bool continueEditing)
    {
        var order = await _orderService.GetOrderById(model.OrderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");

        

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            order.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var orderItems = order.OrderItems;

        var sh = await _shipmentViewModelService.PrepareShipment(order, orderItems.ToList(), model);
        if (sh.shipment == null)
            return RedirectToAction("AddShipment", new { orderId = model.OrderId });

        var shipment = sh.shipment;
        //check stock
        var (valid, message) = await _shipmentViewModelService.ValidStockShipment(shipment);
        if (!valid)
        {
            Error(message);
            return RedirectToAction("AddShipment", new { orderId = model.OrderId });
        }

        //if we have at least one item in the shipment, then save it
        if (shipment.ShipmentItems.Count > 0)
        {
            shipment.TotalWeight = sh.totalWeight;
          
            await _shipmentService.InsertShipment(shipment);



            //add a note
            await _orderService.InsertOrderNote(new OrderNote {
                Note = $"A shipment #{shipment.ShipmentNumber} has been added",
                DisplayToCustomer = false,
                OrderId = order.Id
            });

            Success(_translationService.GetResource("Admin.Orders.Shipments.Added"));
            return continueEditing
                ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                : RedirectToAction("List", new { id = shipment.Id });
        }

        Error(_translationService.GetResource("Admin.Orders.Shipments.NoProductsSelected"));
        return RedirectToAction("AddShipment", new { orderId = model.OrderId });
    }

    [PermissionAuthorizeAction(PermissionActionName.Preview)]
    public async Task<IActionResult> ShipmentDetails(string id)
    {
        var shipment = await _shipmentService.GetShipmentById(id);
        if (shipment == null)
            //No shipment found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var orderId = shipment.OrderId;
        var order = await _orderService.GetOrderById(orderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");

        var model = await _shipmentViewModelService.PrepareShipmentModel(shipment, true, true);
        return View(model);
    }

    [PermissionAuthorizeAction(PermissionActionName.Delete)]
    [HttpPost]
    public async Task<IActionResult> DeleteShipment(string id)
    {
        var shipment = await _shipmentService.GetShipmentById(id);
        if (shipment == null)
            //No shipment found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var orderId = shipment.OrderId;
        var order = await _orderService.GetOrderById(orderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");

        //delete shipment
        await _shipmentService.DeleteShipment(shipment);

        //add a note
        await _orderService.InsertOrderNote(new OrderNote {
            Note = $"A shipment #{shipment.ShipmentNumber} has been deleted",
            DisplayToCustomer = false,
            OrderId = order.Id
        });

        Success(_translationService.GetResource("Admin.Orders.Shipments.Deleted"));

        return RedirectToAction("Edit", "Order", new { order.Id });
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    public async Task<IActionResult> SetTrackingNumber(ShipmentModel model)
    {
        var shipment = await _shipmentService.GetShipmentById(model.Id);
        if (shipment == null)
            //No shipment found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var order = await _orderService.GetOrderById(shipment.OrderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");

        shipment.TrackingNumber = model.TrackingNumber;
        await _shipmentService.UpdateShipment(shipment);

        return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    public async Task<IActionResult> SetShipmentAdminComment(ShipmentModel model)
    {
        var shipment = await _shipmentService.GetShipmentById(model.Id);
        if (shipment == null)
            //No shipment found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var order = await _orderService.GetOrderById(shipment.OrderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");

        shipment.AdminComment = model.AdminComment;
        await _shipmentService.UpdateShipment(shipment);

        return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    public async Task<IActionResult> SetAsShipped(string id)
    {
        var shipment = await _shipmentService.GetShipmentById(id);
        if (shipment == null)
            //No shipment found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var order = await _orderService.GetOrderById(shipment.OrderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");
        try
        {
            await _mediator.Send(new ShipCommand { Shipment = shipment, NotifyCustomer = true });
            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }
        catch (Exception exc)
        {
            //error
            Error(exc);
            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    public async Task<IActionResult> EditShippedDate(ShipmentModel model)
    {
        var shipment = await _shipmentService.GetShipmentById(model.Id);
        if (shipment == null)
            //No shipment found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var order = await _orderService.GetOrderById(shipment.OrderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");
        try
        {
            if (!model.ShippedDate.HasValue) throw new Exception("Enter shipped date");

            shipment.ShippedDateUtc = model.ShippedDate.ConvertToUtcTime(_dateTimeService);
            await _shipmentService.UpdateShipment(shipment);
            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }
        catch (Exception exc)
        {
            //error
            Error(exc);
            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    public async Task<IActionResult> SetAsDelivered(string id)
    {
        var shipment = await _shipmentService.GetShipmentById(id);
        if (shipment == null)
            //No shipment found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var order = await _orderService.GetOrderById(shipment.OrderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");

        try
        {
            await _mediator.Send(new DeliveryCommand { Shipment = shipment, NotifyCustomer = true });
            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }
        catch (Exception exc)
        {
            //error
            Error(exc);
            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }
    }


    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    public async Task<IActionResult> EditDeliveryDate(ShipmentModel model)
    {
        var shipment = await _shipmentService.GetShipmentById(model.Id);
        if (shipment == null)
            //No shipment found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var order = await _orderService.GetOrderById(shipment.OrderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");

        try
        {
            if (!model.DeliveryDate.HasValue) throw new Exception("Enter delivery date");

            shipment.DeliveryDateUtc = model.DeliveryDate.ConvertToUtcTime(_dateTimeService);
            await _shipmentService.UpdateShipment(shipment);
            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }
        catch (Exception exc)
        {
            //error
            Error(exc);
            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    public async Task<IActionResult> EditUserFields(string id, ShipmentModel model)
    {
        var shipment = await _shipmentService.GetShipmentById(id);
        if (shipment == null)
            //No order found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });

        shipment.UserFields = model.UserFields;
        await _shipmentService.UpdateShipment(shipment);

        //selected tab
        await SaveSelectedTabIndex();

        return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
    }

    [PermissionAuthorizeAction(PermissionActionName.Preview)]
    public async Task<IActionResult> PdfPackagingSlip(string shipmentId)
    {
        var shipment = await _shipmentService.GetShipmentById(shipmentId);
        if (shipment == null)
            //no shipment found with the specified id
            return RedirectToAction("List");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return RedirectToAction("List");

        var order = await _orderService.GetOrderById(shipment.OrderId);
        if (order == null)
            //No order found with the specified id
            return RedirectToAction("List");

        var shipments = new List<Shipment> {
            shipment
        };

        byte[] bytes;
        using (var stream = new MemoryStream())
        {
            await _pdfService.PrintPackagingSlipsToPdf(stream, shipments, _contextAccessor.WorkContext.WorkingLanguage.Id);
            bytes = stream.ToArray();
        }

        return File(bytes, "application/pdf", $"packagingslip_{shipment.Id}.pdf");
    }

    [PermissionAuthorizeAction(PermissionActionName.Export)]
    [HttpPost]
    public async Task<IActionResult> PdfPackagingSlipAll(ShipmentListModel model)
    {
        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer))
            model.StoreId = _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId;

        //load shipments
        var shipments = await _shipmentViewModelService.PrepareShipments(model, 1, 100);

        //ensure that we at least one shipment selected
        if (shipments.totalCount == 0)
        {
            Error(_translationService.GetResource("Admin.Orders.Shipments.NoShipmentsSelected"));
            return RedirectToAction("List");
        }

        byte[] bytes;
        using (var stream = new MemoryStream())
        {
            await _pdfService.PrintPackagingSlipsToPdf(stream, shipments.shipments.ToList(),
                _contextAccessor.WorkContext.WorkingLanguage.Id);
            bytes = stream.ToArray();
        }

        return File(bytes, "application/pdf", "packagingslips.pdf");
    }

    [PermissionAuthorizeAction(PermissionActionName.Export)]
    [HttpPost]
    public async Task<IActionResult> PdfPackagingSlipSelected(string selectedIds)
    {
        var shipments = new List<Shipment>();
        var shipments_access = new List<Shipment>();
        if (selectedIds != null)
        {
            var ids = selectedIds
                .Split([','], StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x)
                .ToArray();
            shipments.AddRange(await _shipmentService.GetShipmentsByIds(ids));
        }

        var storeId = "";
        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer))
            storeId = _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId;

        shipments_access = shipments.Where(x => x.StoreId == storeId || string.IsNullOrEmpty(storeId)).ToList();
        //ensure that we at least one shipment selected
        if (shipments.Count == 0)
        {
            Error(_translationService.GetResource("Admin.Orders.Shipments.NoShipmentsSelected"));
            return RedirectToAction("List");
        }

        byte[] bytes;
        using (var stream = new MemoryStream())
        {
            await _pdfService.PrintPackagingSlipsToPdf(stream, shipments_access, _contextAccessor.WorkContext.WorkingLanguage.Id);
            bytes = stream.ToArray();
        }

        return File(bytes, "application/pdf", "packagingslips.pdf");
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    public async Task<IActionResult> SetAsShippedSelected(ICollection<string> selectedIds)
    {
        var shipments = new List<Shipment>();
        var shipments_access = new List<Shipment>();
        if (selectedIds != null) shipments.AddRange(await _shipmentService.GetShipmentsByIds(selectedIds.ToArray()));

        var storeId = "";
        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer))
            storeId = _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId;

        shipments_access = shipments.Where(x => x.StoreId == storeId || string.IsNullOrEmpty(storeId)).ToList();
        foreach (var shipment in shipments_access)
            try
            {
                var order = await _orderService.GetOrderById(shipment.OrderId);
                await _mediator.Send(new ShipCommand { Shipment = shipment, NotifyCustomer = true });
            }
            catch
            {
                //ignore any exception
            }

        return Json(new { Result = true });
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    [HttpPost]
    public async Task<IActionResult> SetAsDeliveredSelected(ICollection<string> selectedIds)
    {
        var shipments = new List<Shipment>();
        var shipments_access = new List<Shipment>();
        if (selectedIds != null) shipments.AddRange(await _shipmentService.GetShipmentsByIds(selectedIds.ToArray()));

        var storeId = "";
        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer))
            storeId = _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId;
        shipments_access = shipments.Where(x => x.StoreId == storeId || string.IsNullOrEmpty(storeId)).ToList();
        foreach (var shipment in shipments_access)
            try
            {
                await _mediator.Send(new DeliveryCommand { Shipment = shipment, NotifyCustomer = true });
            }
            catch
            {
                //ignore any exception
            }

        return Json(new { Result = true });
    }
    #region Shipment notes

    [PermissionAuthorizeAction(PermissionActionName.Preview)]
    [HttpPost]
    public async Task<IActionResult> ShipmentNotesSelect(string shipmentId, DataSourceRequest command)
    {
        var shipment = await _shipmentService.GetShipmentById(shipmentId);
        if (shipment == null)
            throw new ArgumentException("No shipment found with the specified id");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return Content("");

        //shipment notes
        var shipmentNoteModels = await _shipmentViewModelService.PrepareShipmentNotes(shipment);
        var gridModel = new DataSourceResult {
            Data = shipmentNoteModels,
            Total = shipmentNoteModels.Count
        };
        return Json(gridModel);
    }

    [PermissionAuthorizeAction(PermissionActionName.Edit)]
    public async Task<IActionResult> ShipmentNoteAdd(string shipmentId, string downloadId, bool displayToCustomer,
        string message)
    {
        var shipment = await _shipmentService.GetShipmentById(shipmentId);
        if (shipment == null)
            return Json(new { Result = false });

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return Json(new { Result = false });

        await _shipmentViewModelService.InsertShipmentNote(shipment, downloadId, displayToCustomer, message);

        return Json(new { Result = true });
    }

    [PermissionAuthorizeAction(PermissionActionName.Delete)]
    [HttpPost]
    public async Task<IActionResult> ShipmentNoteDelete(string id, string shipmentId)
    {
        var shipment = await _shipmentService.GetShipmentById(shipmentId);
        if (shipment == null)
            throw new ArgumentException("No shipment found with the specified id");

        if (await _groupService.IsStaff(_contextAccessor.WorkContext.CurrentCustomer) &&
            shipment.StoreId != _contextAccessor.WorkContext.CurrentCustomer.StaffStoreId)
            return Json(new { Result = false });

        await _shipmentViewModelService.DeleteShipmentNote(shipment, id);

        return new JsonResult("");
    }

    #endregion

    #endregion
}