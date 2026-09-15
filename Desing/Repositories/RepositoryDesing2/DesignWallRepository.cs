using DAL;
using Desing.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;

namespace Desing.Repositories.RepositoryDesing2
{
    public sealed class Desing2XyzMmDto
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }
    }

    public sealed class Desing2DesignWallLineDto
    {
        public long? Id { get; set; }
        public Desing2XyzMmDto P1Mm { get; set; }
        public Desing2XyzMmDto P2Mm { get; set; }
        public long? PolylineGroupId { get; set; }
        public long? WallGroupId { get; set; }
        public object WallId { get; set; }
        public string WallRole { get; set; }
        public long? LinkOffsetFromLineId { get; set; }
        public double? NumberOffsetMm { get; set; }
        public double? NumberWallFaceSideSign { get; set; }
        public Desing2XyzMmDto WallDrawP1Mm { get; set; }
        public Desing2XyzMmDto WallDrawP2Mm { get; set; }
        public string TextSystem { get; set; }

        public string _idObject { get; set; }
        public string _TypeMesh { get; set; }
        public double? _Datalong { get; set; }
        public double? _DataWith { get; set; }
        public double? _DataHeight { get; set; }
        public double? _XRotation { get; set; }
        public double? _YrRtation { get; set; }
        public double? _ZRotation { get; set; }
        public bool? _IsFormwork { get; set; }
        public bool? _IsUniversalPanel { get; set; }
        public double? _XCoordinate { get; set; }
        public double? _YCoordinate { get; set; }
        public double? _ZCoordinate { get; set; }
        public string _Tape_1 { get; set; }
        public string _Tape_2 { get; set; }
        public string _Idconnection_1 { get; set; }
        public string _Idconnection_2 { get; set; }
        public bool? _CHeckBracketInside { get; set; }
        public bool? _CHeckBracketOutside { get; set; }
        public bool? _CHeckRijiInside { get; set; }
        public bool? _CHeckRijiOutside { get; set; }
        public bool? _CHeckPropInside { get; set; }
        public bool? _CHeckPropOutside { get; set; }
        public bool? _CHeckPropInsideInf { get; set; }
        public bool? _CHeckPropOutsideInf { get; set; }
    }

    public sealed class Desing2DesignWallSaveRequest
    {
        public long DesignId { get; set; }
        public List<Desing2DesignWallLineDto> Lines { get; set; }
        public long? NextSegId { get; set; }
        public long? NextPolylineGroupId { get; set; }
        public long? NextWallGroupId { get; set; }
    }

    public sealed class Desing2DesignWallSnapshotDto
    {
        public List<Desing2DesignWallLineDto> Lines { get; set; }
        public long NextSegId { get; set; }
        public long NextPolylineGroupId { get; set; }
        public long NextWallGroupId { get; set; }
    }

    public sealed class DesignWallRepository
    {
        private readonly ConexionData _db;

        public DesignWallRepository(ConexionData db)
        {
            _db = db;
        }

        public TSql_Design_V2 FindActiveDesign(long designId)
        {
            if (designId <= 0)
            {
                return null;
            }

            return _db.TSql_Design_V2.FirstOrDefault(d => d.SysObjectID == designId && !d.AttIsDeleted);
        }

        public int ReplaceWalls(long designId, IList<Desing2DesignWallLineDto> lines, string userId)
        {
            var existing = _db.TSql_DesignWall
                .Where(w => w.LinkDesign_V2 == designId && !w.Is_Delete)
                .ToList();

            for (var i = 0; i < existing.Count; i++)
            {
                IntranetAuditHelper.SetAuditOnDelete(existing[i], userId);
            }

            var count = 0;
            if (lines == null)
            {
                return count;
            }

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line == null || line.P1Mm == null || line.P2Mm == null)
                {
                    continue;
                }

                if (!line.P1Mm.X.HasValue || !line.P1Mm.Z.HasValue || !line.P2Mm.X.HasValue || !line.P2Mm.Z.HasValue)
                {
                    continue;
                }

                var entity = new TSql_DesignWall
                {
                    LinkDesign_V2 = designId
                };
                MapLineToEntity(line, entity);
                IntranetAuditHelper.SetAuditOnCreate(entity, userId);
                _db.TSql_DesignWall.Add(entity);
                count++;
            }

            return count;
        }

        public Desing2DesignWallSnapshotDto LoadSnapshot(long designId)
        {
            var rows = _db.TSql_DesignWall
                .AsNoTracking()
                .Where(w => w.LinkDesign_V2 == designId && !w.Is_Delete && w.Is_Active)
                .OrderBy(w => w.NumberLineId ?? w.IdObject)
                .ThenBy(w => w.IdObject)
                .ToList();

            var lines = new List<Desing2DesignWallLineDto>(rows.Count);
            long maxId = 0;
            long maxPolyline = 0;
            long maxWallGroup = 0;
            for (var i = 0; i < rows.Count; i++)
            {
                var dto = MapEntityToLine(rows[i]);
                lines.Add(dto);
                if (dto.Id.HasValue && dto.Id.Value > maxId) maxId = dto.Id.Value;
                if (dto.PolylineGroupId.HasValue && dto.PolylineGroupId.Value > maxPolyline)
                {
                    maxPolyline = dto.PolylineGroupId.Value;
                }
                if (dto.WallGroupId.HasValue && dto.WallGroupId.Value > maxWallGroup)
                {
                    maxWallGroup = dto.WallGroupId.Value;
                }
            }

            return new Desing2DesignWallSnapshotDto
            {
                Lines = lines,
                NextSegId = maxId + 1,
                NextPolylineGroupId = maxPolyline + 1,
                NextWallGroupId = maxWallGroup + 1
            };
        }

        private static void MapLineToEntity(Desing2DesignWallLineDto line, TSql_DesignWall entity)
        {
            var clientId = ResolveClientWallId(line);
            entity.TextClientWallId = Truncate(clientId, 128);
            entity.NumberLineId = line.Id;
            entity.NumberWallGroupId = line.WallGroupId;
            entity.NumberPolylineGroupId = line.PolylineGroupId;
            entity.TextWallRole = Truncate(line.WallRole, 50);
            entity.NumberLinkOffsetFromLineId = line.LinkOffsetFromLineId;
            entity.NumberOffsetMm = line.NumberOffsetMm;
            entity.NumberWallFaceSideSign = line.NumberWallFaceSideSign;
            entity.TextSystem = Truncate(line.TextSystem, 50);

            entity.NumberP1X = line.P1Mm.X.Value;
            entity.NumberP1Y = line.P1Mm.Y ?? 0d;
            entity.NumberP1Z = line.P1Mm.Z.Value;
            entity.NumberP2X = line.P2Mm.X.Value;
            entity.NumberP2Y = line.P2Mm.Y ?? 0d;
            entity.NumberP2Z = line.P2Mm.Z.Value;

            ApplyDrawPoint(line.WallDrawP1Mm, out var d1x, out var d1y, out var d1z);
            entity.NumberDrawP1X = d1x;
            entity.NumberDrawP1Y = d1y;
            entity.NumberDrawP1Z = d1z;
            ApplyDrawPoint(line.WallDrawP2Mm, out var d2x, out var d2y, out var d2z);
            entity.NumberDrawP2X = d2x;
            entity.NumberDrawP2Y = d2y;
            entity.NumberDrawP2Z = d2z;

            entity.TextTypeMesh = Truncate(line._TypeMesh, 50);
            entity.NumberLong = line._Datalong;
            entity.NumberWidth = line._DataWith;
            entity.NumberHeight = line._DataHeight;
            entity.NumberXRotation = line._XRotation;
            entity.NumberYRotation = line._YrRtation;
            entity.NumberZRotation = line._ZRotation;
            entity.NumberXCoordinate = line._XCoordinate;
            entity.NumberYCoordinate = line._YCoordinate;
            entity.NumberZCoordinate = line._ZCoordinate;
            entity.Is_Formwork = line._IsFormwork ?? true;
            entity.Is_UniversalPanel = line._IsUniversalPanel ?? true;
            entity.TextTape_1 = Truncate(line._Tape_1, 50);
            entity.TextTape_2 = Truncate(line._Tape_2, 50);
            entity.TextIdConnection_1 = Truncate(line._Idconnection_1, 128);
            entity.TextIdConnection_2 = Truncate(line._Idconnection_2, 128);
            entity.Is_CheckBracketInside = line._CHeckBracketInside ?? true;
            entity.Is_CheckBracketOutside = line._CHeckBracketOutside ?? true;
            entity.Is_CheckRijiInside = line._CHeckRijiInside ?? true;
            entity.Is_CheckRijiOutside = line._CHeckRijiOutside ?? true;
            entity.Is_CheckPropInside = line._CHeckPropInside ?? true;
            entity.Is_CheckPropOutside = line._CHeckPropOutside ?? true;
            entity.Is_CheckPropInsideInf = line._CHeckPropInsideInf ?? true;
            entity.Is_CheckPropOutsideInf = line._CHeckPropOutsideInf ?? true;

            var label = BuildTextLabel(line, clientId);
            entity.TextLabel = string.IsNullOrWhiteSpace(label) ? "Muro" : Truncate(label, 500);
        }

        private static Desing2DesignWallLineDto MapEntityToLine(TSql_DesignWall row)
        {
            var dto = new Desing2DesignWallLineDto
            {
                Id = row.NumberLineId,
                P1Mm = new Desing2XyzMmDto { X = row.NumberP1X, Y = row.NumberP1Y, Z = row.NumberP1Z },
                P2Mm = new Desing2XyzMmDto { X = row.NumberP2X, Y = row.NumberP2Y, Z = row.NumberP2Z },
                PolylineGroupId = row.NumberPolylineGroupId,
                WallGroupId = row.NumberWallGroupId,
                WallId = string.IsNullOrWhiteSpace(row.TextClientWallId) ? (object)row.NumberWallGroupId : row.TextClientWallId,
                WallRole = row.TextWallRole,
                LinkOffsetFromLineId = row.NumberLinkOffsetFromLineId,
                NumberOffsetMm = row.NumberOffsetMm,
                NumberWallFaceSideSign = row.NumberWallFaceSideSign,
                TextSystem = row.TextSystem,
                _idObject = row.TextClientWallId,
                _TypeMesh = row.TextTypeMesh,
                _Datalong = row.NumberLong,
                _DataWith = row.NumberWidth,
                _DataHeight = row.NumberHeight,
                _XRotation = row.NumberXRotation,
                _YrRtation = row.NumberYRotation,
                _ZRotation = row.NumberZRotation,
                _XCoordinate = row.NumberXCoordinate,
                _YCoordinate = row.NumberYCoordinate,
                _ZCoordinate = row.NumberZCoordinate,
                _IsFormwork = row.Is_Formwork,
                _IsUniversalPanel = row.Is_UniversalPanel,
                _Tape_1 = row.TextTape_1,
                _Tape_2 = row.TextTape_2,
                _Idconnection_1 = row.TextIdConnection_1,
                _Idconnection_2 = row.TextIdConnection_2,
                _CHeckBracketInside = row.Is_CheckBracketInside,
                _CHeckBracketOutside = row.Is_CheckBracketOutside,
                _CHeckRijiInside = row.Is_CheckRijiInside,
                _CHeckRijiOutside = row.Is_CheckRijiOutside,
                _CHeckPropInside = row.Is_CheckPropInside,
                _CHeckPropOutside = row.Is_CheckPropOutside,
                _CHeckPropInsideInf = row.Is_CheckPropInsideInf,
                _CHeckPropOutsideInf = row.Is_CheckPropOutsideInf
            };

            if (row.NumberDrawP1X.HasValue && row.NumberDrawP1Z.HasValue)
            {
                dto.WallDrawP1Mm = new Desing2XyzMmDto
                {
                    X = row.NumberDrawP1X,
                    Y = row.NumberDrawP1Y ?? 0d,
                    Z = row.NumberDrawP1Z
                };
            }

            if (row.NumberDrawP2X.HasValue && row.NumberDrawP2Z.HasValue)
            {
                dto.WallDrawP2Mm = new Desing2XyzMmDto
                {
                    X = row.NumberDrawP2X,
                    Y = row.NumberDrawP2Y ?? 0d,
                    Z = row.NumberDrawP2Z
                };
            }

            return dto;
        }

        private static string ResolveClientWallId(Desing2DesignWallLineDto line)
        {
            if (!string.IsNullOrWhiteSpace(line._idObject))
            {
                return line._idObject.Trim();
            }

            if (line.WallId != null)
            {
                var raw = Convert.ToString(line.WallId, CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    return raw.Trim();
                }
            }

            if (line.WallGroupId.HasValue)
            {
                return line.WallGroupId.Value.ToString(CultureInfo.InvariantCulture);
            }

            if (line.Id.HasValue)
            {
                return line.Id.Value.ToString(CultureInfo.InvariantCulture);
            }

            return null;
        }

        private static string BuildTextLabel(Desing2DesignWallLineDto line, string clientId)
        {
            var role = string.IsNullOrWhiteSpace(line.WallRole) ? "muro" : line.WallRole.Trim();
            if (line._Datalong.HasValue)
            {
                return role + " " + line._Datalong.Value.ToString("0.###", CultureInfo.InvariantCulture) + " m";
            }

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                return role + " " + clientId;
            }

            return role;
        }

        private static void ApplyDrawPoint(
            Desing2XyzMmDto pt,
            out double? x,
            out double? y,
            out double? z)
        {
            x = null;
            y = null;
            z = null;
            if (pt == null || !pt.X.HasValue || !pt.Z.HasValue)
            {
                return;
            }

            x = pt.X;
            y = pt.Y ?? 0d;
            z = pt.Z;
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length <= max ? value : value.Substring(0, max);
        }
    }
}
