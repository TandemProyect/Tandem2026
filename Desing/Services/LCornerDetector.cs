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
        private const double TOLERANCIA = 0.01; // Muy pequeña tolerancia para puntos "iguales"
        private const double OFFSET_MAXIMO_PANEL = 1500.0; // Distancia máxima entre líneas paralelas para considerar un panel válido

        /// <summary>
        /// Detecta conexiones entre líneas (puntos donde se tocan)
        /// </summary>
        public DeteccionEsquinasLDTO DetectarEsquinasL(List<LineaDTO> lineas)
        {
            var resultado = new DeteccionEsquinasLDTO
            {
                Esquinas = new List<EsquinaLDTO>(),
                PuntosADibujar = new List<PuntoDTO>()
            };

            if (lineas == null || lineas.Count < 2)
            {
                resultado.Mensaje = "Se requieren al menos 2 líneas";
                GuardarJSON(new { Error = "Menos de 2 líneas" });
                return resultado;
            }

            // Solo procesar líneas simples
            var lineasSimples = lineas.Where(l => l.Tipo == "Line").ToList();

            // 📋 Preparar toda la información para el JSON
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

                                            // ⭐ VALIDACIÓN: El offset entre líneas paralelas no debe superar 1500 unidades
                                            bool dist1Valida = distGrupo1 <= OFFSET_MAXIMO_PANEL;
                                            bool dist2Valida = distGrupo2 <= OFFSET_MAXIMO_PANEL;
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

                int numeroPanelValido = 1;
                foreach (dynamic panel in panelesValidos)
                {
                    // Obtener las 4 líneas del panel
                    int[] lineasGrupo1 = panel.LineasGrupo1;
                    int[] lineasGrupo2 = panel.LineasGrupo2;

                    var l1a = lineas[lineasGrupo1[0]];
                    var l1b = lineas[lineasGrupo1[1]];
                    var l2a = lineas[lineasGrupo2[0]];
                    var l2b = lineas[lineasGrupo2[1]];

                    // Calcular puntos de esquina L por intersección de líneas interiores/exteriores
                    var (interior, exterior) = CalcularPuntosEsquinaL(l1a, l1b, l2a, l2b);

                    // Calcular punto verde: 300u desde el interior, hacia el interior del muro (US-664)
                    var ptVerde = CalcularPuntoVerde(l1a, l1b, l2a, l2b);
                    if (ptVerde != null)
                        resultado.PuntosADibujar.Add(ptVerde);

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
                    punto.TipoPunto = "Interior";
                    resultado.PuntosADibujar.Add(punto);
                }

                foreach (var punto in puntosExteriorUnicos)
                {
                    punto.TipoPunto = "Exterior";
                    resultado.PuntosADibujar.Add(punto);
                }
            }
            else
            {
                // Si no hay paneles válidos, usar conexiones individuales
                foreach (var esquina in resultado.Esquinas)
                {
                    esquina.Vertice.TipoPunto = "Interior"; // Por defecto, las conexiones individuales son interiores
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
            if (ptInterior.HasValue)
                interiores.Add(new PuntoDTO { X = ptInterior.Value.X, Y = ptInterior.Value.Y, Z = 0 });

            // Intersección de líneas exteriores → punto exterior de la esquina
            var ptExterior = IntersectarLineas(outerG1, outerG2);
            if (ptExterior.HasValue)
                exteriores.Add(new PuntoDTO { X = ptExterior.Value.X, Y = ptExterior.Value.Y, Z = 0 });

            return (interiores, exteriores);
        }

        /// <summary>
        /// Calcula el punto verde: 300 unidades desde el punto interior (azul)
        /// en la dirección del interior del muro (a lo largo de innerG2).
        /// Válido para cualquier orientación de la esquina L.
        /// </summary>
        private PuntoDTO CalcularPuntoVerde(LineaDTO l1a, LineaDTO l1b, LineaDTO l2a, LineaDTO l2b)
        {
            const double DISTANCIA_VERDE = 300.0;

            double centroX_g2 = (l2a.InicioX + l2a.FinX + l2b.InicioX + l2b.FinX) / 4.0;
            double centroY_g2 = (l2a.InicioY + l2a.FinY + l2b.InicioY + l2b.FinY) / 4.0;
            double centroX_g1 = (l1a.InicioX + l1a.FinX + l1b.InicioX + l1b.FinX) / 4.0;
            double centroY_g1 = (l1a.InicioY + l1a.FinY + l1b.InicioY + l1b.FinY) / 4.0;

            double dist_l1a = DistanciaLineaPunto(l1a, centroX_g2, centroY_g2);
            double dist_l1b = DistanciaLineaPunto(l1b, centroX_g2, centroY_g2);
            LineaDTO innerG1 = dist_l1a <= dist_l1b ? l1a : l1b;

            double dist_l2a = DistanciaLineaPunto(l2a, centroX_g1, centroY_g1);
            double dist_l2b = DistanciaLineaPunto(l2b, centroX_g1, centroY_g1);
            LineaDTO innerG2 = dist_l2a <= dist_l2b ? l2a : l2b;

            // Punto azul = intersección de líneas interiores
            var ptAzul = IntersectarLineas(innerG1, innerG2);
            if (!ptAzul.HasValue) return null;

            // Dirección: desde azul hacia el punto medio de innerG2
            // El punto medio siempre apunta "hacia el interior del muro",
            // independientemente de la orientación de la esquina
            double midG2X = (innerG2.InicioX + innerG2.FinX) / 2.0;
            double midG2Y = (innerG2.InicioY + innerG2.FinY) / 2.0;

            double dx = midG2X - ptAzul.Value.X;
            double dy = midG2Y - ptAzul.Value.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            if (dist < TOLERANCIA) return null;

            return new PuntoDTO
            {
                X          = ptAzul.Value.X + (dx / dist) * DISTANCIA_VERDE,
                Y          = ptAzul.Value.Y + (dy / dist) * DISTANCIA_VERDE,
                Z          = 0,
                TipoPunto  = "Verde"
            };
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
