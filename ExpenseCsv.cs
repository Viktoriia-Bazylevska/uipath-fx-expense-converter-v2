using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExpenseClaimConverter
{
    /// <summary>Reads the defined expense CSV shape and writes the converted output CSV.</summary>
    public static class ExpenseCsv
    {
        private static readonly string[] RequiredColumns = ["LineId", "ExpenseDate", "Description", "Amount", "Currency"];

        /// <summary>Reads expense lines and validates the required header.</summary>
        public static IReadOnlyList<ExpenseLine> Read(string inputPath)
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException("Input file was not found: " + inputPath, inputPath);
            }

            string[] sourceLines = File.ReadAllLines(inputPath, Encoding.UTF8);
            if (sourceLines.Length == 0)
            {
                throw new InvalidDataException("Missing required column 'LineId'.");
            }

            List<string> header = ParseCsvLine(sourceLines[0]);
            if (header.Count > 0)
            {
                header[0] = header[0].TrimStart('\uFEFF');
            }
            for (int index = 0; index < RequiredColumns.Length; index++)
            {
                if (index >= header.Count || !string.Equals(header[index].Trim(), RequiredColumns[index], StringComparison.Ordinal))
                {
                    throw new InvalidDataException("Missing required column '" + RequiredColumns[index] + "'.");
                }
            }

            List<ExpenseLine> expenses = [];
            for (int lineIndex = 1; lineIndex < sourceLines.Length; lineIndex++)
            {
                if (string.IsNullOrWhiteSpace(sourceLines[lineIndex]))
                {
                    continue;
                }
                List<string> fields = ParseCsvLine(sourceLines[lineIndex]);
                if (fields.Count < RequiredColumns.Length)
                {
                    throw new InvalidDataException("Input line " + (lineIndex + 1).ToString() + " has fewer than five required columns.");
                }
                expenses.Add(new ExpenseLine
                {
                    LineId = fields[0],
                    ExpenseDateText = fields[1],
                    Description = fields[2],
                    AmountText = fields[3],
                    Currency = fields[4]
                });
            }
            return expenses;
        }

        /// <summary>Writes converted lines followed by the required TOTAL row.</summary>
        public static void Write(string outputPath, IEnumerable<ExpenseLine> lines, decimal total, string targetCurrency)
        {
            string directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            StringBuilder output = new();
            output.AppendLine("LineId,ExpenseDate,Description,Amount,Currency,Rate,RateDate,ConvertedAmount,Status,Message");
            foreach (ExpenseLine line in lines)
            {
                output.AppendLine(string.Join(",", new[]
                {
                    Escape(line.LineId), Escape(line.ExpenseDateText), Escape(line.Description), Escape(line.AmountText), Escape(line.Currency),
                    Escape(line.Rate == 0m ? string.Empty : line.Rate.ToString("0.##########", System.Globalization.CultureInfo.InvariantCulture)),
                    Escape(line.RateDate.HasValue ? line.RateDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : string.Empty),
                    Escape(line.ConvertedAmount.HasValue ? line.ConvertedAmount.Value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) : string.Empty),
                    Escape(line.Status), Escape(line.Message)
                }));
            }
            output.AppendLine(string.Join(",", new[]
            {
                Escape("TOTAL"), Escape(string.Empty), Escape(string.Empty), Escape(string.Empty), Escape(targetCurrency), Escape(string.Empty),
                Escape(string.Empty), Escape(total.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)), Escape(string.Empty), Escape(string.Empty)
            }));
            File.WriteAllText(outputPath, output.ToString(), new UTF8Encoding(false));
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> fields = new List<string>();
            StringBuilder field = new StringBuilder();
            bool quoted = false;
            for (int index = 0; index < line.Length; index++)
            {
                char character = line[index];
                if (character == '"')
                {
                    if (quoted && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                }
                else if (character == ',' && !quoted)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(character);
                }
            }
            if (quoted)
            {
                throw new InvalidDataException("Unclosed quoted CSV field.");
            }
            fields.Add(field.ToString());
            return fields;
        }

        private static string Escape(string value)
        {
            string text = value ?? string.Empty;
            if (text.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0)
            {
                return '"' + text.Replace("\"", "\"\"") + '"';
            }
            return text;
        }
    }
}
