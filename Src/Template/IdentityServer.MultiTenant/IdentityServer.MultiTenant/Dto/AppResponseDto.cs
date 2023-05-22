namespace IdentityServer.MultiTenant.Dto
{
    public class AppResponseDto
    {
        public string ClientName { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMsg { get; set; }

        public AppResponseDto() {
        }

        public AppResponseDto(bool result) {
            ErrorCode = result ? 0 : -1;
        }
    }

    public class AppResponseDto<T> : AppResponseDto
    {
        public List<T> ResultList { get; set; }
        public T Result { get; set; }
    }
}
