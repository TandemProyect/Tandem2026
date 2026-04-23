using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ZwcadPlugin.Models;

namespace ZwcadPlugin
{
    public class MVCApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public MVCApiService()
        {
            _httpClient = new HttpClient();
            _baseUrl = "http://ccvallecano-002-site1.rtempurl.com/";
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        #region Diseños

        /// <summary>
        /// Obtiene la lista de todos los diseños
        /// </summary>
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
                throw new Exception($"Error al obtener diseños: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un diseño específico por su ID
        /// </summary>
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
                throw new Exception($"Error al obtener diseño {id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea un nuevo diseño en el servidor
        /// </summary>
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
                throw new Exception($"Error al crear diseño: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un diseño existente
        /// </summary>
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
                throw new Exception($"Error al actualizar diseño {id}: {ex.Message}", ex);
            }
        }

        #endregion

        #region Bloques

        /// <summary>
        /// Obtiene la lista de bloques disponibles en el servidor
        /// </summary>
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
                throw new Exception($"Error al obtener bloques: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Descarga el archivo de un bloque específico
        /// </summary>
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
                throw new Exception($"Error al descargar bloque {nombreBloque}: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
