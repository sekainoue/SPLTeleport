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
        public string Name => "NoClip -> LSHIFT LALT T";  public string Author => "seka";
        public Monster? _selectedMonsterT = null;
        public List<Monster> Monsters { get; set; } = new List<Monster>();
        public Dictionary<Monster, Vector3> LockedCoordinates { get; } = new Dictionary<Monster, Vector3>();
        public void ResetState()
        { 
            _selectedMonsterT = null; 
            _frameCountdown = _framesForMessage;
            _statusMessage = "Reset all targets.";
        }
        public void OnMonsterDestroy(Monster monster)
        {
            _frameCountdown = _framesForMessage;
            _statusMessage = "Reset target.";
        }

        public void LockCoordinates(Monster monster)
        {
            LockedCoordinates[monster] = monster.Position;
        }

        public void LockCoordinates(Monster monster, Vector3 coords)
        {
            LockedCoordinates[monster] = coords;
        }

        public void UnlockCoordinates(Monster monster)
        {
            LockedCoordinates.Remove(monster, out _);
        }
        public void OnMonsterDeath(Monster monster)
        {

            LockedCoordinates.Remove(monster, out _);
            _frameCountdown = _framesForMessage;
            _statusMessage = "Reset target.";
        }

        private uint _lastStage = 0;
        public void OnMonsterCreate(Monster monster) 
        { 
            _lastStage = (uint)Area.CurrentStage; 
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
        public float _movementAmount = 1f;
        private float _minMovementAmount = -1000.0f;
        private float _maxMovementAmount = 1000.0f;
        private NativeFunction<nint, nint, bool> _seiz = new(0x140269c90); 
        private bool _lockPosition = false;
        private bool _mLockPosition = false;
        private Vector3 _inputPosition = new Vector3(0f, 0f, 0f);
        private float _minInputPos = -5000000.000f;
        private float _maxInputPos = 5000000.000f;
        private bool _cMode = false;
        public static float Clamp(float value, float min, float max)  {
            if (value < min) return min;
            if (value > max) return max;
            return value; }
        public unsafe void OnImGuiRender() {
            var player = Player.MainPlayer;
            if (player == null) 
                return;
            
            ImGui.InputFloat("Velocity", ref _movementAmount, 0.0f, 300.0f);
           
            _movementAmount = Clamp(_movementAmount, _minMovementAmount, _maxMovementAmount);
           
            var monsters = Monster.GetAllMonsters().TakeLast(8).ToArray();
            if (monsters == null) 
                return;
            if (ImGui.BeginCombo("Select", $"{_selectedMonsterT}"))  {
                foreach (var monster in monsters) {
                    if (ImGui.Selectable($"{monster}", _selectedMonsterT == monster))
                    {
                        _selectedMonsterT = monster;
                        LockCoordinates(monster);
                    }
                } ImGui.EndCombo();
            }

            if (ImGui.Button("Reset All"))
            {
                ResetState();
            }

            if (ImGui.Button("Mon-to-You")) {
                if (_selectedMonsterT == null || player == null) 
                    return;
                _selectedMonsterT.Teleport(player.Position);
                LockCoordinates(_selectedMonsterT, player.Position);
                _mLockPosition = true;
            }
            if (ImGui.Button("Lock/Unlock")) 
            { 
                if (_selectedMonsterT == null || player == null) 
                    return;
                _mLockPosition = !_mLockPosition;

                if (!_mLockPosition)
                {
                    UnlockCoordinates(_selectedMonsterT);
                }
            }

            if (ImGui.Button("You-to-Mon")) {
                if (_selectedMonsterT == null || player == null) 
                    return; 
                player.Teleport(_selectedMonsterT.Position);
                _lastPosition = player.Position;
            }

            ImGui.InputFloat3("←↑↓→/PgUp/PgDn", ref _inputPosition);

            _inputPosition.X = Clamp(_inputPosition.X, _minInputPos, _maxInputPos);
            _inputPosition.Y = Clamp(_inputPosition.Y, _minInputPos, _maxInputPos);
            _inputPosition.Z = Clamp(_inputPosition.Z, _minInputPos, _maxInputPos);

            var aC = player?.ActionController; 
            if (aC == null) 
                return; 
            if (player == null)
                return;

            uint stageID = (uint)Area.CurrentStage;

            if (ImGui.Button("ToCoord")) 
            {
                if ((stageID >= 100 && stageID <= 109) || (stageID >= 200 && stageID <= 202) || (stageID >= 403 && stageID <= 417) || stageID == 504)   
                {
                    _cMode = true; 
                    if (!_lockPosition) 
                    { 
                        _lockPosition = true; 
                        player.PauseAnimations(); 
                    } else { 
                        _lockPosition = false; 
                        var seiz = new ActionInfo(1, 0); 
                        _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); 
                        player.ResumeAnimations(); 
                    }
                } else if ((stageID >= 301 && stageID <= 306) || (stageID >= 501 && stageID <= 506)) 
                { 
                    _cMode = false;
                    if (!_lockPosition)  
                    { 
                        _lockPosition = true; 
                        var seiz = new ActionInfo(1, 149); 
                        _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); 
                        player.PauseAnimations(); 
                    }
                    else 
                    { 
                        _lockPosition = false; 
                        var seiz = new ActionInfo(1, 0); 
                        _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); 
                        player.ResumeAnimations(); 
                    } 
                } 
                player.Teleport(_inputPosition);
                _lastPosition = player.Position; 
            }

            ImGui.Text($"{player.Position}");

            if (_frameCountdown > 0) 
            {
                ImGui.Text(_statusMessage);
                _frameCountdown--;
            }
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
                        player.PauseAnimations();
                    } else { 
                        _lockPosition = false; 
                        var seiz = new ActionInfo(1, 0); 
                        _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz));
                        player.ResumeAnimations();
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
                        player.PauseAnimations();
                    } else { 
                        _lockPosition = false; 
                        var seiz = new ActionInfo(1, 0); 
                        _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); 
                        player.ResumeAnimations();
                    }
                }
            }
            
            if (player == null) 
                return;
            
            _currentPosition = player.Position;
            
            if (_lockPosition) {
                if (_cMode) 
                { 
                    var seiz = new ActionInfo(1, 593);
                    _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); 
                } 
                player.Teleport(_lastPosition); 
                if (Input.IsDown(Key.UpArrow)) { _currentPosition.Z -= _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.DownArrow)) { _currentPosition.Z += _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.PageUp)) { _currentPosition.Y += _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.PageDown)) {  _currentPosition.Y -= _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.LeftArrow)) {  _currentPosition.X -= _movementAmount; player.Teleport(_currentPosition);  _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.RightArrow)) { _currentPosition.X += _movementAmount; player.Teleport(_currentPosition);  _lastPosition = _currentPosition; }
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