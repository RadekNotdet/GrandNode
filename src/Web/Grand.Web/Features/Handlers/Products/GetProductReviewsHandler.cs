﻿using Grand.Business.Core.Extensions;
using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Domain.Catalog;
using Grand.Domain.Customers;
using Grand.Web.Common.Security.Captcha;
using Grand.Web.Features.Models.Products;
using Grand.Web.Models.Catalog;
using MediatR;

namespace Grand.Web.Features.Handlers.Products;

public class GetProductReviewsHandler : IRequestHandler<GetProductReviews, ProductReviewsModel>
{
    private readonly CaptchaSettings _captchaSettings;
    private readonly CatalogSettings _catalogSettings;
    private readonly ICustomerService _customerService;
    private readonly CustomerSettings _customerSettings;
    private readonly IDateTimeService _dateTimeService;
    private readonly IGroupService _groupService;
    private readonly IProductReviewService _productReviewService;

    public GetProductReviewsHandler(
        ICustomerService customerService,
        IProductReviewService productReviewService,
        IDateTimeService dateTimeService,
        IGroupService groupService,
        CatalogSettings catalogSettings,
        CustomerSettings customerSettings,
        CaptchaSettings captchaSettings)
    {
        _customerService = customerService;
        _productReviewService = productReviewService;
        _dateTimeService = dateTimeService;
        _groupService = groupService;
        _catalogSettings = catalogSettings;
        _customerSettings = customerSettings;
        _captchaSettings = captchaSettings;
    }

    public async Task<ProductReviewsModel> Handle(GetProductReviews request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Product);

        var model = new ProductReviewsModel {
            ProductId = request.Product.Id,
            ProductName = request.Product.GetTranslation(x => x.Name, request.Language.Id),
            ProductSeName = request.Product.GetSeName(request.Language.Id)
        };
        var productReviews =
            await _productReviewService.GetAllProductReviews("", true, null, null, "", "", request.Product.Id, 0,
                request.Size);
        foreach (var pr in productReviews)
        {
            var customer = await _customerService.GetCustomerById(pr.CustomerId);
            model.Items.Add(new ProductReviewModel {
                Id = pr.Id,
                CustomerId = pr.CustomerId,
                CustomerName = customer?.FormatUserName(_customerSettings.CustomerNameFormat),
                Title = pr.Title,
                ReviewText = pr.ReviewText,
                ReplyText = pr.ReplyText,
                Signature = pr.Signature,
                ConfirmedPurchase = pr.ConfirmedPurchase,
                Rating = pr.Rating,
                Helpfulness = new ProductReviewHelpfulnessModel {
                    ProductId = request.Product.Id,
                    ProductReviewId = pr.Id,
                    HelpfulYesTotal = pr.HelpfulYesTotal,
                    HelpfulNoTotal = pr.HelpfulNoTotal
                },
                WrittenOnStr = _dateTimeService.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g")
            });
        }

        model.AddProductReview.CanCurrentCustomerLeaveReview = _catalogSettings.AllowAnonymousUsersToReviewProduct ||
                                                               !await _groupService.IsGuest(request.Customer);
        model.AddProductReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage;

        return model;
    }
}