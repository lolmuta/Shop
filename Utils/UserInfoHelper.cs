namespace Shop.Utils
{
    public class UserInfoHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserInfoHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetUserId()
        {
            return "1";
        }
        public string UserName { get; set; }
    }
}
