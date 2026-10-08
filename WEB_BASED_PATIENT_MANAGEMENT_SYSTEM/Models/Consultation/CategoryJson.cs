using System.Text.Json;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Models
{
    /// <summary>
    /// Saves a category (group) of a record's form fields together in one JSON
    /// column, so a table has one column per category instead of one per input.
    /// The fields stay ordinary properties, so the forms still bind to them by name.
    /// </summary>
    internal static class CategoryJson
    {
        /// <summary>
        /// Serializes the named properties of the record. Empty fields are left out,
        /// and a category with no values is stored as NULL.
        /// </summary>
        public static string? Write(object record, IReadOnlyList<string> fields)
        {
            var values = new Dictionary<string, object>();
            foreach (var field in fields)
            {
                var value = record.GetType().GetProperty(field)!.GetValue(record);
                if (value != null)
                    values[field] = value;
            }

            return values.Count == 0 ? null : JsonSerializer.Serialize(values);
        }

        /// <summary>
        /// Fills the named properties of the record from the stored JSON.
        /// A field that is not in the JSON is set to null.
        /// </summary>
        public static void Read(object record, IReadOnlyList<string> fields, string? json)
        {
            using var document = string.IsNullOrWhiteSpace(json) ? null : JsonDocument.Parse(json);
            foreach (var field in fields)
            {
                var property = record.GetType().GetProperty(field)!;
                object? value = null;

                if (document != null
                    && document.RootElement.TryGetProperty(field, out var element)
                    && element.ValueKind != JsonValueKind.Null)
                {
                    value = property.PropertyType == typeof(string) && element.ValueKind != JsonValueKind.String
                        ? element.GetRawText()
                        : element.Deserialize(property.PropertyType);
                }

                property.SetValue(record, value);
            }
        }
    }
}
