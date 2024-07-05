using ImGuiNET;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using System.Numerics;
using System.Threading;
using SharpPluginLoader.Core.Actions;
using System;

namespace Teleport
{
    public class Teleport : IPlugin
    {
        public string Name => "Teleport";
        public string Author => "Seka";

        public Monster? _selectedMonsterT = null;
        private void OnQuestLeave(int questId) { _selectedMonsterT = null; }
        private void OnQuestComplete(int questId) { _selectedMonsterT = null; }
        private void OnQuestFail(int questId) { _selectedMonsterT = null; }
        private void OnQuestReturn(int questId) { _selectedMonsterT = null; }
        private void OnQuestAbandon(int questId) { _selectedMonsterT = null; }

        private Vector3 _currentPosition;
        private Vector3 _lastPosition;
        private Vector3 _mLastPosition;
        public float _movementAmount = 1f;
        private float _minMovementAmount = -1000.0f;
        private float _maxMovementAmount = 1000.0f;
        private bool _lockPosition = false;
        private bool _mLockPosition = false;
        private Vector3 _inputPosition = new Vector3(0f, 0f, 0f);
        private float _minInputPos = -5000000.000f;
        private float _maxInputPos = 5000000.000f;
        private bool _monsterSelectOn = true;

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        public unsafe void OnImGuiRender()
        {
            var player = Player.MainPlayer;
            if (player is null)
                return;
            
            if (ImGui.Button("Toggle noclip")) {
                if (!_lockPosition)
                { _lockPosition = true; player.PauseAnimations(); } 
                else 
                { _lockPosition = false; player.ResumeAnimations(); }
            }
            ImGui.InputFloat("Velocity", ref _movementAmount, 0.0f, 300.0f);
            _movementAmount = Clamp(_movementAmount, _minMovementAmount, _maxMovementAmount);

            if (ImGui.BeginCombo("Select", (_selectedMonsterT is not null ? $"{_selectedMonsterT.Type} @ 0x{_selectedMonsterT.Instance:X}" : "None"))) // Instance shows address of pointer
            {
                var monsters = Monster.GetAllMonsters().TakeLast(5).ToArray();
                foreach (var monster in monsters)
                {
                    if (ImGui.Selectable($"{monster.Type} @ 0x{monster.Instance:X}", _selectedMonsterT == monster))
                    {
                        _selectedMonsterT = monster;
                        _monsterSelectOn = true;
                    }
                }
                ImGui.EndCombo();
            }
            if (ImGui.Button("Monster Here"))
            {
                if (_selectedMonsterT == null || !_monsterSelectOn)
                    return;
                _selectedMonsterT.Teleport(player.Position);
                _mLastPosition = _selectedMonsterT.Position;
                if (!_mLockPosition)
                { _mLockPosition = true; }
                else
                { _mLockPosition = false; }
            }

            ImGui.InputFloat3("", ref _inputPosition);
            _inputPosition.X = Clamp(_inputPosition.X, _minInputPos, _maxInputPos);
            _inputPosition.Y = Clamp(_inputPosition.Y, _minInputPos, _maxInputPos);
            _inputPosition.Z = Clamp(_inputPosition.Z, _minInputPos, _maxInputPos);

            if (ImGui.Button("Player To"))
            {
                if (!_lockPosition)
                { _lockPosition = true; player.PauseAnimations(); }
                else
                { _lockPosition = false; player.ResumeAnimations(); }
                player.Teleport(_inputPosition);
            }
        }
        public void OnUpdate(float deltaTime)
        {
            var player = Player.MainPlayer;
            if (player is null)
                return;
            _currentPosition = player.Position;


            if (_lockPosition)
            {
                player.Teleport(_lastPosition);

                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.UpArrow))
                {
                    _currentPosition.Z -= _movementAmount;
                    player.Teleport(_currentPosition);
                   _lastPosition = _currentPosition;
                }
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.DownArrow))
                {
                    _currentPosition.Z += _movementAmount;
                    player.Teleport(_currentPosition);
                    _lastPosition = _currentPosition; 
                }
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.PageUp))
                {
                    _currentPosition.Y += _movementAmount;
                    player.Teleport(_currentPosition); 
                    _lastPosition = _currentPosition; 
                }
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.PageDown))
                {
                    _currentPosition.Y -= _movementAmount;
                    player.Teleport(_currentPosition); 
                    _lastPosition = _currentPosition; 
                }
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.LeftArrow))
                {
                    _currentPosition.X -= _movementAmount;
                    player.Teleport(_currentPosition); 
                    _lastPosition = _currentPosition; 
                }
                if (Input.IsDown(Key.LeftAlt) && Input.IsDown(Key.RightArrow))
                {
                    _currentPosition.X += _movementAmount;
                    player.Teleport(_currentPosition); 
                    _lastPosition = _currentPosition; 
                }
            }
            else
            {
                _lastPosition = player.Position;
            }

            if (_selectedMonsterT == null)
                return;

            if(_selectedMonsterT.Health == 0)
            {
                _monsterSelectOn = false;
            }

            if (_mLockPosition && _selectedMonsterT != null && _monsterSelectOn)
            {
                _selectedMonsterT.Teleport(_mLastPosition);
            }

        }
    }
}
