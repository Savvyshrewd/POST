﻿using POST.Core.Domain;
using POST.Core.DTO;
using POST.Core.IRepositories;
using POST.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Infrastructure.Persistence.Repositories
{
    public class PaymentMethodRepository : Repository<PaymentMethod, GIGLSContext>, IPaymentMethodRepository
    {
        private GIGLSContext _context;
        public PaymentMethodRepository(GIGLSContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<PaymentMethodDTO>> GetPaymentMethodByUserActiveCountry(int countryId)
        {
            try
            {
                var  paymentMethods = _context.PaymentMethod.AsQueryable();
                if (countryId > 0)
                {
                    paymentMethods = paymentMethods.Where(x => x.CountryId.Equals(countryId) && x.IsActive == true);
                }

                var transferDetailsDto = GetListOfPaymentMethods(paymentMethods);

                return transferDetailsDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private Task<List<PaymentMethodDTO>> GetListOfPaymentMethods(IQueryable<PaymentMethod> paymentMethods)
        {
            var paymentMethodDto = from p in paymentMethods
                              orderby p.DateCreated ascending
                              select new PaymentMethodDTO
                              {
                                  PaymentMethodId = p.PaymentMethodId,
                                  PaymentMethodName = p.PaymentMethodName,
                                  IsActive = p.IsActive,
                              };
            return Task.FromResult(paymentMethodDto.OrderBy(x => x.PaymentMethodId).ToList());
        }

        public Task<List<PaymentMethodNewDTO>> GetPaymentMethods()
        {
            try
            {
                var paymentMethods = _context.PaymentMethod;
                var paymentdto = from p in paymentMethods
                                 join c in _context.Country on p.CountryId equals c.CountryId
                                orderby p.DateCreated ascending
                                select new PaymentMethodNewDTO
                                {
                                    PaymentMethodId = p.PaymentMethodId,
                                    PaymentMethodName = p.PaymentMethodName,
                                    IsActive = p.IsActive,
                                    DateCreated = p.DateCreated,
                                    DateModified = p.DateModified,
                                    CountryId = p.CountryId,
                                    CountryName = c.CountryName
                                };

                return Task.FromResult(paymentdto.ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<PaymentMethodNewDTO> GetPaymentMethodById(int paymentMethodId)
        {
            var method = Context.PaymentMethod.Where(x => x.PaymentMethodId == paymentMethodId);
            var methodDto = GetPaymentMethod(method);
            return methodDto;
        }

        private Task<PaymentMethodNewDTO> GetPaymentMethod(IQueryable<PaymentMethod> methods)
        {
            var methoddto = from m in methods
                            join c in _context.Country on m.CountryId equals c.CountryId
                            select new PaymentMethodNewDTO
                              {
                                  PaymentMethodId = m.PaymentMethodId,
                                  PaymentMethodName = m.PaymentMethodName,
                                  IsActive = m.IsActive,
                                  CountryId = m.CountryId,
                                  DateCreated = m.DateCreated,
                                  DateModified = m.DateModified,
                                  CountryName = c.CountryName
                              };
            return Task.FromResult(methoddto.FirstOrDefault());
        }
    }
}
