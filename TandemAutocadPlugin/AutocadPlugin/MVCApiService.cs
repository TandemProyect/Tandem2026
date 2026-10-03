using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutocadPlugin.Models;
using Newtonsoft.Json;

namespace AutocadPlugin
{
    public class MVCApiService
    {
        private readonly HttpClient _httpClient;

        public string BaseUrl => PluginExceptionHelper.ResolveBaseUrlFromEnv();

        public MVCApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
            Bind();
        }

        private void Bind()
        {
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<string> ProbarConexionAsync()
        {
            Bind();
            try
            {
                var response = await _httpClient.GetAsync(string.Empty);
                return $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Prueba de conexión fallida", ex, BaseUrl);
            }
        }

        public async Task<List<DisenoResumenDTO>> ObtenerDisenosAsync()
        {
            Bind();
            try
            {
                var response = await _httpClient.GetAsync("api/disenos");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DisenoResumenDTO>>(json) ?? new List<DisenoResumenDTO>();
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error al obtener diseños", ex, BaseUrl);
            }
        }

        public async Task<DisenoDTO> ObtenerDisenoAsync(int id)
        {
            Bind();
            try
            {
                var response = await _httpClient.GetAsync($"api/disenos/{id}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DisenoDTO>(json);
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap($"Error al obtener diseño {id}", ex, BaseUrl);
            }
        }

        public async Task<DisenoDTO> CrearDisenoAsync(DisenoDTO diseno)
        {
            Bind();
            try
            {
                var json = JsonConvert.SerializeObject(diseno);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/disenos", content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DisenoDTO>(responseJson);
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error al crear diseño", ex, BaseUrl);
            }
        }

        public async Task<ApiResponse<PluginAuthResultDTO>> ValidarEquipoPluginAsync(PluginAuthRequestDTO request)
        {
            Bind();
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("DesignToolsAutocad/ValidarEquipoPlugin", content);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResponse<PluginAuthResultDTO>>(responseJson);
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error validando autorización del equipo", ex, BaseUrl);
            }
        }

        public async Task<PluginSaveWallsResponse> SaveDesignWallsAsync(
            long designId,
            string deviceId,
            List<WallLineDto> lines)
        {
            Bind();
            try
            {
                var body = new PluginSaveWallsRequest
                {
                    DesignId = designId,
                    DeviceId = deviceId,
                    Lines = lines ?? new List<WallLineDto>()
                };
                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("DesignToolsAutocad/PluginSaveDesignWalls", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var apiMsg = TryExtractApiMensaje(responseJson);
                    throw new Exception($"Error del servidor ({(int)response.StatusCode}): {apiMsg ?? responseJson}");
                }

                if (string.IsNullOrWhiteSpace(responseJson)
                    || responseJson.TrimStart().StartsWith("<", StringComparison.Ordinal))
                {
                    throw new Exception(
                        "El servidor devolvió HTML. Arranca Develop y vuelve a Salvar.");
                }

                var result = JsonConvert.DeserializeObject<PluginSaveWallsResponse>(responseJson);
                if (result == null)
                    throw new Exception("Respuesta vacía al guardar muros.");
                return result;
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error al salvar muros", ex, BaseUrl);
            }
        }

        public async Task<Atk60FormworkResponse> EncofrarAtk60Async(string idsJson)
        {
            Bind();
            try
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("IdsJson", idsJson ?? "")
                });
                var response = await _httpClient.PostAsync("DesignToolsAutocad/PluginEncofrarAtk60", content);
                var responseJson = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var apiMsg = TryExtractApiMensaje(responseJson);
                    throw new Exception($"Error del servidor ({(int)response.StatusCode}): {apiMsg ?? responseJson}");
                }

                if (string.IsNullOrWhiteSpace(responseJson)
                    || responseJson.TrimStart().StartsWith("<", StringComparison.Ordinal))
                {
                    throw new Exception(
                        "El servidor devolvió HTML en lugar de JSON. Arranca Develop (IIS Express) y vuelve a Encofrar.");
                }

                var result = JsonConvert.DeserializeObject<Atk60FormworkResponse>(responseJson);
                if (result == null)
                    throw new Exception("Respuesta vacía o JSON inválido del encofrado ATK-60.");
                return result;
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error al encofrar ATK-60", ex, BaseUrl);
            }
        }

        public async Task<ApiResponse<DeteccionEsquinasLDTO>> EnviarLineasSeleccionadasAsync(SeleccionLineasDTO seleccion)
        {
            Bind();
            try
            {
                var json = JsonConvert.SerializeObject(seleccion);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("DesignToolsAutocad/ProcesarLineasZwcad", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    var apiMsg = TryExtractApiMensaje(errorBody);
                    throw new Exception($"Error del servidor ({(int)response.StatusCode}): {apiMsg ?? errorBody}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResponse<DeteccionEsquinasLDTO>>(responseJson);
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error al enviar líneas", ex, BaseUrl);
            }
        }

        public async Task<ApiResponse<DeteccionEsquinasLDTO>> AnalizarImagenAsync(byte[] imagenBytes, string nombreArchivo)
        {
            Bind();
            try
            {
                var content = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imagenBytes);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                content.Add(imageContent, "imagen", nombreArchivo);

                var response = await _httpClient.PostAsync("DesignToolsAutocad/DetectarEsquinasImagen", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    var apiMsg = TryExtractApiMensaje(errorBody);
                    throw new Exception($"Error del servidor al analizar imagen ({(int)response.StatusCode}): {apiMsg ?? errorBody}");
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
                return resultado;
            }
            catch (Exception ex)
            {
                throw PluginExceptionHelper.Wrap("Error al enviar imagen", ex, BaseUrl);
            }
        }

        private static string TryExtractApiMensaje(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody)) return null;
            try
            {
                return JsonConvert.DeserializeObject<ApiResponse<object>>(responseBody)?.Mensaje;
            }
            catch
            {
                return null;
            }
        }
    }
}
