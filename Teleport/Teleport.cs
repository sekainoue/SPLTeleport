using ImGuiNET;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using System.Numerics;
using System.Threading;
using SharpPluginLoader.Core.Actions;
using System;
using SharpPluginLoader.Core.Memory;

namespace Teleport
{
    public class Teleport : IPlugin
    {
        public string Name => "NoClip [Shift Alt T]";  public string Author => "Seka";

        public Monster? _selectedMonsterT = null;
        public void OnMonsterDestroy(Monster monster) { if (monster == _selectedMonsterT) { _selectedMonsterT = null; } }
        public void OnMonsterDeath(Monster monster) { if (monster == _selectedMonsterT) { _selectedMonsterT = null; } }
        private uint _lastStage;
        public void OnMonsterCreate(Monster monster) { uint stageID = (uint)Area.CurrentStage; _lastStage = stageID; }
        private Vector3 _currentPosition;
        private Vector3 _lastPosition;
        private Vector3 _mLastPosition;
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
            if (player == null) return;
            ImGui.InputFloat("Velocity", ref _movementAmount, 0.0f, 300.0f);
            _movementAmount = Clamp(_movementAmount, _minMovementAmount, _maxMovementAmount);
            var monsters = Monster.GetAllMonsters().TakeLast(20).ToArray();
            if (monsters == null) return;
            if (ImGui.BeginCombo("Select", $"{_selectedMonsterT}"))  {
                foreach (var monster in monsters) {
                    if (ImGui.Selectable($"{monster}", _selectedMonsterT == monster))
                    { _selectedMonsterT = monster; }
                } ImGui.EndCombo();
            }
            if (ImGui.Button("Mon-to-You")) {
                if (_selectedMonsterT == null || player == null) return;
                _selectedMonsterT.Teleport(player.Position); _mLastPosition = _selectedMonsterT.Position;
                if (!_mLockPosition) { _mLockPosition = true; } else { _mLockPosition = false; }
            }
            if (ImGui.Button("You-to-Mon")) {
                if (_selectedMonsterT == null || player == null) return; player.Teleport(_selectedMonsterT.Position); _lastPosition = player.Position;
            }
            ImGui.InputFloat3("↑↓←→/PgUp/PgDn", ref _inputPosition);
            _inputPosition.X = Clamp(_inputPosition.X, _minInputPos, _maxInputPos);
            _inputPosition.Y = Clamp(_inputPosition.Y, _minInputPos, _maxInputPos);
            _inputPosition.Z = Clamp(_inputPosition.Z, _minInputPos, _maxInputPos);
            var aC = player?.ActionController; if (aC == null) return; if (player == null) return;
            uint stageID = (uint)Area.CurrentStage;
            if (ImGui.Button("ToCoord")) {
                if ((stageID >= 100 && stageID <= 109) || (stageID >= 200 && stageID <= 202) || (stageID >= 403 && stageID <= 417) || stageID == 504) { _cMode = true;
                    if (!_lockPosition) { _lockPosition = true; player.PauseAnimations();
                    } else  { _lockPosition = false; var seiz = new ActionInfo(1, 0);  _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.ResumeAnimations(); }
                } else if ((stageID >= 301 && stageID <= 306) || (stageID >= 501 && stageID <= 506)) {_cMode = false;
                    if (!_lockPosition) { _lockPosition = true; var seiz = new ActionInfo(1, 149); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.PauseAnimations();
                    } else {  _lockPosition = false; var seiz = new ActionInfo(1, 0); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.ResumeAnimations(); } } } }
        public void OnLoad() { KeyBindings.AddKeybind("TeleLock", new Keybind<Key>(Key.T, [Key.LeftShift, Key.LeftAlt])); }
        public unsafe void OnUpdate(float deltaTime)  {
            if ((uint)Area.CurrentStage != _lastStage) { _selectedMonsterT = null; }
            var player = Player.MainPlayer; if (player is null) return;
            var aC = player?.ActionController; if (aC == null) return;
            uint stageID = (uint)Area.CurrentStage; if (player == null) return;
            if ((stageID >= 100 && stageID <= 109) || (stageID >= 200 && stageID <= 202) || (stageID >= 403 && stageID <= 417) || stageID == 504)  { _cMode = true;
                if (KeyBindings.IsPressed("TeleLock")) { 
                    if (!_lockPosition) { _lockPosition = true; player.PauseAnimations();
                    } else { _lockPosition = false; var seiz = new ActionInfo(1, 0); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.ResumeAnimations();}}
            } else if ((stageID >= 301 && stageID <= 306) || (stageID >= 501 && stageID <= 506))  { _cMode = false;
                if (KeyBindings.IsPressed("TeleLock")) {
                    if (!_lockPosition)  {  _lockPosition = true; var seiz = new ActionInfo(1, 149); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.PauseAnimations(); 
                    } else { _lockPosition = false; var seiz = new ActionInfo(1, 0); _seiz.Invoke(aC.Instance, MemoryUtil.AddressOf(ref seiz)); player.ResumeAnimations(); }}}
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
            if (_selectedMonsterT == null) return;
            if (_mLockPosition && _selectedMonsterT != null)  { _selectedMonsterT.Teleport(_mLastPosition); }
        }
    }
}
