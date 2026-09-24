using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TandemRevit.Models;

namespace TandemRevit
{
    public class MVCApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public string BaseUrl => _baseUrl;

        public MVCApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            _httpClient = new HttpClient(handler);
            _baseUrl = PluginExceptionHelper.ResolveBaseUrlFromEnv();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(120);
        }

        public async Task<ApiResponse<DeteccionEsquinasLDTO>> EnviarLineasSeleccionadasAsync(SeleccionLineasDTO seleccion)
        {
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
                throw PluginExceptionHelper.Wrap("Error al enviar líneas", ex, _baseUrl);
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
