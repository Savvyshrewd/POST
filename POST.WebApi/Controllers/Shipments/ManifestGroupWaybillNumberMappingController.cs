﻿using POST.Core.IServices;
using POST.Services.Implementation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using POST.Core.IServices.Shipments;
using POST.Core.DTO.Shipments;
using POST.WebApi.Filters;
using System.Linq;
using POST.CORE.DTO.Report;
using POST.Core.DTO.Fleets;
using System;
using POST.CORE.DTO.Shipments;

namespace POST.WebApi.Controllers.Shipments
{
    [Authorize(Roles = "Shipment, ViewAdmin")]
    [RoutePrefix("api/manifestgroupwaybillnumbermapping")]
    public class ManifestGroupWaybillNumberMappingController : BaseWebApiController
    {
        private readonly IManifestGroupWaybillNumberMappingService _service;
        public ManifestGroupWaybillNumberMappingController(IManifestGroupWaybillNumberMappingService service) : base(nameof(ManifestGroupWaybillNumberMappingController))
        {
            _service = service;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<ManifestGroupWaybillNumberMappingDTO>>> GetAllManifestGroupWayBillNumberMappings()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifestGroupWayBillNumberMappings = await _service.GetAllManifestGroupWayBillNumberMappings();

                return new ServiceResponse<IEnumerable<ManifestGroupWaybillNumberMappingDTO>>
                {
                    Object = manifestGroupWayBillNumberMappings.ToList()
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("manifestbydate")]
        public async Task<IServiceResponse<IEnumerable<ManifestGroupWaybillNumberMappingDTO>>> GetAllManifestGroupWayBillNumberMappings(DateFilterCriteria dateFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifestGroupWayBillNumberMappings = await _service.GetAllManifestGroupWayBillNumberMappings(dateFilterCriteria);

                return new ServiceResponse<IEnumerable<ManifestGroupWaybillNumberMappingDTO>>
                {
                    Object = manifestGroupWayBillNumberMappings
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("movementmanifestbydate")] 
        public async Task<IServiceResponse<IEnumerable<MovementManifestNumberDTO>>> GetAllManifestMovementManifestNumberMappings(DateFilterCriteria dateFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var MovementmanifestNumberMappings = await _service.GetAllManifestMovementManifestNumberMappings(dateFilterCriteria);

                return new ServiceResponse<IEnumerable<MovementManifestNumberDTO>>
                {
                    Object = MovementmanifestNumberMappings
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost] 
        [Route("expectedmovementmanifestbydate")]
        public async Task<IServiceResponse<IEnumerable<MovementManifestNumberDTO>>> GetExpectedManifestMovementManifestNumberMappings(DateFilterCriteria dateFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var MovementmanifestNumberMappings = await _service.GetExpectedManifestMovementManifestNumberMappings(dateFilterCriteria);

                return new ServiceResponse<IEnumerable<MovementManifestNumberDTO>>
                {
                    Object = MovementmanifestNumberMappings
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("supermanifestbydate")]
        public async Task<IServiceResponse<IEnumerable<ManifestDTO>>> GetAllManifestSuperManifestMappings(DateFilterCriteria dateFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifestSuperManifestMappings = await _service.GetAllManifestSuperManifestMappings(dateFilterCriteria);

                return new ServiceResponse<IEnumerable<ManifestDTO>>
                {
                    Object = manifestSuperManifestMappings
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("mapmultiple")]
        public async Task<IServiceResponse<bool>> MappingManifestToGroupWaybillNumber(ManifestGroupWaybillNumberMappingDTO data)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.MappingManifestToGroupWaybillNumber(data.ManifestCode, data.GroupWaybillNumbers);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("mapmovmentmanifest")]
        public async Task<IServiceResponse<bool>> MovementManifestNumberMapping(MovementManifestNumberMappingDTO data) 
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.MappingMovementManifestToManifest(data.MovementManifestCode, data.ManifestNumbers, data.DestinationServiceCentreId); 
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("updatemovmentmanifest")]
        public async Task<IServiceResponse<bool>> UpdateMovementManifestNumberMapping(MovementManifestNumberMappingDTO data) 
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.UpdateMappingMovementManifestToManifest(data.MovementManifestCode, data.ManifestNumbers, data.DestinationServiceCentreId,true);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("mapsupermanifest")]
        public async Task<IServiceResponse<bool>> MappingSuperManifestToManifest(ManifestDTO data)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.MappingSuperManifestToManifest(data.SuperManifestCode, data.ManifestCodes);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("manifestforgroupwaybillnumber/id/{groupwaybillNumberId}")]
        public async Task<IServiceResponse<ManifestDTO>> GetManifestForGroupWaybillNumber(int groupwaybillNumberId)
        {
            return await HandleApiOperationAsync(async () =>
            {

                var manifestDTO = await _service.GetManifestForGroupWaybillNumber(groupwaybillNumberId);

                return new ServiceResponse<ManifestDTO>
                {
                    Object = manifestDTO
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("manifestforgroupwaybillnumber/code/{groupwaybillNumber}")]
        public async Task<IServiceResponse<ManifestDTO>> GetManifestForGroupWaybillNumber(string groupwaybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {

                var manifestDTO = await _service.GetManifestForGroupWaybillNumber(groupwaybillNumber);

                return new ServiceResponse<ManifestDTO>
                {
                    Object = manifestDTO
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("groupwaybillnumbersinmanifest/id/{manifestId}")]
        public async Task<IServiceResponse<List<GroupWaybillNumberDTO>>> GetGroupWaybillNumbersInManifest(int manifestId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var groupWaybillNumbersInManifest = await _service.GetGroupWaybillNumbersInManifest(manifestId);

                return new ServiceResponse<List<GroupWaybillNumberDTO>>
                {
                    Object = groupWaybillNumbersInManifest
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("groupwaybillnumbersinmanifest/code/{manifest}")]
        public async Task<IServiceResponse<List<GroupWaybillNumberDTO>>> GetGroupWaybillNumbersInManifest(string manifest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var groupWaybillNumbersInManifest = await _service.GetGroupWaybillNumbersInManifest(manifest);

                return new ServiceResponse<List<GroupWaybillNumberDTO>>
                {
                    Object = groupWaybillNumbersInManifest
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("manifestnumbersinmovementmanifest/code/{movementmanifestCode}")]
        public async Task<IServiceResponse<List<MovementManifestNumberMappingDTOTwo>>> GetManifestNumbersInMovementManifest(string movementmanifestCode) 
        {
            return await HandleApiOperationAsync(async () =>
            {
                var ManifestNumbersInMovementManifest = await _service.GetManifestNumbersInMovementManifest(movementmanifestCode);

                return new ServiceResponse<List<MovementManifestNumberMappingDTOTwo>>
                {
                    Object = ManifestNumbersInMovementManifest
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("manifestsinsupermanifest/code/{manifest}")]
        public async Task<IServiceResponse<List<ManifestDTO>>> GetManifestInSuperManifest(string manifest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifestsInSuperManifest = await _service.GetManifestsInSuperManifest(manifest);

                return new ServiceResponse<List<ManifestDTO>>
                {
                    Object = manifestsInSuperManifest
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("manifestsinsupermanifestdetails/code/{manifest}")]
        public async Task<IServiceResponse<List<ManifestDTO>>> GetManifestsInSuperManifestDetails(string manifest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifestsInSuperManifest = await _service.GetManifestsInSuperManifestDetails(manifest);

                return new ServiceResponse<List<ManifestDTO>>
                {
                    Object = manifestsInSuperManifest
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("waybillsinmanifest")]
        public async Task<IServiceResponse<List<ManifestWaybillMappingDTO>>> GetWaybillsInListOfManifest([FromUri]string captainId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var WaybillNumbersInMannifest = await _service.GetWaybillsInListOfManifest(captainId);

                return new ServiceResponse<List<ManifestWaybillMappingDTO>>
                {
                    Object = WaybillNumbersInMannifest
                };
            });
        }
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("waybillsinmanifestbydate")]
        public async Task<IServiceResponse<List<ManifestWaybillMappingDTO>>> GetAllWaybillsInListOfManifest([FromUri]string captainId, DateTime StartDate, DateTime EndDate)
        {
            DateFilterCriteria dateFilterCriteria = new DateFilterCriteria();
            dateFilterCriteria.StartDate = StartDate;
            dateFilterCriteria.EndDate = EndDate;

            return await HandleApiOperationAsync(async () =>
            {
                var WaybillsInManifest = await _service.GetAllWaybillsinListOfManifest(captainId, dateFilterCriteria);
                return new ServiceResponse<List<ManifestWaybillMappingDTO>>
                {
                    Object = WaybillsInManifest
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("RemoveGroupWaybillNumberFromManifest/{manifest}/{groupWaybillNumber}")]
        public async Task<IServiceResponse<bool>> RemoveGroupWaybillNumberFromManifest(string manifest, string groupWaybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.RemoveGroupWaybillNumberFromManifest(manifest, groupWaybillNumber);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("RemoveManifestFromMovementManifest/{movementmanifest}/{manifestnumber}")]
        public async Task<IServiceResponse<bool>> RemoveManifestFromMovementManifest(string movementmanifest, string manifestnumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.RemoveManifestFromMovementManifest(manifestnumber, movementmanifest);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("removemanifestfromsupermanifest/{superManifest}/{manifest}")]
        public async Task<IServiceResponse<bool>> RemoveManifestFromSuperManifest(string superManifest, string manifest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.RemoveManifestFromSuperManifest(superManifest, manifest);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("getmanifestforwaybill/{waybill}")]
        public async Task<IServiceResponse<ManifestGroupWaybillNumberMappingDTO>> GetManifestForWaybill(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifestforWaybill = await _service.GetManifestForWaybill(waybill);

                return new ServiceResponse<ManifestGroupWaybillNumberMappingDTO>
                {
                    Object = manifestforWaybill
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("getsupermanifestformanifest/{manifest}")]
        public async Task<IServiceResponse<ManifestDTO>> GetSuperManifestForManifest(string manifest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var supermanifestforManifest = await _service.GetSuperManifestForManifest(manifest);

                return new ServiceResponse<ManifestDTO>
                {
                    Object = supermanifestforManifest
                };
            });
        }

        [GIGLSActivityAuthorize(Activity ="View")]
        [HttpGet]
        [Route("getmanifestsearch/{manifestCode}")]
        public async Task<IServiceResponse<ManifestDTO>> GetManifestSearch(string manifestCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifestSearch = await _service.GetManifestSearch(manifestCode);
                return new ServiceResponse<ManifestDTO>
                {
                    Object = manifestSearch
                };
         });
        }


        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("moveManifestToNewManifest/{manifestCode}")]
        public async Task<IServiceResponse<string>> MoveManifestDetailToNewManifest(string manifestCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifestSearch = await _service.MoveManifestDetailToNewManifest(manifestCode);
                return new ServiceResponse<string>
                {
                    Object = manifestSearch
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("manifests/{manifestCode}")]
        public async Task<IServiceResponse<List<GroupWaybillAndWaybillDTO>>> GetGroupWaybillDataInManifest(string manifestCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.GetGroupWaybillDataInManifest(manifestCode);

                return new ServiceResponse<List<GroupWaybillAndWaybillDTO>>
                {
                    Object = result
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("getallmanifestandgroupwaybill")]
        public async Task<IServiceResponse<List<AllManifestAndGroupWaybillDTO>>> GetGroupWaybillDataInManifest(AllManifestAndGroupWaybillFilterDTO allManifestAndGroupWaybillFilterDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.GetDetailForAllTypesOfManifest(allManifestAndGroupWaybillFilterDTO.Code, allManifestAndGroupWaybillFilterDTO.Type);

                return new ServiceResponse<List<AllManifestAndGroupWaybillDTO>>
                {
                    Object = result
                };
            });
        }
    }
}
