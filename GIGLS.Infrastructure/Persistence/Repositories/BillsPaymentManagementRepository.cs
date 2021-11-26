﻿using GIGLS.Core.Domain;
using GIGLS.Core.IRepositories;
using GIGLS.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Infrastructure.Persistence.Repositories
{
    public class BillsPaymentManagementRepository : Repository<BillsPaymentManagement, GIGLSContext>, IBillsPaymentManagementRepository
    {
        private GIGLSContext _context;
        public BillsPaymentManagementRepository(GIGLSContext context) : base(context)
        {
            _context = context;
        }
    }
}
