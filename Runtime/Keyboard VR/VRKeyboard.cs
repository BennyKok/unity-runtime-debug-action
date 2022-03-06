using System;
using BennyKok.RuntimeDebug.Actions;
using BennyKok.RuntimeDebug.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace BennyKok.RuntimeDebug.VR
{
    public class VRKeyboard : MonoBehaviour
    {
        private bool isCaps;

        public bool IsCaps
        {
            get => isCaps;
            set
            {
                isCaps = value;

                foreach (var key in allKeys)
                {
                    if (!key.keyboard)
                    {
                        key.keyboard = this;

                        // Apply custom font
                        key.Label.font = RuntimeDebugSystem.UIHandler.customFont;
                    }

                    if ((int) key.keyCode >= (int) KeyCode.A && (int) key.keyCode <= (int) KeyCode.Z)
                        key.Label.text = isCaps ? key.Label.text.ToUpper() : key.Label.text.ToLower();
                }
            }
        }

        private void Start()
        {
            RuntimeDebugSystem.Instance.OnDebugMenuToggleEvent += OnMenuToggle;

            if (showVRKeyboardDebugOption)
                RuntimeDebugSystem.RegisterActions("VR Keyboard",
                    autoShowHideFlag,
                    DebugActionBuilder.Toggle()
                        .WithName("Enable")
                        .WithActionGet(() => gameObject.activeSelf)
                        .WithActionSet((isOn) => gameObject.SetActive(isOn))
                );

            gameObject.SetActive(false);

            IsCaps = false;
        }

        public void ShowSecondaryLayout()
        {
            primaryLayoutTransform.gameObject.SetActive(false);
            secondaryLayoutTransform.gameObject.SetActive(true);
        }

        public void HideSecondaryLayout()
        {
            primaryLayoutTransform.gameObject.SetActive(true);
            secondaryLayoutTransform.gameObject.SetActive(false);
        }

        public bool refocusInputFieldOnKeyPress = true;

        public bool showVRKeyboardDebugOption = true;

        public Transform primaryLayoutTransform;
        public Transform secondaryLayoutTransform;

        [NonSerialized] public VRKey[] allKeys;

        private DebugActionFlag autoShowHideFlag;

        private void Awake()
        {
            allKeys = GetComponentsInChildren<VRKey>(true);

            autoShowHideFlag = DebugActionBuilder.Flag()
                .WithName("Auto Show/Hide")
                .WithFlag("vr-keyboard-auto-show", true, true);
        }

        private void OnMenuToggle(bool show)
        {
            if (autoShowHideFlag)
            {
                gameObject.SetActive(show);
            }
        }

        private void OnDestroy()
        {
            RuntimeDebugSystem.Instance.OnDebugMenuToggleEvent -= OnMenuToggle;
            RuntimeDebugSystem.UnregisterActionsByGroup("VR Keyboard");
        }

        public static readonly KeyCode[][] keyboardLayout = new KeyCode[][]
        {
            new KeyCode[]
            {
                KeyCode.Alpha1,
                KeyCode.Alpha2,
                KeyCode.Alpha3,
                KeyCode.Alpha4,
                KeyCode.Alpha5,
                KeyCode.Alpha6,
                KeyCode.Alpha7,
                KeyCode.Alpha8,
                KeyCode.Alpha9,
                KeyCode.Alpha0,
                KeyCode.Backspace,
            },
            new KeyCode[]
            {
                KeyCode.Q,
                KeyCode.W,
                KeyCode.E,
                KeyCode.R,
                KeyCode.T,
                KeyCode.Y,
                KeyCode.U,
                KeyCode.I,
                KeyCode.O,
                KeyCode.P,
            },
            new KeyCode[]
            {
                KeyCode.CapsLock,
                KeyCode.A,
                KeyCode.S,
                KeyCode.D,
                KeyCode.F,
                KeyCode.G,
                KeyCode.H,
                KeyCode.J,
                KeyCode.K,
                KeyCode.L,
                KeyCode.Return,
            },
            new KeyCode[]
            {
                KeyCode.Z,
                KeyCode.X,
                KeyCode.C,
                KeyCode.V,
                KeyCode.B,
                KeyCode.N,
                KeyCode.M,
                KeyCode.Comma,
                KeyCode.Period,
            },
            new KeyCode[]
            {
                KeyCode.Space,
            }
        };

        public static readonly KeyCode[][] secondaryKeyboardLayout = new KeyCode[][]
        {
            new KeyCode[]
            {
                KeyCode.Exclaim,
                KeyCode.At,
                KeyCode.Hash,
                KeyCode.Dollar,
                KeyCode.Percent,
                KeyCode.Caret,
                KeyCode.Ampersand,
                KeyCode.Asterisk,
                KeyCode.LeftParen,
                KeyCode.RightParen,
                KeyCode.Backspace,
            },
            new KeyCode[]
            {
                KeyCode.Minus,
                KeyCode.Plus,
                KeyCode.Equals,
                KeyCode.Underscore,
                KeyCode.LeftBracket,
                KeyCode.RightBracket,
                KeyCode.LeftCurlyBracket,
                KeyCode.RightCurlyBracket,
                KeyCode.Quote,
                KeyCode.DoubleQuote,
            },
            new KeyCode[]
            {
                KeyCode.Tilde,
                KeyCode.Question,
                KeyCode.Slash,
                KeyCode.Backslash,
                KeyCode.Colon,
                KeyCode.Semicolon,
                KeyCode.Less,
                KeyCode.Greater,
                KeyCode.BackQuote,
                KeyCode.Pipe,
            },
        };

        public static char GetKeyCodeForInput(KeyCode keyCode, bool isCaps)
        {
            string keyCodeString = keyCode.ToString().ToLower();
            char character;

            switch (keyCode)
            {
                //Spaces
                case KeyCode.Space:
                    character = ' ';
                    break;
                case KeyCode.Comma:
                    character = ',';
                    break;
                case KeyCode.Period:
                    character = '.';
                    break;
                case KeyCode.Exclaim:
                    character = '!';
                    break;
                case KeyCode.At:
                    character = '@';
                    break;
                case KeyCode.Hash:
                    character = '#';
                    break;
                case KeyCode.Dollar:
                    character = '$';
                    break;
                case KeyCode.Percent:
                    character = '%';
                    break;
                case KeyCode.Caret:
                    character = '^';
                    break;
                case KeyCode.Ampersand:
                    character = '&';
                    break;
                case KeyCode.Asterisk:
                    character = '*';
                    break;
                case KeyCode.LeftParen:
                    character = '(';
                    break;
                case KeyCode.RightParen:
                    character = ')';
                    break;
                case KeyCode.Minus:
                    character = '-';
                    break;
                case KeyCode.Plus:
                    character = '+';
                    break;
                case KeyCode.Equals:
                    character = '=';
                    break;
                case KeyCode.Underscore:
                    character = '_';
                    break;

                case KeyCode.LeftBracket:
                    character = '[';
                    break;
                case KeyCode.RightBracket:
                    character = ']';
                    break;
                case KeyCode.LeftCurlyBracket:
                    character = '{';
                    break;
                case KeyCode.RightCurlyBracket:
                    character = '}';
                    break;
                case KeyCode.Quote:
                    character = '\'';
                    break;
                case KeyCode.DoubleQuote:
                    character = '\"';
                    break;
                case KeyCode.Tilde:
                    character = '~';
                    break;
                case KeyCode.Question:
                    character = '?';
                    break;

                case KeyCode.Slash:
                    character = '/';
                    break;
                case KeyCode.Backslash:
                    character = '\\';
                    break;
                case KeyCode.Colon:
                    character = ':';
                    break;
                case KeyCode.Semicolon:
                    character = ';';
                    break;
                case KeyCode.Less:
                    character = '<';
                    break;
                case KeyCode.Greater:
                    character = '>';
                    break;
                case KeyCode.BackQuote:
                    character = '`';
                    break;
                case KeyCode.Pipe:
                    character = '|';
                    break;

                case var n when (n >= KeyCode.Alpha0 && n <= KeyCode.Alpha9):
                    character = keyCodeString.Substring(5)[0];
                    break;
                default:
                    character = isCaps ? char.ToUpper(keyCodeString[0]) : keyCodeString[0];
                    break;
            }

            return character;
        }

        [ContextMenu("Fill VRKey")]
        public void FillChildren()
        {
            FillKey(keyboardLayout, primaryLayoutTransform);
            FillKey(secondaryKeyboardLayout, secondaryLayoutTransform);
        }

        private void FillKey(KeyCode[][] keyboardLayout, Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (keyboardLayout.Length > i)
                {
                    var row = keyboardLayout[i];
                    int rowOffset = 0;

                    for (int j = 0; j < child.childCount; j++)
                    {
                        var vrKey = child.GetChild(j).GetComponent<VRKey>();
                        if (vrKey.ignoreKey)
                        {
                            rowOffset++;
                            continue;
                        }

                        if (row.Length > (j - rowOffset))
                        {
                            TMPro.TextMeshProUGUI textMeshProUGUI =
                                vrKey.GetComponentInChildren<TMPro.TextMeshProUGUI>();

#if UNITY_EDITOR
                            UnityEditor.Undo.RecordObject(vrKey, "Fill VRKey");
                            UnityEditor.Undo.RecordObject(vrKey.gameObject, "Fill VRKey");
                            if (textMeshProUGUI)
                                UnityEditor.Undo.RecordObject(textMeshProUGUI, "Fill VRKey");
#endif

                            vrKey.keyCode = row[(j - rowOffset)];
                            string v = vrKey.keyCode.ToString();

                            if (vrKey.keyCode == KeyCode.CapsLock) v = "caps";
                            else if (vrKey.keyCode == KeyCode.Return) v = "->";
                            else if (vrKey.keyCode == KeyCode.Backspace) v = "<-";
                            else v = GetKeyCodeForInput(vrKey.keyCode, isCaps).ToString();

                            vrKey.gameObject.name = vrKey.keyCode.ToString();
                            ;
                            if (textMeshProUGUI) textMeshProUGUI.text = v;
                        }
                    }
                }
            }
        }
    }
}