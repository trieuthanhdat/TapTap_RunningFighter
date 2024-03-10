using System;
using System.Collections;
using System.Reflection;
public interface ILoadingService
{
    public abstract IEnumerator Step_LoadUnityService();
    public abstract IEnumerator Step_SignInUnityService();
    public abstract IEnumerator Step_LoadSceneAsync(int index);

    public static int GetCountILoadingServiceMethods()
    {
        Type loadingServiceType = typeof(ILoadingService);
        MethodInfo[] methods = loadingServiceType.GetMethods();
        return methods.Length;
    }
}