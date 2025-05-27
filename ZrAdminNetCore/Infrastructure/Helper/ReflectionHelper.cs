using System;
using System.Reflection;

namespace ZR.Infrastructure.Helper
{
    /// <summary>
    /// Provides utility methods for working with reflection, including setting field values and type conversion.
    /// </summary>
    /// <remarks>The <see cref="ReflectionHelper"/> class contains static methods that simplify common
    /// reflection tasks, such as setting the value of fields (including private fields) and converting objects to
    /// specific types. These methods are designed to handle scenarios where case-insensitive field matching or type
    /// coercion is required.</remarks>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Attempts to set the value of a specified field on the target object.
        /// </summary>
        /// <remarks>This method uses reflection to locate and set the field value. It supports both
        /// public and non-public instance fields. If the field value cannot be set due to an error (e.g., type
        /// conversion failure), the method logs the error and returns <see langword="false"/>.</remarks>
        /// <param name="targetObject">The object whose field value is to be set. Cannot be <see langword="null"/>.</param>
        /// <param name="fieldName">The name of the field to set. The search is case-insensitive.</param>
        /// <param name="fieldValue">The value to assign to the field. The value will be converted to the field's type if necessary.</param>
        /// <returns><see langword="true"/> if the field value was successfully set; otherwise, <see langword="false"/>.</returns>
        public static bool TrySetFieldValue(this object targetObject, string fieldName, object fieldValue)
        {
            try
            {
                if (targetObject == null)
                    throw new ArgumentNullException(nameof(targetObject));

                Type type = targetObject.GetType();
                FieldInfo field = type.GetField(
                    fieldName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.IgnoreCase
                );

                if (field == null)
                    throw new ArgumentException($"字段 '{fieldName}' 不存在。");

                object convertedValue = fieldValue.TryConvertTo(field.FieldType);
                field.SetValue(targetObject, convertedValue);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置字段值失败: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Attempts to set the value of a specified property on the target object.
        /// </summary>
        /// <remarks>This method attempts to set the value of the specified property on the target object.
        /// If the property value cannot be set (e.g., due to type conversion issues or other errors), the method will
        /// catch the exception, log the error, and return <see langword="false"/>.</remarks>
        /// <param name="targetObject">The object whose property value is to be set. Cannot be <see langword="null"/>.</param>
        /// <param name="propertyName">The name of the property to set. The search is case-insensitive.</param>
        /// <param name="propertyValue">The value to assign to the property. It will be converted to the property's type if necessary.</param>
        /// <returns><see langword="true"/> if the property value was successfully set; otherwise, <see langword="false"/>.</returns>
        public static bool TrySetPropertyValue(this object targetObject, string propertyName, object propertyValue)
        {
            try
            {
                if (targetObject == null)
                    throw new ArgumentNullException(nameof(targetObject));
                Type type = targetObject.GetType();
                PropertyInfo property = type.GetProperty(
                    propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.IgnoreCase
                );
                if (property == null)
                    throw new ArgumentException($"属性 '{propertyName}' 不存在。");
                object convertedValue = propertyValue.TryConvertTo(property.PropertyType);
                property.SetValue(targetObject, convertedValue);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置属性值失败: {ex.Message}");
                return false;
            }
        }
        /// <summary>
        /// Converts the specified object to the specified target type.
        /// </summary>
        /// <remarks>This method supports conversion to nullable types, enums, and other basic types. For
        /// enum conversions, if the input value is a string,  it performs a case-insensitive parse. For other types, it
        /// uses <see cref="Convert.ChangeType(object, Type)"/> to perform the conversion.</remarks>
        /// <param name="value">The object to convert. Can be <see langword="null"/> if the target type is nullable.</param>
        /// <param name="targetType">The type to which the object should be converted. Must not be <see langword="null"/>.</param>
        /// <returns>An object of the specified target type. If <paramref name="value"/> is <see langword="null"/> and the target
        /// type is nullable,  <see langword="null"/> is returned. For non-nullable value types, an exception is thrown
        /// if <paramref name="value"/> is <see langword="null"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="value"/> is <see langword="null"/> and <paramref name="targetType"/> is a
        /// non-nullable value type.</exception>
        public static object? TryConvertTo(this object value, Type targetType)
        {
            try
            {
                if (value == null)
                {
                    if (targetType.IsValueType &&
                        (!targetType.IsGenericType ||
                         targetType.GetGenericTypeDefinition() != typeof(Nullable<>)))
                    {
                        throw new InvalidOperationException(
                            $"无法将 null 分配给非可空类型 '{targetType}'。");
                    }
                    return null;
                }

                Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

                // 处理枚举类型
                if (underlyingType.IsEnum)
                {
                    if (value is string enumString)
                        return Enum.Parse(underlyingType, enumString, ignoreCase: true);

                    return Enum.ToObject(underlyingType, value);
                }

                // 处理基础类型转换
                return Convert.ChangeType(value, underlyingType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                     $"无法将值 '{value}' 转换为类型 '{targetType}': {ex.Message}", ex);
                return default; // 返回默认值，通常为 null 或类型的默认值
            }
        }
    }
}
