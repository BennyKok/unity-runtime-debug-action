using System.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BennyKok.RuntimeDebug.DebugInput
{
    /// <summary>
    /// Data class for holding list of input params definition for use in the <see cref="RuntimeDebug.Actions.DebugActionInput"/>
    /// </summary>
    [Serializable]
    public class InputQuery
    {
        /// <summary>
        /// Create a new instance of <see cref="InputQuery"/>
        /// </summary>
        /// <returns>A new instace of <see cref="InputQuery"/></returns>
        public static InputQuery Create() => new InputQuery();

        public List<Param> allParams = new List<Param>();

        public InputQuery SetParams(List<Param> allParams)
        {
            this.allParams = allParams;
            return this;
        }

        /// <summary>
        /// Get a string representation of all the params definition
        /// </summary>
        /// <returns>Returns a string</returns>
        public string GetParamsDisplay()
        {
            return allParams.Aggregate("", (i, j) => i + (i.Length == 0 ? "" : " ") + $"<{j.name}>");
        }

        /// <summary>
        /// Add a new param to the param list
        /// </summary>
        /// <exception cref="ParamWithoutDefaultValueException">Thrown when there's no default value for optional param</exception>
        /// <exception cref="ParamNameDuplicatedException">Thrown when there's a duplicated param name</exception>
        public InputQuery Query(string name, ParamType type)
        {
            return Query(name, type, null, null, null);
        }

        /// <summary>
        /// Add a new param to the param list
        /// </summary>
        /// <exception cref="ParamWithoutDefaultValueException">Thrown when there's no default value for optional param</exception>
        /// <exception cref="ParamNameDuplicatedException">Thrown when there's a duplicated param name</exception>
        public InputQuery Query(string name, ParamType type, string description)
        {
            return Query(name, type, description, null, null);
        }

        /// <summary>
        /// Add a new param to the param list
        /// </summary>
        /// <exception cref="ParamWithoutDefaultValueException">Thrown when there's no default value for optional param</exception>
        /// <exception cref="ParamNameDuplicatedException">Thrown when there's a duplicated param name</exception>
        public InputQuery Query(string name, ParamType type, string description, object defaultValue)
        {
            return Query(name, type, description, defaultValue, null);
        }

        /// <summary>
        /// Add a new param to the param list
        /// </summary>
        /// <exception cref="ParamWithoutDefaultValueException">Thrown when there's no default value for optional param</exception>
        /// <exception cref="ParamNameDuplicatedException">Thrown when there's a duplicated param name</exception>
        public InputQuery Query(string name, ParamType type, string description, Func<object> valuePrefillCallback)
        {
            return Query(name, type, description, null, valuePrefillCallback);
        }

        private InputQuery Query(string name, ParamType type, string description, object defaultValue, Func<object> valuePrefillCallback)
        {
            var param = new Param(name, type, description, defaultValue, valuePrefillCallback);

            if (allParams.Count > 0 && allParams.Last().defaultValue != null)
                throw new ParamWithoutDefaultValueException();

            if (allParams.Count > 0 && allParams.Any(x => x.name == param.name))
                throw new ParamNameDuplicatedException(param.name);

            allParams.Add(param);
            return this;
        }
    }

    public class ParamNameDuplicatedException : Exception
    {
        public ParamNameDuplicatedException(string name) :
            base($"Parameters must have unique name, duplicated name : {name}")
        { }
    }

    public class ParamWithoutDefaultValueException : Exception
    {
        public ParamWithoutDefaultValueException() :
            base("Ending parameters must have default value if the previous param in the list has default value")
        { }
    }
}