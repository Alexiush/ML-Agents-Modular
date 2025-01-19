# ML Agents Modular

[ML-Agents](https://github.com/Unity-Technologies/ml-agents) library allows for both, handcrafting great environments in Unity and experimenting using 
either predefined models or your own via plugins. However these pipelines barely communicate making it harder to properly fit the both parts whenever you try to
go beyond the common functionality.

ML Agents Modular introduces the toolkit to control the model creation from Unity side. 
It is called modular as it treats functionality that is handled under the hood as a module, allowing to manage it and make changes.

## Usage

### Installation

The package can be installed through the UPM by the link `https://github.com/Alexiush/ML-Agents-Modular.git?path=Assets`.
It has a depedency on [ML Agents Configuration](https://github.com/Alexiush/ML-Agents-Configuration).
It also requires [SerializeReferenceExtensions](https://github.com/mackysoft/Unity-SerializeReferenceExtensions) 
and [VYaml](https://github.com/hadashiA/VYaml) packages.

You need to install a python library as well.

### Agent Graphs

Agent Graphs allow to create a "blueprint" of the agent's model: expected sensors input, how it is processed and passed to the action model.
It is a blueprint as it is ready to accept dynamic amount of valid sensors and actuators. 

Base nodes are `Source`, `Sensor`, `Brain`, `Actuator`, `Consumer`.

* `Source` is an entry point used to validate mapping of inputs to the graph
* `Sensor` is an encoder layer
* `Brain` is a layer that accepts flattened encoded inputs and processes them all together. Its output is further encoded for each of the outputs to match them,
thus it is output-driven.
* `Actuator` is a decoder layer that also requests its shapes.
* `Consumer` is an endpoint used to validate mapping of graph to the outputs  

Nodes are extendable, however, some of their functionality is about generating python source code which is also to be provided.

### Custom trainer

Agent graph can be compiled into a python file from its inspector. However, the model it produces can't be bound to default ML-Agents trainers and requires
custom trainer. It features additional settings that allow passing the model file to the trainer as well as the mappings of dynamic parameters like the count of
the sensors.

It also omits some of the settings as they are now set via graph: 
* Network settings are not used as any shapes are set in the graph, but they are required for compatibility
* Trainer always "uses memory" by keeping track of memories count (memories are managed on the layers here) 

Currently, only the PPO trainer is "customized". 

### Modular Agents

To use the model special wrapper over the `Agent` is needed called `ModularAgent`. It requires `AgentGraph` and `BehaviorConfig` passed to it. 
Then Sensors and Actuators should be mapped to graph's IO using `SourceProvider` and `ActuatorProvider` 
which are similar to `SensorComponent` and `ActuatorComponent` though the latter can't be used as of now. 
Finally, `ModularSensorManager` and `ModularActuatorManager` must be present on the GO for it to run properly.

### Settings

Default path for generated configs and models can be changed in the project settings under `Modular agents` tab.

## Samples

**Roller** - Roller agent trained with ML-Agents Modular.

**Crawler** - Showing what can be added to the crawler agent when using ML-Agents Moduale.

**Hallway** - Hallway agent trained with ML-Agents Modular.

**Sorter** - Sorter agent trained with ML-Agents Modular.

## Roadmap
* All types of trainers support
* Better support of symbolic (dynamic) values and their binding
* Optimization