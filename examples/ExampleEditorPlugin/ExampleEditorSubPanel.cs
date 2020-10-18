﻿using System;
using System.Threading.Tasks;
using UtinniCore.Utinni;
using UtinniCoreDotNet.Callbacks;
using UtinniCoreDotNet.Commands;
using UtinniCoreDotNet.PluginFramework;
using UtinniCoreDotNet.UI.Controls;
using UtinniCoreDotNet.Utility;

namespace ExampleEditorPlugin
{
    // SubPanels should inherit EditorPanelBase as it sets up the the correct size automatically.
    public partial class ExampleEditorSubPanel : SubPanel
    {
        private bool enableNodeEditing;

        private readonly IEditorPlugin editorPlugin;
        public ExampleEditorSubPanel(IEditorPlugin editorPlugin) : base(editorPlugin.Information.Name)
        {
            InitializeComponent();
            this.editorPlugin = editorPlugin;

            ObjectCallbacks.AddOnTargetCallback(OnTarget);
            ImGuiCallbacks.AddOnPositionChangedCallback(OnPositionChanged);
            ImGuiCallbacks.AddOnRotationChangedCallback(OnRotationChanged);

            Task updateNodePositionLabel = UpdateNodePositionLabel();
        }

        private void btnAddWsNode_Click(object sender, EventArgs e)
        {
            GroundSceneCallbacks.AddUpdateLoopCall(() =>
            {
                WorldSnapshotReaderWriter.Node node = WorldSnapshot.CreateAddNode("object/tangible/furniture/elegant/shared_chair_s01.iff", Game.Player.ObjectToParent);

                if (node != null)
                {
                    editorPlugin.AddUndoCommand(this, new AddUndoCommandEventArgs(new AddWorldSnapshotNodeCommand(node)));
                }
            });
        }

        private void btnRemoveWsNode_Click(object sender, EventArgs e)
        {
            GroundSceneCallbacks.AddUpdateLoopCall(() =>
            {
                WorldSnapshotReaderWriter.Node node = WorldSnapshotReaderWriter.Get().GetNodeByNetworkId(Game.PlayerLookAtTargetObject.NetworkId);

                if (node != null)
                {
                    editorPlugin.AddUndoCommand(this, new AddUndoCommandEventArgs(new RemoveWorldSnapshotNodeCommand(node)));
                    WorldSnapshot.RemoveNode(node);
                }
            });
        }

        public void OnTarget()
        {
            var target = Game.PlayerLookAtTargetObject;
            if (target == null)
            {
                UtinniCore.ImguiGizmo.imgui_impl.Disable();
            }
            else
            {
                WorldSnapshotReaderWriter.Node node = WorldSnapshotReaderWriter.Get().GetNodeByNetworkId(target.NetworkId);

                if (node == null || !enableNodeEditing)
                {
                    UtinniCore.ImguiGizmo.imgui_impl.Disable();
                }
                else if (enableNodeEditing)
                {
                    UtinniCore.ImguiGizmo.imgui_impl.Enable(target);
                }
            }
        }

        public void OnPositionChanged() // ToDo Something is broken where it sometimes has 1-2 too many undo stages
        {
            GroundSceneCallbacks.AddPreDrawLoopCall(() =>
            {
                var obj = Game.PlayerLookAtTargetObject;
                WorldSnapshotReaderWriter.Node node = WorldSnapshotReaderWriter.Get().GetNodeByNetworkId(obj.NetworkId);

                if (node != null)
                {
                    editorPlugin.AddUndoCommand(this, new AddUndoCommandEventArgs(new WorldSnapshotNodePositionChangedCommand(node, node.Transform, obj.Transform)));
                    node.Transform.Position = obj.Transform.Position;
                }
            });
        }

        public void OnRotationChanged() // ToDo Something is broken where it sometimes has 1-2 too many undo stages
        {
            GroundSceneCallbacks.AddUpdateLoopCall(() =>
            {
                var obj = Game.PlayerLookAtTargetObject;
                WorldSnapshotReaderWriter.Node node = WorldSnapshotReaderWriter.Get().GetNodeByNetworkId(obj.NetworkId);

                if (node != null)
                {
                    editorPlugin.AddUndoCommand(this, new AddUndoCommandEventArgs(new WorldSnapshotNodeRotationChangedCommand(node, node.Transform, obj.Transform)));
                    node.Transform.CopyRotation(obj.Transform);
                    //snapshotPanel.UpdateSelectedNodeControlsPosition(node.Transform.RotationAxis);
                }
            });
        }

        private async Task UpdateNodePositionLabel()
        {
            while (true)
            {
                await Task.Delay(5);
                GroundSceneCallbacks.AddUpdateLoopCall(() =>
                {
                    string result;
                    var obj = Game.PlayerLookAtTargetObject;
                    if (obj == null)
                    {
                        result = "Position: 0, 0, 0";
                    }
                    else
                    {
                        WorldSnapshotReaderWriter.Node node = WorldSnapshotReaderWriter.Get().GetNodeByNetworkId(obj.NetworkId);

                        if (node != null)
                        {
                            var pos = node.Transform.Position;
                            result = String.Format("Position: {0}, {1}, {2}", pos.X, pos.Y, pos.Z);
                        }
                        else
                        {
                            result = "Position: 0, 0, 0";
                        }
                    }

                    if (lblSnapshotNodePosition.Created)
                    {
                        BeginInvoke((Action)(() =>
                        {
                            lblSnapshotNodePosition.Text = result;
                        }));
                    }
                });
            }
        }

        private void chkEnableNodeEditing_CheckedChanged(object sender, EventArgs e)
        {
            enableNodeEditing = chkEnableNodeEditing.Checked;
            OnTarget();
        }
    }
}
