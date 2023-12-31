﻿using POST.Core.Enums;
using POST.Core;
using POST.Core.Domain.Partnership;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using POST.Core.Domain;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIGL.POST.Core.Domain
{
    public class Fleet : BaseDomain, IAuditable
    {
        [Key]
        public int FleetId { get; set; }

        [MaxLength(100)]
        public string RegistrationNumber { get; set; }
        public string ChassisNumber { get; set; }
        public string EngineNumber { get; set; }
        public bool Status { get; set; }
        public FleetType FleetType { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; }

        public int ModelId { get; set; }
        public virtual FleetModel FleetModel { get; set; }

        public int PartnerId { get; set; }
        public virtual Partner Partner { get; set; }

        public string FleetName { get; set; }
        public string EnterprisePartnerId { get; set; }
        [ForeignKey("EnterprisePartnerId")]
        public virtual User EnterprisePartner { get; set; }

        public VehicleFixedStatus IsFixed { get; set; }
    }
}