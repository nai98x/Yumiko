using Yumiko.Model.Entities;

namespace Yumiko.Model.Interfaces.Repositories;

public interface IAnilistUsersRepository
{
    Task<AnilistUserLink?> GetLinkAsync(ulong userId);

    Task SetAnilistAsync(int anilistId, ulong userId);

    Task<bool> DeleteAnilistAsync(ulong userId);
}
