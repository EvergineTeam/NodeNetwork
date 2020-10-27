using DynamicData;
using NodeNetwork.Toolkit.Group;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace ExampleCustom.PostProcessingGraph.Nodes
{
    public class GraphNodeGroupIOBinding : NodeGroupIOBinding
    {
        private readonly IDictionary<NodeOutputViewModel, NodeInputViewModel> _outputInputMapping = new Dictionary<NodeOutputViewModel, NodeInputViewModel>();

        public GraphNodeGroupIOBinding(NodeViewModel groupNode, NodeViewModel entranceNode, NodeViewModel exitNode)
            : base(groupNode, entranceNode, exitNode)
        {
            // For each input on the group node, create an output in the subnet
            groupNode.Inputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Left)
                .Transform(i =>
                {
                    // Dynamic is applied here so that late binding is used to find the most specific 
                    // CreateCompatibleOutput variant for this specific input.
                    NodeOutputViewModel result = CreateCompatibleOutput(i);
                    BindOutputToInput((dynamic)result, (dynamic)i);
                    return result;
                }).PopulateInto(entranceNode.Outputs);
            groupNode.Inputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Right)
                .Transform(i =>
                {
                    NodeOutputViewModel result = CreateCompatibleOutput(i);
                    BindOutputToInput((dynamic)result, (dynamic)i);
                    return result;
                }).PopulateInto(exitNode.Outputs);
            groupNode.Inputs.Connect().OnItemRemoved(input =>
                    _outputInputMapping.Remove(
                    _outputInputMapping.First(kvp => kvp.Value == input)
                    )
                );

            // For each output on the group node, create an input in the subnet
            groupNode.Outputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Right)
                .Transform(o =>
                {
                    NodeInputViewModel result = CreateCompatibleInput(o);
                    BindOutputToInput((dynamic)o, (dynamic)result);
                    return result;
                }).PopulateInto(exitNode.Inputs);
            groupNode.Outputs.Connect()
                .Filter(input => input.PortPosition == PortPosition.Left)
                .Transform(o =>
                {
                    NodeInputViewModel result = CreateCompatibleInput(o);
                    BindOutputToInput((dynamic)o, (dynamic)result);
                    return result;
                }).PopulateInto(entranceNode.Inputs);
            groupNode.Outputs.Connect().OnItemRemoved(output => _outputInputMapping.Remove(output));
        }

        protected virtual void BindEndpointProperties(NodeOutputViewModel output, NodeInputViewModel input)
        {
            input.WhenAnyValue(vm => vm.Name).BindTo(output, vm => vm.Name);
            output.WhenAnyValue(vm => vm.Name).BindTo(input, vm => vm.Name);
            input.WhenAnyValue(vm => vm.SortIndex).BindTo(output, vm => vm.SortIndex);
            output.WhenAnyValue(vm => vm.SortIndex).BindTo(input, vm => vm.SortIndex);
            input.WhenAnyValue(vm => vm.Icon).BindTo(output, vm => vm.Icon);
            output.WhenAnyValue(vm => vm.Icon).BindTo(input, vm => vm.Icon);
        }

        protected virtual void BindOutputToInput(NodeOutputViewModel output, NodeInputViewModel input)
        {
            BindEndpointProperties(output, input);
            _outputInputMapping.Add(output, input);
        }

        private NodeInputViewModel CreateCompatibleInput(NodeOutputViewModel o)
        {
            return new NodeInputViewModel() { Name = $"CI_{o.Name}" };
        }

        private NodeOutputViewModel CreateCompatibleOutput(NodeInputViewModel i)
        {
            return new NodeOutputViewModel() { Name = $"CO_{i.Name}" };
        }

        public override NodeInputViewModel AddNewGroupNodeInput(NodeOutputViewModel candidateOutput)
        {
            NodeInputViewModel input = CreateCompatibleInput(candidateOutput);
            GroupNode.Inputs.Add(input);
            return input;
        }

        public override NodeOutputViewModel AddNewGroupNodeOutput(NodeInputViewModel candidateInput)
        {
            NodeOutputViewModel output = CreateCompatibleOutput(candidateInput);
            GroupNode.Outputs.Add(output);
            return output;
        }

        public override NodeOutputViewModel AddNewSubnetInlet(NodeInputViewModel candidateInput)
        {
            NodeInputViewModel input = this.AddNewGroupNodeInput(CreateCompatibleOutput(candidateInput));
            return this.GetSubnetInlet(input);
        }

        public override NodeInputViewModel AddNewSubnetOutlet(NodeOutputViewModel candidateOutput)
        {
            NodeOutputViewModel output = this.AddNewGroupNodeOutput(CreateCompatibleInput(candidateOutput));
            return this.GetSubnetOutlet(output);
        }        

        public override NodeInputViewModel GetGroupNodeInput(NodeOutputViewModel entranceOutput)
        {
            return _outputInputMapping[entranceOutput];
        }

        public override NodeOutputViewModel GetGroupNodeOutput(NodeInputViewModel exitInput)
        {
            return _outputInputMapping.Single(p => p.Value == exitInput).Key;
        }

        public override NodeOutputViewModel GetSubnetInlet(NodeInputViewModel entranceInput)
        {
            return _outputInputMapping.Single(p => p.Value == entranceInput).Key;
        }

        public override NodeInputViewModel GetSubnetOutlet(NodeOutputViewModel exitOutput)
        {
            return _outputInputMapping[exitOutput];
        }
    }
}
