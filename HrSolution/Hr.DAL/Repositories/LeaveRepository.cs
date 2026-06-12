using Hr.DAL.Data;
using Hr.DAL.Models;
using Hr.DAL.Interfaces.RepositoriesInterfaces;

namespace Hr.DAL.Repositories
{
    public class LeaveRepository : GenericRepository<Leave>, ILeaveRepository
    {
        public LeaveRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
