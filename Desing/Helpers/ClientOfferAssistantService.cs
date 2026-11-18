using System;
using System.Collections.Generic;
using System.Linq;

namespace Desing.Helpers
{
    public class ClientOfferAssistantRequest
    {
        public string Prompt { get; set; }

        public string Oferta { get; set; }

        public string Contexto { get; set; }

        public IDictionary<string, string> Tokens { get; set; }
    }

    public class ClientOfferAssistantResponse
    {
        public string Subject { get; set; }

        public string Header { get; set; }

        public string Body { get; set; }
    }

    public static class ClientOfferAssistantService
    {
        public static ClientOfferAssistantResponse Suggest(ClientOfferAssistantRequest request)
        {
            var safeRequest = request ?? new ClientOfferAssistantRequest();
            var tokens = safeRequest.Tokens ?? new Dictionary<string, string>();

            var normalizedPrompt = Normalize(safeRequest.Prompt);
            var normalizedContext = Normalize(safeRequest.Contexto);
            var normalizedOffer = Normalize(safeRequest.Oferta);
            var combined = string.Join(" ", new[] { normalizedPrompt, normalizedContext, normalizedOffer });

            var isFormal = HasKeyword(combined, "formal");
            var isBrief = HasKeyword(combined, "breve");
            var isTechnical = HasKeyword(combined, "técnica") || HasKeyword(combined, "tecnica");
            var includesPrice = HasKeyword(combined, "precio");

            var empresa = GetToken(tokens, "Empresa", "Tandem");
            var cliente = GetToken(tokens, "Cliente", "Cliente");
            var oferta = FirstNonEmpty(safeRequest.Oferta, GetToken(tokens, "Oferta", "Oferta sin referencia"));
            var sistema = GetToken(tokens, "Sistema", "sistema propuesto");
            var obra = GetToken(tokens, "Obra", "su proyecto");
            var recuento = FirstNonEmpty(GetToken(tokens, "Recuento", ""), GetToken(tokens, "Resumen", ""));

            var subject = BuildSubject(isFormal, isTechnical, includesPrice, oferta, obra, sistema);
            var header = BuildHeader(isFormal, empresa, cliente, oferta);
            var body = BuildBody(isFormal, isBrief, isTechnical, includesPrice, cliente, obra, sistema, oferta, recuento);

            return new ClientOfferAssistantResponse
            {
                Subject = subject,
                Header = header,
                Body = body
            };
        }

        private static string BuildSubject(bool isFormal, bool isTechnical, bool includesPrice, string oferta, string obra, string sistema)
        {
            var baseSubject = isFormal
                ? "Presentación de oferta"
                : "Propuesta";

            if (isTechnical)
            {
                baseSubject += " técnica";
            }

            if (includesPrice)
            {
                baseSubject += " económica";
            }

            return string.Format("{0} {1} · {2} · {3}", baseSubject, oferta, obra, sistema).Trim();
        }

        private static string BuildHeader(bool isFormal, string empresa, string cliente, string oferta)
        {
            if (isFormal)
            {
                return string.Format(
                    "Estimado/a {0}:{1}{1}Por medio de la presente le remitimos la {2} de {3}.",
                    cliente,
                    Environment.NewLine,
                    "oferta",
                    empresa);
            }

            return string.Format(
                "Hola {0},{1}{1}Te compartimos la oferta {2} de {3}.",
                cliente,
                Environment.NewLine,
                oferta,
                empresa);
        }

        private static string BuildBody(
            bool isFormal,
            bool isBrief,
            bool isTechnical,
            bool includesPrice,
            string cliente,
            string obra,
            string sistema,
            string oferta,
            string recuento)
        {
            var lines = new List<string>();
            lines.Add(isFormal
                ? string.Format("Detalle para {0}:", cliente)
                : "Resumen rápido:");

            lines.Add(string.Format("- Obra: {0}", obra));
            lines.Add(string.Format("- Sistema: {0}", sistema));
            lines.Add(string.Format("- Referencia: {0}", oferta));

            if (isTechnical)
            {
                lines.Add("- Enfoque técnico: solución orientada a montaje seguro, trazabilidad y compatibilidad en obra.");
            }

            if (includesPrice)
            {
                lines.Add("- Precio: incluir desglose por partidas y condiciones comerciales vigentes.");
            }

            if (!string.IsNullOrWhiteSpace(recuento))
            {
                lines.Add(string.Format("- Recuento: {0}", recuento.Trim()));
            }

            if (!isBrief)
            {
                lines.Add(isFormal
                    ? "Quedamos a su disposición para ampliar cualquier punto técnico o económico."
                    : "Si quieres, lo vemos juntos y ajustamos lo que necesites.");
            }

            return string.Join(Environment.NewLine, lines.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private static bool HasKeyword(string value, string keyword)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(keyword))
            {
                return false;
            }

            return value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string GetToken(IDictionary<string, string> tokens, string key, string fallback)
        {
            if (tokens == null || string.IsNullOrWhiteSpace(key))
            {
                return fallback;
            }

            string value;
            if (!tokens.TryGetValue(key, out value))
            {
                return fallback;
            }

            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string FirstNonEmpty(string first, string second)
        {
            if (!string.IsNullOrWhiteSpace(first))
            {
                return first.Trim();
            }

            if (!string.IsNullOrWhiteSpace(second))
            {
                return second.Trim();
            }

            return string.Empty;
        }
    }
}
