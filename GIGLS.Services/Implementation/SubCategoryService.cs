﻿using GIGLS.Core;
using GIGLS.Core.Domain;
using GIGLS.Core.DTO;
using GIGLS.Core.IServices;
using GIGLS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Services.Implementation
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly IUnitOfWork _uow;

        public SubCategoryService(IUnitOfWork uow)
        {
            _uow = uow;
            MapperConfig.Initialize();
        }
        public async Task<object> AddSubCategory(SubCategoryDTO subcategory)
        {
            subcategory.SubCategoryName = subcategory.SubCategoryName.Trim().ToLower();
            

            if (await _uow.SubCategory.ExistAsync(v => v.SubCategoryName.ToLower() == subcategory.SubCategoryName && v.Weight == subcategory.Weight))
            {
                throw new GenericException($"Subcategory {subcategory.SubCategoryName} ALREADY EXIST");
            }

            var newSubCategory = new SubCategory
            {
                SubCategoryName = subcategory.SubCategoryName,
                Weight = subcategory.Weight,
                CategoryId = subcategory.Category.CategoryId
                
            };

            _uow.SubCategory.Add(newSubCategory);
            await _uow.CompleteAsync();
            return new { id = newSubCategory.SubCategoryId };
        }

        public async Task<List<SubCategoryDTO>> GetSubCategories()
        {
            var subcategories = await _uow.SubCategory.GetSubCategories();
            return subcategories.OrderBy(x => x.SubCategoryName).ToList();
        }

        

        public async Task RemoveSubCategory(int subcategoryId)
        {
            var state = await _uow.SubCategory.GetAsync(subcategoryId);

            if (state == null)
            {
                throw new GenericException("SUBCATEGORY DOES NOT EXIST");
            }
            _uow.SubCategory.Remove(state);
            await _uow.CompleteAsync();
        }

        public async Task UpdateSubCategory(int subcategoryId, SubCategoryDTO subcategory)
        {
            var state = await _uow.SubCategory.GetAsync(subcategoryId);

            if (state == null)
            {
                throw new GenericException("SUBCATEGORY INFORMATION DOES NOT EXIST");
            }
            state.SubCategoryName = subcategory.SubCategoryName.Trim();
            state.Weight = subcategory.Weight;
            state.CategoryId = subcategory.CategoryId;
            await _uow.CompleteAsync();
        }
    }
}