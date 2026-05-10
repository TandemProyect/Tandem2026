using Desing.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace Desing.Controllers
{
    public class TelegramWebhookController : Controller
    {
        private const long DesignIdFijoMvp = 131;

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Inbound()
        {
            try
            {
                if (!ValidarSecretoWebhook())
                {
                    return new HttpStatusCodeResult(401, "Webhook secret invalido.");
                }

                string body;
                using (var reader = new StreamReader(Request.InputStream))
                {
                    body = reader.ReadToEnd();
                }

                if (string.IsNullOrWhiteSpace(body))
                {
                    return Json(new ApiResponse<object> { Exito = false, Mensaje = "Payload vacio", Datos = null });
                }

                var update = JObject.Parse(body);
                var message = (JObject)(update["message"] ?? update["edited_message"]);
                if (message == null)
                {
                    return Json(new ApiResponse<object> { Exito = true, Mensaje = "Update sin mensaje util", Datos = null });
                }

                var photos = message["photo"] as JArray;
                if (photos == null || photos.Count == 0)
                {
                    return Json(new ApiResponse<object> { Exito = true, Mensaje = "Mensaje recibido sin foto", Datos = null });
                }

                var bestPhoto = (JObject)photos[photos.Count - 1];
                var caption = ((string)message["caption"] ?? (string)message["text"] ?? string.Empty).Trim();
                string chatId = (string)message["chat"]?["id"] ?? string.Empty;
                string userId = (string)message["from"]?["id"] ?? string.Empty;
                long designId = ExtraerDesignId(caption) ?? DesignIdFijoMvp;
                if (!ExisteDiseno(designId))
                {
                    return Json(new ApiResponse<object>
                    {
                        Exito = false,
                        Mensaje = $"No existe el diseno {designId} en dbo.TSql_Design.",
                        Datos = null
                    });
                }

                if (!UsuarioTelegramAutorizado(chatId, userId, designId))
                {
                    return Json(new ApiResponse<object>
                    {
                        Exito = false,
                        Mensaje = $"Chat/usuario no autorizado para el diseno {designId}.",
                        Datos = new { chatId, userId, designId }
                    });
                }

                var dto = new TelegramDesignPhotoDTO
                {
                    DesignId = designId,
                    TelegramMessageId = (string)message["message_id"] ?? string.Empty,
                    TelegramChatId = chatId,
                    TelegramUserId = userId,
                    TelegramUserName = (string)message["from"]?["username"] ?? string.Empty,
                    Caption = caption,
                    FileId = (string)bestPhoto["file_id"] ?? string.Empty,
                    FileUniqueId = (string)bestPhoto["file_unique_id"] ?? string.Empty,
                    WidthPx = (int?)bestPhoto["width"] ?? 0,
                    HeightPx = (int?)bestPhoto["height"] ?? 0,
                    Estado = "Pendiente",
                    FechaMensajeUtc = UnixToUtc((long?)message["date"]),
                    FechaRegistroUtc = DateTime.UtcNow
                };

                long id = GuardarInboxTelegram(dto);

                return Json(new ApiResponse<object>
                {
                    Exito = true,
                    Mensaje = "Foto registrada correctamente.",
                    Datos = new { Id = id, dto.DesignId, dto.FileId, dto.Estado }
                });
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<object> { Exito = false, Mensaje = $"Error Telegram webhook: {ex.Message}", Datos = null });
            }
        }

        [HttpGet]
        public ActionResult FotosPendientes(long? designId, int top = 20)
        {
            try
            {
                long designIdConsulta = designId.GetValueOrDefault(DesignIdFijoMvp);
                if (designIdConsulta <= 0)
                {
                    return Json(new ApiResponse<List<TelegramDesignPhotoDTO>>
                    {
                        Exito = false,
                        Mensaje = "DesignId invalido.",
                        Datos = new List<TelegramDesignPhotoDTO>()
                    }, JsonRequestBehavior.AllowGet);
                }

                if (!ExisteDiseno(designIdConsulta))
                {
                    return Json(new ApiResponse<List<TelegramDesignPhotoDTO>>
                    {
                        Exito = false,
                        Mensaje = $"No existe el diseno {designIdConsulta} en dbo.TSql_Design.",
                        Datos = new List<TelegramDesignPhotoDTO>()
                    }, JsonRequestBehavior.AllowGet);
                }

                var data = ObtenerFotosPendientes(designIdConsulta, top);
                return Json(new ApiResponse<List<TelegramDesignPhotoDTO>>
                {
                    Exito = true,
                    Mensaje = $"Se obtuvieron {data.Count} foto(s) pendientes.",
                    Datos = data
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<List<TelegramDesignPhotoDTO>>
                {
                    Exito = false,
                    Mensaje = $"Error consultando fotos Telegram: {ex.Message}",
                    Datos = new List<TelegramDesignPhotoDTO>()
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult ResolverArchivo(long inboxId)
        {
            try
            {
                var row = ObtenerInboxPorId(inboxId);
                if (row == null)
                {
                    return Json(new ApiResponse<object>
                    {
                        Exito = false,
                        Mensaje = "No existe el registro en inbox.",
                        Datos = null
                    }, JsonRequestBehavior.AllowGet);
                }

                string downloadUrl = ResolverUrlDescargaTelegram(row.FileId);
                return Json(new ApiResponse<object>
                {
                    Exito = true,
                    Mensaje = "URL de descarga resuelta.",
                    Datos = new
                    {
                        row.Id,
                        row.DesignId,
                        row.FileId,
                        DownloadUrl = downloadUrl,
                        row.Estado
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new ApiResponse<object>
                {
                    Exito = false,
                    Mensaje = $"Error resolviendo archivo Telegram: {ex.Message}",
                    Datos = null
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private static DateTime UnixToUtc(long? unixSeconds)
        {
            if (!unixSeconds.HasValue) return DateTime.UtcNow;
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds.Value).UtcDateTime;
        }

        private static bool ExisteDiseno(long designId)
        {
            string cs = ConfigurationManager.ConnectionStrings["IdentityConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException("No existe IdentityConnection en Web.config");
            }

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.TSql_Design WHERE SysObjectID = @DesignId", cn))
                {
                    cmd.Parameters.AddWithValue("@DesignId", designId);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private bool ValidarSecretoWebhook()
        {
            string expectedSecret = ConfigurationManager.AppSettings["TELEGRAM_WEBHOOK_SECRET"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(expectedSecret))
            {
                return true;
            }

            string incomingSecret = Request.Headers["X-Telegram-Bot-Api-Secret-Token"] ?? string.Empty;
            return incomingSecret == expectedSecret;
        }

        private static long? ExtraerDesignId(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;

            int idx = text.IndexOf("131", StringComparison.OrdinalIgnoreCase);
            if (idx >= 0) return 131;

            // MVP: fallback estricto al fijo.
            return null;
        }

        private static bool UsuarioTelegramAutorizado(string chatId, string userId, long designId)
        {
            string cs = ConfigurationManager.ConnectionStrings["IdentityConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException("No existe IdentityConnection en Web.config");
            }

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                string sql = @"
SELECT COUNT(1)
FROM dbo.TSql_TelegramDesignAccess
WHERE LinDesign = @LinDesign
  AND IsActive = 1
  AND (TelegramChatId = @TelegramChatId OR TelegramUserId = @TelegramUserId);";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@LinDesign", designId);
                    cmd.Parameters.AddWithValue("@TelegramChatId", (object)chatId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TelegramUserId", (object)userId ?? DBNull.Value);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private static string ResolverUrlDescargaTelegram(string fileId)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                throw new InvalidOperationException("FileId vacio.");
            }

            string token = ConfigurationManager.AppSettings["TELEGRAM_BOT_TOKEN"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token) || token == "REPLACE_WITH_LOCAL_SECRET")
            {
                throw new InvalidOperationException("Configura TELEGRAM_BOT_TOKEN en Web.config");
            }

            string endpoint = $"https://api.telegram.org/bot{token}/getFile?file_id={Uri.EscapeDataString(fileId)}";
            string json = new WebClient().DownloadString(endpoint);
            var obj = JObject.Parse(json);
            bool ok = (bool?)obj["ok"] ?? false;
            if (!ok)
            {
                throw new InvalidOperationException("Telegram getFile devolvio ok=false.");
            }

            string filePath = (string)obj["result"]?["file_path"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new InvalidOperationException("Telegram no devolvio file_path.");
            }

            return $"https://api.telegram.org/file/bot{token}/{filePath}";
        }

        private static TelegramDesignPhotoDTO ObtenerInboxPorId(long inboxId)
        {
            string cs = ConfigurationManager.ConnectionStrings["IdentityConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException("No existe IdentityConnection en Web.config");
            }

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                string sql = @"
SELECT TOP 1
  SysObjectID, LinDesign, TelegramMessageId, TelegramChatId, TelegramUserId, TelegramUserName,
  Caption, FileId, FileUniqueId, WidthPx, HeightPx, Estado, FechaMensajeUtc, FechaRegistroUtc
FROM dbo.TSql_TelegramDesignInbox
WHERE SysObjectID = @Id;";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@Id", inboxId);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;
                        return new TelegramDesignPhotoDTO
                        {
                            Id = rd["SysObjectID"] == DBNull.Value ? 0 : Convert.ToInt64(rd["SysObjectID"]),
                            DesignId = rd["LinDesign"] == DBNull.Value ? 0 : Convert.ToInt64(rd["LinDesign"]),
                            TelegramMessageId = rd["TelegramMessageId"] as string,
                            TelegramChatId = rd["TelegramChatId"] as string,
                            TelegramUserId = rd["TelegramUserId"] as string,
                            TelegramUserName = rd["TelegramUserName"] as string,
                            Caption = rd["Caption"] as string,
                            FileId = rd["FileId"] as string,
                            FileUniqueId = rd["FileUniqueId"] as string,
                            WidthPx = rd["WidthPx"] == DBNull.Value ? 0 : Convert.ToInt32(rd["WidthPx"]),
                            HeightPx = rd["HeightPx"] == DBNull.Value ? 0 : Convert.ToInt32(rd["HeightPx"]),
                            Estado = rd["Estado"] as string,
                            FechaMensajeUtc = rd["FechaMensajeUtc"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rd["FechaMensajeUtc"]),
                            FechaRegistroUtc = rd["FechaRegistroUtc"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rd["FechaRegistroUtc"])
                        };
                    }
                }
            }
        }

        private static long GuardarInboxTelegram(TelegramDesignPhotoDTO dto)
        {
            string cs = ConfigurationManager.ConnectionStrings["IdentityConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException("No existe IdentityConnection en Web.config");
            }

            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                string sql = @"
INSERT INTO dbo.TSql_TelegramDesignInbox
(
  LinDesign, TelegramMessageId, TelegramChatId, TelegramUserId, TelegramUserName,
  Caption, FileId, FileUniqueId, WidthPx, HeightPx, Estado, FechaMensajeUtc, FechaRegistroUtc
)
VALUES
(
  @LinDesign, @TelegramMessageId, @TelegramChatId, @TelegramUserId, @TelegramUserName,
  @Caption, @FileId, @FileUniqueId, @WidthPx, @HeightPx, @Estado, @FechaMensajeUtc, @FechaRegistroUtc
);
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@LinDesign", dto.DesignId);
                    cmd.Parameters.AddWithValue("@TelegramMessageId", (object)dto.TelegramMessageId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TelegramChatId", (object)dto.TelegramChatId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TelegramUserId", (object)dto.TelegramUserId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TelegramUserName", (object)dto.TelegramUserName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Caption", (object)dto.Caption ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileId", (object)dto.FileId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileUniqueId", (object)dto.FileUniqueId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@WidthPx", dto.WidthPx);
                    cmd.Parameters.AddWithValue("@HeightPx", dto.HeightPx);
                    cmd.Parameters.AddWithValue("@Estado", (object)dto.Estado ?? "Pendiente");
                    cmd.Parameters.AddWithValue("@FechaMensajeUtc", dto.FechaMensajeUtc);
                    cmd.Parameters.AddWithValue("@FechaRegistroUtc", dto.FechaRegistroUtc);

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt64(result);
                }
            }
        }

        private static List<TelegramDesignPhotoDTO> ObtenerFotosPendientes(long designId, int top)
        {
            if (top <= 0) top = 20;
            if (top > 200) top = 200;

            string cs = ConfigurationManager.ConnectionStrings["IdentityConnection"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
            {
                throw new InvalidOperationException("No existe IdentityConnection en Web.config");
            }

            var result = new List<TelegramDesignPhotoDTO>();
            using (var cn = new SqlConnection(cs))
            {
                cn.Open();
                string sql = @"
SELECT TOP (@TopN)
  SysObjectID, LinDesign, TelegramMessageId, TelegramChatId, TelegramUserId, TelegramUserName,
  Caption, FileId, FileUniqueId, WidthPx, HeightPx, Estado, FechaMensajeUtc, FechaRegistroUtc
FROM dbo.TSql_TelegramDesignInbox
WHERE LinDesign = @LinDesign
  AND ISNULL(Estado, 'Pendiente') = 'Pendiente'
ORDER BY FechaRegistroUtc DESC;";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@TopN", top);
                    cmd.Parameters.AddWithValue("@LinDesign", designId);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            result.Add(new TelegramDesignPhotoDTO
                            {
                                Id = rd["SysObjectID"] == DBNull.Value ? 0 : Convert.ToInt64(rd["SysObjectID"]),
                                DesignId = rd["LinDesign"] == DBNull.Value ? 0 : Convert.ToInt64(rd["LinDesign"]),
                                TelegramMessageId = rd["TelegramMessageId"] as string,
                                TelegramChatId = rd["TelegramChatId"] as string,
                                TelegramUserId = rd["TelegramUserId"] as string,
                                TelegramUserName = rd["TelegramUserName"] as string,
                                Caption = rd["Caption"] as string,
                                FileId = rd["FileId"] as string,
                                FileUniqueId = rd["FileUniqueId"] as string,
                                WidthPx = rd["WidthPx"] == DBNull.Value ? 0 : Convert.ToInt32(rd["WidthPx"]),
                                HeightPx = rd["HeightPx"] == DBNull.Value ? 0 : Convert.ToInt32(rd["HeightPx"]),
                                Estado = rd["Estado"] as string,
                                FechaMensajeUtc = rd["FechaMensajeUtc"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rd["FechaMensajeUtc"]),
                                FechaRegistroUtc = rd["FechaRegistroUtc"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(rd["FechaRegistroUtc"])
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
