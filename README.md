# Mirage
Physically based rendering in vvvv

## About
Mirage is a set of nodes and hlsl code designed for physically based rendering of non-traditional geometries, realtime or not.

There is no support for triangle based geometry right now and I personnaly have little need for it.  If I need to render those kinds of scenes I would use a different (and far more effective) set of tools.  Mirage is targeted mainly at distance field based geometries or any other kind of abstract form that would need to be implemented in shader code.  Basically we're trying to handle types of objects that would be really difficult to render efficiently with currently available tools.

One of the main core features of Mirage and what I feel it brings to the world of distance field rendering is the use of a BVH to dramatically accelerate ray traversal.