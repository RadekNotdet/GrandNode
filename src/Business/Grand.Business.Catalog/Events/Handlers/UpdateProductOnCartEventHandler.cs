﻿using Grand.Business.Core.Events.Catalog;
using Grand.Data;
using Grand.Domain.Customers;
using MediatR;

namespace Grand.Business.Catalog.Events.Handlers;

public class UpdateProductOnCartEventHandler : INotificationHandler<UpdateProductOnCartEvent>
{
    private readonly IRepository<Customer> _customerRepository;

    public UpdateProductOnCartEventHandler(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task Handle(UpdateProductOnCartEvent notification, CancellationToken cancellationToken)
    {
        var customers = _customerRepository.Table
            .Where(x => x.ShoppingCartItems.Any(y => y.ProductId == notification.Product.Id)).ToList();
        foreach (var (cs, item) in from cs in customers
                                   from item in cs.ShoppingCartItems.Where(x => x.ProductId == notification.Product.Id)
                                   select (cs, item))
        {
            item.AdditionalShippingChargeProduct = notification.Product.AdditionalShippingCharge;
            item.IsFreeShipping = notification.Product.IsFreeShipping;
            item.IsGiftVoucher = notification.Product.IsGiftVoucher;
            item.IsShipEnabled = notification.Product.IsShipEnabled;
            item.IsTaxExempt = notification.Product.IsTaxExempt;
            await _customerRepository.UpdateToSet(cs.Id, x => x.ShoppingCartItems, z => z.Id, item.Id, item);
        }
    }
}