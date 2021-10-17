using System;
using System.Reflection;
using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.Attributes;

namespace BennyKok.RuntimeDebug.Data
{
    public struct ReflectedActionData
    {
        public PropertyInfo propertyInfo;
        public FieldInfo fieldInfo;
        public MethodInfo methodInfo;
        public ParameterInfo[] parameterInfos;
        public DebugActionAttribute attribute;

        public string GetActionName()
        {
            if (!string.IsNullOrEmpty(attribute.name)) return attribute.name;
            if (fieldInfo != null) return fieldInfo.Name;
            if (propertyInfo != null) return propertyInfo.Name;
            return methodInfo.Name;
        }

        public bool IsButton()
        {
            return methodInfo != null && (parameterInfos == null || parameterInfos.Length == 0);
        }

        public bool IsInput()
        {
            return methodInfo != null && (parameterInfos != null && parameterInfos.Length > 0);
        }

        public string[] GetFlagDisplay()
        {
            Type type = null;
            if (fieldInfo != null) type = fieldInfo.FieldType;
            if (propertyInfo != null) type = propertyInfo.PropertyType;

            if (type == typeof(bool))
                return DebugActionFlag.BOOLEAN_VALUES;

            if (type.IsEnum)
                return Enum.GetNames(type);

            return null;
        }

        public int GetFlagValue(object obj)
        {
            var value = GetValue(obj);
            // if (value != null) return Convert.ToInt32(value);

            // Making sure if the enum has special value, it will still works
            var flagIndex = Array.IndexOf(Enum.GetValues(GetTargetType()), value);
            return flagIndex;
        }

        public void SetFlagValue(object obj, object value)
        {
            // Making sure if the enum has special value, it will still works
            SetValue(obj, Enum.GetValues(GetTargetType()).GetValue(Convert.ToInt32(value)));
        }

        public bool GetBoolValue(object obj)
        {
            var value = GetValue(obj);
            if (value != null) return Convert.ToBoolean(value);
            return false;
        }

        public object GetValue(object obj)
        {
            if (fieldInfo != null) return fieldInfo.GetValue(obj);
            if (propertyInfo != null) return propertyInfo.GetGetMethod(false).Invoke(obj, null);
            return null;
        }

        private static object Cast(object value, Type castTo)
        {
            if (castTo.IsEnum) return Enum.ToObject(castTo, value);
            return Convert.ChangeType(value, castTo);
        }

        public void SetValue(object obj, object value)
        {
            var castedValue = Cast(value, GetTargetType());

            if (fieldInfo != null) fieldInfo.SetValue(obj, castedValue);
            if (propertyInfo != null) propertyInfo.GetSetMethod(false).Invoke(obj, new object[] { castedValue });
        }

        public Type GetTargetType()
        {
            Type type = null;
            if (fieldInfo != null) type = fieldInfo.FieldType;
            if (propertyInfo != null) type = propertyInfo.PropertyType;

            return type;
        }

        public string GetTargetName()
        {
            string type = null;
            if (fieldInfo != null) type = fieldInfo.Name;
            if (propertyInfo != null) type = propertyInfo.Name;

            return type;
        }

        public bool IsEnum()
        {
            var type = GetTargetType();
            return type != null && type.IsEnum;
        }

        public bool IsBoolean()
        {
            var type = GetTargetType();
            return type != null && type == typeof(bool);
        }

        public bool IsInt()
        {
            var type = GetTargetType();
            return type != null && type == typeof(int);
        }

        public bool IsFloat()
        {
            var type = GetTargetType();
            return type != null && type == typeof(float);
        }

        public bool IsString()
        {
            var type = GetTargetType();
            return type != null && type == typeof(string);
        }
    }
}