using System.Reflection;

namespace PaymentApp.Domain.Utilities;

public static class EntityDescriptor
{
    /// <summary>
    /// Returns a description of all public properties on a type.
    /// This is how EF Core discovers your model — no schema files needed.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetProperties<T>() where T : class
    {
        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Returns property names and their types as strings.
    /// </summary>
    public static IEnumerable<(string Name, string TypeName)> Describe<T>() where T : class
    {
        return GetProperties<T>()
            .Select(p => (p.Name, p.PropertyType.Name));
    }

    /// <summary>
    /// Gets a property value by name at runtime.
    /// This is how serializers work — they read/write properties dynamically.
    /// </summary>
    public static object? GetValue<T>(T entity, string propertyName) where T : class
    {
        var prop = typeof(T).GetProperty(propertyName);
        return prop?.GetValue(entity);
    }

    /// <summary>
    /// Sets a property value by name at runtime.
    /// </summary>
    public static void SetValue<T>(T entity, string propertyName, object? value) where T : class
    {
        var prop = typeof(T).GetProperty(propertyName);
        prop?.SetValue(entity, value);
    }
}