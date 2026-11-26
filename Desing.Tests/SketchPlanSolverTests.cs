using Desing.Services;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Desing.Tests
{
    public class SketchPlanSolverTests
    {
        /// <summary>Lectura real (gpt-6.2-sol) del boceto del 26/11/2026: planta con muesca abajo a la derecha.</summary>
        private static string BocetoUsuario(string cotaSuperior) => @"{
  ""contornoExterior"": [[297, 237], [772, 243], [765, 400], [628, 390], [627, 498], [274, 503]],
  ""cotas"": [
    {""valor"": " + cotaSuperior + @", ""eje"": ""H"", ""p1"": [301, 173], ""p2"": [773, 187], ""v1"": 0, ""v2"": 1},
    {""valor"": 10.0, ""eje"": ""V"", ""p1"": [203, 241], ""p2"": [199, 501], ""v1"": 0, ""v2"": 5},
    {""valor"": 5.0, ""eje"": ""V"", ""p1"": [833, 255], ""p2"": [839, 398], ""v1"": 1, ""v2"": 2},
    {""valor"": 5.0, ""eje"": ""V"", ""p1"": [839, 398], ""p2"": [841, 481], ""v1"": 2, ""v2"": null},
    {""valor"": 7.0, ""eje"": ""H"", ""p1"": [288, 595], ""p2"": [627, 563], ""v1"": 5, ""v2"": 4},
    {""valor"": 3.0, ""eje"": ""H"", ""p1"": [627, 563], ""p2"": [763, 558], ""v1"": 4, ""v2"": 2}
  ],
  ""espesor"": 0.3,
  ""altura"": 3.0,
  ""textos"": [""20,00"", ""10,00"", ""E=0,30"", ""h=3,00"", ""5,00"", ""5,00"", ""7,00"", ""3,00""]
}";

        private static SketchPlanResult Resolver(string json) =>
            SketchPlanSolver.Resolver(SketchPlanSolver.LeerRespuestaModelo(json));

        private static void AssertAnillo(double[,] esperado, List<SketchMm> real)
        {
            Assert.Equal(esperado.GetLength(0), real.Count);
            for (int i = 0; i < real.Count; i++)
            {
                Assert.Equal(esperado[i, 0], real[i].X, 1);
                Assert.Equal(esperado[i, 1], real[i].Y, 1);
            }
        }

        [Fact]
        public void BocetoUsuario_ParcialesNoSumanLaTotal_DevuelveConflictoSinPlanta()
        {
            var res = Resolver(BocetoUsuario("20.0"));

            Assert.False(res.Ok);
            Assert.Empty(res.Exterior);
            var conflicto = Assert.Single(res.Conflictos);
            Assert.Equal("Cotas horizontales: 20,00 no cuadra con 7,00 + 3,00 = 10,00.", conflicto);
            Assert.Equal(
                new[]
                {
                    SketchPlanSolver.CotaIncoherente, SketchPlanSolver.CotaUsada, SketchPlanSolver.CotaUsada,
                    SketchPlanSolver.CotaUsada, SketchPlanSolver.CotaRelacionada, SketchPlanSolver.CotaRelacionada
                },
                res.EstadoCotas);
        }

        [Fact]
        public void BocetoCoherente_DevuelveCaraExteriorYEjeExactos()
        {
            var res = Resolver(BocetoUsuario("10.0"));

            Assert.True(res.Ok, res.Error);
            AssertAnillo(new double[,] { { 0, 0 }, { 7000, 0 }, { 7000, 5000 }, { 10000, 5000 }, { 10000, 10000 }, { 0, 10000 } }, res.Exterior);
            AssertAnillo(new double[,] { { 150, 150 }, { 6850, 150 }, { 6850, 5150 }, { 9850, 5150 }, { 9850, 9850 }, { 150, 9850 } }, res.Eje);
            Assert.Equal(300, res.EspesorMm);
            Assert.Equal(3000, res.AlturaMm);
            Assert.Empty(res.Avisos);
        }

        [Fact]
        public void CotasSinVertices_SeSituanPorLosExtremosDeLaLineaDeCota()
        {
            // Misma foto ampliada x2: el modelo no devolvió v1/v2.
            const string json = @"{
  ""contornoExterior"": [[593, 474], [1544, 486], [1529, 800], [1255, 780], [1254, 996], [546, 1006]],
  ""cotas"": [
    {""valor"": 10.0, ""eje"": ""H"", ""p1"": [604, 350], ""p2"": [1548, 376], ""v1"": null, ""v2"": null},
    {""valor"": 10.0, ""eje"": ""V"", ""p1"": [407, 484], ""p2"": [398, 1004], ""v1"": null, ""v2"": null},
    {""valor"": 5.0, ""eje"": ""V"", ""p1"": [1667, 509], ""p2"": [1678, 796], ""v1"": null, ""v2"": null},
    {""valor"": 5.0, ""eje"": ""V"", ""p1"": [1678, 796], ""p2"": [1682, 959], ""v1"": null, ""v2"": null},
    {""valor"": 7.0, ""eje"": ""H"", ""p1"": [581, 1186], ""p2"": [1253, 1121], ""v1"": null, ""v2"": null},
    {""valor"": 3.0, ""eje"": ""H"", ""p1"": [1253, 1121], ""p2"": [1528, 1117], ""v1"": null, ""v2"": null}
  ],
  ""espesor"": 0.3, ""altura"": 3.0
}";
            var res = Resolver(json);

            Assert.True(res.Ok, res.Error);
            AssertAnillo(new double[,] { { 0, 0 }, { 7000, 0 }, { 7000, 5000 }, { 10000, 5000 }, { 10000, 10000 }, { 0, 10000 } }, res.Exterior);
        }

        [Fact]
        public void FaltaLaCotaVertical_SeEstimaAEscalaDelDibujoYSeAvisa()
        {
            const string json = @"{
  ""contornoExterior"": [[100, 100], [600, 102], [598, 350], [102, 348]],
  ""cotas"": [ {""valor"": 10, ""eje"": ""H"", ""p1"": [100, 60], ""p2"": [600, 60], ""v1"": 0, ""v2"": 1} ],
  ""espesor"": 0.25, ""altura"": 2.7
}";
            var res = Resolver(json);

            Assert.True(res.Ok, res.Error);
            Assert.Equal(10000, res.AnchoMm, 1);
            Assert.Equal(5000, res.FondoMm, 1);
            Assert.Contains(res.Avisos, a => a.StartsWith("Faltan cotas verticales"));
        }

        [Fact]
        public void PlantaEnU_AlasCasiAlineadasSinCotaPropia_QuedanALaMismaAltura()
        {
            const string json = @"{
  ""contornoExterior"": [[100, 100], [200, 100], [200, 300], [400, 300], [400, 102], [500, 102], [500, 400], [100, 400]],
  ""cotas"": [
    {""valor"": 8, ""eje"": ""H"", ""v1"": 0, ""v2"": 5},
    {""valor"": 2, ""eje"": ""H"", ""v1"": 0, ""v2"": 1},
    {""valor"": 2, ""eje"": ""H"", ""v1"": 4, ""v2"": 5},
    {""valor"": 6, ""eje"": ""V"", ""v1"": 0, ""v2"": 7},
    {""valor"": 4, ""eje"": ""V"", ""v1"": 1, ""v2"": 2}
  ],
  ""espesor"": 0.2, ""altura"": 3
}";
            var res = Resolver(json);

            Assert.True(res.Ok, res.Error);
            Assert.Empty(res.Avisos);
            AssertAnillo(
                new double[,] { { 0, 0 }, { 8000, 0 }, { 8000, 6000 }, { 6000, 6000 }, { 6000, 2000 }, { 2000, 2000 }, { 2000, 6000 }, { 0, 6000 } },
                res.Exterior);
        }

        [Fact]
        public void SinEspesor_SePideEYConElDelUsuarioSeResuelve()
        {
            var lectura = SketchPlanSolver.LeerRespuestaModelo(
                BocetoUsuario("10.0").Replace(@"""espesor"": 0.3", @"""espesor"": null").Replace("E=0,30", "x"));

            var sinE = SketchPlanSolver.Resolver(lectura);

            Assert.False(sinE.Ok);
            Assert.True(sinE.FaltaEspesor);
            Assert.False(sinE.FaltaAltura);
            Assert.StartsWith("Falta el espesor", sinE.Error);
            Assert.Equal("El boceto no indica el espesor del muro (E=0,30).", Assert.Single(sinE.DatosQueFaltan));

            SketchPlanSolver.AplicarEspesorYAltura(lectura, 0.3, 3.0);
            var conE = SketchPlanSolver.Resolver(lectura);

            Assert.True(conE.Ok, conE.Error);
            AssertAnillo(new double[,] { { 150, 150 }, { 6850, 150 }, { 6850, 5150 }, { 9850, 5150 }, { 9850, 9850 }, { 150, 9850 } }, conE.Eje);
        }

        [Fact]
        public void SinAltura_SeResuelvenLosMurosYSePideH()
        {
            var res = Resolver(BocetoUsuario("10.0").Replace(@"""altura"": 3.0", @"""altura"": null").Replace("h=3,00", "x"));

            Assert.True(res.Ok, res.Error);
            Assert.True(res.FaltaAltura);
            Assert.Null(res.AlturaMm);
            Assert.Equal(6, res.Eje.Count);
            Assert.Empty(res.Avisos);
            Assert.Equal("El boceto no indica la altura del muro (h=3,00).", Assert.Single(res.DatosQueFaltan));
        }

        [Fact]
        public void LeerRespuestaModelo_ComaDecimalYAnotacionesEnTextos()
        {
            const string json = @"```json
{ ""contornoExterior"": [], ""cotas"": [ {""valor"": ""7,00"", ""eje"": ""horizontal""} ],
  ""espesor"": null, ""altura"": null, ""textos"": [""E = 0,30"", ""h=3,00""] }
```";
            var lectura = SketchPlanSolver.LeerRespuestaModelo(json);

            Assert.Equal(0.3, lectura.EspesorM);
            Assert.Equal(3.0, lectura.AlturaM);
            var cota = Assert.Single(lectura.Cotas);
            Assert.Equal(7.0, cota.Valor);
            Assert.Equal("H", cota.Eje);
            Assert.Null(cota.V1);
        }
    }
}
