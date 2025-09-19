using UnityEngine;
using System;
using System.Collections.Generic;
public class AStarMinHeap
{
    AStarNode[] heapArray;
    Dictionary<Vector3Int, int> positionToIndex = new Dictionary<Vector3Int, int>();
    int capacity;
    int currSize;

    public AStarMinHeap(int capacity = 100)
    {
        this.capacity = capacity;
        heapArray = new AStarNode[capacity];
        currSize = 0;
    }

    /// <summary>
    /// Insert a new noode into heap and restore heap.
    /// </summary>
    /// <param name="node"></param>
    public void Insert(AStarNode node)
    {
        if (heapArray == null || node == null)
            return;

        // resize array if capacity exceeded
        if (currSize >= capacity)
        {
            capacity *= 2;
            Array.Resize(ref heapArray, capacity);
        }

        // place node at end
        heapArray[currSize] = node;
        positionToIndex[node.position] = currSize;
        currSize++;

        // Restore heap
        HeapifyUp(currSize - 1);
    }

    /// <summary>
    /// Remove and return node with smallest f-cost (root)
    /// </summary>
    /// <returns></returns>
    public AStarNode ExtractMin()
    {
        if (currSize == 0)
            return null;

        // store smallest node to return
        AStarNode smallestNode = heapArray[0];
        positionToIndex.Remove(smallestNode.position);

        // swap root node with last node
        SwapNodes(0, currSize - 1);
        currSize--;

        // heapify down
        HeapifyDown(0);

        return smallestNode;
    }

    /// <summary>
    /// Move node at index upwards until heap property restored.
    /// </summary>
    /// <param name="index"></param>
    public void HeapifyUp(int index)
    {
        while(index > 0)
        {
            // get the parent node
            int parentIndex = (index - 1) / 2;
            AStarNode current = heapArray[index];
            AStarNode parent = heapArray[parentIndex];

            // swap parent node with currnode if parent's combined heuristic (f) is larger
            if (parent.f > current.f || (parent.f == current.f && parent.h > current.h))
            {
                SwapNodes(parentIndex, index);
                index = parentIndex;
            }
            else return;
        }
    }

    /// <summary>
    /// Move node at given index downward until heap property is restored.
    /// </summary>
    /// <param name="index"></param>
    public void HeapifyDown(int index)
    {
        while (index < currSize)
        {
            int leftChildIndex = index * 2 + 1;
            int rightChildIndex = index * 2 + 2;
            int smallestIndex = index;

            if (leftChildIndex < currSize)
            {
                // swap nodes if smallest node have higher f cost.
                // if smallest node and node have same f cost, then compare the h cost
                if (heapArray[smallestIndex].f > heapArray[leftChildIndex].f ||
                    (heapArray[smallestIndex].f == heapArray[leftChildIndex].f &&
                     heapArray[smallestIndex].h > heapArray[leftChildIndex].h))
                {
                    smallestIndex = leftChildIndex;
                }
            }

            if (rightChildIndex < currSize)
            {
                // swap nodes if smallest node have higher f cost.
                // if smallest node and node have same f cost, then compare the h cost
                if (heapArray[smallestIndex].f > heapArray[rightChildIndex].f ||
                    (heapArray[smallestIndex].f == heapArray[rightChildIndex].f &&
                     heapArray[smallestIndex].h > heapArray[rightChildIndex].h))
                {
                    smallestIndex = rightChildIndex;
                }
            }

            if (smallestIndex == index)
                return;

            SwapNodes(index, smallestIndex);
            index = smallestIndex;
        }
    }

    /// <summary>
    /// Swap two nodes in the heap and update indices in dictionary.
    /// </summary>
    /// <param name="indexA"></param>
    /// <param name="indexB"></param>
    public void SwapNodes(int indexA, int indexB)
    {
        if (indexA < 0 || indexB < 0 || indexA > currSize || indexB > currSize)
            return;

        AStarNode temp = heapArray[indexA];
        heapArray[indexA] = heapArray[indexB];
        heapArray[indexB] = temp;

        // update dictionary
        positionToIndex[heapArray[indexA].position] = indexA;
        positionToIndex[heapArray[indexB].position] = indexB;
    }

    /// <summary>
    /// Return smallest node without removing it.
    /// </summary>
    /// <returns></returns>
    public AStarNode Peek()
    {
        if (currSize == 0) return null;
        return heapArray[0];
    }

    /// <summary>
    /// Check if heap is empty.
    /// </summary>
    /// <returns>Returns true if empty.</returns>
    public bool IsEmpty()
    {
        return currSize == 0;
    }

    /// <summary>
    /// Return number of nodes in heap.
    /// </summary>
    /// <returns></returns>
    public int Count()
    {
        return currSize;
    }

    /// <summary>
    /// Check if a node is at a given position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool Contains(Vector3Int position)
    {
        return positionToIndex.ContainsKey(position);
    }

    /// <summary>
    /// Update a node in heap (when f/h cost change).
    /// </summary>
    /// <param name="updatedNode"></param>
    public void Update(AStarNode updatedNode)
    {
        int index;
        if (!positionToIndex.TryGetValue(updatedNode.position, out index))
            return;

        heapArray[index] = updatedNode;

        // update dictionary entry
        positionToIndex[updatedNode.position] = index;

        HeapifyUp(index);
        HeapifyDown(index); // in case f increased
    }
}
