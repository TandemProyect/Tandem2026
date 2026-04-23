using System;
using System.Collections.Generic;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;
using ZwcadPlugin.Models;

namespace ZwcadPlugin
{
    public static class ZwcadHelper
    {
        #region Conversiones de Entidades a DTOs

        /// <summary>
        /// Extrae todas las entidades del dibujo actual
        /// </summary>
        public static List<EntidadDTO> ExtraerEntidades(Database db)
        {
            var entidades = new List<EntidadDTO>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId objId in btr)
                {
                    Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);
                    EntidadDTO dto = ConvertirEntidad(ent);

                    if (dto != null)
                        entidades.Add(dto);
                }

                tr.Commit();
            }

            return entidades;
        }

        /// <summary>
        /// Convierte una entidad de ZWCAD a DTO
        /// </summary>
        private static EntidadDTO ConvertirEntidad(Entity ent)
        {
            var dto = new EntidadDTO
            {
                Layer = ent.Layer,
                Color = ent.Color.ToString(),
                Propiedades = new Dictionary<string, object>()
            };

            if (ent is Line linea)
            {
                dto.Tipo = "Linea";
                dto.Propiedades["InicioX"] = linea.StartPoint.X;
                dto.Propiedades["InicioY"] = linea.StartPoint.Y;
                dto.Propiedades["InicioZ"] = linea.StartPoint.Z;
                dto.Propiedades["FinX"] = linea.EndPoint.X;
                dto.Propiedades["FinY"] = linea.EndPoint.Y;
                dto.Propiedades["FinZ"] = linea.EndPoint.Z;
            }
            else if (ent is Circle circulo)
            {
                dto.Tipo = "Circulo";
                dto.Propiedades["CentroX"] = circulo.Center.X;
                dto.Propiedades["CentroY"] = circulo.Center.Y;
                dto.Propiedades["CentroZ"] = circulo.Center.Z;
                dto.Propiedades["Radio"] = circulo.Radius;
            }
            else if (ent is Arc arco)
            {
                dto.Tipo = "Arco";
                dto.Propiedades["CentroX"] = arco.Center.X;
                dto.Propiedades["CentroY"] = arco.Center.Y;
                dto.Propiedades["CentroZ"] = arco.Center.Z;
                dto.Propiedades["Radio"] = arco.Radius;
                dto.Propiedades["AnguloInicio"] = arco.StartAngle;
                dto.Propiedades["AnguloFin"] = arco.EndAngle;
            }
            else if (ent is Polyline polilinea)
            {
                dto.Tipo = "Polilinea";
                dto.Propiedades["Cerrada"] = polilinea.Closed;

                var vertices = new List<Dictionary<string, double>>();
                for (int i = 0; i < polilinea.NumberOfVertices; i++)
                {
                    Point2d pt = polilinea.GetPoint2dAt(i);
                    vertices.Add(new Dictionary<string, double>
                    {
                        { "X", pt.X },
                        { "Y", pt.Y }
                    });
                }
                dto.Propiedades["Vertices"] = vertices;
            }
            else if (ent is BlockReference bloque)
            {
                dto.Tipo = "ReferenciaBloque";
                dto.Propiedades["NombreBloque"] = bloque.Name;
                dto.Propiedades["PosicionX"] = bloque.Position.X;
                dto.Propiedades["PosicionY"] = bloque.Position.Y;
                dto.Propiedades["PosicionZ"] = bloque.Position.Z;
                dto.Propiedades["Rotacion"] = RadianesAGrados(bloque.Rotation);

                // Extraer atributos si existen
                var atributos = new Dictionary<string, string>();
                if (bloque.AttributeCollection.Count > 0)
                {
                    foreach (ObjectId attId in bloque.AttributeCollection)
                    {
                        using (var att = (AttributeReference)attId.GetObject(OpenMode.ForRead))
                        {
                            atributos[att.Tag] = att.TextString;
                        }
                    }
                }
                dto.Propiedades["Atributos"] = atributos;
            }
            else
            {
                // Tipo no soportado
                return null;
            }

            return dto;
        }

        #endregion

        #region Conversiones de Layers

        /// <summary>
        /// Extrae todos los layers del dibujo
        /// </summary>
        public static List<LayerDTO> ExtraerLayers(Database db)
        {
            var layers = new List<LayerDTO>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

                foreach (ObjectId layerId in lt)
                {
                    LayerTableRecord ltr = (LayerTableRecord)tr.GetObject(layerId, OpenMode.ForRead);

                    layers.Add(new LayerDTO
                    {
                        Nombre = ltr.Name,
                        Color = ltr.Color.ToString(),
                        Visible = !ltr.IsOff,
                        Bloqueado = ltr.IsLocked
                    });
                }

                tr.Commit();
            }

            return layers;
        }

        #endregion

        #region Conversiones de Bloques

        /// <summary>
        /// Extrae información de todas las referencias a bloques
        /// </summary>
        public static List<BloqueDTO> ExtraerBloques(Database db)
        {
            var bloques = new List<BloqueDTO>();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId objId in btr)
                {
                    Entity ent = (Entity)tr.GetObject(objId, OpenMode.ForRead);

                    if (ent is BlockReference bloque)
                    {
                        var bloqueDto = new BloqueDTO
                        {
                            Nombre = bloque.Name,
                            PuntoInsertX = bloque.Position.X,
                            PuntoInsertY = bloque.Position.Y,
                            PuntoInsertZ = bloque.Position.Z,
                            Escala = bloque.ScaleFactors.X, // Asumimos escala uniforme
                            Rotacion = RadianesAGrados(bloque.Rotation),
                            Atributos = new Dictionary<string, string>()
                        };

                        // Extraer atributos
                        if (bloque.AttributeCollection.Count > 0)
                        {
                            foreach (ObjectId attId in bloque.AttributeCollection)
                            {
                                using (var att = (AttributeReference)attId.GetObject(OpenMode.ForRead))
                                {
                                    bloqueDto.Atributos[att.Tag] = att.TextString;
                                }
                            }
                        }

                        bloques.Add(bloqueDto);
                    }
                }

                tr.Commit();
            }

            return bloques;
        }

        #endregion

        #region Utilidades

        /// <summary>
        /// Convierte radianes a grados
        /// </summary>
        public static double RadianesAGrados(double radianes)
        {
            return radianes * (180.0 / Math.PI);
        }

        /// <summary>
        /// Convierte grados a radianes
        /// </summary>
        public static double GradosARadianes(double grados)
        {
            return grados * (Math.PI / 180.0);
        }

        /// <summary>
        /// Obtiene el nombre de usuario actual del sistema
        /// </summary>
        public static string ObtenerUsuarioActual()
        {
            return Environment.UserName;
        }

        #endregion
    }
}
