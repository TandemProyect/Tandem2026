using UnityEngine.Networking;

namespace TandemXR
{
    /// <summary>HTTPS dev (IIS Express). No usar en producción.</summary>
    public sealed class TandemDevCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData) => true;
    }
}
