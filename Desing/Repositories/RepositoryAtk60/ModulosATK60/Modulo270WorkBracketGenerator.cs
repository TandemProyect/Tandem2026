using Desing.Repositories.Repository_Desing_2.Atk60;
using Desing.Repositories.RepositoryCommun;
using System.Collections.Generic;

namespace Desing.Repositories.RepositoryAtk60.ModulosATK60
{
    /// <summary>
    /// Puente. La ménsula vive en Repository_Desing_2.Atk60.WorkingBracket.
    /// </summary>
    internal static class Modulo270WorkBracketGenerator
    {
        internal static List<Atk60ElementPaintItem> Build(
            Desing2FormworkWallDto wall,
            Atk60WallPaintAnchor anchor,
            double formedLengthMm,
            bool protectionWood,
            IList<double> stiffenerAlongMm)
        {
            return WorkingBracket.Build(wall, anchor, formedLengthMm, protectionWood, stiffenerAlongMm);
        }
    }
}
