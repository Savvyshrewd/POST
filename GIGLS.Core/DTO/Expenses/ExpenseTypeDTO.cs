﻿using POST.CORE.DTO;

namespace POST.Core.DTO.Expenses
{
    public class ExpenseTypeDTO : BaseDomainDTO
    {
        public int ExpenseTypeId { get; set; }
        public string ExpenseTypeName { get; set; }
    }
}