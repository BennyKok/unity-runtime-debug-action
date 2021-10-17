using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BennyKok.RuntimeDebug.DebugInput;
using BennyKok.RuntimeDebug.Systems;
using BennyKok.RuntimeDebug.Utils;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Actions
{
    /// <summary>
    /// Input action will be represented as a button in the debug menu, once triggered, an input field will popup, the input will be split by space and parse using regex, if more than one paramenters is defined and you have space between your string, use double quote for string during input.
    /// </summary>
    /// <example>
    /// <code>
    /// RuntimeDebugSystem.RegisterActions(
    ///     DebugActionBuilder.Input()
    ///         .WithName("Clear Player Prefs")
    ///         .WithInputQuery(InputQuery.Create().Query("clear", ParamType.String, "Type clear to confirm clearing"))
    ///         .WithInputAction((response) =>
    ///          {
    ///              if (response.GetParamString("clear") == "clear")
    ///                  PlayerPrefs.DeleteAll();
    ///          })
    /// );
    /// </code>
    /// </example>
    [Serializable]
    public class DebugActionInput : FluentAction<DebugActionInput>
    {
        [Title("Input", 3)]
        public InputQuery inputQuery;

        public Action<InputResponse> inputAction;

        public InputResponse CurrentInputResponse { get; private set; }

        //https://stackoverflow.com/questions/29489689/split-a-string-based-on-whitespace-but-ignore-those-in-quotes
        private static Regex regexParser = new Regex(@"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)");

        private string paramsDescription;

        public override void ResolveAction()
        {
            if (!RuntimeDebugSystem.UIHandler.IsVisible)
            {
                RuntimeDebugSystem.UIHandler.TogglePanel(true, true);
            }
            RuntimeDebugSystem.UIHandler.inputHandler.AskForInput(inputQuery, (result) =>
            {
                var responsesMap = new Dictionary<Param, string>();

                //Special case handling for single string input
                if (inputQuery.allParams.Count == 1 && inputQuery.allParams[0].type == ParamType.String)
                {
                    responsesMap.Add(inputQuery.allParams[0], result);
                }
                else
                {
                    var responseStrings = regexParser.Split(result).Where(s => s != String.Empty).ToArray();

                    //Clean up the " surrounded string, also replace "" to "
                    for (int i = 0; i < responseStrings.Length; i++)
                        responseStrings[i] = responseStrings[i].Trim('\"').Replace("\"\"", "\"");

                    var requiredParamLength = inputQuery.allParams.Sum(x => x.defaultValue == null ? 1 : 0);
                    if (responseStrings.Length < requiredParamLength)
                    {
                        Debug.Log($"Not enough args entered, required : {requiredParamLength}, entered : {responseStrings.Length}");
                        return;
                    }
                    else if (responseStrings.Length > inputQuery.allParams.Count)
                    {
                        Debug.Log($"Too many args entered, required : {requiredParamLength}, entered : {responseStrings.Length}");
                        return;
                    }

                    for (int i = 0; i < responseStrings.Length; i++)
                        responsesMap.Add(inputQuery.allParams[i], responseStrings[i]);
                }

                CurrentInputResponse = new InputResponse(responsesMap, inputQuery);
                if (CurrentInputResponse.AllInputValid)
                    inputAction?.Invoke(CurrentInputResponse);
                else
                    Debug.LogWarning("Input not valid.");

                base.ResolveAction();
            });
        }

        public override bool CanDisplayAction()
        {
            return false;
        }

        public override string GetDescription()
        {
            return base.GetDescription() + paramsDescription;
        }

        public DebugActionInput WithInputAction(Action<InputResponse> inputAction)
        {
            this.inputAction = inputAction;
            return this;
        }

        public DebugActionInput WithInputQuery(InputQuery inputQuery)
        {
            this.inputQuery = inputQuery;
            paramsDescription = inputQuery.allParams.Aggregate("", (description, param) =>
            {
                var hasDefaultValue = param.defaultValue != null && !string.IsNullOrWhiteSpace(param.defaultValue.ToString());
                var hasDescription = !string.IsNullOrEmpty(param.description);
                return $"{description}\n<i><u>{param.name}{(hasDefaultValue ? "?" : null)}</u></i> : <color=#00FFFF>{param.type}</color>{(hasDefaultValue ? $" = <u>{param.defaultValue}</u>" : null)}{(hasDescription ? $" ({param.description})" : null)}";
            });
            if (!string.IsNullOrEmpty(paramsDescription))
            {
                paramsDescription = "<color=\"green\">Input Params</color>: " + paramsDescription + "\n";
            }
            return this;
        }

        public DebugActionInput WithInputSlider(float min = 0, float max = 1)
        {
            // Should throw an error
            if (inputQuery.allParams.Count != 1)
            {
                Debug.LogWarning("Slider can only handle single input query!");
                return this;
            }

            Param param = inputQuery.allParams.First();

            // Should throw an error if valuePrefillCallback is null
            if (param.valuePrefillCallback == null)
            {
                Debug.LogWarning("ValuePrefillCallback is null!");
                return this;
            }

            object v = param.valuePrefillCallback();

            // Should throw an error if its not support type
            if (!(v is float || v is int))
            {
                Debug.LogWarning("Slide does not support " + v.GetType().Name);
                return this;
            }

            var targetQuery = (float)v;

            this.onGetSliderValue = () =>
            {
                object v = param.valuePrefillCallback();
                return (float)v;
            };
            this.onSliderValueChanged = (v) =>
            {
                var responsesMap = new Dictionary<Param, string>();
                responsesMap.Add(inputQuery.allParams[0], v.ToString());
                CurrentInputResponse = new InputResponse(responsesMap, inputQuery);
                inputAction.Invoke(CurrentInputResponse);
            };
            this.onGetMinMax = () => new Vector2(min, max);
            return this;
        }
    }

}