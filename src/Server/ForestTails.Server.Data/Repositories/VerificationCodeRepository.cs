using ForestTails.Server.Data.Enums;

namespace ForestTails.Server.Data.Repositories
{
    public interface IVerificationCodeRepository
    {
        Task SaveCodeAsync(string email, string code, CodeType type);
        Task<bool> ValidateCodeAsync(string email, string code, CodeType type);
    }
}
