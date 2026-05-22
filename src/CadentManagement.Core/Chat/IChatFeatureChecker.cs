namespace CadentManagement.Chat;

public interface IChatFeatureChecker
{
    void CheckChatFeatures(int? sourceTenantId, int? targetTenantId);
}

