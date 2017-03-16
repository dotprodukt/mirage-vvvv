# Mirage

[![Join the chat at https://gitter.im/mirage-vvvv/Lobby](https://badges.gitter.im/mirage-vvvv/Lobby.svg)](https://gitter.im/mirage-vvvv/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
Physically based rendering in vvvv

## About
Mirage is a set of nodes and hlsl code designed for physically based rendering of non-traditional geometries, realtime or not.

There is no support for triangle based geometry right now and I personnaly have little need for it.  If I did I would use a different (and far more effective) set of tools.

Mirage is targeted primarily at distance field based geometries or any other kind of abstract form that would need to be implemented in shader code.  I'm trying to handle types of objects that would typically be difficult produce and render efficiently with currently available tools.

## Features
Much of this doesn't exist yet, but this is what we're aiming for...
+ BVH to accelerate ray traversal.
+ Realtime Physically Based Renderer
+ Physically Based Monte-Carlo Pathtracer
+ Unified Material Model
+ Support for Radiometric or Photometric input data (color, lights, etc...)
+ Easy to use HLSL library for creating objects, materials and overall consistent interaction within the pipeline.
+ Nodes for aiding in the management and control of scenes and rendering processes.
+ More to come...