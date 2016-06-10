# Mirage
Physically based rendering in vvvv

## About
Mirage is a set of nodes and hlsl code designed for physically based rendering of non-traditional geometries, realtime or not.

There is no support for triangle based geometry right now and I personnaly have little need for it.  If I did I would use a different (and far more effective) set of tools.

Mirage is targeted primarily at distance field based geometries or any other kind of abstract form that would need to be implemented in shader code.  I'm trying to handle types of objects that would typically be difficult produce and render efficiently with currently available tools.

One of the main core features of Mirage is the use of a BVH to dramatically accelerate ray traversal.