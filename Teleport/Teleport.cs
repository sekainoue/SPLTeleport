﻿using ImGuiNET;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using System.Numerics;
using SharpPluginLoader.Core.Actions;
using SharpPluginLoader.Core.Memory;
using System.Collections.Concurrent;

namespace Teleport
{
    public class Teleport : IPlugin
    {
        public string Name => "-noclip";  public string Author => "seka";
        public Monster? _selectedMonsterT = null;
        public List<Monster> Monsters { get; set; } = new List<Monster>();
        public Dictionary<Monster, Vector3> LockedCoordinates { get; } = new Dictionary<Monster, Vector3>();
        public void ResetState()
        { 
            _selectedMonsterT = null; 
            _frameCountdown = _framesForMessage;
            _statusMessage = "All targets reset.";
        }
        public void OnMonsterDestroy(Monster monster)
        {
            _frameCountdown = _framesForMessage;
            _statusMessage = $"{monster} destroyed.";
        }

        public void LockCoordinates(Monster monster)
        {
            LockedCoordinates[monster] = monster.Position;
            _frameCountdown = _framesForMessage;
            _statusMessage = $"Locked {monster} current coordinates.";
        }

        public void LockCoordinates(Monster monster, Vector3 coords)
        {
            LockedCoordinates[monster] = coords;
            _frameCountdown = _framesForMessage;
            _statusMessage = $"Locked {monster} new coordinates.";
        }

        public void UnlockCoordinates(Monster monster)
        {
            LockedCoordinates.Remove(monster, out _);
            _frameCountdown = _framesForMessage;
            _statusMessage = $"Unlocked {monster} coordinates.";
        }
        public void OnMonsterDeath(Monster monster)
        {

            LockedCoordinates.Remove(monster, out _);
            _frameCountdown = _framesForMessage;
            _statusMessage = $"Reset {monster}.";
        }

        private uint _lastStage = 0;
        public void OnMonsterCreate(Monster monster) 
        { 
            var presentMonsters = Monster.GetAllMonsters().TakeLast(8).ToArray();
            Monsters.Clear();
            Monsters.AddRange(presentMonsters);

            foreach (Monster monsterToList in presentMonsters)
            {
                if (!LockedCoordinates.ContainsKey(monsterToList))
                {
                    LockedCoordinates[monsterToList] = monsterToList.Position;
                }
            }
        }
        public void OnQuestLeave(int questId) => ResetState();
        public void OnQuestComplete(int questId) => ResetState();
        public void OnQuestFail(int questId) => ResetState();
        public void OnQuestReturn(int questId) => ResetState();
        public void OnQuestAbandon(int questId) => ResetState();
        public void OnQuestEnter(int questId) => ResetState();

        private int _frameCountdown = 0;
        private const int _framesForMessage = 180;
        private string _statusMessage = "";
        private Vector3 _currentPosition;
        private Vector3 _lastPosition;
        public float _movementAmount = 10f;
        private float _minMovementAmount = -1000.0f;
        private float _maxMovementAmount = 1000.0f;
        private NativeFunction<nint, nint, bool> _seiz = new(0x140269c90); 
        private bool _lockPosition = false;
        private bool _mLockPosition = false;
        private Vector3 _inputPosition = new Vector3(0f, 0f, 0f);
        private float _minInputPos = -5000000.000f;
        private float _maxInputPos = 5000000.000f;
        private bool _cMode = false;
        private float _playerRotationY;
        private float _minRotation = -1.8f;
        private float _maxRotation = 1.8f;
        public static float Clamp(float value, float min, float max)  {
            if (value < min) return min;
            if (value > max) return max;
            return value; }
        public unsafe void OnImGuiRender() {
            var player = Player.MainPlayer;
            if (player == null) 
                return;

            if (_frameCountdown > 0)
            {
                ImGui.Text(_statusMessage);
                _frameCountdown--;
            }
            else { ImGui.Text("Keys: Lshift Lalt T ←↑↓→ PgUp/PgDn QE"); }

            ImGui.PushItemWidth(100.0f);
            ImGui.InputFloat("Speed", ref _movementAmount, 0.0f, 300.0f);
            ImGui.PopItemWidth();

            _movementAmount = Clamp(_movementAmount, _minMovementAmount, _maxMovementAmount);

            var monsters = Monster.GetAllMonsters().TakeLast(8).ToArray();
            if (monsters == null) 
                return;

            ImGui.SameLine();
            if (ImGui.Button("Unset"))
            {
                _selectedMonsterT = null;
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(250.0f);
            if (ImGui.BeginCombo("Select", $"{_selectedMonsterT}"))  {
                foreach (var monster in monsters) {
                    if (ImGui.Selectable($"{monster}", _selectedMonsterT == monster))
                    {
                        _selectedMonsterT = monster;
                        LockCoordinates(monster);
                        _lastStage = (uint)Area.CurrentStage;
                    }
                } ImGui.EndCombo();
            }
            ImGui.PopItemWidth();

            if (ImGui.Button("Reset All"))
            {
                ResetState();
            }
            ImGui.SameLine();
            if (ImGui.Button("Mon-to-You")) {
                if (_selectedMonsterT == null || player == null) 
                    return;
                _selectedMonsterT.Teleport(player.Position);
                LockCoordinates(_selectedMonsterT, player.Position);
                _mLockPosition = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Lock Mon")) 
            { 
                if (_selectedMonsterT == null || player == null) 
                    return;
                _mLockPosition = !_mLockPosition;

                if (!_mLockPosition)
                {
                    UnlockCoordinates(_selectedMonsterT);
                }
                else
                {
                    LockCoordinates(_selectedMonsterT);
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("You-to-Mon")) {
                if (_selectedMonsterT == null || player == null) 
                    return; 
                player.Teleport(_selectedMonsterT.Position);
                _lastPosition = player.Position;
                _selectedMonsterT = null;
            }

            ImGui.InputFloat3("", ref _inputPosition);

            _inputPosition.X = Clamp(_inputPosition.X, _minInputPos, _maxInputPos);
            _inputPosition.Y = Clamp(_inputPosition.Y, _minInputPos, _maxInputPos);
            _inputPosition.Z = Clamp(_inputPosition.Z, _minInputPos, _maxInputPos);

            var aC = player?.ActionController; 
            if (aC == null) 
                return; 
            if (player == null)
                return;

            uint stageID = (uint)Area.CurrentStage;
            ImGui.SameLine();
            if (ImGui.Button("You-To-Coord")) 
            {
                if ((stageID >= 100 && stageID <= 109) || (stageID >= 200 && stageID <= 202) || (stageID >= 403 && stageID <= 417) || stageID == 504)   
                {
                    _cMode = true;
                    _lockPosition = true;
                    var seiz = new ActionInfo(1, 0);
                    _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz));
                } else if ((stageID >= 301 && stageID <= 306) || (stageID >= 501 && stageID <= 506)) 
                { 
                    _cMode = false;
                    _lockPosition = true;
                    var seiz = new ActionInfo(1, 149);
                    _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz));
                } 
                player.Teleport(_inputPosition);
                _lastPosition = player.Position; 
            }

            ImGui.Text($"{player.Position} {player.Rotation.Y * 180}°");

        }


        public void OnLoad() { KeyBindings.AddKeybind("TeleLock", new Keybind<Key>(Key.T, [Key.LeftShift, Key.LeftAlt])); }
        public unsafe void OnUpdate(float deltaTime)  {
            var player = Player.MainPlayer; 
            if (player is null) 
                return;
            if ((uint)Area.CurrentStage != _lastStage)
            { 
                ResetState(); 
            }

            var aC = player?.ActionController; 
            if (aC == null) 
                return;

            uint stageID = (uint)Area.CurrentStage; 
            if (player == null) 
                return;
            if ((stageID >= 100 && stageID <= 109) || (stageID >= 200 && stageID <= 202) || (stageID >= 403 && stageID <= 417) || stageID == 504)  
            { 
                _cMode = true;
                if (KeyBindings.IsPressed("TeleLock")) 
                { 
                    if (!_lockPosition) 
                    { 
                        _lockPosition = true; 
                        //player.PauseAnimations();
                    } else { 
                        _lockPosition = false; 
                        var seiz = new ActionInfo(1, 0); 
                        _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz));
                        //player.ResumeAnimations();
                    }
                }
            } else if ((stageID >= 301 && stageID <= 306) || (stageID >= 501 && stageID <= 506))  
            { 
                _cMode = false;
                if (KeyBindings.IsPressed("TeleLock")) 
                {
                    if (!_lockPosition)  
                    { 
                        _lockPosition = true; 
                        var seiz = new ActionInfo(1, 149); 
                        _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); 
                        //player.PauseAnimations();
                    } else { 
                        _lockPosition = false; 
                        var seiz = new ActionInfo(1, 0); 
                        _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); 
                        //player.ResumeAnimations();
                    }
                }
            }
            
            if (player == null) 
                return;
            
            _currentPosition = player.Position;
            _playerRotationY = player.Rotation.Y; // remember this is different from player.Rotation.Y = _playerRotationY; 
            _playerRotationY = Clamp(_playerRotationY, _minRotation, _maxRotation);

            Vector3 upVector = new Vector3(0f, 1f, 0f); //normalized manually as 0,1,0
            var normalizedSide = Vector3.Normalize(Vector3.Cross(player.Forward, upVector));  // normalize the cross product
            if (normalizedSide == Vector3.Zero) return;
            

            if (_lockPosition) {
                if (_cMode) 
                { 
                    var seiz = new ActionInfo(1, 593);
                    _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); 
                } 
                player.Teleport(_lastPosition); 

                if (Input.IsDown(Key.Q)) { _playerRotationY += 0.02f; player.Rotation.Y = _playerRotationY;  }
                if (Input.IsDown(Key.E)) { _playerRotationY -= 0.02f; player.Rotation.Y = _playerRotationY;  }
                if (Input.IsDown(Key.UpArrow)) { _currentPosition += player.Forward * _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.DownArrow)) { _currentPosition -= player.Forward * _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.PageUp)) { _currentPosition.Y += _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.PageDown)) {  _currentPosition.Y -= _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.LeftArrow)) { _currentPosition -= normalizedSide * _movementAmount; player.Teleport(_currentPosition);  _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.RightArrow)) { _currentPosition += normalizedSide * _movementAmount; player.Teleport(_currentPosition);  _lastPosition = _currentPosition; }
            } else {
                _lastPosition = player.Position; 
            }

            if (_mLockPosition && _selectedMonsterT != null)
            {
                foreach (var monster in LockedCoordinates)
                {
                    monster.Key.Teleport(monster.Value);
                }
            }
        }
    }
}