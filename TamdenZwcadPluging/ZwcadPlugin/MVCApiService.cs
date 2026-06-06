using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ZwcadPlugin.Models;

namespace ZwcadPlugin
{
    public class MVCApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public string BaseUrl => _baseUrl;

        public MVCApiService()
        {
            _httpClient = new HttpClient();
            _baseUrl = PluginExceptionHelper.ResolveBaseUrlFromEnv();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(120);

            // SOLO DESARROLLO: ignorar errores de certificado SSL en localhost
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;
        }

        /// <summary>
        /// Prueba conectividad HTTP con el servidor MVC (GET raíz).
        /// </summary>
        public async Task<string> ProbarConexionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(string.Empty);
                return $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Prueba de conexión fallida", ex, _baseUrl);
            }
        }

        #region Diseños

        public async Task<List<DisenoResumenDTO>> ObtenerDisenosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/disenos");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var disenos = JsonConvert.DeserializeObject<List<DisenoResumenDTO>>(json);

                return disenos ?? new List<DisenoResumenDTO>();
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error al obtener diseños", ex, _baseUrl);
            }
        }

        public async Task<DisenoDTO> ObtenerDisenoAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/disenos/{id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var diseno = JsonConvert.DeserializeObject<DisenoDTO>(json);

                return diseno;
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap($"Error al obtener diseño {id}", ex, _baseUrl);
            }
        }

        public async Task<DisenoDTO> CrearDisenoAsync(DisenoDTO diseno)
        {
            try
            {
                var json = JsonConvert.SerializeObject(diseno);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("api/disenos", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var disenoCreado = JsonConvert.DeserializeObject<DisenoDTO>(responseJson);

                return disenoCreado;
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error al crear diseño", ex, _baseUrl);
            }
        }

        public async Task<DisenoDTO> ActualizarDisenoAsync(int id, DisenoDTO diseno)
        {
            try
            {
                var json = JsonConvert.SerializeObject(diseno);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"api/disenos/{id}", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var disenoActualizado = JsonConvert.DeserializeObject<DisenoDTO>(responseJson);

                return disenoActualizado;
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap($"Error al actualizar diseño {id}", ex, _baseUrl);
            }
        }

        #endregion

        #region Bloques

        public async Task<List<BloqueDTO>> ObtenerBloquesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/bloques");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var bloques = JsonConvert.DeserializeObject<List<BloqueDTO>>(json);

                return bloques ?? new List<BloqueDTO>();
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error al obtener bloques", ex, _baseUrl);
            }
        }

        public async Task<byte[]> DescargarBloqueAsync(string nombreBloque)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/bloques/descargar/{nombreBloque}");
                response.EnsureSuccessStatusCode();

                var bytes = await response.Content.ReadAsByteArrayAsync();
                return bytes;
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap($"Error al descargar bloque {nombreBloque}", ex, _baseUrl);
            }
        }

        #endregion

        #region Seguridad Plugin

        public async Task<ApiResponse<PluginAuthResultDTO>> ValidarEquipoPluginAsync(PluginAuthRequestDTO request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("DesignToolsAutocad/ValidarEquipoPlugin", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var resultado = JsonConvert.DeserializeObject<ApiResponse<PluginAuthResultDTO>>(responseJson);
                return resultado;
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error validando autorización del equipo", ex, _baseUrl);
            }
        }

        #endregion

        #region Líneas y Polilíneas

        public async Task<ApiResponse<DeteccionEsquinasLDTO>> EnviarLineasSeleccionadasAsync(SeleccionLineasDTO seleccion)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔴 [MVCApiService] Iniciando envío de {seleccion.Lineas.Count} líneas");
                System.Diagnostics.Debug.WriteLine($"🔴 [MVCApiService] URL: {_baseUrl}DesignToolsAutocad/ProcesarLineasZwcad");

                var json = JsonConvert.SerializeObject(seleccion);
                System.Diagnostics.Debug.WriteLine($"🔴 [MVCApiService] JSON serializado: {json.Length} caracteres");

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"🔴 [MVCApiService] Enviando POST...");
                var response = await _httpClient.PostAsync("DesignToolsAutocad/ProcesarLineasZwcad", content);
                System.Diagnostics.Debug.WriteLine($"🔴 [MVCApiService] Status Code: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    var apiMsg = TryExtractApiMensaje(errorBody);
                    var detail = apiMsg ?? errorBody;
                    throw new Exception($"Error del servidor ({(int)response.StatusCode}): {detail}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"🔴 [MVCApiService] Respuesta recibida: {responseJson.Length} caracteres");

                var resultado = JsonConvert.DeserializeObject<ApiResponse<DeteccionEsquinasLDTO>>(responseJson);
                System.Diagnostics.Debug.WriteLine($"🔴 [MVCApiService] Deserializado. Éxito: {resultado.Exito}");

                return resultado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [MVCApiService] ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ [MVCApiService] StackTrace: {ex.StackTrace}");
                throw PluginExceptionHelper.Wrap("Error al enviar líneas", ex, _baseUrl);
            }
        }

        #endregion

        #region Imagen

        public async Task<ApiResponse<DeteccionEsquinasLDTO>> AnalizarImagenAsync(byte[] imagenBytes, string nombreArchivo)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[MVCApiService] Enviando imagen: {nombreArchivo} ({imagenBytes.Length} bytes)");

                var content = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imagenBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                content.Add(imageContent, "imagen", nombreArchivo);

                var response = await _httpClient.PostAsync("DesignToolsAutocad/DetectarEsquinasImagen", content);
                System.Diagnostics.Debug.WriteLine($"[MVCApiService] Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    var apiMsg = TryExtractApiMensaje(errorBody);
                    var detail = !string.IsNullOrWhiteSpace(apiMsg)
                        ? apiMsg
                        : (!string.IsNullOrWhiteSpace(errorBody) ? errorBody : response.ReasonPhrase);
                    throw new Exception($"Error del servidor al analizar imagen ({(int)response.StatusCode}): {detail}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseJson) ||
                    responseJson.TrimStart().StartsWith("<", StringComparison.Ordinal))
                {
                    throw new Exception(
                        "El servidor devolvió HTML en lugar de JSON. ¿Sesión expirada o endpoint sin [AllowAnonymous]? Reinicie Desing tras actualizar el código.");
                }

                var resultado = JsonConvert.DeserializeObject<ApiResponse<DeteccionEsquinasLDTO>>(responseJson);
                if (resultado == null)
                    throw new Exception("Respuesta vacía o JSON inválido del servidor MVC.");

                System.Diagnostics.Debug.WriteLine($"[MVCApiService] Imagen analizada. Éxito: {resultado.Exito}");

                return resultado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ [MVCApiService] ERROR imagen: {ex.Message}");
                throw PluginExceptionHelper.Wrap("Error al enviar imagen", ex, _baseUrl);
            }
        }

        #endregion

        private static string TryExtractApiMensaje(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody)) return null;

            try
            {
                var apiResp = JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody);
                return apiResp?.Mensaje;
            }
            catch
            {
                return null;
            }
        }
    }
}
