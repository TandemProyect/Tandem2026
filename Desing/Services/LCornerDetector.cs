using Desing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

namespace Desing.Services
{
    /// <summary>
    /// Detector SIMPLE de conexiones entre líneas
    /// </summary>
    public class LCornerDetector
    {
        private class DebugMuroRectoRegistro
        {
            public string Metodo { get; set; }
            public string Tipo { get; set; }
            public string Estado { get; set; }
            public string Motivo { get; set; }
            public string ParLineas { get; set; }
            public object Geometria { get; set; }
        }

        private enum TipoMuroRecto
        {
            Tipo1_AmbosExtremosConectados = 1, // nace y muere en esquina
            Tipo2_InicioConectado_FinLibre = 2,
            Tipo3_InicioLibre_FinConectado = 3,
            Tipo4_AmbosExtremosLibres = 4
        }

        /// <summary>
        /// US-688 T5 (#693) — info por panel L para construir muros rectos B/C
        /// </summary>
        private class PanelInfoMuro
        {
            public PuntoDTO Verde;
            public PuntoDTO Amarillo;
            public PuntoDTO Blanco;
            public PuntoDTO Cian;
            public LineaDTO InnerH;
            public LineaDTO OuterH;
            public LineaDTO InnerV;
            public LineaDTO OuterV;
            // US-688 T6 (#694) — vértices de la esquina L para localizar el extremo libre del muro
            public double AzulX;
            public double AzulY;
            public double RojoX;
            public double RojoY;
        }

        // US-697 — altura para extrusión ModelDesing (mm). Sobrescrita en cada llamada a DetectarEsquinasL.
        private double _alturaMuroMm = 2700;
        private readonly List<DebugMuroRectoRegistro> _debugMuros = new List<DebugMuroRectoRegistro>();

        private const double TOLERANCIA = 0.01; // Muy pequeña tolerancia para puntos "iguales"
        private const double OFFSET_MINIMO_PANEL = 50.0;   // Distancia mínima entre líneas paralelas (rechaza colineales de distintas esquinas)
        private const double OFFSET_MAXIMO_PANEL = 1500.0; // Distancia máxima entre líneas paralelas para considerar un panel válido

        private static readonly double[] MEDIDAS_PANEL = { 300, 450, 600, 750, 1050, 1200, 1350, 1500, 1650, 1800, 2100 };

        private bool EsMedidaEstandar(double dist)
        {
            const double TOL = 1.0;
            return MEDIDAS_PANEL.Any(m => Math.Abs(dist - m) <= TOL);
        }

        private double MayorEstandarMenorQue(double dist)
        {
            double resultado = -1;
            foreach (var m in MEDIDAS_PANEL)
                if (m < dist - 1.0) resultado = m;
            return resultado;
        }

        /// <summary>
        /// Detecta conexiones entre líneas (puntos donde se tocan)
        /// </summary>
        public DeteccionEsquinasLDTO DetectarEsquinasL(List<LineaDTO> lineas, double alturaMuroMm = 2700)
        {
            _alturaMuroMm = alturaMuroMm > 0 ? alturaMuroMm : 2700;
            _debugMuros.Clear();

            var resultado = new DeteccionEsquinasLDTO
            {
                Esquinas = new List<EsquinaLDTO>(),
                PuntosADibujar = new List<PuntoDTO>(),
                PolilineasADibujar = new List<PolilineaDTO>()
            };

            if (lineas == null || lineas.Count < 2)
            {
                resultado.Mensaje = "Se requieren al menos 2 líneas";
                GuardarJSON(new { Error = "Menos de 2 líneas" });
                return resultado;
            }

            // Expandir polilíneas en segmentos individuales
            lineas = ExpandirPolilineas(lineas);

            // Solo procesar líneas simples
            var lineasSimples = lineas.Where(l => l.Tipo == "Line").ToList();

            // 📋 Preparar toda la informaciçón para el JSON
            var infoCompleta = new
            {
                FechaAnalisis = DateTime.Now,
                TotalLineasRecibidas = lineas.Count,
                TotalLineasSimples = lineasSimples.Count,
                Tolerancia = TOLERANCIA,

                // 📍 PRIMERO: TODAS LAS LÍNEAS RECIBIDAS (SIN FILTRAR) PARA VER EL TIPO
                LineasRecibidas = lineas.Select((linea, index) => new
                {
                    Indice = index,
                    Tipo = linea.Tipo,  // ⭐ IMPORTANTE: Ver qué tipo llega
                    Inicio = new { X = linea.InicioX, Y = linea.InicioY, Z = linea.InicioZ },
                    Fin = new { X = linea.FinX, Y = linea.FinY, Z = linea.FinZ },
                    Longitud = linea.Longitud,
                    Layer = linea.Layer,
                    Color = linea.Color
                }).ToList(),

                // 📍 SEGUNDO: LAS LÍNEAS FILTRADAS (SOLO "Line")
                LineasFiltradas = lineasSimples.Select((linea, index) => new
                {
                    Indice = index,
                    Tipo = linea.Tipo,
                    Inicio = new { X = linea.InicioX, Y = linea.InicioY, Z = linea.InicioZ },
                    Fin = new { X = linea.FinX, Y = linea.FinY, Z = linea.FinZ },
                    Longitud = linea.Longitud,
                    Layer = linea.Layer,
                    Color = linea.Color
                }).ToList(),

                // 🔍 COMPARACIONES: Revisar todas las combinaciones
                Comparaciones = new List<object>(),

                // ✅ CONEXIONES ENCONTRADAS
                ConexionesDetectadas = new List<object>()
            };

            // Comparar cada línea con las demás (USAR TODAS, no solo las filtradas)
            for (int i = 0; i < lineas.Count; i++)
            {
                for (int j = i + 1; j < lineas.Count; j++)
                {
                    var linea1 = lineas[i];
                    var linea2 = lineas[j];

                    // Los 4 puntos de las dos líneas
                    var p1Inicio = new { X = linea1.InicioX, Y = linea1.InicioY };
                    var p1Fin = new { X = linea1.FinX, Y = linea1.FinY };
                    var p2Inicio = new { X = linea2.InicioX, Y = linea2.InicioY };
                    var p2Fin = new { X = linea2.FinX, Y = linea2.FinY };

                    // Calcular las 4 distancias posibles
                    double dist_p1I_p2I = Distancia(p1Inicio.X, p1Inicio.Y, p2Inicio.X, p2Inicio.Y);
                    double dist_p1I_p2F = Distancia(p1Inicio.X, p1Inicio.Y, p2Fin.X, p2Fin.Y);
                    double dist_p1F_p2I = Distancia(p1Fin.X, p1Fin.Y, p2Inicio.X, p2Inicio.Y);
                    double dist_p1F_p2F = Distancia(p1Fin.X, p1Fin.Y, p2Fin.X, p2Fin.Y);

                    bool sonPerpendiculares = SonLineasPerpendiculares(linea1, linea2);

                    // Crear el objeto de comparación
                    var comparacion = new
                    {
                        Linea1_Indice = i,
                        Linea2_Indice = j,
                        SonPerpendiculares = sonPerpendiculares,
                        Casos = new[]
                        {
                            new { Caso = "L1.Inicio <-> L2.Inicio", Distancia = dist_p1I_p2I, SeTocan = dist_p1I_p2I <= TOLERANCIA, 
                                  P1 = p1Inicio, P2 = p2Inicio },
                            new { Caso = "L1.Inicio <-> L2.Fin", Distancia = dist_p1I_p2F, SeTocan = dist_p1I_p2F <= TOLERANCIA, 
                                  P1 = p1Inicio, P2 = p2Fin },
                            new { Caso = "L1.Fin <-> L2.Inicio", Distancia = dist_p1F_p2I, SeTocan = dist_p1F_p2I <= TOLERANCIA, 
                                  P1 = p1Fin, P2 = p2Inicio },
                            new { Caso = "L1.Fin <-> L2.Fin", Distancia = dist_p1F_p2F, SeTocan = dist_p1F_p2F <= TOLERANCIA, 
                                  P1 = p1Fin, P2 = p2Fin }
                        }
                    };

                    ((List<object>)infoCompleta.Comparaciones).Add(comparacion);

                    // ✅ ¿Hay alguna conexión?
                    if (sonPerpendiculares && dist_p1I_p2I <= TOLERANCIA)
                    {
                        ((List<object>)infoCompleta.ConexionesDetectadas).Add(new
                        {
                            Linea1 = i,
                            Linea2 = j,
                            TipoConexion = "L1.Inicio <-> L2.Inicio",
                            PuntoConexion = p1Inicio,
                            Distancia = dist_p1I_p2I
                        });

                        resultado.Esquinas.Add(new EsquinaLDTO
                        {
                            Vertice = new PuntoDTO { X = linea1.InicioX, Y = linea1.InicioY, Z = linea1.InicioZ },
                            Angulo = 90.0,
                            IndiceLinea1 = i,
                            IndiceLinea2 = j
                        });
                    }

                    if (sonPerpendiculares && dist_p1I_p2F <= TOLERANCIA)
                    {
                        ((List<object>)infoCompleta.ConexionesDetectadas).Add(new
                        {
                            Linea1 = i,
                            Linea2 = j,
                            TipoConexion = "L1.Inicio <-> L2.Fin",
                            PuntoConexion = p1Inicio,
                            Distancia = dist_p1I_p2F
                        });

                        resultado.Esquinas.Add(new EsquinaLDTO
                        {
                            Vertice = new PuntoDTO { X = linea1.InicioX, Y = linea1.InicioY, Z = linea1.InicioZ },
                            Angulo = 90.0,
                            IndiceLinea1 = i,
                            IndiceLinea2 = j
                        });
                    }

                    if (sonPerpendiculares && dist_p1F_p2I <= TOLERANCIA)
                    {
                        ((List<object>)infoCompleta.ConexionesDetectadas).Add(new
                        {
                            Linea1 = i,
                            Linea2 = j,
                            TipoConexion = "L1.Fin <-> L2.Inicio",
                            PuntoConexion = p1Fin,
                            Distancia = dist_p1F_p2I
                        });

                        resultado.Esquinas.Add(new EsquinaLDTO
                        {
                            Vertice = new PuntoDTO { X = linea1.FinX, Y = linea1.FinY, Z = linea1.FinZ },
                            Angulo = 90.0,
                            IndiceLinea1 = i,
                            IndiceLinea2 = j
                        });
                    }

                    if (sonPerpendiculares && dist_p1F_p2F <= TOLERANCIA)
                    {
                        ((List<object>)infoCompleta.ConexionesDetectadas).Add(new
                        {
                            Linea1 = i,
                            Linea2 = j,
                            TipoConexion = "L1.Fin <-> L2.Fin",
                            PuntoConexion = p1Fin,
                            Distancia = dist_p1F_p2F
                        });

                        resultado.Esquinas.Add(new EsquinaLDTO
                        {
                            Vertice = new PuntoDTO { X = linea1.FinX, Y = linea1.FinY, Z = linea1.FinZ },
                            Angulo = 90.0,
                            IndiceLinea1 = i,
                            IndiceLinea2 = j
                        });
                    }
                }
            }

            // 🆕 DETECTAR LÍNEAS PARALELAS Y ESQUINAS FORMADAS
            var paresParalelos = new List<object>();
            var esquinasDetectadas = new List<object>();

            // Detectar pares de líneas paralelas
            for (int i = 0; i < lineas.Count; i++)
            {
                for (int j = i + 1; j < lineas.Count; j++)
                {
                    var linea1 = lineas[i];
                    var linea2 = lineas[j];

                    // Verificar si son paralelas
                    bool sonParalelas = SonLineasParalelas(linea1, linea2);
                    double distanciaEntreLineas = CalcularDistanciaEntreLineasParalelas(linea1, linea2);

                    if (sonParalelas)
                    {
                        paresParalelos.Add(new
                        {
                            Linea1_Indice = i,
                            Linea2_Indice = j,
                            Orientacion = ObtenerOrientacion(linea1),
                            DistanciaEntreLineas = distanciaEntreLineas,
                            Linea1_Geometria = $"({linea1.InicioX},{linea1.InicioY}) -> ({linea1.FinX},{linea1.FinY})",
                            Linea2_Geometria = $"({linea2.InicioX},{linea2.InicioY}) -> ({linea2.FinX},{linea2.FinY})"
                        });
                    }
                }
            }

            // Buscar esquinas formadas por líneas paralelas Y PERPENDICULARES
            // Una esquina puede estar en cualquier orientación, lo importante es:
            // 1. Que haya 2 pares de líneas paralelas entre sí
            // 2. Que cada par sea perpendicular al otro par

            // Agrupar líneas paralelas en grupos (independientemente de orientación)
            var gruposParalelos = new List<List<int>>();
            var yaAgrupadas = new HashSet<int>();

            for (int i = 0; i < lineas.Count; i++)
            {
                if (yaAgrupadas.Contains(i)) continue;

                var grupo = new List<int> { i };
                yaAgrupadas.Add(i);

                for (int j = i + 1; j < lineas.Count; j++)
                {
                    if (yaAgrupadas.Contains(j)) continue;

                    if (SonLineasParalelas(lineas[i], lineas[j]))
                    {
                        grupo.Add(j);
                        yaAgrupadas.Add(j);
                    }
                }

                if (grupo.Count >= 2) // Solo grupos con al menos 2 líneas paralelas
                {
                    gruposParalelos.Add(grupo);
                }
            }

            // Buscar pares de grupos perpendiculares entre sí
            // Un panel rectangular = 2 grupos perpendiculares, cada uno con al menos 2 líneas
            if (gruposParalelos.Count >= 2)
            {
                // Verificar cada combinación de 2 grupos para ver si son perpendiculares
                for (int g1 = 0; g1 < gruposParalelos.Count; g1++)
                {
                    for (int g2 = g1 + 1; g2 < gruposParalelos.Count; g2++)
                    {
                        var grupo1 = gruposParalelos[g1];
                        var grupo2 = gruposParalelos[g2];

                        // Tomar una línea representativa de cada grupo
                        var lineaReprGrupo1 = lineas[grupo1[0]];
                        var lineaReprGrupo2 = lineas[grupo2[0]];

                        // Verificar si los dos grupos son perpendiculares entre sí
                        if (SonLineasPerpendiculares(lineaReprGrupo1, lineaReprGrupo2))
                        {
                            // Tenemos 2 grupos perpendiculares, ahora verificar cada par de líneas
                            foreach (var idx1a in grupo1)
                            {
                                foreach (var idx1b in grupo1)
                                {
                                    if (idx1a >= idx1b) continue; // Evitar duplicados

                                    foreach (var idx2a in grupo2)
                                    {
                                        foreach (var idx2b in grupo2)
                                        {
                                            if (idx2a >= idx2b) continue; // Evitar duplicados

                                            var l1a = lineas[idx1a];
                                            var l1b = lineas[idx1b];
                                            var l2a = lineas[idx2a];
                                            var l2b = lineas[idx2b];

                                            double distGrupo1 = CalcularDistanciaEntreLineasParalelas(l1a, l1b);
                                            double distGrupo2 = CalcularDistanciaEntreLineasParalelas(l2a, l2b);

                                            // ⭐ VALIDACIÓN: El offset entre líneas paralelas debe estar entre 50 y 1500 unidades
                                            // y las 4 líneas deben cerrar un contorno rectangular real.
                                            bool dist1Valida = distGrupo1 >= OFFSET_MINIMO_PANEL && distGrupo1 <= OFFSET_MAXIMO_PANEL;
                                            bool dist2Valida = distGrupo2 >= OFFSET_MINIMO_PANEL && distGrupo2 <= OFFSET_MAXIMO_PANEL;
                                            bool panelConectado = EsPanelRectangularConectado(l1a, l1b, l2a, l2b);
                                            // IMPORTANTE: no bloquear flujo por conectividad estricta para no perder
                                            // paneles/muros válidos en trazados abiertos de polilínea.
                                            bool esPanelValido = dist1Valida && dist2Valida;

                                            esquinasDetectadas.Add(new
                                            {
                                                TipoEsquina = esPanelValido
                                                    ? "Panel rectangular (4 líneas paralelas)"
                                                    : "Panel INVÁLIDO - Distancia entre líneas excede el límite",
                                                LineasGrupo1 = new[] { idx1a, idx1b },
                                                LineasGrupo2 = new[] { idx2a, idx2b },
                                                DistanciaGrupo1 = distGrupo1,
                                                DistanciaGrupo2 = distGrupo2,
                                                Dist1Valida = dist1Valida,
                                                Dist2Valida = dist2Valida,
                                                PanelConectado = panelConectado,
                                                EsPanelValido = esPanelValido,
                                                OffsetMaximoPermitido = OFFSET_MAXIMO_PANEL,
                                                MotivoRechazo = !esPanelValido
                                                    ? (!dist1Valida ? $"Distancia grupo 1 ({distGrupo1:F2}) > {OFFSET_MAXIMO_PANEL}" : "")
                                                    + (!dist1Valida && !dist2Valida ? " y " : "")
                                                    + (!dist2Valida ? $"Distancia grupo 2 ({distGrupo2:F2}) > {OFFSET_MAXIMO_PANEL}" : "")
                                                    + ((!dist1Valida || !dist2Valida) && !panelConectado ? " y " : "")
                                                    + (!panelConectado ? "Las 4 líneas no cierran un contorno rectangular conectado" : "")
                                                    : null,
                                                Geometrias = new
                                                {
                                                    G1_L1 = $"({l1a.InicioX},{l1a.InicioY}) -> ({l1a.FinX},{l1a.FinY})",
                                                    G1_L2 = $"({l1b.InicioX},{l1b.InicioY}) -> ({l1b.FinX},{l1b.FinY})",
                                                    G2_L1 = $"({l2a.InicioX},{l2a.InicioY}) -> ({l2a.FinX},{l2a.FinY})",
                                                    G2_L2 = $"({l2b.InicioX},{l2b.InicioY}) -> ({l2b.FinX},{l2b.FinY})"
                                                }
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Reconstruir objeto con paralelas

            // 🔍 FILTRAR SOLO PANELES VÁLIDOS (offset <= 1500)
            var panelesValidos = esquinasDetectadas.Where(e => ((dynamic)e).EsPanelValido == true).ToList();
            var panelesInvalidos = esquinasDetectadas.Where(e => ((dynamic)e).EsPanelValido == false).ToList();

            // 📍 Agregar puntos a dibujar - SOLO para conexiones individuales si NO hay paneles válidos
            var detalleConexionesPorPanel = new List<object>(); // 🆕 Detalle interior/exterior por panel

            if (panelesValidos != null && panelesValidos.Count > 0)
            {
                // Recopilar todos los puntos únicos de todos los paneles válidos
                var todosPuntosInterior = new List<PuntoDTO>();
                var todosPuntosExterior = new List<PuntoDTO>();
                var todosPuntosVerde    = new List<PuntoDTO>();
                var todosPuntosAmarillo = new List<PuntoDTO>();
                var todosPuntosBlanco   = new List<PuntoDTO>();
                var todosPuntosCian     = new List<PuntoDTO>();
                var todosPuntosMagenta  = new List<PuntoDTO>();
                var todosPuntosCriss    = new List<PuntoDTO>();

                // US-688 T5 (#693) — info de cada panel L para detectar muros rectos entre esquinas (B/C)
                var panelesInfoMuro = new List<PanelInfoMuro>();

                // Evitar procesar el mismo conjunto de 4 líneas dos veces
                // (el algoritmo puede detectar el mismo panel con grupos intercambiados)
                var panelesProcesados = new HashSet<string>();

                int numeroPanelValido = 1;
                foreach (dynamic panel in panelesValidos)
                {
                    // Obtener las 4 líneas del panel
                    int[] lineasGrupo1 = panel.LineasGrupo1;
                    int[] lineasGrupo2 = panel.LineasGrupo2;

                    // Clave canónica: los 4 índices ordenados
                    var indices = new[] { lineasGrupo1[0], lineasGrupo1[1], lineasGrupo2[0], lineasGrupo2[1] };
                    Array.Sort(indices);
                    string clavePanel = string.Join("-", indices);
                    if (panelesProcesados.Contains(clavePanel)) continue;
                    panelesProcesados.Add(clavePanel);

                    var l1a = lineas[lineasGrupo1[0]];
                    var l1b = lineas[lineasGrupo1[1]];
                    var l2a = lineas[lineasGrupo2[0]];
                    var l2b = lineas[lineasGrupo2[1]];

                    // Calcular puntos de esquina L por intersección de líneas interiores/exteriores
                    var (interior, exterior) = CalcularPuntosEsquinaL(l1a, l1b, l2a, l2b);

                    // Calcular 6 puntos de panel (US-668 + US-671) + info líneas (US-688 T5)
                    var (ptVerde, ptAmarillo, ptBlanco, ptCian, ptMagenta, ptCriss, infoMuro) = CalcularPuntosPanelConLineas(l1a, l1b, l2a, l2b);
                    if (infoMuro != null) panelesInfoMuro.Add(infoMuro);
                    if (ptVerde    != null) todosPuntosVerde.Add(ptVerde);
                    if (ptAmarillo != null) todosPuntosAmarillo.Add(ptAmarillo);
                    if (ptBlanco   != null) todosPuntosBlanco.Add(ptBlanco);
                    if (ptCian     != null) todosPuntosCian.Add(ptCian);
                    if (ptMagenta  != null) todosPuntosMagenta.Add(ptMagenta);
                    if (ptCriss    != null) todosPuntosCriss.Add(ptCriss);

                    // US-675/679: polilíneas por esquina — orden 3→5→2→4→1→7 (cerrada)
                    var ptInterior = interior.FirstOrDefault();
                    var ptExterior = exterior.FirstOrDefault();
                    if (ptVerde != null && ptInterior != null && ptAmarillo != null &&
                        ptCian  != null && ptExterior != null && ptBlanco   != null)
                    {
                        var verticesEsquina = new List<PuntoDTO> { ptVerde, ptInterior, ptAmarillo, ptCian, ptExterior, ptBlanco };

                        // Polilínea original — capa ObjetoDB2d, sin extrusión
                        resultado.PolilineasADibujar.Add(new PolilineaDTO
                        {
                            Cerrada         = true,
                            Capa            = "ObjetoDB2d",
                            ColorIndex      = 256,
                            AlturaExtrusion = 0,
                            Vertices        = verticesEsquina
                        });

                        // Polilínea extruida — capa ModelDesing, altura configurable (US-697)
                        resultado.PolilineasADibujar.Add(new PolilineaDTO
                        {
                            Cerrada         = true,
                            Capa            = "ModelDesing",
                            ColorIndex      = 256,
                            AlturaExtrusion = _alturaMuroMm,
                            Vertices        = verticesEsquina
                        });
                    }

                    // 🆕 Registrar detalle de conexiones interiores/exteriores de este panel
                    detalleConexionesPorPanel.Add(new
                    {
                        NumeroPanel = numeroPanelValido,
                        LineasInvolucradas = new
                        {
                            Grupo1 = lineasGrupo1,
                            Grupo2 = lineasGrupo2
                        },
                        ConexionesInteriores = interior.Select(p => new { X = p.X, Y = p.Y, Z = p.Z }).ToList(),
                        ConexionesExteriores = exterior.Select(p => new { X = p.X, Y = p.Y, Z = p.Z }).ToList(),
                        TotalInteriores = interior.Count,
                        TotalExteriores = exterior.Count
                    });

                    todosPuntosInterior.AddRange(interior);
                    todosPuntosExterior.AddRange(exterior);
                    numeroPanelValido++;
                }

                // Eliminar duplicados (puntos a menos de TOLERANCIA de distancia)
                var puntosInteriorUnicos = EliminarPuntosDuplicados(todosPuntosInterior);
                var puntosExteriorUnicos = EliminarPuntosDuplicados(todosPuntosExterior);

                // Agregar puntos únicos a la lista de dibujo
                foreach (var punto in puntosInteriorUnicos)
                {
                    punto.TipoPunto  = "PtEInterior";
                    punto.ColorIndex = (int)TipoPunto.PtEInterior;
                    resultado.PuntosADibujar.Add(punto);
                }

                foreach (var punto in puntosExteriorUnicos)
                {
                    punto.TipoPunto  = "PtEExteriro";
                    punto.ColorIndex = (int)TipoPunto.PtEExteriro;
                    resultado.PuntosADibujar.Add(punto);
                }

                foreach (var punto in EliminarPuntosDuplicados(todosPuntosVerde))    resultado.PuntosADibujar.Add(punto);
                foreach (var punto in EliminarPuntosDuplicados(todosPuntosAmarillo)) resultado.PuntosADibujar.Add(punto);
                foreach (var punto in EliminarPuntosDuplicados(todosPuntosBlanco))   resultado.PuntosADibujar.Add(punto);
                foreach (var punto in EliminarPuntosDuplicados(todosPuntosCian))     resultado.PuntosADibujar.Add(punto);
                foreach (var punto in EliminarPuntosDuplicados(todosPuntosMagenta))  resultado.PuntosADibujar.Add(punto);
                foreach (var punto in EliminarPuntosDuplicados(todosPuntosCriss))    resultado.PuntosADibujar.Add(punto);

                // Tipo 1 primero: muros entre esquinas conectadas en ambos extremos.
                // Se detectan por estaciones comunes sobre pares de líneas paralelas
                // usando vértices de esquina + puntos de referencia ya calculados.
                GenerarMurosTipo1DesdeEsquinas(lineas, resultado.Esquinas, resultado.PuntosADibujar, resultado);

                // US-688 T6 (#694) — Muros rectos con UNA esquina L y un extremo libre (A, CC, D, Cara E)
                GenerarMurosLConExtremoLibre(panelesInfoMuro, lineas, resultado);

                // IMPORTANTE: cuando hay paneles/esquinas válidas, la geometría de muros rectos
                // debe nacer/morir en los puntos derivados de esas esquinas. Evitamos fallback
                // basado solo en líneas para no extender muros fuera de esquina.

                // US-688 T7 (#695) — Muros rectos sin ninguna esquina L (E, F) — pares paralelos aislados
                GenerarMurosLibresAislados(panelesInfoMuro, lineas, resultado);
            }
            else
            {
                // Si no hay paneles válidos, usar conexiones individuales
                foreach (var esquina in resultado.Esquinas)
                {
                    esquina.Vertice.TipoPunto  = "PtEInterior";
                    esquina.Vertice.ColorIndex = (int)TipoPunto.PtEInterior;
                    resultado.PuntosADibujar.Add(esquina.Vertice);
                }

                // Sin paneles L válidos: tipificar y resolver muros por conectividad de extremos.
                // - Tipo 1/2/3: se intentan construir desde pares paralelos.
                // - Tipo 4: se delega al generador de muros aislados.
                GenerarMurosConUnExtremoLibreDesdeLineas(lineas, resultado, resultado.PuntosADibujar);
                GenerarMurosLibresAislados(new List<PanelInfoMuro>(), lineas, resultado);
            }

            // 🔍 LÓGICA MEJORADA: Si hay paneles VÁLIDOS detectados, las conexiones individuales son parte del panel
            int esquinasEnLIndependientes = ((List<object>)infoCompleta.ConexionesDetectadas).Count;
            int esquinasEnLReales = panelesValidos.Count > 0 ? panelesValidos.Count : esquinasEnLIndependientes;

            var infoCompletaConParalelas = new
            {
                FechaAnalisis = infoCompleta.FechaAnalisis,
                TotalLineasRecibidas = infoCompleta.TotalLineasRecibidas,
                TotalLineasSimples = infoCompleta.TotalLineasSimples,
                Tolerancia = infoCompleta.Tolerancia,
                LineasRecibidas = infoCompleta.LineasRecibidas,
                LineasFiltradas = infoCompleta.LineasFiltradas,
                Comparaciones = infoCompleta.Comparaciones,
                ConexionesDetectadas = infoCompleta.ConexionesDetectadas,
                ParesLineasParalelas = paresParalelos,
                EsquinasOPanelesDetectados = esquinasDetectadas,

                // 🆕 DIFERENCIA ENTRE CONEXIONES INTERIORES Y EXTERIORES POR PANEL VÁLIDO
                DetalleConexionesInteriorExteriorPorPanel = detalleConexionesPorPanel,

                // 📊 RESUMEN EJECUTIVO
                ResumenFinal = new
                {
                    TotalLineasAnalizadas = lineas.Count,
                    TotalConexionesIndividuales = ((List<object>)infoCompleta.ConexionesDetectadas).Count,
                    TotalParesParalelos = paresParalelos.Count,

                    // ⭐ VALIDACIÓN DE OFFSET
                    OffsetMaximoPermitido = OFFSET_MAXIMO_PANEL,
                    TotalPanelesDetectados = esquinasDetectadas.Count,
                    TotalPanelesValidos = panelesValidos.Count,
                    TotalPanelesInvalidos = panelesInvalidos.Count,

                    // ⭐ CLAVE: Solo paneles VÁLIDOS cuentan como esquinas
                    TotalEsquinasEnL = esquinasEnLReales,

                    // Explicación de la lógica
                    NotaDeteccion = panelesValidos.Count > 0 
                        ? $"Se detectaron {esquinasEnLIndependientes} conexiones punto-a-punto y {esquinasDetectadas.Count} panel(es) rectangular(es). De estos, {panelesValidos.Count} panel(es) cumple(n) con el offset máximo ({OFFSET_MAXIMO_PANEL} unidades) y se cuenta(n) como {esquinasEnLReales} esquina(s) en L."
                        : esquinasDetectadas.Count > 0
                            ? $"Se detectaron {esquinasDetectadas.Count} panel(es) rectangular(es), pero NINGUNO cumple con el offset máximo ({OFFSET_MAXIMO_PANEL} unidades). Las {esquinasEnLIndependientes} conexiones se cuentan como esquinas independientes."
                            : $"Se detectaron {esquinasEnLIndependientes} esquinas en L independientes (no forman paneles completos).",

                    // Desglose de conexiones
                    DetalleConexionesIndividuales = ((List<object>)infoCompleta.ConexionesDetectadas).Select((c, idx) =>
                    {
                        dynamic conn = c;
                        var l1 = lineas[conn.Linea1];
                        var l2 = lineas[conn.Linea2];
                        return new
                        {
                            NumeroConexion = idx + 1,
                            PuntoConexion = $"({conn.PuntoConexion.X}, {conn.PuntoConexion.Y})",
                            TipoEsquina = panelesValidos.Count > 0 
                                ? "Parte de un panel rectangular VÁLIDO" 
                                : panelesInvalidos.Count > 0
                                    ? "Parte de un panel rectangular INVÁLIDO (offset > 1500)"
                                    : "Esquina en L independiente (90°)",
                            Linea1_Descripcion = $"Línea {conn.Linea1}: {ObtenerOrientacion(l1)} de ({l1.InicioX},{l1.InicioY}) a ({l1.FinX},{l1.FinY})",
                            Linea2_Descripcion = $"Línea {conn.Linea2}: {ObtenerOrientacion(l2)} de ({l2.InicioX},{l2.InicioY}) a ({l2.FinX},{l2.FinY})",
                            Distancia = conn.Distancia,
                            EsPerfecta = conn.Distancia == 0.0
                        };
                    }).ToList(),

                    // Desglose de paneles VÁLIDOS
                    DetallePanelesValidos = panelesValidos.Select((p, idx) =>
                    {
                        dynamic panel = p;
                        return new
                        {
                            NumeroPanel = idx + 1,
                            Tipo = "✅ Esquina en L VÁLIDA - Panel rectangular (offset <= 1500)",
                            EsValido = true,
                            LineasInvolucradas = new
                            {
                                Grupo1 = panel.LineasGrupo1,
                                Grupo2 = panel.LineasGrupo2
                            },
                            Dimensiones = new
                            {
                                DistanciaGrupo1 = panel.DistanciaGrupo1,
                                DistanciaGrupo2 = panel.DistanciaGrupo2
                            },
                            Validacion = new
                            {
                                OffsetMaximo = OFFSET_MAXIMO_PANEL,
                                Grupo1Cumple = panel.Dist1Valida,
                                Grupo2Cumple = panel.Dist2Valida
                            }
                        };
                    }).ToList(),

                    // Desglose de paneles INVÁLIDOS
                    DetallePanelesInvalidos = panelesInvalidos.Select((p, idx) =>
                    {
                        dynamic panel = p;
                        return new
                        {
                            NumeroPanel = idx + 1,
                            Tipo = "❌ Panel INVÁLIDO - Offset excede el límite",
                            EsValido = false,
                            MotivoRechazo = panel.MotivoRechazo,
                            LineasInvolucradas = new
                            {
                                Grupo1 = panel.LineasGrupo1,
                                Grupo2 = panel.LineasGrupo2
                            },
                            Dimensiones = new
                            {
                                DistanciaGrupo1 = panel.DistanciaGrupo1,
                                DistanciaGrupo2 = panel.DistanciaGrupo2
                            },
                            Validacion = new
                            {
                                OffsetMaximo = OFFSET_MAXIMO_PANEL,
                                Grupo1Cumple = panel.Dist1Valida,
                                Grupo2Cumple = panel.Dist2Valida
                            }
                        };
                    }).ToList(),

                    // Conclusión
                    Conclusion = panelesValidos.Count > 0
                        ? $"✅ Se detectó {panelesValidos.Count} esquina(s) en L VÁLIDA(S) formada(s) por panel(es) rectangular(es) completo(s) con offset <= {OFFSET_MAXIMO_PANEL} unidades."
                        + (panelesInvalidos.Count > 0 ? $" Se rechazaron {panelesInvalidos.Count} panel(es) por exceder el offset máximo." : "")
                        : panelesInvalidos.Count > 0
                            ? $"❌ Se detectaron {panelesInvalidos.Count} panel(es) rectangular(es), pero TODOS fueron rechazados por exceder el offset máximo ({OFFSET_MAXIMO_PANEL} unidades). Las {esquinasEnLIndependientes} conexiones se cuentan como esquinas independientes."
                            : $"✅ Se detectaron {esquinasEnLIndependientes} esquinas en L independientes (no forman paneles rectangulares completos).",

                    // 🆕 RESUMEN DE DIFERENCIAS INTERIORES/EXTERIORES
                    ResumenConexionesInteriorExterior = panelesValidos.Count > 0
                        ? new
                        {
                            TotalPuntosInterior = resultado.PuntosADibujar.Count(p => p.TipoPunto == "Interior"),
                            TotalPuntosExterior = resultado.PuntosADibujar.Count(p => p.TipoPunto == "Exterior"),
                            ListaPuntosInterior = resultado.PuntosADibujar.Where(p => p.TipoPunto == "Interior").Select(p => new { p.X, p.Y, p.Z }).ToList(),
                            ListaPuntosExterior = resultado.PuntosADibujar.Where(p => p.TipoPunto == "Exterior").Select(p => new { p.X, p.Y, p.Z }).ToList(),
                            Nota = "Los puntos interiores (azul) marcan las esquinas internas del panel. Los puntos exteriores (rojo) marcan el perímetro exterior."
                        }
                        : null
                }
                ,
                DebugMurosRectos = _debugMuros
            };

            resultado.TotalEsquinasDetectadas = esquinasEnLReales;
            resultado.Mensaje = $"Se detectaron {esquinasEnLReales} esquina(s) en L, {panelesValidos.Count} panel(es) válido(s), {panelesInvalidos.Count} panel(es) inválido(s)";

            // 💾 GUARDAR JSON COMPLETO CON PARALELAS
            GuardarJSON(infoCompletaConParalelas);
            GuardarJSONDiagnosticoMuros(new
            {
                FechaAnalisis = DateTime.Now,
                AlturaMuroMm = _alturaMuroMm,
                TotalLineas = lineas.Count,
                TotalEsquinasL = resultado.TotalEsquinasDetectadas,
                TotalMurosRectos = resultado.TotalMurosRectos,
                TotalPolilineasSalida = resultado.PolilineasADibujar?.Count ?? 0,
                TotalPuntosSalida = resultado.PuntosADibujar?.Count ?? 0,
                DebugMurosRectos = _debugMuros,
                LineasEntrada = lineas.Select((l, idx) => new
                {
                    Indice = idx,
                    l.Tipo,
                    Inicio = new { l.InicioX, l.InicioY, l.InicioZ },
                    Fin = new { l.FinX, l.FinY, l.FinZ },
                    l.Longitud,
                    l.Layer,
                    l.Color
                }).ToList(),
                EsquinasL = resultado.Esquinas.Select((e, idx) => new
                {
                    Indice = idx,
                    Vertice = new { e.Vertice?.X, e.Vertice?.Y, e.Vertice?.Z },
                    e.Angulo,
                    e.IndiceLinea1,
                    e.IndiceLinea2
                }).ToList(),
                PuntosDibujo = resultado.PuntosADibujar.Select((p, idx) => new
                {
                    Indice = idx,
                    p.TipoPunto,
                    p.Forma,
                    p.ColorIndex,
                    p.Tamano,
                    p.X,
                    p.Y,
                    p.Z
                }).ToList(),
                PolilineasSalida = resultado.PolilineasADibujar.Select((pl, idx) => new
                {
                    Indice = idx,
                    pl.Capa,
                    pl.ColorIndex,
                    pl.Cerrada,
                    pl.AlturaExtrusion,
                    Vertices = pl.Vertices?.Select(v => new { v.X, v.Y, v.Z }).ToList()
                }).ToList()
            });

            return resultado;
        }

        /// <summary>
        /// Determina si dos líneas son paralelas (mismo ángulo de inclinación)
        /// </summary>
        private bool SonLineasParalelas(LineaDTO linea1, LineaDTO linea2)
        {
            const double TOLERANCIA_ANGULO = 0.1; // Grados de tolerancia

            double angulo1 = Math.Atan2(linea1.FinY - linea1.InicioY, linea1.FinX - linea1.InicioX) * (180.0 / Math.PI);
            double angulo2 = Math.Atan2(linea2.FinY - linea2.InicioY, linea2.FinX - linea2.InicioX) * (180.0 / Math.PI);

            // Normalizar ángulos a [0, 180)
            if (angulo1 < 0) angulo1 += 180;
            if (angulo2 < 0) angulo2 += 180;

            double diferencia = Math.Abs(angulo1 - angulo2);

            // Son paralelas si tienen el mismo ángulo o difieren en 180°
            return diferencia <= TOLERANCIA_ANGULO || Math.Abs(diferencia - 180) <= TOLERANCIA_ANGULO;
        }

        /// <summary>
        /// Determina si dos líneas son perpendiculares (ángulo de 90° entre ellas)
        /// </summary>
        private bool SonLineasPerpendiculares(LineaDTO linea1, LineaDTO linea2)
        {
            const double TOLERANCIA_ANGULO = 1.0; // Grados de tolerancia

            double angulo1 = Math.Atan2(linea1.FinY - linea1.InicioY, linea1.FinX - linea1.InicioX) * (180.0 / Math.PI);
            double angulo2 = Math.Atan2(linea2.FinY - linea2.InicioY, linea2.FinX - linea2.InicioX) * (180.0 / Math.PI);

            // Calcular diferencia de ángulos
            double diferencia = Math.Abs(angulo1 - angulo2);

            // Normalizar a [0, 180]
            if (diferencia > 180) diferencia = 360 - diferencia;

            // Son perpendiculares si el ángulo está cerca de 90° o 270°
            return Math.Abs(diferencia - 90) <= TOLERANCIA_ANGULO || Math.Abs(diferencia - 270) <= TOLERANCIA_ANGULO;
        }

        /// <summary>
        /// Calcula la distancia perpendicular entre dos líneas paralelas
        /// </summary>
        private double CalcularDistanciaEntreLineasParalelas(LineaDTO linea1, LineaDTO linea2)
        {
            // Simplificación: distancia del punto inicial de línea2 a la línea1
            double x1 = linea1.InicioX;
            double y1 = linea1.InicioY;
            double x2 = linea1.FinX;
            double y2 = linea1.FinY;
            double x0 = linea2.InicioX;
            double y0 = linea2.InicioY;

            // Distancia de un punto a una línea
            double numerador = Math.Abs((y2 - y1) * x0 - (x2 - x1) * y0 + x2 * y1 - y2 * x1);
            double denominador = Math.Sqrt(Math.Pow(y2 - y1, 2) + Math.Pow(x2 - x1, 2));

            return denominador > 0 ? numerador / denominador : 0;
        }

        /// <summary>
        /// Obtiene la orientación de una línea (Horizontal, Vertical, Diagonal)
        /// </summary>
        private string ObtenerOrientacion(LineaDTO linea)
        {
            const double TOLERANCIA_ANGULO = 1.0; // Grados

            double dx = linea.FinX - linea.InicioX;
            double dy = linea.FinY - linea.InicioY;

            if (Math.Abs(dy) <= TOLERANCIA_ANGULO) return "Horizontal";
            if (Math.Abs(dx) <= TOLERANCIA_ANGULO) return "Vertical";

            return "Diagonal";
        }

        /// <summary>
        /// Calcula distancia euclidiana 2D entre dos puntos
        /// </summary>
        private double Distancia(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        /// <summary>
        /// Guarda el JSON en C:\temp\conexiones.json
        /// </summary>
        private void GuardarJSON(object datos)
        {
            try
            {
                string carpeta = @"C:\temp";
                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                string archivo = Path.Combine(carpeta, "conexiones.json");
                string json = JsonConvert.SerializeObject(datos, Formatting.Indented);

                File.WriteAllText(archivo, json);

                System.Diagnostics.Debug.WriteLine($"✅ JSON guardado en: {archivo}");
                Console.WriteLine($"✅ JSON guardado en: {archivo}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error guardando JSON: {ex.Message}");
                Console.WriteLine($"❌ Error guardando JSON: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene los 4 vértices únicos de un rectángulo formado por 4 líneas.
        /// </summary>
        private List<PuntoDTO> ObtenerVerticesRectangulo(LineaDTO l1a, LineaDTO l1b, LineaDTO l2a, LineaDTO l2b)
        {
            // Recopilar todos los puntos de las 4 líneas
            var puntos = new List<(double X, double Y, double Z)>
            {
                (l1a.InicioX, l1a.InicioY, l1a.InicioZ),
                (l1a.FinX, l1a.FinY, l1a.FinZ),
                (l1b.InicioX, l1b.InicioY, l1b.InicioZ),
                (l1b.FinX, l1b.FinY, l1b.FinZ),
                (l2a.InicioX, l2a.InicioY, l2a.InicioZ),
                (l2a.FinX, l2a.FinY, l2a.FinZ),
                (l2b.InicioX, l2b.InicioY, l2b.InicioZ),
                (l2b.FinX, l2b.FinY, l2b.FinZ)
            };

            // Eliminar duplicados
            var puntosUnicos = new List<(double X, double Y, double Z)>();
            foreach (var p in puntos)
            {
                bool esDuplicado = false;
                foreach (var pu in puntosUnicos)
                {
                    if (Distancia(p.X, p.Y, pu.X, pu.Y) < TOLERANCIA)
                    {
                        esDuplicado = true;
                        break;
                    }
                }
                if (!esDuplicado)
                {
                    puntosUnicos.Add(p);
                }
            }

            // Convertir a PuntoDTO
            return puntosUnicos.Select(p => new PuntoDTO { X = p.X, Y = p.Y, Z = p.Z }).ToList();
        }

        /// <summary>
        /// Clasifica los 4 vértices de un rectángulo en interiores (los 2 más cercanos al centro)
        /// y exteriores (los 2 más lejanos al centro).
        /// </summary>
        private (List<PuntoDTO> Interior, List<PuntoDTO> Exterior) ClasificarVerticesInteriorExterior(List<PuntoDTO> vertices)
        {
            if (vertices.Count != 4)
            {
                // Fallback si no hay exactamente 4 vértices
                return (new List<PuntoDTO>(), new List<PuntoDTO>());
            }

            // Calcular centro del rectángulo
            double centroX = vertices.Average(v => v.X);
            double centroY = vertices.Average(v => v.Y);

            // Ordenar por distancia al centro
            var verticesOrdenados = vertices
                .Select(v => new { Punto = v, Distancia = Distancia(v.X, v.Y, centroX, centroY) })
                .OrderBy(x => x.Distancia)
                .ToList();

            // Los 2 puntos más cercanos al centro son "interiores" (las 2 esquinas internas del marco)
            // Los 2 puntos más lejanos al centro son "exteriores" (las 2 esquinas externas del marco)
            var interior = new List<PuntoDTO> { verticesOrdenados[0].Punto, verticesOrdenados[1].Punto };
            var exterior = new List<PuntoDTO> { verticesOrdenados[2].Punto, verticesOrdenados[3].Punto };

            return (interior, exterior);
        }

        /// <summary>
        /// Calcula el punto interior y exterior de una esquina L a partir de 2 pares de líneas paralelas perpendiculares.
        /// El punto interior es la intersección de las dos líneas más cercanas entre sí (caras interiores del muro).
        /// El punto exterior es la intersección de las dos líneas más alejadas entre sí (caras exteriores del muro).
        /// </summary>
        private (List<PuntoDTO> Interior, List<PuntoDTO> Exterior) CalcularPuntosEsquinaL(
            LineaDTO l1a, LineaDTO l1b,
            LineaDTO l2a, LineaDTO l2b)
        {
            // Centroide del grupo 2 para determinar qué línea del grupo 1 está más cerca
            double centroX_g2 = (l2a.InicioX + l2a.FinX + l2b.InicioX + l2b.FinX) / 4.0;
            double centroY_g2 = (l2a.InicioY + l2a.FinY + l2b.InicioY + l2b.FinY) / 4.0;

            // Centroide del grupo 1 para determinar qué línea del grupo 2 está más cerca
            double centroX_g1 = (l1a.InicioX + l1a.FinX + l1b.InicioX + l1b.FinX) / 4.0;
            double centroY_g1 = (l1a.InicioY + l1a.FinY + l1b.InicioY + l1b.FinY) / 4.0;

            // Línea interior del grupo 1 = la más cercana al grupo 2
            double dist_l1a = DistanciaLineaPunto(l1a, centroX_g2, centroY_g2);
            double dist_l1b = DistanciaLineaPunto(l1b, centroX_g2, centroY_g2);
            LineaDTO innerG1 = dist_l1a <= dist_l1b ? l1a : l1b;
            LineaDTO outerG1 = dist_l1a <= dist_l1b ? l1b : l1a;

            // Línea interior del grupo 2 = la más cercana al grupo 1
            double dist_l2a = DistanciaLineaPunto(l2a, centroX_g1, centroY_g1);
            double dist_l2b = DistanciaLineaPunto(l2b, centroX_g1, centroY_g1);
            LineaDTO innerG2 = dist_l2a <= dist_l2b ? l2a : l2b;
            LineaDTO outerG2 = dist_l2a <= dist_l2b ? l2b : l2a;

            var interiores = new List<PuntoDTO>();
            var exteriores = new List<PuntoDTO>();

            // Intersección de líneas interiores → punto interior de la esquina
            var ptInterior = IntersectarLineas(innerG1, innerG2);
            if (ptInterior.HasValue && PuntoEnSegmento(ptInterior.Value.X, ptInterior.Value.Y, innerG1) && PuntoEnSegmento(ptInterior.Value.X, ptInterior.Value.Y, innerG2))
                interiores.Add(new PuntoDTO { X = ptInterior.Value.X, Y = ptInterior.Value.Y, Z = 0 });

            // Intersección de líneas exteriores → punto exterior de la esquina
            var ptExterior = IntersectarLineas(outerG1, outerG2);
            if (ptExterior.HasValue && PuntoEnSegmento(ptExterior.Value.X, ptExterior.Value.Y, outerG1) && PuntoEnSegmento(ptExterior.Value.X, ptExterior.Value.Y, outerG2))
                exteriores.Add(new PuntoDTO { X = ptExterior.Value.X, Y = ptExterior.Value.Y, Z = 0 });

            return (interiores, exteriores);
        }

        /// <summary>
        /// US-688 T5 (#693) — Detecta pares de paneles L que comparten exactamente la misma
        /// pareja de líneas inner/outer en un eje (H o V). Cada par corresponde a un muro recto
        /// entre dos esquinas (muros B y C en Muro_Recto3.png).
        ///
        /// Por cada par se emite 2 PolilineaDTO (patrón US-679):
        ///   - Capa ObjetoDB2d,  AlturaExtrusion = 0
        ///   - Capa ModelDesing, AlturaExtrusion = 2700
        /// </summary>
        private void GenerarMurosRectosEntreEsquinas(List<PanelInfoMuro> paneles, DeteccionEsquinasLDTO resultado)
        {
            if (paneles == null || paneles.Count < 2) return;

            var paresUsados = new HashSet<string>();

            for (int i = 0; i < paneles.Count; i++)
            {
                for (int j = i + 1; j < paneles.Count; j++)
                {
                    var pA = paneles[i];
                    var pB = paneles[j];

                    // --- Muro HORIZONTAL compartido: ambos paneles usan el MISMO innerH y MISMO outerH
                    if (SonMismaRectaDeMuro(pA.InnerH, pB.InnerH) && SonMismaRectaDeMuro(pA.OuterH, pB.OuterH))
                    {
                        string clave = "H-" + Math.Min(i, j) + "-" + Math.Max(i, j);
                        if (!paresUsados.Contains(clave) &&
                            pA.Verde != null && pA.Blanco != null && pB.Verde != null && pB.Blanco != null)
                        {
                            paresUsados.Add(clave);
                            var vertices = new List<PuntoDTO> { pA.Verde, pA.Blanco, pB.Blanco, pB.Verde };
                            AgregarMuroRecto(resultado, vertices);
                            AgregarMarcadoresVerticesMuro(resultado, vertices);
                        }
                    }

                    // --- Muro VERTICAL compartido: ambos paneles usan el MISMO innerV y MISMO outerV
                    if (SonMismaRectaDeMuro(pA.InnerV, pB.InnerV) && SonMismaRectaDeMuro(pA.OuterV, pB.OuterV))
                    {
                        string clave = "V-" + Math.Min(i, j) + "-" + Math.Max(i, j);
                        if (!paresUsados.Contains(clave) &&
                            pA.Amarillo != null && pA.Cian != null && pB.Amarillo != null && pB.Cian != null)
                        {
                            paresUsados.Add(clave);
                            var vertices = new List<PuntoDTO> { pA.Amarillo, pA.Cian, pB.Cian, pB.Amarillo };
                            AgregarMuroRecto(resultado, vertices);
                            AgregarMarcadoresVerticesMuro(resultado, vertices);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tipo 1: detecta muros rectos delimitados por esquina en inicio y esquina en final.
        /// Se construye por "estaciones" de esquina comunes sobre pares de líneas paralelas.
        /// </summary>
        private void GenerarMurosTipo1DesdeEsquinas(
            List<LineaDTO> lineas,
            List<EsquinaLDTO> esquinas,
            List<PuntoDTO> puntosReferencia,
            DeteccionEsquinasLDTO resultado)
        {
            if (lineas == null || lineas.Count < 2 || esquinas == null || esquinas.Count == 0) return;

            const double COS_PARALELO_MIN = 0.999;
            const double TOL_ESTACION = 5.0;   // mm
            const double LONG_MURO_MINIMA = 600.0;

            var claves = new HashSet<string>();
            var candidatas = lineas.Where(l => l != null && l.Tipo == "Line").ToList();

            for (int i = 0; i < candidatas.Count; i++)
            {
                for (int j = i + 1; j < candidatas.Count; j++)
                {
                    var lA = candidatas[i];
                    var lB = candidatas[j];

                    double dxA = lA.FinX - lA.InicioX, dyA = lA.FinY - lA.InicioY;
                    double dxB = lB.FinX - lB.InicioX, dyB = lB.FinY - lB.InicioY;
                    double normA = Math.Sqrt(dxA * dxA + dyA * dyA);
                    double normB = Math.Sqrt(dxB * dxB + dyB * dyB);
                    if (normA < TOLERANCIA || normB < TOLERANCIA)
                    {
                        RegistrarDebugMuro("Tipo1", "Tipo1_AmbosExtremosConectados", "Descartado",
                            "Linea degenerada", lA, lB, null);
                        continue;
                    }

                    double cosAng = Math.Abs((dxA * dxB + dyA * dyB) / (normA * normB));
                    if (cosAng < COS_PARALELO_MIN)
                    {
                        RegistrarDebugMuro("Tipo1", "Tipo1_AmbosExtremosConectados", "Descartado",
                            "No son paralelas", lA, lB, new { cosAng, COS_PARALELO_MIN });
                        continue;
                    }

                    double dist = CalcularDistanciaEntreLineasParalelas(lA, lB);
                    if (dist < OFFSET_MINIMO_PANEL || dist > OFFSET_MAXIMO_PANEL)
                    {
                        RegistrarDebugMuro("Tipo1", "Tipo1_AmbosExtremosConectados", "Descartado",
                            "Distancia entre caras fuera de rango", lA, lB, new { dist, OFFSET_MINIMO_PANEL, OFFSET_MAXIMO_PANEL });
                        continue;
                    }

                    double ux = dxA / normA, uy = dyA / normA;

                    var estacionesA = new List<double>();
                    var estacionesB = new List<double>();
                    var fuentes = new List<PuntoDTO>();
                    fuentes.AddRange(esquinas.Where(e => e?.Vertice != null).Select(e => e.Vertice));
                    if (puntosReferencia != null && puntosReferencia.Count > 0)
                        fuentes.AddRange(puntosReferencia.Where(p => p != null));

                    foreach (var p in EliminarPuntosDuplicados(fuentes))
                    {
                        if (PuntoSobreSegmentoConTolerancia(p, lA))
                        {
                            double t = (p.X - lA.InicioX) * ux + (p.Y - lA.InicioY) * uy;
                            estacionesA.Add(t);
                        }
                        if (PuntoSobreSegmentoConTolerancia(p, lB))
                        {
                            double t = (p.X - lA.InicioX) * ux + (p.Y - lA.InicioY) * uy;
                            estacionesB.Add(t);
                        }
                    }

                    if (estacionesA.Count < 2 || estacionesB.Count < 2)
                    {
                        RegistrarDebugMuro("Tipo1", "Tipo1_AmbosExtremosConectados", "Descartado",
                            "Insuficientes estaciones por cara", lA, lB, new { estacionesA = estacionesA.Count, estacionesB = estacionesB.Count });
                        continue;
                    }

                    // Estaciones válidas = existen en ambas líneas (misma sección transversal de esquina).
                    var estacionesComunes = new List<double>();
                    foreach (var ta in estacionesA)
                    {
                        bool existeEnB = estacionesB.Any(tb => Math.Abs(tb - ta) <= TOL_ESTACION);
                        if (!existeEnB) continue;
                        if (!estacionesComunes.Any(t => Math.Abs(t - ta) <= TOL_ESTACION))
                            estacionesComunes.Add(ta);
                    }

                    if (estacionesComunes.Count < 2)
                    {
                        RegistrarDebugMuro("Tipo1", "Tipo1_AmbosExtremosConectados", "Descartado",
                            "No hay estaciones comunes entre caras", lA, lB, new { estacionesA, estacionesB });
                        continue;
                    }
                    estacionesComunes.Sort();

                    for (int k = 0; k < estacionesComunes.Count - 1; k++)
                    {
                        double t0 = estacionesComunes[k];
                        double t1 = estacionesComunes[k + 1];
                        if ((t1 - t0) < LONG_MURO_MINIMA)
                        {
                            RegistrarDebugMuro("Tipo1", "Tipo1_AmbosExtremosConectados", "Descartado",
                                "Longitud menor al mínimo", lA, lB, new { t0, t1, largo = t1 - t0, LONG_MURO_MINIMA });
                            continue;
                        }

                        var a0 = new PuntoDTO { X = lA.InicioX + ux * t0, Y = lA.InicioY + uy * t0, Z = lA.InicioZ };
                        var a1 = new PuntoDTO { X = lA.InicioX + ux * t1, Y = lA.InicioY + uy * t1, Z = lA.InicioZ };

                        double bTIni = (lB.InicioX - lA.InicioX) * ux + (lB.InicioY - lA.InicioY) * uy;
                        var b0 = new PuntoDTO { X = lB.InicioX + ux * (t0 - bTIni), Y = lB.InicioY + uy * (t0 - bTIni), Z = lB.InicioZ };
                        var b1 = new PuntoDTO { X = lB.InicioX + ux * (t1 - bTIni), Y = lB.InicioY + uy * (t1 - bTIni), Z = lB.InicioZ };

                        string clave = $"{ClaveParLineas(lA, lB)}|{Math.Round(t0, 2):F2}|{Math.Round(t1, 2):F2}";
                        if (claves.Contains(clave))
                        {
                            RegistrarDebugMuro("Tipo1", "Tipo1_AmbosExtremosConectados", "Descartado",
                                "Duplicado por clave", lA, lB, new { clave });
                            continue;
                        }
                        claves.Add(clave);

                        var vertices = new List<PuntoDTO> { a0, a1, b1, b0 };
                        AgregarMuroRecto(resultado, vertices);
                        AgregarMarcadoresVerticesMuro(resultado, vertices);
                        RegistrarDebugMuro("Tipo1", "Tipo1_AmbosExtremosConectados", "Generado",
                            "OK", lA, lB, new { clave, t0, t1, vertices = vertices.Select(v => new { v.X, v.Y, v.Z }).ToList() });
                    }
                }
            }
        }

        private bool PuntoSobreSegmentoConTolerancia(PuntoDTO p, LineaDTO l)
        {
            if (p == null || l == null) return false;
            const double TOL_PUNTO_LINEA = 2.0; // mm
            const double TOL_RANGO = 2.0;       // mm

            // Debe estar cerca de la recta.
            if (DistanciaLineaPunto(l, p.X, p.Y) > TOL_PUNTO_LINEA) return false;

            // Debe caer dentro del tramo del segmento (con holgura).
            double minX = Math.Min(l.InicioX, l.FinX) - TOL_RANGO;
            double maxX = Math.Max(l.InicioX, l.FinX) + TOL_RANGO;
            double minY = Math.Min(l.InicioY, l.FinY) - TOL_RANGO;
            double maxY = Math.Max(l.InicioY, l.FinY) + TOL_RANGO;
            return p.X >= minX && p.X <= maxX && p.Y >= minY && p.Y <= maxY;
        }

        /// <summary>
        /// Compara dos líneas por coordenadas de sus extremos (en cualquier orden).
        /// Necesario porque el mismo segmento puede aparecer referenciado como objetos distintos.
        /// </summary>
        private bool MismaLinea(LineaDTO a, LineaDTO b)
        {
            if (a == null || b == null) return false;
            if (ReferenceEquals(a, b)) return true;
            bool sameDir =
                Math.Abs(a.InicioX - b.InicioX) < TOLERANCIA && Math.Abs(a.InicioY - b.InicioY) < TOLERANCIA &&
                Math.Abs(a.FinX    - b.FinX)    < TOLERANCIA && Math.Abs(a.FinY    - b.FinY)    < TOLERANCIA;
            bool revDir =
                Math.Abs(a.InicioX - b.FinX)    < TOLERANCIA && Math.Abs(a.InicioY - b.FinY)    < TOLERANCIA &&
                Math.Abs(a.FinX    - b.InicioX) < TOLERANCIA && Math.Abs(a.FinY    - b.InicioY) < TOLERANCIA;
            return sameDir || revDir;
        }

        /// <summary>
        /// True si dos segmentos representan la misma recta de muro (colineales),
        /// aunque no tengan exactamente los mismos endpoints.
        /// </summary>
        private bool SonMismaRectaDeMuro(LineaDTO a, LineaDTO b)
        {
            if (a == null || b == null) return false;
            if (MismaLinea(a, b)) return true;

            double dxA = a.FinX - a.InicioX, dyA = a.FinY - a.InicioY;
            double dxB = b.FinX - b.InicioX, dyB = b.FinY - b.InicioY;
            double normA = Math.Sqrt(dxA * dxA + dyA * dyA);
            double normB = Math.Sqrt(dxB * dxB + dyB * dyB);
            if (normA < TOLERANCIA || normB < TOLERANCIA) return false;

            // Deben ser prácticamente paralelas.
            double cosAng = Math.Abs((dxA * dxB + dyA * dyB) / (normA * normB));
            if (cosAng < 0.9999) return false;

            // Y estar sobre la misma recta (distancia casi cero).
            const double TOL_COLINEAL = 2.0; // mm
            double d1 = DistanciaLineaPunto(a, b.InicioX, b.InicioY);
            double d2 = DistanciaLineaPunto(a, b.FinX, b.FinY);
            double d3 = DistanciaLineaPunto(b, a.InicioX, a.InicioY);
            double d4 = DistanciaLineaPunto(b, a.FinX, a.FinY);
            return d1 <= TOL_COLINEAL && d2 <= TOL_COLINEAL && d3 <= TOL_COLINEAL && d4 <= TOL_COLINEAL;
        }

        /// <summary>
        /// Emite las 2 polilíneas de un muro recto (ObjetoDB2d plana + ModelDesing extruida).
        /// US-697 — la altura de extrusión procede de _alturaMuroMm e incrementa TotalMurosRectos.
        /// </summary>
        private void AgregarMuroRecto(DeteccionEsquinasLDTO resultado, List<PuntoDTO> vertices)
        {
            resultado.TotalMurosRectos++;
            resultado.PolilineasADibujar.Add(new PolilineaDTO
            {
                Cerrada         = true,
                Capa            = "ObjetoDB2d",
                ColorIndex      = 256,
                AlturaExtrusion = 0,
                Vertices        = vertices
            });

            resultado.PolilineasADibujar.Add(new PolilineaDTO
            {
                Cerrada         = true,
                Capa            = "ModelDesing",
                ColorIndex      = 256,
                AlturaExtrusion = _alturaMuroMm,
                Vertices        = vertices
            });
        }

        /// <summary>
        /// US-688 T1 — Emite 4 marcadores tipo "Cuadrado" (uno por vértice del muro recto).
        /// Tamaño = 100 mm (la mitad del radio del círculo por defecto = 200 mm).
        /// Cada posición usa un ColorIndex distintivo que NO colisiona con los colores de los
        /// puntos de esquina (5=azul, 1=rojo, 3=verde, 2=amarillo, 7=blanco, 4=cian, 6=magenta, 9=gris).
        /// </summary>
        private void AgregarMarcadoresVerticesMuro(DeteccionEsquinasLDTO resultado, List<PuntoDTO> verticesMuro)
        {
            // 4 colores distintivos por posición de vértice del muro (paleta ZWCAD)
            int[] coloresVerticeMuro = { 30, 140, 210, 90 }; // naranja, púrpura, magenta-oscuro, turquesa
            const double LADO_SEMI = 20.0; // 20% del original (100 mm) — coherente con cliente

            for (int k = 0; k < verticesMuro.Count && k < 4; k++)
            {
                var v = verticesMuro[k];
                if (v == null) continue;
                resultado.PuntosADibujar.Add(new PuntoDTO
                {
                    X          = v.X,
                    Y          = v.Y,
                    Z          = v.Z,
                    TipoPunto  = "VerticeMuro",
                    ColorIndex = coloresVerticeMuro[k],
                    Forma      = "Cuadrado",
                    Tamano     = LADO_SEMI
                });
            }
        }

        /// <summary>
        /// US-688 T6 (#694) — Detecta muros rectos donde UN extremo nace de una esquina L
        /// y el OTRO extremo es libre (sin conexión con ningún otro segmento del input).
        ///
        /// Cubre los muros A, CC (espejo de A), D y Cara E (espejo de D) del documento
        /// "Muro_Recto 1 esquina.png".
        ///
        /// Por cada panel y cada eje (H, V):
        ///   1. Localiza extremo "L" (cerca de ptAzul/ptRojo) vs extremo "libre" (lejos)
        ///   2. Verifica que ambos extremos libres (inner+outer) NO conectan con otros segmentos
        ///   3. Verifica que el muro tiene longitud suficiente (descarta el simple brazo de la L)
        ///   4. Construye el rectángulo con los 4 vértices y emite las 2 polilíneas + 4 cuadrados
        /// </summary>
        private void GenerarMurosLConExtremoLibre(
            List<PanelInfoMuro> paneles, List<LineaDTO> lineas, DeteccionEsquinasLDTO resultado)
        {
            if (paneles == null || paneles.Count == 0) return;

            const double LONG_MURO_MINIMA = 600.0; // mm, evita falsos positivos en brazos cortos de la L

            foreach (var panel in paneles)
            {
                // --- Eje HORIZONTAL: innerH + outerH ---
                if (panel.InnerH != null && panel.OuterH != null &&
                    panel.Verde   != null && panel.Blanco   != null)
                {
                    var freeEndInner = ExtremoLejano(panel.InnerH, panel.AzulX, panel.AzulY);
                    var freeEndOuter = ExtremoLejano(panel.OuterH, panel.RojoX, panel.RojoY);

                    double longInner = Distancia(freeEndInner.X, freeEndInner.Y, panel.AzulX, panel.AzulY);
                    double longOuter = Distancia(freeEndOuter.X, freeEndOuter.Y, panel.RojoX, panel.RojoY);

                    if (longInner >= LONG_MURO_MINIMA && longOuter >= LONG_MURO_MINIMA &&
                        EsExtremoLibre(freeEndInner.X, freeEndInner.Y, lineas, panel.InnerH) &&
                        EsExtremoLibre(freeEndOuter.X, freeEndOuter.Y, lineas, panel.OuterH))
                    {
                        freeEndInner.TipoPunto = "VerticeMuroLibre";
                        freeEndOuter.TipoPunto = "VerticeMuroLibre";

                        var vertices = new List<PuntoDTO> { panel.Verde, freeEndInner, freeEndOuter, panel.Blanco };
                        AgregarMuroRecto(resultado, vertices);
                        AgregarMarcadoresVerticesMuro(resultado, vertices);
                    }
                }

                // --- Eje VERTICAL: innerV + outerV ---
                if (panel.InnerV != null && panel.OuterV != null &&
                    panel.Amarillo != null && panel.Cian != null)
                {
                    var freeEndInner = ExtremoLejano(panel.InnerV, panel.AzulX, panel.AzulY);
                    var freeEndOuter = ExtremoLejano(panel.OuterV, panel.RojoX, panel.RojoY);

                    double longInner = Distancia(freeEndInner.X, freeEndInner.Y, panel.AzulX, panel.AzulY);
                    double longOuter = Distancia(freeEndOuter.X, freeEndOuter.Y, panel.RojoX, panel.RojoY);

                    if (longInner >= LONG_MURO_MINIMA && longOuter >= LONG_MURO_MINIMA &&
                        EsExtremoLibre(freeEndInner.X, freeEndInner.Y, lineas, panel.InnerV) &&
                        EsExtremoLibre(freeEndOuter.X, freeEndOuter.Y, lineas, panel.OuterV))
                    {
                        freeEndInner.TipoPunto = "VerticeMuroLibre";
                        freeEndOuter.TipoPunto = "VerticeMuroLibre";

                        var vertices = new List<PuntoDTO> { panel.Amarillo, freeEndInner, freeEndOuter, panel.Cian };
                        AgregarMuroRecto(resultado, vertices);
                        AgregarMarcadoresVerticesMuro(resultado, vertices);
                    }
                }
            }
        }

        /// <summary>
        /// Devuelve como PuntoDTO el extremo de una línea más LEJANO al punto de referencia.
        /// </summary>
        private PuntoDTO ExtremoLejano(LineaDTO linea, double refX, double refY)
        {
            double dIni = Distancia(linea.InicioX, linea.InicioY, refX, refY);
            double dFin = Distancia(linea.FinX,    linea.FinY,    refX, refY);
            return dFin >= dIni
                ? new PuntoDTO { X = linea.FinX,    Y = linea.FinY,    Z = linea.FinZ    }
                : new PuntoDTO { X = linea.InicioX, Y = linea.InicioY, Z = linea.InicioZ };
        }

        /// <summary>
        /// True si el punto (x, y) NO coincide con el endpoint de ningún otro segmento del input.
        /// Excluye <paramref name="excluir"/> (la propia línea cuyo extremo se está chequeando).
        /// </summary>
        private bool EsExtremoLibre(double x, double y, List<LineaDTO> lineas, LineaDTO excluir)
        {
            const double TOL_CONEXION = 1.0; // 1 mm — algo más laxo que TOLERANCIA para tolerar dibujo manual
            foreach (var l in lineas)
            {
                if (ReferenceEquals(l, excluir)) continue;
                if (Distancia(x, y, l.InicioX, l.InicioY) < TOL_CONEXION) return false;
                if (Distancia(x, y, l.FinX,    l.FinY)    < TOL_CONEXION) return false;
            }
            return true;
        }

        /// <summary>
        /// US-688 T7 (#695) — Detecta muros rectos formados por DOS LÍNEAS PARALELAS independientes,
        /// sin ninguna esquina L conectada y con los 4 endpoints libres (muros E y F).
        ///
        /// Filtros aplicados a cada par (i, j) de líneas:
        ///   1. Ninguna de las 2 líneas pertenece a un panel L ya procesado.
        ///   2. Son paralelas (coseno del ángulo entre direcciones |·| ≥ 0.999).
        ///   3. Distancia perpendicular entre OFFSET_MINIMO_PANEL y OFFSET_MAXIMO_PANEL.
        ///   4. Se solapan longitudinalmente (>= 100 mm de proyección común).
        ///   5. Los 4 endpoints son extremos libres (no conectan con ninguna otra línea).
        ///
        /// Si pasa todos los filtros, se construye el rectángulo emparejando endpoints por lado
        /// (proyección sobre el eje del muro) y se emite con el patrón estándar.
        /// </summary>
        private void GenerarMurosLibresAislados(
            List<PanelInfoMuro> paneles, List<LineaDTO> lineas, DeteccionEsquinasLDTO resultado)
        {
            if (lineas == null || lineas.Count < 2) return;

            // Conjunto de líneas YA usadas como cara de panel L (las descartamos)
            var lineasDePaneles = new HashSet<LineaDTO>();
            if (paneles != null)
            {
                foreach (var p in paneles)
                {
                    if (p.InnerH != null) lineasDePaneles.Add(p.InnerH);
                    if (p.OuterH != null) lineasDePaneles.Add(p.OuterH);
                    if (p.InnerV != null) lineasDePaneles.Add(p.InnerV);
                    if (p.OuterV != null) lineasDePaneles.Add(p.OuterV);
                }
            }

            // Sólo consideramos líneas simples (no polilíneas)
            var candidatas = lineas.Where(l => l.Tipo == "Line" && !lineasDePaneles.Contains(l)).ToList();

            const double COS_PARALELO_MIN  = 0.999; // ~2.5° de tolerancia angular
            const double SOLAPAMIENTO_MIN  = 100.0; // mm
            var paresUsados = new HashSet<string>();

            for (int i = 0; i < candidatas.Count; i++)
            {
                for (int j = i + 1; j < candidatas.Count; j++)
                {
                    var lA = candidatas[i];
                    var lB = candidatas[j];

                    // 2. Paralelismo
                    double dxA = lA.FinX - lA.InicioX, dyA = lA.FinY - lA.InicioY;
                    double dxB = lB.FinX - lB.InicioX, dyB = lB.FinY - lB.InicioY;
                    double normA = Math.Sqrt(dxA * dxA + dyA * dyA);
                    double normB = Math.Sqrt(dxB * dxB + dyB * dyB);
                    if (normA < TOLERANCIA || normB < TOLERANCIA) continue;
                    double cosAng = Math.Abs((dxA * dxB + dyA * dyB) / (normA * normB));
                    if (cosAng < COS_PARALELO_MIN) continue;

                    // 3. Distancia perpendicular dentro de rango de espesor de muro
                    double dist = CalcularDistanciaEntreLineasParalelas(lA, lB);
                    if (dist < OFFSET_MINIMO_PANEL || dist > OFFSET_MAXIMO_PANEL) continue;

                    // 4. Solapamiento longitudinal: proyectamos los endpoints de B sobre la dirección de A
                    double ux = dxA / normA, uy = dyA / normA;
                    double a0 = 0, a1 = normA; // proyecciones de los extremos de A sobre su eje (medido desde su Inicio)
                    double b0 = (lB.InicioX - lA.InicioX) * ux + (lB.InicioY - lA.InicioY) * uy;
                    double b1 = (lB.FinX    - lA.InicioX) * ux + (lB.FinY    - lA.InicioY) * uy;
                    double bMin = Math.Min(b0, b1), bMax = Math.Max(b0, b1);
                    double overlap = Math.Min(a1, bMax) - Math.Max(a0, bMin);
                    if (overlap < SOLAPAMIENTO_MIN) continue;

                    // 5. Los 4 endpoints son libres
                    if (!EsExtremoLibre(lA.InicioX, lA.InicioY, lineas, lA)) continue;
                    if (!EsExtremoLibre(lA.FinX,    lA.FinY,    lineas, lA)) continue;
                    if (!EsExtremoLibre(lB.InicioX, lB.InicioY, lineas, lB)) continue;
                    if (!EsExtremoLibre(lB.FinX,    lB.FinY,    lineas, lB)) continue;

                    // Evitar duplicados (mismo par procesado dos veces)
                    string clave = i + "-" + j;
                    if (paresUsados.Contains(clave)) continue;
                    paresUsados.Add(clave);

                    // Construir rectángulo emparejando endpoints "Inicio" de B con el extremo de A
                    // que esté en el mismo lado (proyección menor o mayor sobre el eje).
                    bool bInvertido = b0 > b1; // si Inicio de B está más lejos en el eje que Fin de B
                    var pA0 = new PuntoDTO { X = lA.InicioX, Y = lA.InicioY, Z = lA.InicioZ };
                    var pA1 = new PuntoDTO { X = lA.FinX,    Y = lA.FinY,    Z = lA.FinZ    };
                    var pB0 = new PuntoDTO { X = lB.InicioX, Y = lB.InicioY, Z = lB.InicioZ };
                    var pB1 = new PuntoDTO { X = lB.FinX,    Y = lB.FinY,    Z = lB.FinZ    };

                    // Vértices: A0 → A1 → (extremo de B en mismo lado que A1) → (extremo de B en mismo lado que A0)
                    var vertices = bInvertido
                        ? new List<PuntoDTO> { pA0, pA1, pB0, pB1 }
                        : new List<PuntoDTO> { pA0, pA1, pB1, pB0 };

                    AgregarMuroRecto(resultado, vertices);
                    AgregarMarcadoresVerticesMuro(resultado, vertices);
                }
            }
        }

        /// <summary>
        /// Detecta pares de líneas paralelas que forman un muro con un único extremo libre.
        /// Útil para trazados abiertos donde faltan muros A/D por emparejado de paneles.
        /// </summary>
        private void GenerarMurosConUnExtremoLibreDesdeLineas(List<LineaDTO> lineas, DeteccionEsquinasLDTO resultado, List<PuntoDTO> puntosReferencia)
        {
            if (lineas == null || lineas.Count < 2) return;

            const double COS_PARALELO_MIN = 0.999;
            const double SOLAPAMIENTO_MIN = 300.0;
            const double LONG_MURO_MINIMA = 600.0;
            var paresUsados = new HashSet<string>();
            var candidatas = lineas.Where(l => l != null && l.Tipo == "Line").ToList();
            var referencias = (puntosReferencia ?? new List<PuntoDTO>()).Where(p => p != null).ToList();

            for (int i = 0; i < candidatas.Count; i++)
            {
                for (int j = i + 1; j < candidatas.Count; j++)
                {
                    var lA = candidatas[i];
                    var lB = candidatas[j];

                    double dxA = lA.FinX - lA.InicioX, dyA = lA.FinY - lA.InicioY;
                    double dxB = lB.FinX - lB.InicioX, dyB = lB.FinY - lB.InicioY;
                    double normA = Math.Sqrt(dxA * dxA + dyA * dyA);
                    double normB = Math.Sqrt(dxB * dxB + dyB * dyB);
                    if (normA < TOLERANCIA || normB < TOLERANCIA) continue;

                    double cosAng = Math.Abs((dxA * dxB + dyA * dyB) / (normA * normB));
                    if (cosAng < COS_PARALELO_MIN) continue;

                    double dist = CalcularDistanciaEntreLineasParalelas(lA, lB);
                    if (dist < OFFSET_MINIMO_PANEL || dist > OFFSET_MAXIMO_PANEL) continue;

                    double ux = dxA / normA, uy = dyA / normA;
                    var aIni = new PuntoDTO { X = lA.InicioX, Y = lA.InicioY, Z = lA.InicioZ };
                    var aFin = new PuntoDTO { X = lA.FinX, Y = lA.FinY, Z = lA.FinZ };
                    var bIni = new PuntoDTO { X = lB.InicioX, Y = lB.InicioY, Z = lB.InicioZ };
                    var bFin = new PuntoDTO { X = lB.FinX, Y = lB.FinY, Z = lB.FinZ };

                    double aTIni = 0.0;
                    double aTFin = normA;
                    double bTIni = (bIni.X - lA.InicioX) * ux + (bIni.Y - lA.InicioY) * uy;
                    double bTFin = (bFin.X - lA.InicioX) * ux + (bFin.Y - lA.InicioY) * uy;

                    double tMinA = Math.Min(aTIni, aTFin);
                    double tMaxA = Math.Max(aTIni, aTFin);
                    double tMinB = Math.Min(bTIni, bTFin);
                    double tMaxB = Math.Max(bTIni, bTFin);
                    double tStart = Math.Max(tMinA, tMinB);
                    double tEnd = Math.Min(tMaxA, tMaxB);
                    double overlap = tEnd - tStart;
                    if (overlap < SOLAPAMIENTO_MIN) continue;

                    // Endpoints reales de cada línea (para detectar conexión/libre correctamente).
                    var aLowEnd = aTIni <= aTFin ? aIni : aFin;
                    var aHighEnd = aTIni <= aTFin ? aFin : aIni;
                    var bLowEnd = bTIni <= bTFin ? bIni : bFin;
                    var bHighEnd = bTIni <= bTFin ? bFin : bIni;

                    // Recortar al tramo de solape para que el muro nazca/muera en esquina.
                    var aLow = new PuntoDTO
                    {
                        X = lA.InicioX + ux * tStart,
                        Y = lA.InicioY + uy * tStart,
                        Z = lA.InicioZ
                    };
                    var aHigh = new PuntoDTO
                    {
                        X = lA.InicioX + ux * tEnd,
                        Y = lA.InicioY + uy * tEnd,
                        Z = lA.InicioZ
                    };
                    var bLow = new PuntoDTO
                    {
                        X = bIni.X + ux * (tStart - bTIni),
                        Y = bIni.Y + uy * (tStart - bTIni),
                        Z = bIni.Z
                    };
                    var bHigh = new PuntoDTO
                    {
                        X = bIni.X + ux * (tEnd - bTIni),
                        Y = bIni.Y + uy * (tEnd - bTIni),
                        Z = bIni.Z
                    };

                    bool lowLibre = EsExtremoLibre(aLowEnd.X, aLowEnd.Y, lineas, lA) && EsExtremoLibre(bLowEnd.X, bLowEnd.Y, lineas, lB);
                    bool highLibre = EsExtremoLibre(aHighEnd.X, aHighEnd.Y, lineas, lA) && EsExtremoLibre(bHighEnd.X, bHighEnd.Y, lineas, lB);
                    bool lowConectado = !lowLibre;
                    bool highConectado = !highLibre;

                    // Tipificación explícita solicitada:
                    // Tipo 1: conectado-conectado, Tipo 2: conectado-libre,
                    // Tipo 3: libre-conectado, Tipo 4: libre-libre.
                    TipoMuroRecto tipoMuro;
                    if (lowConectado && highConectado) tipoMuro = TipoMuroRecto.Tipo1_AmbosExtremosConectados;
                    else if (lowConectado && !highConectado) tipoMuro = TipoMuroRecto.Tipo2_InicioConectado_FinLibre;
                    else if (!lowConectado && highConectado) tipoMuro = TipoMuroRecto.Tipo3_InicioLibre_FinConectado;
                    else tipoMuro = TipoMuroRecto.Tipo4_AmbosExtremosLibres;

                    // El tipo 4 se resuelve en GenerarMurosLibresAislados para evitar duplicados.
                    if (tipoMuro == TipoMuroRecto.Tipo4_AmbosExtremosLibres)
                    {
                        RegistrarDebugMuro("TipificadoDesdeLineas", tipoMuro.ToString(), "Descartado",
                            "Tipo 4 se delega a muros aislados", lA, lB, null);
                        continue;
                    }

                    // Tipo 1/2/3: extremos conectados anclados a puntos de esquina detectados.
                    if (lowConectado)
                    {
                        if (TrySnapExtremoConectadoConPuntos(aLowEnd, lA, referencias, out var aLowSnap)) aLow = aLowSnap;
                        if (TrySnapExtremoConectadoConPuntos(bLowEnd, lB, referencias, out var bLowSnap)) bLow = bLowSnap;
                    }
                    if (highConectado)
                    {
                        if (TrySnapExtremoConectadoConPuntos(aHighEnd, lA, referencias, out var aHighSnap)) aHigh = aHighSnap;
                        if (TrySnapExtremoConectadoConPuntos(bHighEnd, lB, referencias, out var bHighSnap)) bHigh = bHighSnap;
                    }

                    if (Distancia(aLow.X, aLow.Y, aHigh.X, aHigh.Y) < LONG_MURO_MINIMA ||
                        Distancia(bLow.X, bLow.Y, bHigh.X, bHigh.Y) < LONG_MURO_MINIMA)
                    {
                        RegistrarDebugMuro("TipificadoDesdeLineas", tipoMuro.ToString(), "Descartado",
                            "Longitud menor al mínimo", lA, lB, new { LONG_MURO_MINIMA });
                        continue;
                    }

                    string clave = ClaveParLineas(lA, lB);
                    if (paresUsados.Contains(clave))
                    {
                        RegistrarDebugMuro("TipificadoDesdeLineas", tipoMuro.ToString(), "Descartado",
                            "Duplicado por clave", lA, lB, new { clave });
                        continue;
                    }
                    paresUsados.Add(clave);

                    var vertices = new List<PuntoDTO> { aLow, aHigh, bHigh, bLow };
                    AgregarMuroRecto(resultado, vertices);
                    AgregarMarcadoresVerticesMuro(resultado, vertices);
                    RegistrarDebugMuro("TipificadoDesdeLineas", tipoMuro.ToString(), "Generado",
                        "OK", lA, lB, new { clave, vertices = vertices.Select(v => new { v.X, v.Y, v.Z }).ToList() });
                }
            }
        }

        private void RegistrarDebugMuro(
            string metodo,
            string tipo,
            string estado,
            string motivo,
            LineaDTO lA,
            LineaDTO lB,
            object extra)
        {
            _debugMuros.Add(new DebugMuroRectoRegistro
            {
                Metodo = metodo,
                Tipo = tipo,
                Estado = estado,
                Motivo = motivo,
                ParLineas = $"{ClaveLinea(lA)} | {ClaveLinea(lB)}",
                Geometria = extra ?? new
                {
                    LineaA = new { lA?.InicioX, lA?.InicioY, lA?.FinX, lA?.FinY },
                    LineaB = new { lB?.InicioX, lB?.InicioY, lB?.FinX, lB?.FinY }
                }
            });
        }

        private string ClaveLinea(LineaDTO l)
        {
            if (l == null) return "null";
            return $"({l.InicioX:F3},{l.InicioY:F3})->({l.FinX:F3},{l.FinY:F3})";
        }

        private void GuardarJSONDiagnosticoMuros(object datos)
        {
            try
            {
                string carpeta = @"C:\temp";
                if (!Directory.Exists(carpeta))
                {
                    Directory.CreateDirectory(carpeta);
                }

                string archivo = Path.Combine(carpeta, "diagnostico_muros_rectos.json");
                string json = JsonConvert.SerializeObject(datos, Formatting.Indented);
                File.WriteAllText(archivo, json);
            }
            catch
            {
                // No interrumpir el flujo principal por fallo de diagnóstico.
            }
        }

        private string ClaveParLineas(LineaDTO a, LineaDTO b)
        {
            string ka = $"{Math.Min(a.InicioX, a.FinX):F3},{Math.Min(a.InicioY, a.FinY):F3}-{Math.Max(a.InicioX, a.FinX):F3},{Math.Max(a.InicioY, a.FinY):F3}";
            string kb = $"{Math.Min(b.InicioX, b.FinX):F3},{Math.Min(b.InicioY, b.FinY):F3}-{Math.Max(b.InicioX, b.FinX):F3},{Math.Max(b.InicioY, b.FinY):F3}";
            return string.CompareOrdinal(ka, kb) <= 0 ? $"{ka}|{kb}" : $"{kb}|{ka}";
        }

        private bool TrySnapExtremoConectadoConPuntos(PuntoDTO extremo, LineaDTO linea, List<PuntoDTO> referencias, out PuntoDTO snapped)
        {
            snapped = null;
            if (extremo == null || linea == null || referencias == null || referencias.Count == 0) return false;

            const double MAX_DIST_EXTREMO = 600.0;
            const double MAX_DIST_A_LINEA = 2.0;

            PuntoDTO mejor = null;
            double mejorDist = double.MaxValue;
            foreach (var p in referencias)
            {
                double dExt = Distancia(extremo.X, extremo.Y, p.X, p.Y);
                if (dExt > MAX_DIST_EXTREMO) continue;
                double dLinea = DistanciaLineaPunto(linea, p.X, p.Y);
                if (dLinea > MAX_DIST_A_LINEA) continue;
                if (!PuntoEnSegmento(p.X, p.Y, linea)) continue;
                if (dExt < mejorDist)
                {
                    mejorDist = dExt;
                    mejor = p;
                }
            }

            if (mejor == null) return false;
            snapped = new PuntoDTO { X = mejor.X, Y = mejor.Y, Z = mejor.Z };
            return true;
        }

        /// <summary>
        /// Calcula los 4 puntos de panel (US-668):
        ///   Verde    = ptAzul + 300mm brazo interior HORIZONTAL
        ///   Amarillo = ptAzul + 300mm brazo interior VERTICAL
        ///   Blanco   = ptRojo + (espV + 300mm) cara exterior HORIZONTAL
        ///   Cian     = ptRojo + (espH + 300mm) cara exterior VERTICAL
        /// </summary>
        private (PuntoDTO Verde, PuntoDTO Amarillo, PuntoDTO Blanco, PuntoDTO Cian, PuntoDTO Magenta, PuntoDTO Criss) CalcularPuntosPanel(
            LineaDTO l1a, LineaDTO l1b, LineaDTO l2a, LineaDTO l2b)
        {
            var (v, a, b, c, m, cr, _) = CalcularPuntosPanelConLineas(l1a, l1b, l2a, l2b);
            return (v, a, b, c, m, cr);
        }

        /// <summary>
        /// US-688 T5 (#693) — Variante de CalcularPuntosPanel que además devuelve las 4 líneas
        /// inner/outer identificadas. Necesario para emparejar paneles L que comparten un muro recto.
        /// </summary>
        private (PuntoDTO Verde, PuntoDTO Amarillo, PuntoDTO Blanco, PuntoDTO Cian, PuntoDTO Magenta, PuntoDTO Criss, PanelInfoMuro Info) CalcularPuntosPanelConLineas(
            LineaDTO l1a, LineaDTO l1b, LineaDTO l2a, LineaDTO l2b)
        {
            const double DIST = 300.0;

            double cxG2 = (l2a.InicioX + l2a.FinX + l2b.InicioX + l2b.FinX) / 4.0;
            double cyG2 = (l2a.InicioY + l2a.FinY + l2b.InicioY + l2b.FinY) / 4.0;
            double cxG1 = (l1a.InicioX + l1a.FinX + l1b.InicioX + l1b.FinX) / 4.0;
            double cyG1 = (l1a.InicioY + l1a.FinY + l1b.InicioY + l1b.FinY) / 4.0;

            bool l1aEsInner = DistanciaLineaPunto(l1a, cxG2, cyG2) <= DistanciaLineaPunto(l1b, cxG2, cyG2);
            LineaDTO innerG1 = l1aEsInner ? l1a : l1b;
            LineaDTO outerG1 = l1aEsInner ? l1b : l1a;

            bool l2aEsInner = DistanciaLineaPunto(l2a, cxG1, cyG1) <= DistanciaLineaPunto(l2b, cxG1, cyG1);
            LineaDTO innerG2 = l2aEsInner ? l2a : l2b;
            LineaDTO outerG2 = l2aEsInner ? l2b : l2a;

            var ptAzul = IntersectarLineas(innerG1, innerG2);
            var ptRojo = IntersectarLineas(outerG1, outerG2);
            if (!ptAzul.HasValue || !ptRojo.HasValue) return (null, null, null, null, null, null, null);
            if (!PuntoEnSegmento(ptAzul.Value.X, ptAzul.Value.Y, innerG1) || !PuntoEnSegmento(ptAzul.Value.X, ptAzul.Value.Y, innerG2)) return (null, null, null, null, null, null, null);
            if (!PuntoEnSegmento(ptRojo.Value.X, ptRojo.Value.Y, outerG1) || !PuntoEnSegmento(ptRojo.Value.X, ptRojo.Value.Y, outerG2)) return (null, null, null, null, null, null, null);

            bool g1EsH = Math.Abs(innerG1.FinX - innerG1.InicioX) >= Math.Abs(innerG1.FinY - innerG1.InicioY);
            LineaDTO innerH = g1EsH ? innerG1 : innerG2;
            LineaDTO innerV = g1EsH ? innerG2 : innerG1;
            LineaDTO outerH = g1EsH ? outerG1 : outerG2;
            LineaDTO outerV = g1EsH ? outerG2 : outerG1;

            double espV = CalcularDistanciaEntreLineasParalelas(g1EsH ? l2a : l1a, g1EsH ? l2b : l1b);
            double espH = CalcularDistanciaEntreLineasParalelas(g1EsH ? l1a : l2a, g1EsH ? l1b : l2b);

            var verde    = PuntoPolar(ptAzul.Value, innerH, DIST,        "PtEInt300H");
            var amarillo = PuntoPolar(ptAzul.Value, innerV, DIST,        "PtEInt300V");
            var blanco   = PuntoPolar(ptRojo.Value, outerH, espV + DIST, "PtEExt300H");
            var cian     = PuntoPolar(ptRojo.Value, outerV, espH + DIST, "PtEExt300V");

            // US-671: puntos de remate — solo si espV+300 / espH+300 NO son medida estándar
            PuntoDTO magenta = null, criss = null;

            double distBlanco = espV + DIST;
            if (!EsMedidaEstandar(distBlanco))
            {
                double dMagenta = MayorEstandarMenorQue(distBlanco);
                if (dMagenta > 0) magenta = PuntoPolar(ptRojo.Value, outerH, dMagenta, "PtEExtPanelH");
            }

            double distCian = espH + DIST;
            if (!EsMedidaEstandar(distCian))
            {
                double dCriss = MayorEstandarMenorQue(distCian);
                if (dCriss > 0) criss = PuntoPolar(ptRojo.Value, outerV, dCriss, "PtEExtPanelV");
            }

            var info = new PanelInfoMuro
            {
                Verde    = verde,
                Amarillo = amarillo,
                Blanco   = blanco,
                Cian     = cian,
                InnerH   = innerH,
                OuterH   = outerH,
                InnerV   = innerV,
                OuterV   = outerV,
                AzulX    = ptAzul.Value.X,
                AzulY    = ptAzul.Value.Y,
                RojoX    = ptRojo.Value.X,
                RojoY    = ptRojo.Value.Y
            };
            return (verde, amarillo, blanco, cian, magenta, criss, info);
        }

        private PuntoDTO PuntoPolar((double X, double Y) ptBase, LineaDTO linea, double distancia, string tipo)
        {
            double dIni2 = Math.Pow(linea.InicioX - ptBase.X, 2) + Math.Pow(linea.InicioY - ptBase.Y, 2);
            double dFin2 = Math.Pow(linea.FinX    - ptBase.X, 2) + Math.Pow(linea.FinY    - ptBase.Y, 2);
            double refX  = dFin2 >= dIni2 ? linea.FinX : linea.InicioX;
            double refY  = dFin2 >= dIni2 ? linea.FinY : linea.InicioY;
            double dx = refX - ptBase.X;
            double dy = refY - ptBase.Y;
            double d  = Math.Sqrt(dx * dx + dy * dy);
            if (d < TOLERANCIA) return null;
            int colorIdx = System.Enum.TryParse<TipoPunto>(tipo, out var tipoPuntoEnum) ? (int)tipoPuntoEnum : (int)TipoPunto.PtEInterior;
            return new PuntoDTO { X = ptBase.X + (dx / d) * distancia, Y = ptBase.Y + (dy / d) * distancia, Z = 0, TipoPunto = tipo, ColorIndex = colorIdx };
        }

        private List<LineaDTO> ExpandirPolilineas(List<LineaDTO> lineas)
        {
            var resultado = new List<LineaDTO>();
            foreach (var l in lineas)
            {
                if (l.Tipo != "Polyline" || l.Vertices == null || l.Vertices.Count < 2)
                {
                    resultado.Add(l);
                    continue;
                }
                for (int i = 0; i < l.Vertices.Count - 1; i++)
                {
                    var v0 = l.Vertices[i];
                    var v1 = l.Vertices[i + 1];
                    double dx = v1.X - v0.X, dy = v1.Y - v0.Y;
                    resultado.Add(new LineaDTO
                    {
                        Tipo     = "Line",
                        InicioX  = v0.X, InicioY = v0.Y, InicioZ = v0.Z,
                        FinX     = v1.X, FinY    = v1.Y, FinZ    = v1.Z,
                        Layer    = l.Layer,
                        Color    = l.Color,
                        Longitud = Math.Sqrt(dx * dx + dy * dy)
                    });
                }
            }
            return resultado;
        }

        /// <summary>
        /// Calcula la intersección de dos líneas (extensión infinita). Devuelve null si son paralelas.
        /// </summary>
        private (double X, double Y)? IntersectarLineas(LineaDTO l1, LineaDTO l2)
        {
            double x1 = l1.InicioX, y1 = l1.InicioY, x2 = l1.FinX, y2 = l1.FinY;
            double x3 = l2.InicioX, y3 = l2.InicioY, x4 = l2.FinX, y4 = l2.FinY;

            double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(denom) < 1e-10) return null;

            double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;

            return (x1 + t * (x2 - x1), y1 + t * (y2 - y1));
        }

        /// <summary>
        /// Verifica que 4 líneas (2 pares paralelos perpendiculares) formen realmente
        /// un rectángulo conectado dentro de los segmentos, no solo por orientación.
        /// </summary>
        private bool EsPanelRectangularConectado(LineaDTO l1a, LineaDTO l1b, LineaDTO l2a, LineaDTO l2b)
        {
            int interseccionesValidas = 0;
            var vistos = new List<(double X, double Y)>();
            var combinaciones = new[]
            {
                (A: l1a, B: l2a),
                (A: l1a, B: l2b),
                (A: l1b, B: l2a),
                (A: l1b, B: l2b)
            };

            foreach (var c in combinaciones)
            {
                var inter = IntersectarLineas(c.A, c.B);
                if (!inter.HasValue)
                    continue;

                double x = inter.Value.X;
                double y = inter.Value.Y;
                if (!PuntoEnSegmento(x, y, c.A) || !PuntoEnSegmento(x, y, c.B))
                    continue;

                bool duplicado = vistos.Any(v => Distancia(v.X, v.Y, x, y) <= TOLERANCIA);
                if (!duplicado)
                {
                    vistos.Add((x, y));
                    interseccionesValidas++;
                }
            }

            // Un rectángulo real debe producir 4 esquinas/intersecciones distintas.
            return interseccionesValidas == 4;
        }

        private bool PuntoEnSegmento(double px, double py, LineaDTO seg)
        {
            double minX = Math.Min(seg.InicioX, seg.FinX) - TOLERANCIA;
            double maxX = Math.Max(seg.InicioX, seg.FinX) + TOLERANCIA;
            double minY = Math.Min(seg.InicioY, seg.FinY) - TOLERANCIA;
            double maxY = Math.Max(seg.InicioY, seg.FinY) + TOLERANCIA;
            return px >= minX && px <= maxX && py >= minY && py <= maxY;
        }

        /// <summary>
        /// Distancia perpendicular de un punto (px, py) a la línea definida por la LineaDTO.
        /// </summary>
        private double DistanciaLineaPunto(LineaDTO linea, double px, double py)
        {
            double x1 = linea.InicioX, y1 = linea.InicioY;
            double x2 = linea.FinX,   y2 = linea.FinY;
            double num = Math.Abs((y2 - y1) * px - (x2 - x1) * py + x2 * y1 - y2 * x1);
            double den = Math.Sqrt(Math.Pow(y2 - y1, 2) + Math.Pow(x2 - x1, 2));
            return den > 0 ? num / den : 0;
        }

        /// <summary>
        /// Elimina puntos duplicados (a menos de TOLERANCIA de distancia).
        /// </summary>
        private List<PuntoDTO> EliminarPuntosDuplicados(List<PuntoDTO> puntos)
        {
            var puntosUnicos = new List<PuntoDTO>();

            foreach (var p in puntos)
            {
                bool esDuplicado = false;
                foreach (var pu in puntosUnicos)
                {
                    if (Distancia(p.X, p.Y, pu.X, pu.Y) < TOLERANCIA)
                    {
                        esDuplicado = true;
                        break;
                    }
                }
                if (!esDuplicado)
                {
                    puntosUnicos.Add(p);
                }
            }

            return puntosUnicos;
        }

    }
}
