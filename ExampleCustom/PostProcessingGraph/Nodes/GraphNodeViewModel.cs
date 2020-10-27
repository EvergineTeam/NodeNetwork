using DynamicData;
using NodeNetwork;
using NodeNetwork.ViewModels;
using NodeNetwork.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExampleCustom.ViewModels.Nodes
{
    public class GraphNodeViewModel : NodeViewModel
    {

        static GraphNodeViewModel()
        {
            NNViewRegistrar.AddRegistration(() => new NodeView(), typeof(IViewFor<GraphNodeViewModel>));
        }

        public GraphNodeViewModel(string name, bool generatePorts = true)
        {
            this.Name = name;

            if (generatePorts)
            {
                this.Inputs.Add(new NodeInputViewModel() { Name = "Input0" });
                this.Inputs.Add(new NodeInputViewModel() { Name = "Input1" });
                this.Outputs.Add(new NodeOutputViewModel() { Name = "Output0" });
                this.Outputs.Add(new NodeOutputViewModel() { Name = "Output1" });
            }
        }
    }
}
