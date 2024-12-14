using Core.Abstractions;

namespace Infrastructure
{
    public class TechnicianRepository: ITechnicianRepository
    {
        private readonly IDbSession _db;
        public TechnicianRepository(IDbSession db)
        {
            _db = db;
        }
    }
}
