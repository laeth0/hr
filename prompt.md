Act as a senior .NET developer. I need to implement two new features in my N-Tier ASP.NET Core Web API project (which includes PL, BLL, and DAL layers):

1. **Global Output Caching**:
   - Please configure ASP.NET Core Output Caching in `Program.cs`.
   - Set up a default caching policy that applies globally (or show how to apply it across all endpoints).
   - Provide an example of how to override or customize this cache policy on a specific controller or endpoint (e.g., using `[OutputCache]` attributes).

2. **Integrate Dapper (For Learning)**:
   - I want to learn how to use Dapper alongside my existing Entity Framework Core setup.
   - Please add the necessary Dapper package references or configuration.
   - Implement a read-only repository or a specific query method (for example, fetching a list of Employees) in the `Hr.DAL` layer using Dapper.
   - Explain how the IDbConnection is injected and managed alongside the existing EF Core `ApplicationDbContext`.

**Constraints & Preferences:**
- Consistently apply SOLID and DRY principles.
- Include detailed comments explaining the "why" and "how" of the implementation, especially for the Dapper integration, since this is for learning.
- Ensure the changes align with my existing architectural layers (Controllers in PL, Services in BLL, Repositories in DAL).