using Unity.Services.Authentication;
using Unity.Services.Core;

namespace TD.UServices.Authentication
{
    public interface IUnityAuthentication
    {
        public abstract void HandleSuccessfulSignIn();

        public abstract void HandleAuthenticationError(AuthenticationException ex);

        public abstract void HandleRequestFailedError(RequestFailedException ex);
    }
}
