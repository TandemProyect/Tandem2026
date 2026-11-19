using System.Collections.Generic;

namespace Desing.Helpers
{
    /// <summary>
    /// Sustituye tokens {Empresa}, {Cliente}, {Diseno}, {Recuento}, etc.
    /// Un token desconocido se deja escrito. Un valor nulo se sustituye por vacío.
    /// </summary>
    public static class ClientOfferMerge
    {
        public const string LogosMarker = "[[LOGOS]]";

        public const string DefaultSubject = "Oferta {Sistema} — {Obra}";

        public const string DefaultHeader =
            "{Empresa}\r\n{DireccionEmpresa}\r\n\r\nDirección cliente: {DireccionCliente}\r\nContacto: {NombreContacto} ({EmailContacto})\r\nAsunto: {Asunto}";

        public const string DefaultBody =
            "Diseño: {Diseno}\r\nOferta: {Oferta}\r\nObra: {Obra}\r\nDirección cliente: {DireccionCliente}\r\nContacto: {NombreContacto} ({EmailContacto})\r\nEmpresa: {Empresa}\r\nPlantilla: {Plantilla}\r\nSistema: {Sistema}\r\n\r\nMateriales:\r\n{Material}\r\n\r\nRecuento:\r\n{Recuento}";

        public const string DefaultAddressesBlock =
            "DATOS DEL CLIENTE\r\nEmpresa / Cliente: {Cliente}\r\nA la atención de: {NombreContacto}\r\nDirección de obra: {DireccionObra}\r\nCIF / NIF: {ClienteCifNif}\r\n\r\nDATOS DE LA EMPRESA\r\nEmpresa: {Empresa}\r\nDirección: {DireccionEmpresa}";

        public static string Apply(string template, IDictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(template))
            {
                return string.Empty;
            }

            if (values == null || values.Count == 0)
            {
                return template;
            }

            var result = template;
            foreach (var pair in values)
            {
                if (string.IsNullOrEmpty(pair.Key))
                {
                    continue;
                }

                var token = pair.Key[0] == '{' ? pair.Key : "{" + pair.Key + "}";
                result = result.Replace(token, pair.Value ?? string.Empty);
            }

            return result;
        }
    }
}
