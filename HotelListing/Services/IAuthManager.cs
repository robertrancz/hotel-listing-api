using HotelListing.Models;
using System.Threading.Tasks;

namespace HotelListing.Services
{
    public interface IAuthManager
    {
        Task<bool> ValidateUserAsync(UserLoginDto userDto);
        Task<string> CreateTokenAsync();
    }
}
