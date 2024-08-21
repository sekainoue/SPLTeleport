using ImGuiNET;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using SharpPluginLoader.Core.Actions;
using SharpPluginLoader.Core.Memory;

namespace FlyMeToTheMon
{
    public class FlyMeToTheMon : IPlugin
    {
        public string Name => "Fly Me to the Mon"; public string Author => "seka";
        public Monster? _selectedMonsterT = null;
        public void ResetState()
        {
            _selectedMonsterT = null;
        }

        private uint _lastStage = 0;
        public void OnMonsterCreate(Monster monster)
        {
            _lastStage = (uint)Area.CurrentStage;
        }
        public void OnQuestLeave(int questId) => ResetState();
        public void OnQuestComplete(int questId) => ResetState();
        public void OnQuestFail(int questId) => ResetState();
        public void OnQuestReturn(int questId) => ResetState();
        public void OnQuestAbandon(int questId) => ResetState();
        public void OnQuestEnter(int questId) => ResetState();
        private NativeFunction<nint, nint, bool> _fly = new(0x140269c90);

        public void OnLoad()
        {
            KeyBindings.AddKeybind("ToWingdrake", new Keybind<Key>(Key.V, [Key.LeftShift, Key.LeftAlt]));
        }

        public unsafe void OnImGuiRender()
        {
            var player = Player.MainPlayer;
            if (player == null)
                return;

            var monsters = Monster.GetAllMonsters().TakeLast(5).ToArray();
            if (monsters == null)
                return;
            if (ImGui.BeginCombo("Shift Alt V to Main", $"{_selectedMonsterT}"))
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

            if (ImGui.Button("Go!"))
            {
                if (_selectedMonsterT == null || player == null)
                    return;
                player.Teleport(_selectedMonsterT.Position);
                _selectedMonsterT = null;
            }
        }

        public unsafe void OnUpdate(float deltaTime)
        {
            var player = Player.MainPlayer;
            if (player is null)
                return;
            if ((uint)Area.CurrentStage != _lastStage)
            {
                ResetState();
            }

            uint stageID = (uint)Area.CurrentStage;

            var aC = player?.ActionController;
            if (aC == null)
                return;

            if (Quest.CurrentQuestId != -1 && KeyBindings.IsPressed("ToWingdrake"))
            {
                var flyToMon = new ActionInfo(1, 318);
                _fly.Invoke(aC.Instance, MemoryUtil.AddressOf(ref flyToMon));
            }
        }
    }
}