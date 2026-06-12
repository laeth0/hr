using Hr.DAL.Data;
using Hr.DAL.Models;
using Hr.DAL.Interfaces.RepositoriesInterfaces;

namespace Hr.DAL.Repositories
{
    public class AddressRepository : GenericRepository<Address>, IAddressRepository
    {
        public AddressRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
