using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebShop.Model;
using WebShop.Persistence;

namespace ModelTest
{
    public class UserModelTest
    {
        private readonly UserModel _model;
        private readonly DataDbContext _context;

        public UserModelTest()
        {
            _context = DbContextFactory.Create();
            _model = new UserModel(_context);
        }


        #region Register (count, role, id)



        #endregion



        #region Login (id, role)



        #endregion



        #region ChangePassword (password)


        #endregion













    }
}
