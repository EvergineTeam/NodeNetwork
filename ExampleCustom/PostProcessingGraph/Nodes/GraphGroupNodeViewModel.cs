using ExampleCustom.ViewModels.Nodes;
using NodeNetwork;
using NodeNetwork.Toolkit.Group.AddEndpointDropPanel;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using System;

namespace ExampleCustom.PostProcessingGraph.Nodes
{
    public class GraphGroupNodeViewModel : GraphNodeViewModel
    {
        static GraphGroupNodeViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeView(), typeof(IViewFor<GraphGroupNodeViewModel>));
        }

        public NetworkViewModel Subnet { get; }

        public GraphNodeGroupIOBinding IOBinding
        {
            get => _ioBinding;
            set
            {
                if (_ioBinding != null)
                {
                    throw new InvalidOperationException("IOBinding is already set.");
                }
                _ioBinding = value;
                AddEndpointDropPanelVM = new AddEndpointDropPanelViewModel
                {
                    NodeGroupIOBinding = IOBinding
                };
            }
        }
        private GraphNodeGroupIOBinding _ioBinding;

        public AddEndpointDropPanelViewModel AddEndpointDropPanelVM { get; private set; }

        public GraphGroupNodeViewModel(NetworkViewModel subnet, string groupName) 
            : base(groupName, false)
        {            
            this.Subnet = subnet;
        }
    }
}
