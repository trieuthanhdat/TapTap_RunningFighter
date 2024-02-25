
using Unity.Services.Core;
using UnityEngine;

namespace TD.UServices.Authentication
{
    [CreateAssetMenu (fileName = "AuthenticationConfig", menuName = "UnityServices")]
    public class UnityAuthenticationConfig : ScriptableObject
    {
        public AuthenticationType AuthenticationType;
    }
   
}


