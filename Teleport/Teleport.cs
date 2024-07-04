using ImGuiNET;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using System.Numerics;

namespace Teleport
{
    public class Teleport : IPlugin
    {
        public string Name => "Teleport";
        public string Author => "Seka";

        private Monster? _selectedMonster = null;
        public void OnQuestLeave(int questId) { _selectedMonster = null; }
        public void OnQuestComplete(int questId) { _selectedMonster = null; }
        public void OnQuestFail(int questId) { _selectedMonster = null; }
        public void OnQuestReturn(int questId) { _selectedMonster = null; }
        public void OnQuestAbandon(int questId) { _selectedMonster = null; }

        private Vector3 _playerPosition = new Vector3(0f, 0f, 0f);
        public float _movementAmount = 10.0f;
        private bool _lockPosition = false;
       
        public unsafe void OnImGuiRender()
        {
            var player = Player.MainPlayer;
            if (player is null)
                return;

            if (ImGui.Button("noclip Toggle")) {
                if (!_lockPosition)
                { _lockPosition = true; } 
                else 
                { _lockPosition = false; }
            }

            if (ImGui.BeginCombo("Teleport to Monster:", (_selectedMonster is not null ? $"{_selectedMonster.Type} @ 0x{_selectedMonster.Instance:X}" : "None"))) // Instance shows address of pointer
            {
                var monsters = Monster.GetAllMonsters().TakeLast(5).ToArray();
                foreach (var monster in monsters)
                {
                    if (ImGui.Selectable($"{monster.Type} @ 0x{monster.Instance:X}", _selectedMonster == monster))
                    {
                        _selectedMonster = monster;
                    }
                }
                ImGui.EndCombo();
            }
            if (ImGui.Button("Go"))
            {
                if (_selectedMonster == null)
                    return;
                player.Teleport(_selectedMonster.Position);
            }
        }

        public void OnUpdate (float deltatime)
        {
            var player = Player.MainPlayer;
            if (player is null)
                return;
            _playerPosition = player.Position;
            if (_lockPosition) {
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.UpArrow)) { _playerPosition.Z -= _movementAmount; player.Teleport(_playerPosition); }
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.DownArrow)) { _playerPosition.Z += _movementAmount; player.Teleport(_playerPosition); }
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.PageUp)) { _playerPosition.Y += _movementAmount; player.Teleport(_playerPosition); }
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.PageDown)) { _playerPosition.Y -= _movementAmount; player.Teleport(_playerPosition); }
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.LeftArrow)) { _playerPosition.X -= _movementAmount; player.Teleport(_playerPosition); }
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.RightArrow)) { _playerPosition.X += _movementAmount; player.Teleport(_playerPosition); }
            }
        }
    }
}
