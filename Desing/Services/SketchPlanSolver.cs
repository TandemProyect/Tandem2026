using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Desing.Services
{
    /// <summary>Punto en píxeles de la imagen del boceto (x hacia la derecha, y hacia abajo).</summary>
    public sealed class SketchPx
    {
        public SketchPx() { }

        public SketchPx(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>Punto de planta en mm (Y hacia arriba, como en CAD).</summary>
    public sealed class SketchMm
    {
        public SketchMm() { }

        public SketchMm(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>Cota tal y como la lee el modelo de visión.</summary>
    public sealed class SketchCotaLeida
    {
        /// <summary>Valor escrito, en metros.</summary>
        public double Valor { get; set; }

        /// <summary>"H": mide una distancia horizontal; "V": una vertical.</summary>
        public string Eje { get; set; }

        /// <summary>Extremos de la línea de cota (px); pueden faltar.</summary>
        public SketchPx P1 { get; set; }

        public SketchPx P2 { get; set; }

        /// <summary>Vértice de <see cref="SketchReading.Contorno"/> al que llega la referencia de P1 / P2 (o null).</summary>
        public int? V1 { get; set; }

        public int? V2 { get; set; }
    }

    /// <summary>Lo que el modelo de visión lee del boceto: dibujo y textos, sin medidas resueltas.</summary>
    public sealed class SketchReading
    {
        /// <summary>Cara exterior del muro (px), en orden. Puede tener null si un punto no se pudo leer.</summary>
        public List<SketchPx> Contorno { get; set; } = new List<SketchPx>();

        public List<SketchCotaLeida> Cotas { get; set; } = new List<SketchCotaLeida>();

        public double? EspesorM { get; set; }

        public double? AlturaM { get; set; }

        /// <summary>Valor de E leído pero fuera de rango (para explicárselo al usuario).</summary>
        public string EspesorFueraDeRango { get; set; }

        /// <summary>Valor de h leído pero fuera de rango.</summary>
        public string AlturaFueraDeRango { get; set; }

        public List<string> Textos { get; set; } = new List<string>();
    }

    /// <summary>Planta resuelta a partir de la lectura del boceto.</summary>
    public sealed class SketchPlanResult
    {
        public bool Ok { get; set; }

        public string Error { get; set; }

        public List<string> Avisos { get; set; } = new List<string>();

        /// <summary>Una línea por incoherencia entre cotas. Si hay alguna, <see cref="Ok"/> es false.</summary>
        public List<string> Conflictos { get; set; } = new List<string>();

        /// <summary>Estado de cada cota leída, en el orden de <see cref="SketchReading.Cotas"/>.</summary>
        public List<string> EstadoCotas { get; set; } = new List<string>();

        /// <summary>No hay E válido: la forma está resuelta, pero hace falta el espesor para los muros (<see cref="Ok"/> false).</summary>
        public bool FaltaEspesor { get; set; }

        /// <summary>No hay h válida: los muros están resueltos, pero hay que pedir la altura.</summary>
        public bool FaltaAltura { get; set; }

        /// <summary>Qué falta y por qué, para pedírselo al usuario (E primero).</summary>
        public List<string> DatosQueFaltan { get; set; } = new List<string>();

        /// <summary>Cara exterior del muro (mm), antihoraria; origen en la esquina inferior izquierda del envolvente.</summary>
        public List<SketchMm> Exterior { get; set; } = new List<SketchMm>();

        /// <summary>Eje de los muros: la cara exterior desplazada medio espesor hacia dentro (mm).</summary>
        public List<SketchMm> Eje { get; set; } = new List<SketchMm>();

        public double EspesorMm { get; set; }

        public double? AlturaMm { get; set; }

        public double AnchoMm { get; set; }

        public double FondoMm { get; set; }
    }

    /// <summary>
    /// Boceto de planta a mano → muros. El modelo de visión solo LEE (contorno exterior en píxeles, cotas con sus
    /// referencias, E y h); aquí se resuelven las medidas de forma determinista. Cada cota fija la distancia entre
    /// dos líneas del contorno (niveles X o Y): las parciales encadenadas y las totales tienen que cuadrar, y lo que
    /// no tenga cota se estima a escala del dibujo. Solo muros en ángulo recto.
    /// </summary>
    public static class SketchPlanSolver
    {
        public const string CotaUsada = "ok";
        public const string CotaIncoherente = "incoherente";
        public const string CotaRelacionada = "relacionada";
        public const string CotaIgnorada = "ignorada";

        public const double EspesorMinM = 0.05;
        public const double EspesorMaxM = 1.0;
        public const double AlturaMinM = 0.5;
        public const double AlturaMaxM = 12.0;

        /// <summary>Diferencia admitida entre una cota y la suma de las que miden lo mismo.</summary>
        private const double TolCotasM = 0.02;
        private const double MaxGradosFueraDeEje = 30.0;
        private const double RedondeoEstimadoM = 0.05;
        private const double SeparacionMinimaM = 0.01;

        private static readonly CultureInfo Es = CultureInfo.GetCultureInfo("es-ES");

        /// <summary>Convierte la respuesta JSON del modelo de visión en una lectura (tolerante con formatos).</summary>
        public static SketchReading LeerRespuestaModelo(string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
                throw new FormatException("Respuesta vacía del modelo de visión.");

            int ini = contenido.IndexOf('{');
            int fin = contenido.LastIndexOf('}');
            if (ini < 0 || fin <= ini)
                throw new FormatException("La respuesta del modelo de visión no contiene JSON.");

            var obj = JObject.Parse(contenido.Substring(ini, fin - ini + 1));
            var lectura = new SketchReading();

            if ((obj["contornoExterior"] ?? obj["contorno"]) is JArray contorno)
            {
                foreach (var t in contorno)
                    lectura.Contorno.Add(LeerPx(t));
            }

            if (obj["cotas"] is JArray cotas)
            {
                foreach (var t in cotas)
                {
                    if (!(t is JObject c))
                        continue;
                    double? valor = LeerNumero(c["valor"] ?? c["len"]);
                    string eje = NormalizarEje(c["eje"]);
                    if (!valor.HasValue || valor.Value <= 0 || valor.Value > 1000 || eje == null)
                        continue;
                    lectura.Cotas.Add(new SketchCotaLeida
                    {
                        Valor = valor.Value,
                        Eje = eje,
                        P1 = LeerPx(c["p1"]),
                        P2 = LeerPx(c["p2"]),
                        V1 = LeerEntero(c["v1"]),
                        V2 = LeerEntero(c["v2"])
                    });
                }
            }

            if (obj["textos"] is JArray textos)
            {
                foreach (var t in textos)
                {
                    if (t != null && t.Type != JTokenType.Null)
                        lectura.Textos.Add(t.ToString());
                }
            }

            double? e = LeerNumero(obj["espesor"] ?? obj["espesorMuro"]) ?? BuscarAnotacion(lectura.Textos, "E");
            double? h = LeerNumero(obj["altura"] ?? obj["alturaMuro"]) ?? BuscarAnotacion(lectura.Textos, "H");
            AplicarEspesorYAltura(lectura, e, h);
            return lectura;
        }

        /// <summary>
        /// Asigna E y h en metros si están en rango; si no, guarda el valor leído para explicárselo al usuario.
        /// Sirve para lo que lee la IA y para lo que escribe el usuario cuando el boceto no lo trae.
        /// </summary>
        public static void AplicarEspesorYAltura(SketchReading lectura, double? espesorM, double? alturaM)
        {
            lectura.EspesorM = null;
            lectura.EspesorFueraDeRango = null;
            if (espesorM.HasValue)
            {
                if (espesorM.Value >= EspesorMinM && espesorM.Value <= EspesorMaxM)
                    lectura.EspesorM = espesorM.Value;
                else
                    lectura.EspesorFueraDeRango = Fmt(espesorM.Value);
            }

            lectura.AlturaM = null;
            lectura.AlturaFueraDeRango = null;
            if (alturaM.HasValue)
            {
                if (alturaM.Value >= AlturaMinM && alturaM.Value <= AlturaMaxM)
                    lectura.AlturaM = alturaM.Value;
                else
                    lectura.AlturaFueraDeRango = Fmt(alturaM.Value);
            }
        }

        /// <summary>Resuelve la planta: contorno exterior y eje de muros en mm, o el motivo por el que no se puede.</summary>
        public static SketchPlanResult Resolver(SketchReading lectura)
        {
            var res = new SketchPlanResult();
            if (lectura == null)
                return Fallo(res, "No se ha podido leer el boceto.");
            for (int i = 0; i < lectura.Cotas.Count; i++)
                res.EstadoCotas.Add(CotaIgnorada);

            // E y h no cambian la forma: primero se resuelven contorno y cotas, y al final se pide lo que falte.
            double? espesorM = lectura.EspesorM;
            if (espesorM.HasValue)
                res.EspesorMm = Math.Round(espesorM.Value * 1000.0, 1);
            if (lectura.AlturaM.HasValue)
                res.AlturaMm = Math.Round(lectura.AlturaM.Value * 1000.0, 1);

            var verts = LimpiarContorno(lectura.Contorno, out double tamPx);
            if (verts.Count < 4)
                return Fallo(res, "No se ha reconocido el contorno del muro (doble línea) en el boceto.");
            if (TieneTramoDiagonal(verts, tamPx))
            {
                return Fallo(res,
                    "Hay tramos de muro en diagonal: de momento solo se admiten muros en ángulo recto " +
                    "(si la foto está girada, endérézala).");
            }

            FusionarTramosAlineados(verts);
            if (verts.Count < 4)
                return Fallo(res, "No se ha reconocido un contorno cerrado de muros en el boceto.");

            int n = verts.Count;
            var esH = new bool[n];
            for (int e = 0; e < n; e++)
                esH[e] = EsHorizontal(verts[e], verts[(e + 1) % n]);

            // Tramos verticales → niveles X (cotas "H"); horizontales → niveles Y (cotas "V"), con Y hacia arriba.
            var ejeX = new EjeNiveles("H");
            var ejeY = new EjeNiveles("V");
            for (int e = 0; e < n; e++)
            {
                var a = verts[e];
                var b = verts[(e + 1) % n];
                if (esH[e])
                    ejeY.AgregarTramo(e, -(a.Y + b.Y) / 2.0);
                else
                    ejeX.AgregarTramo(e, (a.X + b.X) / 2.0);
            }

            var tramoV = new int[n];
            var tramoH = new int[n];
            for (int k = 0; k < n; k++)
            {
                int previo = (k - 1 + n) % n;
                tramoH[k] = esH[k] ? k : previo;
                tramoV[k] = esH[k] ? previo : k;
            }

            double maxDistRef = 0.25 * tamPx;

            int? TramoPorPunto(SketchPx p, bool horizontal, EjeNiveles eje) =>
                p == null ? (int?)null : eje.TramoMasCercano(horizontal ? p.X : -p.Y, maxDistRef);

            int? TramoPorVertice(int? v, bool horizontal, EjeNiveles eje)
            {
                if (!v.HasValue || v.Value < 0 || v.Value >= lectura.Contorno.Count)
                    return null;
                int k = verts.FindIndex(q => q.Orig == v.Value);
                if (k >= 0)
                    return horizontal ? tramoV[k] : tramoH[k];
                return TramoPorPunto(lectura.Contorno[v.Value], horizontal, eje);
            }

            for (int ci = 0; ci < lectura.Cotas.Count; ci++)
            {
                var c = lectura.Cotas[ci];
                bool horizontal = c.Eje == "H";
                var eje = horizontal ? ejeX : ejeY;
                int? ta = TramoPorVertice(c.V1, horizontal, eje) ?? TramoPorPunto(c.P1, horizontal, eje);
                int? tb = TramoPorVertice(c.V2, horizontal, eje) ?? TramoPorPunto(c.P2, horizontal, eje);
                if (ta.HasValue && tb.HasValue && ta.Value == tb.Value)
                {
                    int? pa = TramoPorPunto(c.P1, horizontal, eje);
                    int? pb = TramoPorPunto(c.P2, horizontal, eje);
                    if (pa.HasValue && pb.HasValue && pa.Value != pb.Value)
                    {
                        ta = pa;
                        tb = pb;
                    }
                }

                if (!ta.HasValue || !tb.HasValue || ta.Value == tb.Value)
                {
                    // La cota del espesor (E) cruza el muro: no relaciona dos líneas del contorno.
                    if (!espesorM.HasValue || Math.Abs(c.Valor - espesorM.Value) > 0.005)
                        res.Avisos.Add("No se ha podido situar la cota " + Fmt(c.Valor) + " en el dibujo: se ignora.");
                    continue;
                }

                eje.Cotas.Add(new CotaEje { Indice = ci, TramoA = ta.Value, TramoB = tb.Value, Valor = c.Valor });
            }

            double tolNivelPx = Math.Max(4.0, 0.02 * tamPx);
            ejeX.Agrupar(tolNivelPx);
            ejeY.Agrupar(tolNivelPx);
            if (ejeX.NivelCoord.Length < 2 || ejeY.NivelCoord.Length < 2)
                return Fallo(res, "No se ha reconocido un contorno cerrado de muros en el boceto.");

            var conflictosX = ejeX.ResolverCotas(TolCotasM);
            var conflictosY = ejeY.ResolverCotas(TolCotasM);
            MarcarEstados(res, ejeX, conflictosX);
            MarcarEstados(res, ejeY, conflictosY);
            foreach (var k in conflictosX)
                res.Conflictos.Add(TextoConflicto("horizontales", k));
            foreach (var k in conflictosY)
                res.Conflictos.Add(TextoConflicto("verticales", k));
            if (res.Conflictos.Count > 0)
            {
                return Fallo(res,
                    "Las cotas del boceto no cuadran. Corrige el boceto y vuelve a importarlo:\n- " +
                    string.Join("\n- ", res.Conflictos));
            }

            double? escalaX = ejeX.Escala();
            double? escalaY = ejeY.Escala();
            if (!escalaX.HasValue && !escalaY.HasValue)
            {
                return Fallo(res,
                    "No se ha podido usar ninguna cota del boceto: revisa que cada cota llegue a las líneas del muro.");
            }

            string error = ejeX.Completar(escalaX ?? escalaY.Value, "horizontales", res.Avisos)
                ?? ejeY.Completar(escalaY ?? escalaX.Value, "verticales", res.Avisos);
            if (error != null)
                return Fallo(res, error);

            var anillo = new List<SketchMm>();
            for (int k = 0; k < n; k++)
                anillo.Add(new SketchMm(ejeX.PosicionDeTramo(tramoV[k]), ejeY.PosicionDeTramo(tramoH[k])));

            double minX = anillo.Min(p => p.X);
            double minY = anillo.Min(p => p.Y);
            foreach (var p in anillo)
            {
                p.X -= minX;
                p.Y -= minY;
            }

            LimpiarAnillo(anillo);
            if (anillo.Count < 4 || !EsPoligonoSimple(anillo))
                return Fallo(res, "Las cotas no encajan con la forma del dibujo: el contorno resultante se cruza.");

            double area = AreaConSigno(anillo);
            if (Math.Abs(area) < 1e-6)
                return Fallo(res, "Las cotas no encajan con la forma del dibujo.");
            if (area < 0)
                anillo.Reverse();

            res.Exterior = anillo.Select(p => new SketchMm(Mm(p.X), Mm(p.Y))).ToList();
            res.AnchoMm = Mm(anillo.Max(p => p.X));
            res.FondoMm = Mm(anillo.Max(p => p.Y));

            if (!lectura.AlturaM.HasValue)
            {
                res.FaltaAltura = true;
                res.DatosQueFaltan.Add(string.IsNullOrEmpty(lectura.AlturaFueraDeRango)
                    ? "El boceto no indica la altura del muro (h=3,00)."
                    : "La altura leída (h=" + lectura.AlturaFueraDeRango + ") no es válida.");
            }

            if (!espesorM.HasValue)
            {
                res.FaltaEspesor = true;
                res.DatosQueFaltan.Insert(0, string.IsNullOrEmpty(lectura.EspesorFueraDeRango)
                    ? "El boceto no indica el espesor del muro (E=0,30)."
                    : "El espesor leído (E=" + lectura.EspesorFueraDeRango + ") no es válido.");
                return Fallo(res, string.IsNullOrEmpty(lectura.EspesorFueraDeRango)
                    ? "Falta el espesor del muro: escribe E=0,30 (en metros) en el boceto y vuelve a intentarlo."
                    : "El espesor leído (E=" + lectura.EspesorFueraDeRango + ") no es válido: escríbelo en metros, por ejemplo E=0,30.");
            }

            var eje2 = DesplazarHaciaDentro(anillo, espesorM.Value / 2.0);
            if (eje2 == null)
            {
                return Fallo(res,
                    "Hay tramos de muro más cortos que el espesor (E=" + Fmt(espesorM.Value) + "): revisa las cotas del boceto.");
            }

            res.Eje = eje2.Select(p => new SketchMm(Mm(p.X), Mm(p.Y))).ToList();
            res.Ok = true;
            return res;
        }

        /// <summary>Formato de medida en metros como en el boceto: 7,00.</summary>
        public static string Fmt(double metros) => metros.ToString("0.00", Es);

        private static double Mm(double metros) => Math.Round(metros * 1000.0, 1);

        private static SketchPlanResult Fallo(SketchPlanResult res, string error)
        {
            res.Ok = false;
            res.Error = error;
            return res;
        }

        private static void MarcarEstados(SketchPlanResult res, EjeNiveles eje, List<Conflicto> conflictos)
        {
            foreach (var c in eje.CotasCoherentes)
                res.EstadoCotas[c.Indice] = CotaUsada;
            foreach (var k in conflictos)
            {
                foreach (var paso in k.Camino)
                    res.EstadoCotas[paso.Cota.Indice] = CotaRelacionada;
            }

            foreach (var k in conflictos)
                res.EstadoCotas[k.Cota.Indice] = CotaIncoherente;
        }

        private static string TextoConflicto(string ejeLargo, Conflicto k)
        {
            if (k.Camino.Count == 1)
            {
                return "Cotas " + ejeLargo + ": " + Fmt(k.Cota.Valor) + " y " + Fmt(k.Camino[0].Cota.Valor) +
                    " miden la misma distancia.";
            }

            var suma = new StringBuilder();
            for (int i = 0; i < k.Camino.Count; i++)
            {
                string num = Fmt(k.Camino[i].Cota.Valor);
                if (i == 0)
                    suma.Append(k.Camino[i].Signo < 0 ? "-" + num : num);
                else
                    suma.Append(k.Camino[i].Signo < 0 ? " - " : " + ").Append(num);
            }

            return "Cotas " + ejeLargo + ": " + Fmt(k.Cota.Valor) + " no cuadra con " + suma + " = " +
                Fmt(k.Calculado) + ".";
        }

        private sealed class Vtx
        {
            public double X;
            public double Y;
            public int Orig;
        }

        private sealed class CotaEje
        {
            public int Indice;
            public int TramoA;
            public int TramoB;
            public double Valor;
            public int NivelA;
            public int NivelB;
        }

        private sealed class Conflicto
        {
            public CotaEje Cota;
            public double Calculado;
            public List<(CotaEje Cota, int Signo)> Camino;
        }

        /// <summary>Niveles (líneas de referencia) de un eje y las cotas que los relacionan.</summary>
        private sealed class EjeNiveles
        {
            private readonly List<int> tramos = new List<int>();
            private readonly List<double> coord = new List<double>();
            private readonly List<CotaEje> aceptadas = new List<CotaEje>();
            private int[] nivelDeTramoLocal;
            private int[] padre;
            private double[] desp;
            private double[] pos;

            public EjeNiveles(string nombre)
            {
                Nombre = nombre;
            }

            public string Nombre { get; }

            public List<CotaEje> Cotas { get; } = new List<CotaEje>();

            /// <summary>Cotas que cuadran (las que fijan medidas y las redundantes que coinciden).</summary>
            public List<CotaEje> CotasCoherentes { get; } = new List<CotaEje>();

            /// <summary>Coordenada en px de cada nivel, ascendente (x, o −y para que crezca hacia arriba).</summary>
            public double[] NivelCoord { get; private set; } = new double[0];

            public void AgregarTramo(int tramo, double c)
            {
                tramos.Add(tramo);
                coord.Add(c);
            }

            public int? TramoMasCercano(double c, double maxDist)
            {
                int mejor = -1;
                double dMejor = double.MaxValue;
                for (int i = 0; i < coord.Count; i++)
                {
                    double d = Math.Abs(coord[i] - c);
                    if (d < dMejor)
                    {
                        dMejor = d;
                        mejor = i;
                    }
                }

                return mejor >= 0 && dMejor <= maxDist ? tramos[mejor] : (int?)null;
            }

            public double PosicionDeTramo(int tramo) => pos[nivelDeTramoLocal[tramos.IndexOf(tramo)]];

            /// <summary>
            /// Une tramos casi alineados en un mismo nivel (p. ej. las dos alas de una U). Dos tramos con cota
            /// propia no se unen nunca: su posición la deciden las cotas.
            /// </summary>
            public void Agrupar(double tolPx)
            {
                var orden = Enumerable.Range(0, coord.Count).OrderBy(i => coord[i]).ToList();
                var grupos = new List<List<int>>();
                foreach (int i in orden)
                {
                    var ultimo = grupos.Count > 0 ? grupos[grupos.Count - 1] : null;
                    bool unir = ultimo != null
                        && coord[i] - coord[ultimo[0]] <= tolPx
                        && !(Referenciado(tramos[i]) && ultimo.Any(j => Referenciado(tramos[j])));
                    if (unir)
                        ultimo.Add(i);
                    else
                        grupos.Add(new List<int> { i });
                }

                nivelDeTramoLocal = new int[coord.Count];
                NivelCoord = new double[grupos.Count];
                for (int g = 0; g < grupos.Count; g++)
                {
                    NivelCoord[g] = grupos[g].Average(i => coord[i]);
                    foreach (int i in grupos[g])
                        nivelDeTramoLocal[i] = g;
                }

                foreach (var c in Cotas)
                {
                    int a = nivelDeTramoLocal[tramos.IndexOf(c.TramoA)];
                    int b = nivelDeTramoLocal[tramos.IndexOf(c.TramoB)];
                    c.NivelA = Math.Min(a, b);
                    c.NivelB = Math.Max(a, b);
                }
            }

            /// <summary>
            /// Aplica las cotas de menor a mayor alcance (primero las parciales). Una cota que relaciona niveles ya
            /// fijados por otras se comprueba: si no coincide, es un conflicto.
            /// </summary>
            public List<Conflicto> ResolverCotas(double tolM)
            {
                int m = NivelCoord.Length;
                padre = Enumerable.Range(0, m).ToArray();
                desp = new double[m];
                var conflictos = new List<Conflicto>();
                foreach (var c in Cotas.Where(c => c.NivelA != c.NivelB)
                             .OrderBy(c => c.NivelB - c.NivelA)
                             .ThenBy(c => c.Indice))
                {
                    int ra = Raiz(c.NivelA);
                    int rb = Raiz(c.NivelB);
                    if (ra == rb)
                    {
                        double calc = desp[c.NivelB] - desp[c.NivelA];
                        if (Math.Abs(calc - c.Valor) > tolM)
                            conflictos.Add(new Conflicto { Cota = c, Calculado = calc, Camino = Camino(c.NivelA, c.NivelB) });
                        else
                            CotasCoherentes.Add(c);
                        continue;
                    }

                    padre[rb] = ra;
                    desp[rb] = c.Valor - desp[c.NivelB] + desp[c.NivelA];
                    aceptadas.Add(c);
                    CotasCoherentes.Add(c);
                }

                return conflictos;
            }

            /// <summary>Metros por píxel según las cotas de este eje (mediana), o null si no hay.</summary>
            public double? Escala()
            {
                var r = aceptadas
                    .Select(c => c.Valor / (NivelCoord[c.NivelB] - NivelCoord[c.NivelA]))
                    .Where(v => v > 0 && !double.IsInfinity(v) && !double.IsNaN(v))
                    .OrderBy(v => v)
                    .ToList();
                if (r.Count == 0)
                    return null;
                return r.Count % 2 == 1 ? r[r.Count / 2] : (r[r.Count / 2 - 1] + r[r.Count / 2]) / 2.0;
            }

            /// <summary>
            /// Posición de cada nivel en metros. Los grupos de niveles sin cota que los relacione se colocan a escala
            /// del dibujo (mínimos cuadrados sobre los huecos entre niveles). Devuelve un error o null.
            /// </summary>
            public string Completar(double escala, string ejeLargo, List<string> avisos)
            {
                int m = NivelCoord.Length;
                var raiz = new int[m];
                for (int i = 0; i < m; i++)
                    raiz[i] = Raiz(i);

                var comp = new Dictionary<int, int>();
                for (int i = 0; i < m; i++)
                {
                    if (!comp.ContainsKey(raiz[i]))
                        comp[raiz[i]] = comp.Count;
                }

                var offset = new double[comp.Count];
                if (comp.Count > 1)
                {
                    int u = comp.Count - 1;
                    var a = new double[u, u];
                    var b = new double[u];
                    for (int i = 0; i + 1 < m; i++)
                    {
                        int ca = comp[raiz[i]];
                        int cb = comp[raiz[i + 1]];
                        if (ca == cb)
                            continue;
                        // Hueco i → i+1 ≈ escala · px: residuo = O_cb − O_ca − t.
                        double t = escala * (NivelCoord[i + 1] - NivelCoord[i]) - desp[i + 1] + desp[i];
                        if (cb > 0)
                        {
                            a[cb - 1, cb - 1] += 1;
                            b[cb - 1] += t;
                            if (ca > 0)
                                a[cb - 1, ca - 1] -= 1;
                        }

                        if (ca > 0)
                        {
                            a[ca - 1, ca - 1] += 1;
                            b[ca - 1] -= t;
                            if (cb > 0)
                                a[ca - 1, cb - 1] -= 1;
                        }
                    }

                    var o = ResolverSistemaLineal(a, b);
                    if (o == null)
                        return "No se han podido estimar las medidas sin cota del boceto.";
                    for (int c = 1; c < comp.Count; c++)
                        offset[c] = Math.Round(o[c - 1] / RedondeoEstimadoM) * RedondeoEstimadoM;

                    avisos.Add("Faltan cotas " + ejeLargo + ": " + (comp.Count - 1) +
                        " medida(s) estimada(s) a escala del dibujo. Revísalas.");
                }

                pos = new double[m];
                for (int i = 0; i < m; i++)
                    pos[i] = offset[comp[raiz[i]]] + desp[i];

                for (int i = 0; i + 1 < m; i++)
                {
                    if (pos[i + 1] - pos[i] < SeparacionMinimaM)
                    {
                        return "Las cotas " + ejeLargo +
                            " no encajan con el dibujo: revisa que cada cota llegue a la línea de muro correcta.";
                    }
                }

                return null;
            }

            private bool Referenciado(int tramo) => Cotas.Any(c => c.TramoA == tramo || c.TramoB == tramo);

            private int Raiz(int i)
            {
                if (padre[i] == i)
                    return i;
                int p = padre[i];
                int r = Raiz(p);
                desp[i] += desp[p];
                padre[i] = r;
                return r;
            }

            /// <summary>Cotas aceptadas que llevan de un nivel a otro, con su signo (para explicar un conflicto).</summary>
            private List<(CotaEje Cota, int Signo)> Camino(int desde, int hasta)
            {
                var previo = new Dictionary<int, (int Nodo, CotaEje Cota, int Signo)>
                {
                    [desde] = (-1, null, 0)
                };
                var cola = new Queue<int>();
                cola.Enqueue(desde);
                while (cola.Count > 0)
                {
                    int u = cola.Dequeue();
                    if (u == hasta)
                        break;
                    foreach (var c in aceptadas)
                    {
                        int w;
                        int signo;
                        if (c.NivelA == u)
                        {
                            w = c.NivelB;
                            signo = 1;
                        }
                        else if (c.NivelB == u)
                        {
                            w = c.NivelA;
                            signo = -1;
                        }
                        else
                        {
                            continue;
                        }

                        if (previo.ContainsKey(w))
                            continue;
                        previo[w] = (u, c, signo);
                        cola.Enqueue(w);
                    }
                }

                var camino = new List<(CotaEje Cota, int Signo)>();
                if (!previo.ContainsKey(hasta))
                    return camino;
                for (int v = hasta; v != desde; v = previo[v].Nodo)
                    camino.Add((previo[v].Cota, previo[v].Signo));
                camino.Reverse();
                return camino;
            }
        }

        private static List<Vtx> LimpiarContorno(List<SketchPx> contorno, out double tamPx)
        {
            var validos = new List<Vtx>();
            if (contorno != null)
            {
                for (int i = 0; i < contorno.Count; i++)
                {
                    var p = contorno[i];
                    if (p == null || !EsFinito(p.X) || !EsFinito(p.Y))
                        continue;
                    validos.Add(new Vtx { X = p.X, Y = p.Y, Orig = i });
                }
            }

            tamPx = validos.Count == 0
                ? 0
                : Math.Max(validos.Max(v => v.X) - validos.Min(v => v.X), validos.Max(v => v.Y) - validos.Min(v => v.Y));
            double minPx = Math.Max(2.0, 0.005 * tamPx);
            var salida = new List<Vtx>();
            foreach (var v in validos)
            {
                if (salida.Count == 0 || Dist(salida[salida.Count - 1], v) >= minPx)
                    salida.Add(v);
            }

            while (salida.Count > 2 && Dist(salida[0], salida[salida.Count - 1]) < minPx)
                salida.RemoveAt(salida.Count - 1);
            return salida;
        }

        private static bool TieneTramoDiagonal(List<Vtx> v, double tamPx)
        {
            for (int i = 0; i < v.Count; i++)
            {
                var a = v[i];
                var b = v[(i + 1) % v.Count];
                double dx = Math.Abs(b.X - a.X);
                double dy = Math.Abs(b.Y - a.Y);
                if (Math.Max(dx, dy) < 0.05 * tamPx)
                    continue;
                double grados = Math.Atan2(Math.Min(dx, dy), Math.Max(dx, dy)) * 180.0 / Math.PI;
                if (grados > MaxGradosFueraDeEje)
                    return true;
            }

            return false;
        }

        /// <summary>Quita vértices entre dos tramos de la misma orientación (puntos sobre una recta, trazos rotos).</summary>
        private static void FusionarTramosAlineados(List<Vtx> v)
        {
            bool cambio = true;
            while (cambio && v.Count >= 4)
            {
                cambio = false;
                int n = v.Count;
                for (int k = 0; k < n; k++)
                {
                    var previo = v[(k - 1 + n) % n];
                    var actual = v[k];
                    var siguiente = v[(k + 1) % n];
                    if (EsHorizontal(previo, actual) == EsHorizontal(actual, siguiente))
                    {
                        v.RemoveAt(k);
                        cambio = true;
                        break;
                    }
                }
            }
        }

        private static bool EsHorizontal(Vtx a, Vtx b) => Math.Abs(b.X - a.X) >= Math.Abs(b.Y - a.Y);

        private static double Dist(Vtx a, Vtx b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        private static bool EsFinito(double v) => !double.IsNaN(v) && !double.IsInfinity(v);

        /// <summary>Quita vértices repetidos y alineados (p. ej. tras unir dos niveles en uno).</summary>
        private static void LimpiarAnillo(List<SketchMm> anillo)
        {
            const double eps = 1e-6;
            bool cambio = true;
            while (cambio && anillo.Count >= 3)
            {
                cambio = false;
                int n = anillo.Count;
                for (int k = 0; k < n; k++)
                {
                    var a = anillo[(k - 1 + n) % n];
                    var p = anillo[k];
                    var b = anillo[(k + 1) % n];
                    bool repetido = Math.Abs(p.X - b.X) < eps && Math.Abs(p.Y - b.Y) < eps;
                    double cruz = (p.X - a.X) * (b.Y - p.Y) - (p.Y - a.Y) * (b.X - p.X);
                    if (repetido || Math.Abs(cruz) < eps)
                    {
                        anillo.RemoveAt(k);
                        cambio = true;
                        break;
                    }
                }
            }
        }

        private static double AreaConSigno(List<SketchMm> anillo)
        {
            double s = 0;
            for (int i = 0; i < anillo.Count; i++)
            {
                var a = anillo[i];
                var b = anillo[(i + 1) % anillo.Count];
                s += a.X * b.Y - b.X * a.Y;
            }

            return s / 2.0;
        }

        private static bool EsPoligonoSimple(List<SketchMm> anillo)
        {
            int n = anillo.Count;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (j == i + 1 || (i == 0 && j == n - 1))
                        continue;
                    if (SegmentosSeTocan(anillo[i], anillo[(i + 1) % n], anillo[j], anillo[(j + 1) % n]))
                        return false;
                }
            }

            return true;
        }

        private static bool SegmentosSeTocan(SketchMm p1, SketchMm p2, SketchMm q1, SketchMm q2)
        {
            double d1 = Orientacion(q1, q2, p1);
            double d2 = Orientacion(q1, q2, p2);
            double d3 = Orientacion(p1, p2, q1);
            double d4 = Orientacion(p1, p2, q2);
            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) && ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;
            return (d1 == 0 && EnSegmento(q1, q2, p1)) || (d2 == 0 && EnSegmento(q1, q2, p2))
                || (d3 == 0 && EnSegmento(p1, p2, q1)) || (d4 == 0 && EnSegmento(p1, p2, q2));
        }

        private static double Orientacion(SketchMm a, SketchMm b, SketchMm c)
        {
            double v = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
            return Math.Abs(v) < 1e-9 ? 0 : v;
        }

        private static bool EnSegmento(SketchMm a, SketchMm b, SketchMm p) =>
            p.X >= Math.Min(a.X, b.X) - 1e-9 && p.X <= Math.Max(a.X, b.X) + 1e-9
            && p.Y >= Math.Min(a.Y, b.Y) - 1e-9 && p.Y <= Math.Max(a.Y, b.Y) + 1e-9;

        /// <summary>
        /// Desplaza cada tramo de un anillo antihorario hacia dentro y corta tramos consecutivos (inglete).
        /// Devuelve null si algún tramo es más corto que el desplazamiento y se invertiría.
        /// </summary>
        private static List<SketchMm> DesplazarHaciaDentro(List<SketchMm> anillo, double d)
        {
            int n = anillo.Count;
            var origen = new SketchMm[n];
            var dir = new SketchMm[n];
            for (int i = 0; i < n; i++)
            {
                var a = anillo[i];
                var b = anillo[(i + 1) % n];
                double len = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
                if (len < 1e-9)
                    return null;
                var u = new SketchMm((b.X - a.X) / len, (b.Y - a.Y) / len);
                dir[i] = u;
                origen[i] = new SketchMm(a.X - u.Y * d, a.Y + u.X * d);
            }

            var salida = new List<SketchMm>();
            for (int k = 0; k < n; k++)
            {
                int e1 = (k - 1 + n) % n;
                double cruz = dir[e1].X * dir[k].Y - dir[e1].Y * dir[k].X;
                if (Math.Abs(cruz) < 1e-9)
                    return null;
                double qx = origen[k].X - origen[e1].X;
                double qy = origen[k].Y - origen[e1].Y;
                double t = (qx * dir[k].Y - qy * dir[k].X) / cruz;
                salida.Add(new SketchMm(origen[e1].X + dir[e1].X * t, origen[e1].Y + dir[e1].Y * t));
            }

            for (int i = 0; i < n; i++)
            {
                var a = salida[i];
                var b = salida[(i + 1) % n];
                double avance = (b.X - a.X) * dir[i].X + (b.Y - a.Y) * dir[i].Y;
                if (avance < 1e-6)
                    return null;
            }

            return salida;
        }

        private static double[] ResolverSistemaLineal(double[,] a, double[] b)
        {
            int n = b.Length;
            var m = (double[,])a.Clone();
            var r = (double[])b.Clone();
            for (int col = 0; col < n; col++)
            {
                int piv = col;
                for (int row = col + 1; row < n; row++)
                {
                    if (Math.Abs(m[row, col]) > Math.Abs(m[piv, col]))
                        piv = row;
                }

                if (Math.Abs(m[piv, col]) < 1e-12)
                    return null;
                if (piv != col)
                {
                    for (int k = 0; k < n; k++)
                    {
                        double tmp = m[col, k];
                        m[col, k] = m[piv, k];
                        m[piv, k] = tmp;
                    }

                    double tr = r[col];
                    r[col] = r[piv];
                    r[piv] = tr;
                }

                for (int row = col + 1; row < n; row++)
                {
                    double f = m[row, col] / m[col, col];
                    for (int k = col; k < n; k++)
                        m[row, k] -= f * m[col, k];
                    r[row] -= f * r[col];
                }
            }

            var x = new double[n];
            for (int row = n - 1; row >= 0; row--)
            {
                double s = r[row];
                for (int k = row + 1; k < n; k++)
                    s -= m[row, k] * x[k];
                x[row] = s / m[row, row];
            }

            return x;
        }

        private static SketchPx LeerPx(JToken t)
        {
            double? x = null;
            double? y = null;
            if (t is JArray arr && arr.Count >= 2)
            {
                x = LeerNumero(arr[0]);
                y = LeerNumero(arr[1]);
            }
            else if (t is JObject o)
            {
                x = LeerNumero(o["x"] ?? o["X"]);
                y = LeerNumero(o["y"] ?? o["Y"]);
            }

            return x.HasValue && y.HasValue ? new SketchPx(x.Value, y.Value) : null;
        }

        private static double? LeerNumero(JToken t)
        {
            if (t == null || t.Type == JTokenType.Null)
                return null;
            if (t.Type == JTokenType.Integer || t.Type == JTokenType.Float)
                return t.Value<double>();
            var m = Regex.Match(t.ToString(), @"\d+(?:[.,]\d+)?");
            if (!m.Success)
                return null;
            return double.TryParse(m.Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double v)
                ? v
                : (double?)null;
        }

        private static int? LeerEntero(JToken t)
        {
            double? v = LeerNumero(t);
            return v.HasValue ? (int)Math.Round(v.Value) : (int?)null;
        }

        private static string NormalizarEje(JToken t)
        {
            string s = t == null || t.Type == JTokenType.Null ? null : t.ToString().Trim().ToUpperInvariant();
            if (s == "H" || s == "HORIZONTAL" || s == "X")
                return "H";
            if (s == "V" || s == "VERTICAL" || s == "Y")
                return "V";
            return null;
        }

        /// <summary>Busca «E=0,30» o «h = 3,00» en los textos leídos.</summary>
        private static double? BuscarAnotacion(List<string> textos, string letra)
        {
            var m = Regex.Match(
                string.Join(" | ", textos),
                @"(?<![A-Za-zÁÉÍÓÚáéíóúÑñ])" + letra + @"\s*[=:]\s*(\d+(?:[.,]\d+)?)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (!m.Success)
                return null;
            return double.TryParse(m.Groups[1].Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double v)
                ? v
                : (double?)null;
        }
    }
}
