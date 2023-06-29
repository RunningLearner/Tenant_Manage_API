using Challenge04_TenantManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Challenge04_TenantManagementApi.Data;

public class SoftDeleteInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// DB에 저장할 때 deleted의 상태를 가진 엔티티들을 soft delete
    /// </summary>
    /// <param name="eventData">현재 컨텍스트에 있는 데이터</param>
    /// <param name="result">인터셉터의 결과</param>
    /// <returns></returns>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is null) return result;

        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            if (entry is not { State: EntityState.Deleted, Entity: ISoftDelete delete })
            {
                continue;
            }

            entry.State = EntityState.Modified;
            delete.IsDeleted = true;
        }

        return result;
    }
}