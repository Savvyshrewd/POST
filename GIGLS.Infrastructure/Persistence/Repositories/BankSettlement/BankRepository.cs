﻿using GIGLS.Core.Domain;
using GIGLS.Core.IRepositories.BankSettlement;
using GIGLS.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Infrastructure.Persistence.Repositories.BankSettlement
{
    public class BankRepository: Repository<Bank, GIGLSContext>, IBankRepository
    {
        public BankRepository(GIGLSContext context) : base(context)
        {

        }
    }
}
