using AutoMapper;
using LinkPara.Audit;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchantUsers;
using LinkPara.PF.Application.Features.SubMerchantUsers.Queries.GetAllSubMerchantUsers;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class SubMerchantUserService : ISubMerchantUserService
{
    private readonly PfDbContext _context;

    public SubMerchantUserService(
        PfDbContext context)
    {
        _context = context;
    }

    public async Task<SubMerchantUserDto> GetByIdAsync(Guid id)
    {
        var subMerchantUser = await _context.SubMerchantUser
            .Include(s => s.SubMerchant)
            .Where(s => s.Id == id)
            .Join(_context.Merchant,
          user => user.SubMerchant.MerchantId,
         merchant => merchant.Id,
          (user, merchant) => new SubMerchantUserDto
          {
              Id = user.Id,
              Name = user.Name,
              Surname = user.Surname,
              UserId = user.UserId,
              BirthDate = user.BirthDate,
              Email = user.Email,
              IdentityNumber = user.IdentityNumber,
              MobilePhoneNumber = user.MobilePhoneNumber,
              RoleId = user.RoleId,
              RoleName = user.RoleName,
              CreateDate = user.CreateDate,
              CreatedBy = user.CreatedBy,
              RecordStatus = user.RecordStatus,
              SubMerchantId = user.SubMerchantId,
              PhoneCode = merchant.PhoneCode,
              SubMerchant = new SubMerchantDto
              {
                  Id = user.SubMerchant.Id,
                  MerchantId = user.SubMerchant.MerchantId,
                  MerchantName = merchant.Name,
                  MerchantNumber = merchant.Number,
                  IsOnUsPaymentPageAllowed = user.SubMerchant.IsOnUsPaymentPageAllowed,
                  City = user.SubMerchant.City,
                  CityName = user.SubMerchant.CityName,
                  CreateDate = user.SubMerchant.CreateDate,
                  InstallmentAllowed = user.SubMerchant.InstallmentAllowed,
                  InternationalCardAllowed = user.SubMerchant.InternationalCardAllowed,
                  Is3dRequired = user.SubMerchant.Is3dRequired,
                  IsExcessReturnAllowed = user.SubMerchant.IsExcessReturnAllowed,
                  IsLinkPaymentPageAllowed = user.SubMerchant.IsLinkPaymentPageAllowed,
                  IsManuelPaymentPageAllowed = user.SubMerchant.IsManuelPaymentPageAllowed,
                  MerchantType = user.SubMerchant.MerchantType,
                  Name = user.SubMerchant.Name,
                  Number = user.SubMerchant.Number,
                  PaymentReturnAllowed = user.SubMerchant.PaymentReturnAllowed,
                  PaymentReverseAllowed = user.SubMerchant.PaymentReverseAllowed,
                  PreAuthorizationAllowed = user.SubMerchant.PreAuthorizationAllowed,
                  RejectReason = user.SubMerchant.RejectReason,
                  RecordStatus = user.SubMerchant.RecordStatus

              }
          }).FirstOrDefaultAsync();

        if (subMerchantUser is null)
        {
            throw new NotFoundException(nameof(SubMerchantUser), id);
        }

        return subMerchantUser;
    }

    public async Task<PaginatedList<SubMerchantUserDto>> GetListAsync(GetAllSubMerchantUserQuery query)
    {
        var merchantUsers = _context.SubMerchantUser.Include(s => s.SubMerchant).AsQueryable();

        if (query.MerchantId is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.SubMerchant.MerchantId == query.MerchantId);
        }

        if (query.SubMerchantId is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.SubMerchantId == query.SubMerchantId);
        }

        if (!string.IsNullOrEmpty(query.Fullname))
        {
            merchantUsers = merchantUsers.Where(b =>
                                       (b.Name.ToLower() + " " + b.Surname.ToLower())
                                       .Contains(query.Fullname.ToLower()));
        }

        if (!string.IsNullOrEmpty(query.Email))
        {
            merchantUsers = merchantUsers.Where(b => b.Email.ToLower().Contains(query.Email.ToLower()));
        }

        if (query.BirthDate is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.BirthDate
                               == query.BirthDate);
        }

        if (!string.IsNullOrEmpty(query.PhoneNumber))
        {
            merchantUsers = merchantUsers.Where(b => (b.MobilePhoneNumber).Contains(query.PhoneNumber));
        }

        if (!string.IsNullOrEmpty(query.RoleId))
        {
            merchantUsers = merchantUsers.Where(b => b.RoleId == query.RoleId);
        }

        if (!string.IsNullOrEmpty(query.IdentityNumber))
        {
            merchantUsers = merchantUsers.Where(b => b.IdentityNumber == query.IdentityNumber);
        }

        if (query.UserId is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.UserId == query.UserId);
        }

        if (query.CreateDateStart is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.CreateDate
                               >= query.CreateDateStart);
        }

        if (query.CreateDateEnd is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.CreateDate
                               <= query.CreateDateEnd);
        }

        if (query.RecordStatus is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.RecordStatus
                               == query.RecordStatus);
        }


        var results = await merchantUsers
         .Join(_context.Merchant,
          user => user.SubMerchant.MerchantId,
         merchant => merchant.Id,
          (user, merchant) => new SubMerchantUserDto
          {
              Id = user.Id,
              Name = user.Name,
              Surname = user.Surname,
              UserId = user.UserId,
              BirthDate = user.BirthDate,
              Email = user.Email,
              IdentityNumber = user.IdentityNumber,
              MobilePhoneNumber = user.MobilePhoneNumber,
              RoleId = user.RoleId,
              RoleName = user.RoleName,
              CreateDate = user.CreateDate,
              CreatedBy = user.CreatedBy,
              RecordStatus = user.RecordStatus,
              SubMerchantId = user.SubMerchantId,
              PhoneCode = merchant.PhoneCode,
              SubMerchant = new SubMerchantDto
              {
                  Id = user.SubMerchant.Id,
                  MerchantId = user.SubMerchant.MerchantId,
                  MerchantName = merchant.Name,
                  MerchantNumber = merchant.Number,
                  IsOnUsPaymentPageAllowed = user.SubMerchant.IsOnUsPaymentPageAllowed,
                  City = user.SubMerchant.City,
                  CityName = user.SubMerchant.CityName,
                  CreateDate = user.SubMerchant.CreateDate,
                  InstallmentAllowed = user.SubMerchant.InstallmentAllowed,
                  InternationalCardAllowed = user.SubMerchant.InternationalCardAllowed,
                  Is3dRequired = user.SubMerchant.Is3dRequired,
                  IsExcessReturnAllowed = user.SubMerchant.IsExcessReturnAllowed,
                  IsLinkPaymentPageAllowed = user.SubMerchant.IsLinkPaymentPageAllowed,
                  IsManuelPaymentPageAllowed = user.SubMerchant.IsManuelPaymentPageAllowed,
                  MerchantType = user.SubMerchant.MerchantType,
                  Name = user.SubMerchant.Name,
                  Number = user.SubMerchant.Number,
                  PaymentReturnAllowed = user.SubMerchant.PaymentReturnAllowed,
                  PaymentReverseAllowed = user.SubMerchant.PaymentReverseAllowed,
                  PreAuthorizationAllowed = user.SubMerchant.PreAuthorizationAllowed,
                  RejectReason = user.SubMerchant.RejectReason,
                  RecordStatus = user.SubMerchant.RecordStatus

              }
          }).PaginatedListAsync(query.Page, query.Size, query.OrderBy, query.SortBy);

        return results;
    }
}
