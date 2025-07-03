


using Vanguard_DB;

using VanguardDev_API.VanguardTest.Models;


namespace VanguardDev_API.Service
{
    public class UserService : DBServiceBase, IUserService
    {
        public  async Task<List<UserEntity>> GetUserEntities()
        {
            var res = await Repository().GetListAsync<UserEntity>();
            
            return res.Skip(0).Take(20).ToList();
        }
    }
}
