using Blunatic.Core;
using Blunatic.Mgc;
using Blunatic.Scenes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blunatic.Scenes.FileExplorerScene;

namespace MadaEmulator_MonoGame_Edition
{
    public class ValueChangeScene : IScene
    {
        // Classes
        public class ValueChangeResultEventArgs : EventArgs
        {
            public MonoGameInstance MonoGameInstance;
            public string Input { get; init; }

            private RejectionStatus _rejectionStatusObject;

            public ValueChangeResultEventArgs(MonoGameInstance mgi, RejectionStatus rejectionStatusObject)
            {
                MonoGameInstance = mgi;
                _rejectionStatusObject = rejectionStatusObject;
            }

            public void Reject(string message)
            {
                _rejectionStatusObject.HasBeenRejected = true;
                _rejectionStatusObject.RejectionMessage = message;
            }
        }

        // Properties
        public bool UpdatePreviousScene => false;
        public bool DrawPreviousScene => true;

        // Fields
        private MonoGameConsole _mgc;

        private MonoGameConsoleTextInput _textInput;

        private string _title;

        // Events
        public event EventHandler<ValueChangeResultEventArgs> ResultSelected;

        // Constructors
        public ValueChangeScene(MonoGameInstance mgi, string title, int maxLength, string startingText, string allowedCharacters)
        {
            _mgc = new MonoGameConsole(mgi, new Vec(maxLength + 10, 3));
            _mgc.Transparency = MonoGameConsole.TransparencyType.NotWrittenTo;

            _textInput = new MonoGameConsoleTextInput(new Vec(5, 1), maxLength, startingText, allowedCharacters);
            _mgc.AddElement(_textInput);
            _textInput.TextUpdated += _respondToTextUpdate;

            _title = title;
        }

        // Methods
        private void _respondToTextUpdate(object sender, MonoGameConsoleTextInput.TextUpdatedEventArgs args)
        {
            args.MonoGameInstance.SceneOut();

            RejectionStatus rejectionStatusObject = new RejectionStatus();
            ResultSelected?.Invoke(this, new ValueChangeResultEventArgs(args.MonoGameInstance, rejectionStatusObject) { Input = args.ChangedTo });

            if (!rejectionStatusObject.HasBeenRejected)
            {
                return;
            }

            args.MonoGameInstance.SceneIn(this);
            args.MonoGameInstance.SceneIn(new PopupScene(args.MonoGameInstance, PopupScene.Type.Error, rejectionStatusObject.RejectionMessage));

            return;
        }

        public void Update(MonoGameInstance mgi)
        {
            if (mgi.ControlWasJustPressed("escape"))
            {
                mgi.SceneOut();
                return;
            }

            _mgc.Update(mgi);
        }
        public void Draw(MonoGameInstance mgi)
        {
            _mgc.Box(mgi, new Rectangle(_textInput.Position - 1, _textInput.Dimensions + 2), _title);
            _mgc.Draw(mgi);
        }
    }
}
