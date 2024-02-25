using System;
using System.Collections;
using System.Reflection;
public interface ILoadingService
{
    public abstract IEnumerator LoadUnityService();
    public abstract IEnumerator SignInUnityService();
    public abstract IEnumerator LoadSceneAsync(int index);

    public static int GetCountILoadingServiceMethods()
    {
        Type loadingServiceType = typeof(ILoadingService);
        MethodInfo[] methods = loadingServiceType.GetMethods();
        return methods.Length;
    }
}