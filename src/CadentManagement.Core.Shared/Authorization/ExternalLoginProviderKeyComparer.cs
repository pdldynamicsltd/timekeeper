using System;

namespace CadentManagement.Authorization;

public static class ExternalLoginProviderKeyComparer
{
    public static bool AreEqual(string providedKey, string expectedKey)
    {
        if (string.IsNullOrWhiteSpace(providedKey) || string.IsNullOrWhiteSpace(expectedKey))
        {
            return false;
        }

        if (string.Equals(expectedKey, providedKey, StringComparison.Ordinal))
        {
            return true;
        }

        return string.Equals(
            expectedKey,
            providedKey.Replace("-", "").TrimStart('0'),
            StringComparison.Ordinal
        );
    }
}
