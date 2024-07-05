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
        public void OnMonsterDestroy(Monster monster) {
            if (monster == _selectedMonsterT) { _selectedMonsterT = null; }
        }
        public void OnMonsterDeath(Monster monster) {
            if (monster == _selectedMonsterT) { _selectedMonsterT = null; }
        }


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

            var monsters = Monster.GetAllMonsters().TakeLast(5).ToArray();
            if (monsters == null)
                return;

            if (ImGui.BeginCombo("Select", $"{_selectedMonsterT}"))
            {
                foreach (var monster in monsters)
                {
                    if (ImGui.Selectable($"{monster}", _selectedMonsterT == monster))
                    {
                        _selectedMonsterT = monster;
                    }
                }
                ImGui.EndCombo();
            }
            if (ImGui.Button("Mon-to-You"))
            {
                if (_selectedMonsterT == null)
                    return;
                _selectedMonsterT.Teleport(player.Position);
                _mLastPosition = _selectedMonsterT.Position;
                if (!_mLockPosition)
                { _mLockPosition = true; }
                else
                { _mLockPosition = false; }
            }
            if (ImGui.Button("You-to-Mon"))
            {
                if (_selectedMonsterT == null)
                    return;
                player.Teleport(_selectedMonsterT.Position);
                _lastPosition = player.Position;
            }

            ImGui.InputFloat3("", ref _inputPosition);
            _inputPosition.X = Clamp(_inputPosition.X, _minInputPos, _maxInputPos);
            _inputPosition.Y = Clamp(_inputPosition.Y, _minInputPos, _maxInputPos);
            _inputPosition.Z = Clamp(_inputPosition.Z, _minInputPos, _maxInputPos);

            if (ImGui.Button("ToCoord"))
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

            if (_mLockPosition && _selectedMonsterT != null)  { _selectedMonsterT.Teleport(_mLastPosition); }

        }
    }
}
