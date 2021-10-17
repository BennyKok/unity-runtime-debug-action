using System;
using System.Collections.Generic;
using System.Linq;

namespace BennyKok.RuntimeDebug.DebugInput
{
    /// <summary>
    /// Data class to warp around the response returned by the Input debug action
    /// </summary>
    public class InputResponse
    {
        public Dictionary<Param, string> responses;

        private InputQuery query;

        public bool AllInputValid => parsedResponse != null;

        private List<object> parsedResponse;

        public InputResponse(Dictionary<Param, string> responses, InputQuery query)
        {
            this.responses = responses;
            this.query = query;

            //There maybe error
            try
            {
                parsedResponse = ParseToObjects();
            }
            catch (System.Exception) { }
        }

        private static object Cast(object obj, Type castTo)
        {
            return Convert.ChangeType(obj, castTo);
        }

        public object[] GetObjectsForReflection(bool fillMissing = true)
        {
            //if there is some optional input, have to fill them with the default value (Type.Missing)
            if (fillMissing && responses.Count < query.allParams.Count)
            {
                var newResponses = parsedResponse.ToList();
                for (int i = query.allParams.Count - responses.Count; i < query.allParams.Count; i++)
                {
                    var param = query.allParams[i];
                    newResponses.Add(Type.Missing);
                }
                return newResponses.ToArray();
            }
            else
            {
                return parsedResponse.ToArray();
            }
        }

        private List<object> ParseToObjects()
        {
            var reference = this;

            var result = responses.Select<KeyValuePair<Param, string>, object>(entry =>
                {
                    switch (entry.Key.type)
                    {
                        case ParamType.Bool:
                            return reference.GetParamBool(entry);
                        case ParamType.Int:
                            return reference.GetParamInt(entry);
                        case ParamType.Float:
                            return reference.GetParamFloat(entry);
                        case ParamType.String:
                            return reference.GetParamString(entry);
                        default:
                            return null;
                    }
                }).ToList();

            return result;
        }

        public KeyValuePair<Param, string> GetParam(string paramName)
        {
            foreach (var response in responses)
                if (response.Key.name == paramName)
                    return response;

            //This is a optional param
            if (query.allParams.Exists(x => x.name == paramName))
                return default(KeyValuePair<Param, string>);

            throw new Exception("Unable to find param: " + paramName);
        }

        public string GetParamString(string paramName) => GetParamString(GetParam(paramName));

        public string GetParamString(KeyValuePair<Param, string> entry)
        {
            if (entry.Value == null && entry.Key != null)
                return (string)entry.Key.defaultValue;

            return entry.Value;
        }

        public bool GetParamBool(string paramName) => GetParamBool(GetParam(paramName));

        public bool GetParamBool(KeyValuePair<Param, string> entry)
        {
            if (entry.Value == null && entry.Key != null)
                return (bool)entry.Key.defaultValue;

            return ParseBool(entry.Value);
        }

        public float GetParamFloat(string paramName) => GetParamFloat(GetParam(paramName));

        public float GetParamFloat(KeyValuePair<Param, string> entry)
        {
            //Make sure the number is not null
            if (!string.IsNullOrEmpty(entry.Value))
                if (float.TryParse(entry.Value, out var value))
                    return value;
                else
                    UnityEngine.Debug.LogWarning("Unable to parse the input float");

            return (float)entry.Key.defaultValue;
        }

        public int GetParamInt(string paramName) => GetParamInt(GetParam(paramName));

        public int GetParamInt(KeyValuePair<Param, string> entry)
        {
            //Make sure the number is not null
            if (!string.IsNullOrEmpty(entry.Value))
                if (int.TryParse(entry.Value, out var value))
                    return value;
                else
                    UnityEngine.Debug.LogWarning("Unable to parse the input int");

            return (int)entry.Key.defaultValue;
        }

        private bool ParseBool(string value)
        {
            if (Boolean.TryParse(value, out var boolValue))
                return boolValue;

            return false;
        }
    }
}