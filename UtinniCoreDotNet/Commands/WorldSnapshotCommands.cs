﻿/**
 * MIT License
 *
 * Copyright (c) 2020 Philip Klatt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
**/

using UtinniCore.Swg.Math;
using UtinniCore.Utinni;
using UtinniCoreDotNet.Callbacks;
using UtinniCoreDotNet.UndoRedo;

namespace UtinniCoreDotNet.Commands
{
    public class AddWorldSnapshotNodeCommand : IUndoCommand
    {
        private WorldSnapshotReaderWriter.Node nodeCopy;

        public AddWorldSnapshotNodeCommand(WorldSnapshotReaderWriter.Node node) // Node needs to already be created and added and passed to this ctor
        {
            nodeCopy = new WorldSnapshotReaderWriter.Node(node);
        }

        public string GetText()
        {
            return "Added WorldSnapshot Node: (" + nodeCopy.Id + ") " + nodeCopy.ObjectTemplateName;
        }

        public void Execute()
        {
            GroundSceneCallbacks.AddUpdateLoopCall(() =>
            {
                WorldSnapshot.AddNode(nodeCopy);
            });
        }

        public void Undo()
        {
            GroundSceneCallbacks.AddUpdateLoopCall(() =>
            {
                // As we merely store a copy of the node, we need to fetch the actual node first before removing it
                if (nodeCopy.ParentId == 0)
                {
                    WorldSnapshot.RemoveNode(WorldSnapshotReaderWriter.Get().LastNode);
                }
                else
                {
                    WorldSnapshot.RemoveNode(nodeCopy.ParentNode.LastChild);
                }
            });
        }

        public bool AllowMerge() { return false; }

        public bool Merge(IUndoCommand newCommand) { return false; }
    }

    public class RemoveWorldSnapshotNodeCommand : IUndoCommand
    {
        private readonly WorldSnapshotReaderWriter.Node nodeCopy;

        public RemoveWorldSnapshotNodeCommand(WorldSnapshotReaderWriter.Node node)
        {
            nodeCopy = new WorldSnapshotReaderWriter.Node(node);
        }

        public string GetText()
        {
            return "Removed WorldSnapshot Node: (" + nodeCopy.Id + ") " + nodeCopy.ObjectTemplateName;
        }

        public void Execute()
        {
            GroundSceneCallbacks.AddUpdateLoopCall(() =>
            {
                // As we merely store a copy of the node, we need to fetch the actual node first before removing it
                if (nodeCopy.ParentId == 0)
                {
                    WorldSnapshot.RemoveNode(WorldSnapshotReaderWriter.Get().LastNode);
                }
                else
                {
                    WorldSnapshot.RemoveNode(nodeCopy.ParentNode.LastChild);
                }
            });
        }

        public void Undo()
        {
            GroundSceneCallbacks.AddUpdateLoopCall(() =>
            {
                WorldSnapshot.AddNode(nodeCopy);
            });
        }

        public bool AllowMerge() { return false; }

        public bool Merge(IUndoCommand newCommand) { return false; }
    }

    public class WorldSnapshotNodePositionChangedCommand : IUndoCommand
    {
        private readonly WorldSnapshotReaderWriter.Node nodeCopy;
        private readonly Transform originalTransform;
        private readonly Transform newTransform;

        public WorldSnapshotNodePositionChangedCommand(WorldSnapshotReaderWriter.Node node, Transform originalTransform, Transform newTransform)
        {
            nodeCopy = new WorldSnapshotReaderWriter.Node(node);
            this.originalTransform = new Transform(originalTransform);
            this.newTransform = new Transform(newTransform);
        }

        public string GetText()
        {
            return "Changed Node position: (" + nodeCopy.Id + ") " + nodeCopy.ObjectTemplateName;
        }

        private void SetPosition(Vector position)
        {
            GroundSceneCallbacks.AddUpdateLoopCall(() =>
            {
                var obj = Network.GetObjectById(nodeCopy.Id);
                obj.Transform.Position = position;

                WorldSnapshotReaderWriter.Node node;
                if (nodeCopy.ParentId > 0)
                {
                    node = nodeCopy.ParentNode.GetChildById(nodeCopy.Id);
                }
                else
                {
                    node = WorldSnapshotReaderWriter.Get().GetNodeById(nodeCopy.Id);
                }

                obj.PositionAndRotationChanged(false, node.Transform.Position);
                node.Transform.Position = position;

                WorldSnapshot.DetailLevelChanged();
            });
        }

        public void Execute()
        {
            SetPosition(newTransform.Position);
        }

        public void Undo()
        {
            SetPosition(originalTransform.Position);
        }

        public bool AllowMerge()
        {
            return false;
        }

        public bool Merge(IUndoCommand newCommand)
        {
            return false;
        }
    }

    public class WorldSnapshotNodeRotationChangedCommand : IUndoCommand
    {
        private readonly WorldSnapshotReaderWriter.Node nodeCopy;
        private readonly Transform originalTransform;
        private readonly Transform newTransform;

        public WorldSnapshotNodeRotationChangedCommand(WorldSnapshotReaderWriter.Node node, Transform originalTransform, Transform newTransform)
        {
            nodeCopy = new WorldSnapshotReaderWriter.Node(node);
            this.originalTransform = new Transform(originalTransform);
            this.newTransform = new Transform(newTransform);
        }

        public string GetText()
        {
            return "Changed Node rotation: (" + nodeCopy.Id + ") " + nodeCopy.ObjectTemplateName;
        }

        private void SetRotation(Transform transform)
        {
            GroundSceneCallbacks.AddUpdateLoopCall(() =>
            {
                var obj = Network.GetObjectById(nodeCopy.Id);
                obj.Transform.CopyRotation(transform);

                WorldSnapshotReaderWriter.Node node;
                if (nodeCopy.ParentId > 0)
                {
                    node = nodeCopy.ParentNode.GetChildById(nodeCopy.Id);
                }
                else
                {
                    node = WorldSnapshotReaderWriter.Get().GetNodeById(nodeCopy.Id);
                }

                obj.PositionAndRotationChanged(false, node.Transform.Position);
                node.Transform.CopyRotation(transform);

                WorldSnapshot.DetailLevelChanged();
            });
        }

        public void Execute()
        {
            SetRotation(newTransform);
        }

        public void Undo()
        {
            SetRotation(originalTransform);
        }

        public bool AllowMerge()
        {
            return false;
        }

        public bool Merge(IUndoCommand newCommand)
        {
            return false;
        }
    }
}
