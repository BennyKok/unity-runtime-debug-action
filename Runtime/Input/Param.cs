using System;

namespace BennyKok.RuntimeDebug.DebugInput
{
    /// <summary>
    /// Data class for holding input param definition for use in the <see cref="RuntimeDebug.DebugInput.InputQuery"/>
    /// </summary>
    [Serializable]
    public class Param
    {
        public string name;
        public ParamType type;
        public string description;
        public object defaultValue;
        public Func<object> valuePrefillCallback = null;

        public Param(string name, ParamType type, string description = null, object defaultValue = null, Func<object> valuePrefillCallback = null)
        {
            this.name = name;
            this.type = type;
            this.description = description;
            this.defaultValue = defaultValue;
            this.valuePrefillCallback = valuePrefillCallback;
        }

        public static ParamType MapTypeToParamType(Type type)
        {
            if (type == typeof(bool))
                return ParamType.Bool;

            if (type == typeof(int))
                return ParamType.Int;

            if (type == typeof(float))
                return ParamType.Float;

            if (type == typeof(string))
                return ParamType.String;

            throw new Exception("Unsupported ParamType");
        }

        public static Type MapParamTypeToType(ParamType type)
        {
            if (type == ParamType.Bool)
                return typeof(bool);

            if (type == ParamType.Int)
                return typeof(int);

            if (type == ParamType.Float)
                return typeof(float);

            if (type == ParamType.String)
                return typeof(string);

            throw new Exception("Unsupported Type");
        }
    }
}