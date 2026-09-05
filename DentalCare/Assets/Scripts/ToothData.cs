using System.Collections.Generic;
using UnityEngine;

public class ToothData
{
    public List<Vector2Int> pixels;
    public int minX, maxX, minY, maxY;
    public float widthPixels, heightPixels;

    public ToothData()
    {
        pixels = new List<Vector2Int>();
    }

    public void CalculateBounds()
    {
        if (pixels == null || pixels.Count == 0) return;

        minX = pixels[0].x;
        maxX = pixels[0].x;
        minY = pixels[0].y;
        maxY = pixels[0].y;

        foreach (Vector2Int pixel in pixels)
        {
            if (pixel.x < minX) minX = pixel.x;
            if (pixel.x > maxX) maxX = pixel.x;
            if (pixel.y < minY) minY = pixel.y;
            if (pixel.y > maxY) maxY = pixel.y;
        }

        widthPixels = maxX - minX;
        heightPixels = maxY - minY;
    }
}