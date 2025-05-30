﻿using Grand.Business.Core.Extensions;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Domain.Customers;
using Grand.Domain.Vendors;
using Grand.Infrastructure;
using Grand.Web.Common.Security.Captcha;
using Grand.Web.Features.Models.Vendors;
using Grand.Web.Models.Vendors;
using MediatR;

namespace Grand.Web.Features.Handlers.Vendors;

public class GetVendorReviewsHandler : IRequestHandler<GetVendorReviews, VendorReviewsModel>
{
    private readonly CaptchaSettings _captchaSettings;
    private readonly ICustomerService _customerService;
    private readonly CustomerSettings _customerSettings;
    private readonly IDateTimeService _dateTimeService;
    private readonly IGroupService _groupService;
    private readonly IVendorService _vendorService;
    private readonly VendorSettings _vendorSettings;
    private readonly IContextAccessor _contextAccessor;

    public GetVendorReviewsHandler(
        IContextAccessor contextAccessor,
        IVendorService vendorService,
        ICustomerService customerService,
        IDateTimeService dateTimeService,
        IGroupService groupService,
        CustomerSettings customerSettings,
        VendorSettings vendorSettings,
        CaptchaSettings captchaSettings)
    {
        _contextAccessor = contextAccessor;
        _vendorService = vendorService;
        _customerService = customerService;
        _dateTimeService = dateTimeService;
        _groupService = groupService;
        _customerSettings = customerSettings;
        _vendorSettings = vendorSettings;
        _captchaSettings = captchaSettings;
    }

    public async Task<VendorReviewsModel> Handle(GetVendorReviews request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request.Vendor);

        var model = new VendorReviewsModel {
            VendorId = request.Vendor.Id,
            VendorName = request.Vendor.GetTranslation(x => x.Name, _contextAccessor.WorkContext.WorkingLanguage.Id),
            VendorSeName = request.Vendor.GetSeName(_contextAccessor.WorkContext.WorkingLanguage.Id)
        };

        var vendorReviews = await _vendorService.GetAllVendorReviews("", true, null, null, "", request.Vendor.Id, 0,
            _vendorSettings.NumberOfReview);

        foreach (var pr in vendorReviews)
        {
            var customer = await _customerService.GetCustomerById(pr.CustomerId);
            model.Items.Add(new VendorReviewModel {
                Id = pr.Id,
                CustomerId = pr.CustomerId,
                CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                Title = pr.Title,
                ReviewText = pr.ReviewText,
                Rating = pr.Rating,
                Helpfulness = new VendorReviewHelpfulnessModel {
                    VendorId = request.Vendor.Id,
                    VendorReviewId = pr.Id,
                    HelpfulYesTotal = pr.HelpfulYesTotal,
                    HelpfulNoTotal = pr.HelpfulNoTotal
                },
                WrittenOnStr = _dateTimeService.ConvertToUserTime(pr.CreatedOnUtc, DateTimeKind.Utc).ToString("g")
            });
        }

        model.AddVendorReview.CanCurrentCustomerLeaveReview = _vendorSettings.AllowAnonymousUsersToReviewVendor ||
                                                              !await _groupService.IsGuest(_contextAccessor.WorkContext.CurrentCustomer);
        model.AddVendorReview.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnVendorReviewPage;

        return model;
    }
}