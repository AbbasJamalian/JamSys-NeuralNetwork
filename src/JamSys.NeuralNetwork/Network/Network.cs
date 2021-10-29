﻿using JamSys.NeuralNetwork.Layers;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JamSys.NeuralNetwork.Network
{
    public class Network : INetwork
    {
        public List<ILayer> Layers { get; set; }

        [JsonIgnore]
        public Tensor Input { get { return Layers?[0]?.Input; } }

        [JsonIgnore]
        public Tensor Output { get { return Layers?[Layers.Count - 1]?.Output; } }

        public Network()
        {
            Layers = new List<ILayer>();
        }

        public INetwork AddDenseLayer(int numNeurons, ActivationFunctionEnum activation)
        {
            Layers.Add(new DenseLayer(numNeurons, activation));
            return this;
        }

        public INetwork AddInputLayer(int width, int height = 1, int depth = 1)
        {
            if(Layers.Count > 0)
                throw new InvalidOperationException("Input Layer should be the first layer in the network");

            Layers.Add(new InputLayer(width, height, depth));
            return this;
        }

        public INetwork AddOutputLayer(int numNeurons, ActivationFunctionEnum activation)
        {
            return AddDenseLayer(numNeurons, activation);
        }

        public INetwork AddSoftmaxLayer(int outputCount)
        {
            Layers.Add(new SoftmaxLayer(outputCount));
            return this;
        }

        public INetwork Build()
        {
            ILayer previousLayer = null;
            foreach (var layer in Layers)
            {
                layer.Build(previousLayer);
                previousLayer = layer;
            }
            return this;
        }

        public string Save()
        {
            string result = null;

            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Converters = { new LayerSerializer() },
            };

            result = JsonSerializer.Serialize(this, options);

            return result;
        }

        public INetwork Load(string jsonValue)
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                Converters = { new LayerSerializer() },
            };

            INetwork network = JsonSerializer.Deserialize(jsonValue, typeof(Network), options) as INetwork;
            this.Layers = network.Layers;
            Build();

            return this;
        }


        public void Dispose()
        {
            if (Layers != null)
            {
                Layers.ForEach(l => l.Dispose());
                Layers.Clear();
                Layers = null;
            }
        }

        public Tensor Run(Tensor input)
        {
            if (Layers.Count > 1)
            {
                if (input == null || !input.HasValues)
                    throw new ArgumentException("Input may not be null or without values");

                if(input.Width != Input.Width || input.Depth != Input.Depth || input.Height != Input.Height)
                    throw new ArgumentException("Input must match the network's input");

                Input.Copy(input);
                Layers.ForEach(l => l.Run());
                return Output;
            }
            else
            {
                throw new InvalidOperationException("Network should have at least three layers");
            }
        }

        public void Backpropagate(Tensor delta, double rate)
        {
            for (int layer = Layers.Count - 1; layer >= 0; layer--)
            {
                delta = Layers[layer].Backpropagate(delta, rate);                
            }
        }
    }
}