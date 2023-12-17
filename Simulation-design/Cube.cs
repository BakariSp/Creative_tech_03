using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum CubeState { Red, Black, Green, White, Empty, Blue }
public class Cube : MonoBehaviour
{
    public CubeState state;
    public Material redMaterial;
    public Material blackMaterial; // This will represent the 'empty' state
    public Material greenMaterial;
    public Material whiteMaterial;
    private Renderer _renderer;

    public int top;
    public int bottom;
    public int Left;
    public int right;
    public int front;
    public int back;

    // Method to change the cube's material/color based on its state
    public void UpdateMaterial()
    {
        // Change the material based on the current state
        switch (state)
        {
            case CubeState.Red:
                _renderer.material = redMaterial;
                break;
            case CubeState.Black:
                _renderer.material = blackMaterial;
                break;
            case CubeState.Green:
                _renderer.material = greenMaterial;
                break;
            case CubeState.White:
                _renderer.material = whiteMaterial;
                break;
            default:
                Debug.LogError("Unknown cube state: " + state);
                break;
        }
    }

    // Call this method when you need to set the cube to 'empty'
    public void SetEmpty()
    {
        state = CubeState.Black; // 'Black' is our 'empty' state
        UpdateMaterial();
    }

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogError("Renderer component not found on the cube", this);
        }
    }

}
