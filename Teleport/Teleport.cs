﻿using ImGuiNET;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using System.Numerics;
using System.Threading;
using SharpPluginLoader.Core.Actions;
using System;
using SharpPluginLoader.Core.Memory;
using System.Xml.Schema;
using System.Drawing;

namespace Teleport
{
    public class Teleport : IPlugin
    {
        public string Name => "NoClip [Shift Alt T]";  public string Author => "Seka";
        public Monster? _selectedMonsterT = null;
        public Monster? _selectedMonster1 = null; public Monster? _selectedMonster2 = null; public Monster? _selectedMonster3 = null; public Monster? _selectedMonster4 = null;
        public Monster? _selectedMonster5 = null; public Monster? _selectedMonster6 = null; public Monster? _selectedMonster7 = null; public Monster? _selectedMonster8 = null;
        public void ResetState()
        {
            _selectedMonster1 = null; _selectedMonster2 = null; _selectedMonster3 = null; _selectedMonster4 = null;
            _selectedMonster5 = null; _selectedMonster6 = null; _selectedMonster7 = null; _selectedMonster8 = null;
            _selectedMonsterT = null; 
            _eenieMeenie = 0; 
            _frameCountdown = _framesForMessage;
            _statusMessage = "Reset all targets.";
        }
        public void OnMonsterDestroy(Monster monster)
        {
            if (monster == _selectedMonsterT) { _selectedMonsterT = null; }
            if (monster == _selectedMonster1) { _selectedMonster1 = null; }
            if (monster == _selectedMonster2) { _selectedMonster2 = null; }
            if (monster == _selectedMonster3) { _selectedMonster3 = null; }
            if (monster == _selectedMonster4) { _selectedMonster4 = null; }
            if (monster == _selectedMonster5) { _selectedMonster5 = null; }
            if (monster == _selectedMonster6) { _selectedMonster6 = null; }
            if (monster == _selectedMonster7) { _selectedMonster7 = null; }
            if (monster == _selectedMonster8) { _selectedMonster8 = null; }
            _eenieMeenie = 0;
            _frameCountdown = _framesForMessage;
            _statusMessage = "Reset target.";
        }
        public void OnMonsterDeath(Monster monster)
        {
            if (monster == _selectedMonsterT) { _selectedMonsterT = null; }
            if (monster == _selectedMonster1) { _selectedMonster1 = null; }
            if (monster == _selectedMonster2) { _selectedMonster2 = null; }
            if (monster == _selectedMonster3) { _selectedMonster3 = null; }
            if (monster == _selectedMonster4) { _selectedMonster4 = null; }
            if (monster == _selectedMonster5) { _selectedMonster5 = null; }
            if (monster == _selectedMonster6) { _selectedMonster6 = null; }
            if (monster == _selectedMonster7) { _selectedMonster7 = null; }
            if (monster == _selectedMonster8) { _selectedMonster8 = null; }
            _eenieMeenie = 0;
            _frameCountdown = _framesForMessage;
            _statusMessage = "Reset target.";
        }

        private uint _lastStage = 0;
        public void OnMonsterCreate(Monster monster) { _lastStage = (uint)Area.CurrentStage; }
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
        private Vector3 _mLastPosition1; private Vector3 _mLastPosition2; private Vector3 _mLastPosition3; private Vector3 _mLastPosition4; private Vector3 _mLastPosition5;
        private Vector3 _mLastPosition6; private Vector3 _mLastPosition7; private Vector3 _mLastPosition8;
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
        private int _eenieMeenie = 0;
        public static float Clamp(float value, float min, float max)  {
            if (value < min) return min;
            if (value > max) return max;
            return value; }
        public unsafe void OnImGuiRender() {
            var player = Player.MainPlayer;
            if (player == null) return;
            ImGui.InputFloat("Velocity", ref _movementAmount, 0.0f, 300.0f);
            _movementAmount = Clamp(_movementAmount, _minMovementAmount, _maxMovementAmount);
            var monsters = Monster.GetAllMonsters().TakeLast(8).ToArray();
            if (monsters == null) return;
            if (ImGui.BeginCombo("Select", $"{_selectedMonsterT}"))  {
                foreach (var monster in monsters) {
                    if (ImGui.Selectable($"{monster}", _selectedMonsterT == monster))
                    {
                        _selectedMonsterT = monster;
                        var monsterMap = new Dictionary<int, Action<Monster>> {
                        { 0, m => _selectedMonster1 = m },
                        { 1, m => _selectedMonster2 = m },
                        { 2, m => _selectedMonster3 = m },
                        { 3, m => _selectedMonster4 = m },
                        { 4, m => _selectedMonster5 = m },
                        { 5, m => _selectedMonster6 = m },
                        { 6, m => _selectedMonster7 = m },
                        { 7, m => _selectedMonster8 = m }
                        };

                        if (monsterMap.ContainsKey(_eenieMeenie))
                        {
                            monsterMap[_eenieMeenie](monster);  // m is the monster parameter here
                        }
                        _eenieMeenie = (_eenieMeenie + 1) % 8;
                    }
                } ImGui.EndCombo();
            }
            if (ImGui.Button("Mon-to-You")) {
                if (_selectedMonsterT == null || player == null) return;
                if (_selectedMonster1 != null && _eenieMeenie == 1) { _selectedMonster1.Teleport(player.Position); _mLastPosition1 = _selectedMonster1.Position; }
                if (_selectedMonster2 != null && _eenieMeenie == 2) { _selectedMonster2.Teleport(player.Position); _mLastPosition2 = _selectedMonster2.Position; }
                if (_selectedMonster3 != null && _eenieMeenie == 3) { _selectedMonster3.Teleport(player.Position); _mLastPosition3 = _selectedMonster3.Position; }
                if (_selectedMonster4 != null && _eenieMeenie == 4) { _selectedMonster4.Teleport(player.Position); _mLastPosition4 = _selectedMonster4.Position; }
                if (_selectedMonster5 != null && _eenieMeenie == 5) { _selectedMonster5.Teleport(player.Position); _mLastPosition5 = _selectedMonster5.Position; }
                if (_selectedMonster6 != null && _eenieMeenie == 6) { _selectedMonster6.Teleport(player.Position); _mLastPosition6 = _selectedMonster6.Position; }
                if (_selectedMonster7 != null && _eenieMeenie == 7) { _selectedMonster7.Teleport(player.Position); _mLastPosition7 = _selectedMonster7.Position; }
                if (_selectedMonster8 != null && _eenieMeenie == 0) { _selectedMonster8.Teleport(player.Position); _mLastPosition8 = _selectedMonster8.Position; }
                _mLockPosition = true;
            }
            if (ImGui.Button("Lock/Unlock")) 
            { 
                if (_selectedMonsterT == null || player == null) return; 
                _mLockPosition = !_mLockPosition; 
            }
            if (ImGui.Button("You-to-Mon")) {
                if (_selectedMonsterT == null || player == null) return; 
                player.Teleport(_selectedMonsterT.Position); _lastPosition = player.Position;
            }
            ImGui.InputFloat3("↑↓←→/PgUp/PgDn", ref _inputPosition);
            _inputPosition.X = Clamp(_inputPosition.X, _minInputPos, _maxInputPos);
            _inputPosition.Y = Clamp(_inputPosition.Y, _minInputPos, _maxInputPos);
            _inputPosition.Z = Clamp(_inputPosition.Z, _minInputPos, _maxInputPos);
            var aC = player?.ActionController; if (aC == null) return; if (player == null) return;
            uint stageID = (uint)Area.CurrentStage;
            if (ImGui.Button("ToCoord")) {
                if ((stageID >= 100 && stageID <= 109) || (stageID >= 200 && stageID <= 202) || (stageID >= 403 && stageID <= 417) || stageID == 504)   {
                    _cMode = true; if (!_lockPosition) { _lockPosition = true; player.PauseAnimations(); } else { 
                        _lockPosition = false; var seiz = new ActionInfo(1, 0); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.ResumeAnimations(); }
                } else if ((stageID >= 301 && stageID <= 306) || (stageID >= 501 && stageID <= 506)) { _cMode = false;
                    if (!_lockPosition)  { _lockPosition = true; var seiz = new ActionInfo(1, 149); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.PauseAnimations(); }
                    else { _lockPosition = false; var seiz = new ActionInfo(1, 0); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.ResumeAnimations(); } 
                } player.Teleport(_inputPosition); _lastPosition = player.Position; }
            if (ImGui.Button("Reset All")) { ResetState(); }
            ImGui.Text($"{player.Position}");
            if (_frameCountdown > 0) {ImGui.Text(_statusMessage);
                _frameCountdown--;
            }
        }

        public void OnLoad() { KeyBindings.AddKeybind("TeleLock", new Keybind<Key>(Key.T, [Key.LeftShift, Key.LeftAlt])); }
        public unsafe void OnUpdate(float deltaTime)  {
            var player = Player.MainPlayer; if (player is null) return;
            if ((uint)Area.CurrentStage != _lastStage) { ResetState(); }
            var aC = player?.ActionController; if (aC == null) return;
            uint stageID = (uint)Area.CurrentStage; if (player == null) return;
            if ((stageID >= 100 && stageID <= 109) || (stageID >= 200 && stageID <= 202) || (stageID >= 403 && stageID <= 417) || stageID == 504)  { _cMode = true;
                if (KeyBindings.IsPressed("TeleLock")) { 
                    if (!_lockPosition) { _lockPosition = true; player.PauseAnimations();
                    } else { _lockPosition = false; var seiz = new ActionInfo(1, 0); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.ResumeAnimations();}}
            } else if ((stageID >= 301 && stageID <= 306) || (stageID >= 501 && stageID <= 506))  { _cMode = false;
                if (KeyBindings.IsPressed("TeleLock")) {
                    if (!_lockPosition)  {  _lockPosition = true; var seiz = new ActionInfo(1, 149); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.PauseAnimations(); ; 
                    } else { _lockPosition = false; var seiz = new ActionInfo(1, 0); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.ResumeAnimations();}}}
            if (player == null) return;
            _currentPosition = player.Position;
            if (_lockPosition) {
                if (_cMode) { var seiz = new ActionInfo(1, 593); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); } player.Teleport(_lastPosition); 
                if (Input.IsDown(Key.UpArrow)) { _currentPosition.Z -= _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.DownArrow)) { _currentPosition.Z += _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.PageUp)) { _currentPosition.Y += _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.PageDown)) {  _currentPosition.Y -= _movementAmount; player.Teleport(_currentPosition); _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.LeftArrow)) {  _currentPosition.X -= _movementAmount; player.Teleport(_currentPosition);  _lastPosition = _currentPosition; }
                if (Input.IsDown(Key.RightArrow)) { _currentPosition.X += _movementAmount; player.Teleport(_currentPosition);  _lastPosition = _currentPosition; }
            } else { _lastPosition = player.Position; }
            if (_mLockPosition)  
            {
                if (_selectedMonsterT == null || player == null) return;
                if (_selectedMonster1 != null) { _selectedMonster1.Teleport(_mLastPosition1); }
                if (_selectedMonster2 != null) { _selectedMonster2.Teleport(_mLastPosition2); }
                if (_selectedMonster3 != null) { _selectedMonster3.Teleport(_mLastPosition3); }
                if (_selectedMonster4 != null) { _selectedMonster4.Teleport(_mLastPosition4); }
                if (_selectedMonster5 != null) { _selectedMonster5.Teleport(_mLastPosition5); }
                if (_selectedMonster6 != null) { _selectedMonster6.Teleport(_mLastPosition6); }
                if (_selectedMonster7 != null) { _selectedMonster7.Teleport(_mLastPosition7); }
                if (_selectedMonster8 != null) { _selectedMonster8.Teleport(_mLastPosition8); }
            }
        }
    }
}