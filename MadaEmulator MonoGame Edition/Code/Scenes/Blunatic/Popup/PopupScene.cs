using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Blunatic.Core;
using Blunatic.Mgc;

namespace Blunatic.Scenes
{
    public class PopupScene : IScene
    {
        // Constants
        private const int EDGE_VOID = 8;

        // Enums
        public enum Type
        {
            Info,
            YesNo,
            Error,
            YesCancel,
            YesNoCancel,
        }
        public enum Response
        {
            OK,
            Yes,
            No,
            Cancel,
        }

        // Classes
        public class PopupEventArgs : EventArgs
        {
            public Response Response { get; init; }
        }

        // Properties
        public bool UpdatePreviousScene => false;
        public bool DrawPreviousScene => true;

        // Events
        public event EventHandler<PopupEventArgs> Acknowledged;

        // Fields
        private MonoGameConsole _mgc;
        private string _message;
        private string _title;
        private Color _titleColor;
        private Response _defaultResponse;
        private Dictionary<Response, MonoGameConsoleButton> _buttons;

        // Constructors
        public PopupScene(MonoGameInstance mgi, Type type, string message)
        {
            _message = message;
            _mgc = new MonoGameConsole(mgi, new Vec(70, 30));
            _mgc.Transparency = MonoGameConsole.TransparencyType.NotWrittenTo;

            _buttons = new Dictionary<Response, MonoGameConsoleButton>();

            void addButton(Response response, int pos)
            {
                _buttons.Add(response, new MonoGameConsoleButton(mgi, new Vec(pos, _mgc.Dimensions.Y - 1 - EDGE_VOID), response.ToString()));
                _buttons[response].Centre();
                _mgc.AddElement(_buttons[response]);
            }

            switch (type)
            {
                case Type.Error:
                    {
                        _defaultResponse = Response.OK;

                        _title = "Error";
                        _titleColor = Color.Red;

                        addButton(Response.OK, (_mgc.Dimensions.X - 1) / 2);
                    }
                    break;
                case Type.YesNo:
                    {
                        _defaultResponse = Response.OK;

                        _title = "Query";
                        _titleColor = Color.White;

                        addButton(Response.No, (_mgc.Dimensions.X - 1) / 2 - 4);
                        addButton(Response.Yes, (_mgc.Dimensions.X - 1) / 2 + 4);
                    }
                    break;
                case Type.YesCancel:
                    {
                        _defaultResponse = Response.OK;

                        _title = "Query";
                        _titleColor = Color.White;

                        addButton(Response.Cancel, (_mgc.Dimensions.X - 1) / 2 - 5);
                        addButton(Response.Yes, (_mgc.Dimensions.X - 1) / 2 + 6);
                    }
                    break;
                case Type.YesNoCancel:
                    {
                        _defaultResponse = Response.OK;

                        _title = "Query";
                        _titleColor = Color.White;

                        addButton(Response.Cancel, (_mgc.Dimensions.X - 1) / 2 - 8);
                        addButton(Response.No, (_mgc.Dimensions.X - 1) / 2 + 1);
                        addButton(Response.Yes, (_mgc.Dimensions.X - 1) / 2 + 8);
                    }
                    break;
                case Type.Info:
                    {
                        _defaultResponse = Response.OK;

                        _title = "";

                        addButton(Response.OK, (_mgc.Dimensions.X - 1) / 2);
                    }
                    break;
                default:
                    {
                        _defaultResponse = Response.OK;

                        _title = $"���";
                        _titleColor = Color.Magenta;

                        addButton(Response.OK, (_mgc.Dimensions.X - 1) / 2);
                    }
                    break;
            }
        }

        // Methods
        public void Update(MonoGameInstance mgi)
        {
            _mgc.Update(mgi);

            foreach (KeyValuePair<Response, MonoGameConsoleButton> kvp in _buttons)
            {
                if (kvp.Value.IsLeftClicked)
                {
                    Acknowledged?.Invoke(this, new PopupEventArgs() { Response = kvp.Key });
                    mgi.SceneOut();
                    return;
                }
            }

            if (mgi.ControlWasJustPressed("escape"))
            {
                Acknowledged?.Invoke(this, new PopupEventArgs() { Response = _defaultResponse });
                mgi.SceneOut();
                return;
            }
        }
        public void Draw(MonoGameInstance mgi)
        {
            _mgc.Box(mgi, new Rectangle(new Vec(EDGE_VOID), _mgc.Dimensions - new Vec(EDGE_VOID*2)), $"{Fm.Fg(_titleColor)}{_title}", Color.White, Color.Black, true);
            _mgc.WriteString(mgi, new Vec(2) + new Vec(EDGE_VOID), _message, _mgc.Dimensions.X - 4 - EDGE_VOID*2, MonoGameConsole.WrapType.WordWrap);
            _mgc.Draw(mgi);
        }
    }
}
