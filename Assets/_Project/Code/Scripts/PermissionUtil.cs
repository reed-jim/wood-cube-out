using UnityEngine;
using UnityEngine.Android;

public static class PermissionUtil
{
    public static bool ManuallyRequestPermission()
    {
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            return true;
        }
        else
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        return Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
    }
}