using Hr.DAL.Data;
using Hr.DAL.Models;
using Hr.DAL.Repositories.Interfaces;

namespace Hr.DAL.Repositories
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
