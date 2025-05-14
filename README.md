![image](https://github.com/user-attachments/assets/2007fb25-0635-4589-a53f-6126cf10b2fe)

[English](README.md) | [中文](README_zh.md)

# Introduction
Use Voronoi algorithm in Unity to generate 2D mesh and achieve 2D crushing effect.

Features: 1. Pre-process and generate mesh data; 2. Real-time mesh generation;

Reference [this csdn article](https://blog.csdn.net/w1594731007/article/details/89705489) algorithm implementation. I added cell generation to support creating meshes and UV data.

# How to Use

## Example 1: Run CreateMesh Scene
Features:
1. Pre-process and generate mesh data;
2. Customize the generated mesh data;

### Generate Mesh
* Modify parameters in the VoronoiCreator node in the CreateMesh scene, then click `Save Mesh` to generate the mesh after running the scene.
* During this time, you can delete/combine meshes. Select one or multiple meshes to: 1) Delete: Choose `Delete Mesh` in the Inspector panel. 2) Combine: Select multiple meshes, then choose `Combine Meshes` in the Inspector panel.

Important parameters:
* **Is Alpha Test Enable**: Whether to enable alpha testing. If enabled, meshes in transparent areas of the texture will be discarded.
* **Uv**: UV range, default (1.0,1.0), mesh UV range will be from (0,0) to (1.0,1.0).
* **Seed**: Random seed, defaults to 0 which generates a new random seed, or you can manually input a seed.
* **Mesh Size**: Size of the generated mesh;
* **Point Count X/Y**: Number of Voronoi feature points in X/Y direction, higher values generate more meshes;
* **Max Offset X/Y**: Maximum offset of Voronoi feature points in X/Y direction, higher values generate more irregular meshes;

## Example 2: Run MeshModifier Scene
Features:
Modify meshes generated in Example 1, including:
1. Mesh Mode: Combine/Delete meshes;
2. Vertex Mode: Adjust mesh vertex positions;

### 1. Mesh Mode
* Run the VoronoiModifier scene
* In the `Game` view, switch to: `Modify Mesh`
* In the `Scene` view, select meshes to edit: 1) Delete: Choose `Delete Mesh` in the Inspector panel. 2) Combine: Select multiple meshes, then choose `Combine Meshes` in the Inspector panel.

### 2. Vertex Mode
* Run the VoronoiModifier scene
* In the `Game` view, switch to: `Modify Vertex`
* In the `Scene` view, select vertices to edit (blue spheres), move them to adjust positions.

### 3. Save
* Click `Save` in the `Game` view to save mesh data.
