using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;



public class TicketGenerator : MonoBehaviour
{

    public static TicketGenerator instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public List<int[,]> GenerateTheTicket()
    {
        List<int[,]> ticketMatrixList = new List<int[,]>();
        for (int i = 0; i < Test.instance.numberOfTicket; i++)
        {
            ticketMatrixList.Add(new int[3, 9]);
        }

        Node[] nodes = new Node[6];
        for (int i = 0; i < 6; i++)
        {
            nodes[i] = new Node();
        }
        var l1 = new List<int>();
        for (int i = 1; i <= 9; i++)
        {
            l1.Add(i);
        }
        var l2 = new List<int>();
        for (int i = 10; i <= 19; i++)
        {
            l2.Add(i);
        }
        var l3 = new List<int>();
        for (int i = 20; i <= 29; i++)
        {
            l3.Add(i);
        }
        var l4 = new List<int>();
        for (int i = 30; i <= 39; i++)
        {
            l4.Add(i);
        }
        var l5 = new List<int>();
        for (int i = 40; i <= 49; i++)
        {
            l5.Add(i);
        }
        var l6 = new List<int>();
        for (int i = 50; i <= 59; i++)
        {
            l6.Add(i);
        }
        var l7 = new List<int>();
        for (int i = 60; i <= 69; i++)
        {
            l7.Add(i);
        }
        var l8 = new List<int>();
        for (int i = 70; i <= 79; i++)
        {
            l8.Add(i);
        }
        var l9 = new List<int>();
        for (int i = 80; i <= 90; i++)
        {
            l9.Add(i);
        }
        var columns = new List<List<int>>();
        columns.Add(l1);
        columns.Add(l2);
        columns.Add(l3);
        columns.Add(l4);
        columns.Add(l5);
        columns.Add(l6);
        columns.Add(l7);
        columns.Add(l8);
        columns.Add(l9);
        var set1 = new List<List<int>>();
        var set2 = new List<List<int>>();
        var set3 = new List<List<int>>();
        var set4 = new List<List<int>>();
        var set5 = new List<List<int>>();
        var set6 = new List<List<int>>();
        for (int i = 0; i < 9; i++)
        {
            set1.Add(new List<int>());
            set2.Add(new List<int>());
            set3.Add(new List<int>());
            set4.Add(new List<int>());
            set5.Add(new List<int>());
            set6.Add(new List<int>());
        }
        var sets = new List<List<List<int>>>();
        sets.Add(set1);
        sets.Add(set2);
        sets.Add(set3);
        sets.Add(set4);
        sets.Add(set5);
        sets.Add(set6);
        // assigning elements to each set for each column
        for (int i = 0; i < 9; i++)
        {
            var li = columns[i];
            for (int j = 0; j < 6; j++)
            {
                int v = Node.getRand(0, li.Count - 1);
                int k = li[v];
                var set = sets[j][i];
                set.Add(k);
                li.RemoveAt(v);
            }
        }
        // assign element from last column to random set
        var lastCol = columns[8];
        var randNumIndex = Node.getRand(0, lastCol.Count - 1);
        var randNum = lastCol[randNumIndex];
        var randSetIndex = Node.getRand(0, sets.Count - 1);
        var randSet = sets[randSetIndex][8];
        randSet.Add(randNum);
        lastCol.RemoveAt(randNumIndex);
        // 3 passes over the remaining columns
        for (int pass = 0; pass < 3; pass++)
        {
            for (int i = 0; i < 9; i++)
            {
                var col = columns[i];
                if (col.Count == 0)
                {
                    continue;
                }
                var randNumIndex_p = Node.getRand(0, col.Count - 1);
                var randNum_p = col[randNumIndex_p];
                var vacantSetFound = false;
                while (!vacantSetFound)
                {
                    var randSetIndex_p = Node.getRand(0, sets.Count - 1);
                    var randSet_p = sets[randSetIndex_p];
                    if (Node.getNumberOfElementsInSet(randSet_p) == 15 || randSet_p[i].Count == 2)
                    {
                        continue;
                    }
                    vacantSetFound = true;
                    randSet_p[i].Add(randNum_p);
                    col.RemoveAt(randNumIndex_p);
                }
            }
        }
        // one more pass over the remaining columns
        for (int i = 0; i < 9; i++)
        {
            var col = columns[i];
            if (col.Count == 0)
            {
                continue;
            }
            var randNumIndex_p = Node.getRand(0, col.Count - 1);
            var randNum_p = col[randNumIndex_p];
            var vacantSetFound = false;
            while (!vacantSetFound)
            {
                var randSetIndex_p = Node.getRand(0, sets.Count - 1);
                var randSet_p = sets[randSetIndex_p];
                if (Node.getNumberOfElementsInSet(randSet_p) == 15 || randSet_p[i].Count == 3)
                {
                    continue;
                }
                vacantSetFound = true;
                randSet_p[i].Add(randNum_p);
                col.RemoveAt(randNumIndex_p);
            }
        }
        // sort the internal sets
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                sets[i][j].Sort();
            }
        }
        // got the sets - need to arrange in tickets now
        for (int setIndex = 0; setIndex < 6; setIndex++)
        {
            var currSet = sets[setIndex];
            var currTicket = nodes[setIndex];
            // fill first row
            for (int size = 3; size > 0; size--)
            {
                if (currTicket.getRowCount(0) == 5)
                {
                    break;
                }
                for (int colIndex = 0; colIndex < 9; colIndex++)
                {
                    if (currTicket.getRowCount(0) == 5)
                    {
                        break;
                    }
                    if (currTicket.A[0, colIndex] != 0)
                    {
                        continue;
                    }
                    var currSetCol = currSet[colIndex];
                    if (currSetCol.Count != size)
                    {
                        continue;
                    }

                    currTicket.A[0, colIndex] = currSetCol[0];
                    currSetCol.RemoveAt(0);
                }
            }
            // fill second row
            for (int size = 2; size > 0; size--)
            {
                if (currTicket.getRowCount(1) == 5)
                {
                    break;
                }
                for (int colIndex = 0; colIndex < 9; colIndex++)
                {
                    if (currTicket.getRowCount(1) == 5)
                    {
                        break;
                    }
                    if (currTicket.A[1, colIndex] != 0)
                    {
                        continue;
                    }
                    var currSetCol = currSet[colIndex];
                    if (currSetCol.Count != size)
                    {
                        continue;
                    }
                    currTicket.A[1, colIndex] = currSetCol[0];
                    currSetCol.RemoveAt(0);
                }
            }
            // fill third row
            for (int size = 1; size > 0; size--)
            {
                if (currTicket.getRowCount(2) == 5)
                {
                    break;
                }
                for (int colIndex = 0; colIndex < 9; colIndex++)
                {
                    if (currTicket.getRowCount(2) == 5)
                    {
                        break;
                    }
                    if (currTicket.A[2, colIndex] != 0)
                    {
                        continue;
                    }
                    var currSetCol = currSet[colIndex];
                    if (currSetCol.Count != size)
                    {
                        continue;
                    }
                    currTicket.A[2, colIndex] = currSetCol[0];
                    currSetCol.RemoveAt(0);
                }
            }
        }
        try
        {
            // quick patch to ensure columns are sorted
            for (int i = 0; i < 6; i++)
            {
                var currTicket = nodes[i];
                currTicket.sortColumns();
            }
        }
        catch (Exception e)
        {
            // something wrong, not a P0...eating the exception
            Console.WriteLine("Note: there is a small probability your columns may not be sorted, it should not impact the gameplay");
            Console.WriteLine("Please create an issue in the github for investigation");
           
        }

        for (int i = 0; i < ticketMatrixList.Count; i++)
        {
            var currTicket = nodes[i];
            for (int r = 0; r < 3; r++)
            {
                for (int col = 0; col < 9; col++)
                {
                    var num = currTicket.A[r, col];
                    ticketMatrixList[i][r, col] = num;
                }
            }
        }

        return ticketMatrixList;
    }

}


public  class Node
{
    public int[,] A;
    public Node()
    {
        this.A = new int[3, 9];
    }
    public int getRowCount(int r)
    {
        var count = 0;
        for (int i = 0; i < 9; i++)
        {
            if (this.A[r, i] != 0)
            {
                count++;
            }
        }
        return count;
    }
    public int getColCount(int c)
    {
        var count = 0;
        for (int i = 0; i < 3; i++)
        {
            if (this.A[i, c] != 0)
            {
                count++;
            }
        }
        return count;
    }
    // gives the row number of first found empty cell in given column
    public int getEmptyCellInCol(int c)
    {
        for (int i = 0; i < 3; i++)
        {
            if (this.A[i, c] == 0)
            {
                return i;
            }
        }
        return -1;
    }
    public void sortColumnWithThreeNumbers(int c)
    {
        var emptyCell = this.getEmptyCellInCol(c);
        if (emptyCell != -1)
        {
            throw new Exception("Hey! your column has <3 cells filled, invalid function called");
        }

        int[] tempArr = new int[] { this.A[0, c], this.A[1, c], this.A[2, c] };
        Array.Sort(tempArr);
            for (int r = 0; r< 3; r++)
            {
                this.A[r, c] = tempArr[r];
            }
    }
    private void sortColumnWithTwoNumbers(int c)
    {
            var emptyCell = this.getEmptyCellInCol(c);
            if (emptyCell == -1)
            {
                    throw new Exception("Hey! your column has 3 cells filled, invalid function called");
            }
            int cell1;
            int cell2;
            if (emptyCell == 0)
            {
                cell1 = 1;
                cell2 = 2;
            }
            else if (emptyCell == 1)
            {
                cell1 = 0;
                cell2 = 2;
            }
            else 
            {
                // emptyCell == 2
                cell1 = 0;
                cell2 = 1;
            }
            if (this.A [cell1, c] < this.A [cell2, c])
            {
                return;
            }
            else 
            {
        // swap
                var temp = this.A[cell1, c];
                this.A[cell1, c] = this.A[cell2, c];
                this.A[cell2, c] = temp;
            }
    }
        // 		 * Three possible scenarios: 
        // 		 * 1) only one number in the column - leave 
        // 		 * 2) Two numbers in the column & not sorted - swap 
        // 		 * 3) Three numbers in the column - sort
    private void sortColumn(int c) 
    {
            if (this.getColCount(c) == 1)
            {
                 return;
            }
            else if (this.getColCount(c) == 2)
            {
                this.sortColumnWithTwoNumbers(c);
            }
            else 
            {
                this.sortColumnWithThreeNumbers(c);
            }
    }
    public void sortColumns()
    {

            for (int c = 0; c < 9; c++)
            {
                this.sortColumn(c);
            }
    }
    
    public static int getRand(int min, int max)
    {
        var rand = new System.Random();
        return rand.Next(max - min + 1) + min;
    }
    public static int getNumberOfElementsInSet(List<List<int>> set)
    {
        var count = 0;
        foreach (List<int> li in set)
        {
            count += li.Count;
        }
        return count;

    }
}
    