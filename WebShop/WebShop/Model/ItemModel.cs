using Microsoft.EntityFrameworkCore;
using WebShop.Dto;
using WebShop.Persistence;

namespace WebShop.Model
{
    public class ItemModel
    {
        private readonly DataDbContext _context;
        public ItemModel(DataDbContext context)
        {
            _context = context;
        }

        #region AllItems
        #endregion

        #region ItemsByCategory
        #endregion

        #region ItemsByPriceMax
        #region

        #region ItemsByPriceMin
        #region

        #region ItemsByMaxMinPrice
        #endregion

        #region ItemsByCategoryPriceAsc
        #endregion

        #region ItemsByCategoryPriceDesc
        #endregion

        #region SearchItemByName
        #endregion

        #region SearchByParameter
        #endregion

        #region CompareItems
        #endregion
    }
}
