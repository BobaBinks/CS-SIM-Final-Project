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

    public void Insert(AStarNode node)
    {
        if (heapArray == null || node == null)
            return;

        if(currSize >= capacity)
        {
            // resize array
            capacity *= 2;
            Array.Resize(ref heapArray, capacity);
        }

        heapArray[currSize] = node;
        positionToIndex[node.position] = currSize;
        currSize++;
        HeapifyUp(currSize - 1);
    }

    public AStarNode ExtractMin()
    {
        if (currSize == 0)
            return null;

        AStarNode smallestNode = heapArray[0];
        positionToIndex.Remove(smallestNode.position);

        // swap root node with last node
        SwapNodes(0, currSize - 1);
        currSize--;

        // heapify down
        HeapifyDown(0);

        return smallestNode;
    }

    // for when inserting
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

    // for when extracting
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

    public AStarNode Peek()
    {
        if (currSize == 0) return null;
        return heapArray[0];
    }

    public bool IsEmpty()
    {
        return currSize == 0;
    }

    public int Count()
    {
        return currSize;
    }

    public bool Contains(Vector3Int position)
    {
        return positionToIndex.ContainsKey(position);
    }

    public void Update(AStarNode updatedNode)
    {
        int index;
        if (!positionToIndex.TryGetValue(updatedNode.position, out index))
            return;

        heapArray[index] = updatedNode;

        // update dictionary entry (in case reference changed)
        positionToIndex[updatedNode.position] = index;

        HeapifyUp(index);
        HeapifyDown(index); // in case f increased
    }
}
