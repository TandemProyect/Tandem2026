using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Desing.Repositories.RepositoryCommun
{
    public sealed class FormworkJsonCommonRepository
    {
        public List<Desing2FormworkWallDto> ParseAndNormalizeWalls(string idsJson)
        {
            var walls = new List<Desing2FormworkWallDto>();
            if (string.IsNullOrWhiteSpace(idsJson))
            {
                return walls;
            }

            var parsed = JsonConvert.DeserializeObject<List<Desing2FormworkWallDto>>(idsJson);
            if (parsed == null || parsed.Count == 0)
            {
                return walls;
            }

            return parsed
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Id ?? x.LineId))
                .Select(x =>
                {
                    if (string.IsNullOrWhiteSpace(x.Id))
                    {
                        x.Id = x.LineId;
                    }

                    return x;
                })
                .GroupBy(x => (x.Id ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
        }
    }
}
