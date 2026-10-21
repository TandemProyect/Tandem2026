using Desing.Repositories.Repository_Desing_2.Atk60;
using Desing.Repositories.RepositoryCommun;
using System.Collections.Generic;

namespace Desing.Repositories.RepositoryAtk60.ModulosATK60
{
    /// <summary>
    /// Puente. El encofrado vive en Repository_Desing_2, una clase por elemento.
    /// </summary>
    internal static class Modulo270PanelElementGenerator
    {
        public static List<Atk60ElementPaintItem> Build(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            long module270Count)
        {
            return Atk60ModuleAssembly.Build(wall, anchor, module270Count);
        }

        public static List<Atk60ElementPaintItem> Build(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            long moduleCount,
            double moduleLengthMm,
            double startAlongMm,
            int moduleIndexOffset,
            int moduleCountInWall,
            bool suppressLastEndJoint = false,
            bool trimStartTie = false,
            bool trimEndTie = false,
            double trimStartJointMm = 0d,
            double trimEndJointMm = -1d,
            bool skipStartTie = false,
            bool skipFreeEndTie = false)
        {
            return Atk60ModuleAssembly.Build(
                wall, anchor, moduleCount, moduleLengthMm, startAlongMm,
                moduleIndexOffset, moduleCountInWall, suppressLastEndJoint, false, trimStartTie, trimEndTie,
                trimStartJointMm, trimEndJointMm, skipStartTie, skipFreeEndTie);
        }

        public static List<Atk60ElementPaintItem> BuildRemate(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            double remateLengthMm,
            double startAlongMm)
        {
            return EndFiller.Build(wall, anchor, remateLengthMm, startAlongMm);
        }
    }
}
