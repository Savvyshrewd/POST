﻿using POST.Core.Domain;
using POST.Core.Enums;
using POST.CORE.DTO;
using System.Collections.Generic;

namespace POST.Core.DTO.Zone
{
    public class SpecialDomesticPackageDTO : BaseDomainDTO
    {
        public int SpecialDomesticPackageId { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public decimal Weight { get; set; }
        public SpecialDomesticPackageType SpecialDomesticPackageType { get; set; }


        //new properties added for categorization
        public SubCategoryDTO SubCategory { get; set; }
        public string WeightRange { get; set; }

    }


    public class SpecialResultDTO : BaseDomainDTO
    {
        public IEnumerable<SpecialDomesticPackageDTO> Specialpackages { get; set; }
        public List<CategoryDTO> Categories { get; set; }

        public List<SubCategoryDTO> SubCategories { get; set; }

        public Dictionary<string, List<string>> DictionaryCategory { get; set; }
        public Dictionary<string, List<string>> WeightRangeDictionaryCategory { get; set; }

    }
}
