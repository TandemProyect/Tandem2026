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
        public DeteccionEsquinasLDTO DetectarEsquinasL(List<LineaDTO> lineas)
        {
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

                    // Crear el objeto de comparación
                    var comparacion = new
                    {
                        Linea1_Indice = i,
                        Linea2_Indice = j,
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
                    if (dist_p1I_p2I <= TOLERANCIA)
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

                    if (dist_p1I_p2F <= TOLERANCIA)
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

                    if (dist_p1F_p2I <= TOLERANCIA)
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

                    if (dist_p1F_p2F <= TOLERANCIA)
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
                                            bool dist1Valida = distGrupo1 >= OFFSET_MINIMO_PANEL && distGrupo1 <= OFFSET_MAXIMO_PANEL;
                                            bool dist2Valida = distGrupo2 >= OFFSET_MINIMO_PANEL && distGrupo2 <= OFFSET_MAXIMO_PANEL;
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
                                                EsPanelValido = esPanelValido,
                                                OffsetMaximoPermitido = OFFSET_MAXIMO_PANEL,
                                                MotivoRechazo = !esPanelValido
                                                    ? (!dist1Valida ? $"Distancia grupo 1 ({distGrupo1:F2}) > {OFFSET_MAXIMO_PANEL}" : "")
                                                    + (!dist1Valida && !dist2Valida ? " y " : "")
                                                    + (!dist2Valida ? $"Distancia grupo 2 ({distGrupo2:F2}) > {OFFSET_MAXIMO_PANEL}" : "")
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

                        // Polilínea extruida — capa ModelDesing, 2700mm en Z
                        resultado.PolilineasADibujar.Add(new PolilineaDTO
                        {
                            Cerrada         = true,
                            Capa            = "ModelDesing",
                            ColorIndex      = 256,
                            AlturaExtrusion = 2700,
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

                // US-688 T5 (#693) — Muros rectos entre dos esquinas L adyacentes (muros B y C)
                GenerarMurosRectosEntreEsquinas(panelesInfoMuro, resultado);

                // US-688 T6 (#694) — Muros rectos con UNA esquina L y un extremo libre (A, CC, D, Cara E)
                GenerarMurosLConExtremoLibre(panelesInfoMuro, lineas, resultado);
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
            };

            resultado.TotalEsquinasDetectadas = esquinasEnLReales;
            resultado.Mensaje = $"Se detectaron {esquinasEnLReales} esquina(s) en L, {panelesValidos.Count} panel(es) válido(s), {panelesInvalidos.Count} panel(es) inválido(s)";

            // 💾 GUARDAR JSON COMPLETO CON PARALELAS
            GuardarJSON(infoCompletaConParalelas);

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
                    if (MismaLinea(pA.InnerH, pB.InnerH) && MismaLinea(pA.OuterH, pB.OuterH))
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
                    if (MismaLinea(pA.InnerV, pB.InnerV) && MismaLinea(pA.OuterV, pB.OuterV))
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
        /// Emite las 2 polilíneas de un muro recto (ObjetoDB2d + ModelDesing extruida 2700mm).
        /// </summary>
        private void AgregarMuroRecto(DeteccionEsquinasLDTO resultado, List<PuntoDTO> vertices)
        {
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
                AlturaExtrusion = 2700,
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
            const double LADO_SEMI = 100.0; // semi-lado del cuadrado (radio circulo / 2)

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
