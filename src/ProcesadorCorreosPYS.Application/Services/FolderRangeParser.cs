using ProcesadorCorreosPYS.Application.Abstractions;

namespace ProcesadorCorreosPYS.Application.Services;

public sealed class FolderRangeParser : IFolderRangeParser
{
    public IReadOnlyList<int> Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return [];
        }

        var values = new SortedSet<int>();
        var chunks = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var chunk in chunks)
        {
            if (chunk.Contains('-'))
            {
                var parts = chunk.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length != 2 || !int.TryParse(parts[0], out var start) || !int.TryParse(parts[1], out var end))
                {
                    throw new FormatException($"Rango inválido: {chunk}");
                }

                if (start > end)
                {
                    (start, end) = (end, start);
                }

                for (var current = start; current <= end; current++)
                {
                    values.Add(current);
                }

                continue;
            }

            if (!int.TryParse(chunk, out var single))
            {
                throw new FormatException($"Número inválido: {chunk}");
            }

            values.Add(single);
        }

        return values.ToArray();
    }
}
