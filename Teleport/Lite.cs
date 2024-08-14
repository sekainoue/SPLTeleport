using ImGuiNET;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core;

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

        public unsafe void OnImGuiRender()
        {
            var player = Player.MainPlayer;
            if (player == null)
                return;

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

        }
    }
}