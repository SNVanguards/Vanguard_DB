using VanguardDev_API.VanguardTest.Models;
namespace VanguardDev_API.Service
{
    public interface IUserService
    {
        /// <summary>
        /// 无脑查询用户信息列表
        /// </summary>
        /// <returns></returns>
        Task<List<UserEntity>> GetUserEntities();
    }
}
