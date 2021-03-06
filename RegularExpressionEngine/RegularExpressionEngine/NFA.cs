
using System;
using SCG = System.Collections.Generic;
using C5;

using state = System.Int32;
using input = System.Char;

namespace RegularExpressionEngine
{

  class NFA
  {
    public state initial;
    public state final;
    private int size;
    public int _size { get { return size; } }

    public SortedArray<input> inputs;
    public input[][] transTable;


    public enum Constants
    {
      Epsilon = 'ε',
      None = '\0'
    }

    public NFA(NFA nfa)
    {
      initial = nfa.initial;
      final = nfa.final;
      size = nfa.size;
      inputs = nfa.inputs;
      transTable = nfa.transTable;
    }


    public NFA(int size_, state initial_, state final_)
    {
      initial = initial_;
      final = final_;
      size = size_;

      IsLegalState(initial);
      IsLegalState(final);

      inputs = new SortedArray<input>();


      transTable = new input[size][];

      for(int i = 0; i < size; ++i)
        transTable[i] = new input[size];
    }

    public bool IsLegalState(state s)
    {
      if(s < 0 || s >= size)
        return false;

      return true;
    }


    public void AddTrans(state from, state to, input @in)
    {
      IsLegalState(from);
      IsLegalState(to);

      transTable[from][to] = @in;

      if(@in != (char)Constants.Epsilon)
        inputs.Add(@in);
    }

 
    public void FillStates(NFA other)
    {
      for(state i = 0; i < other.size; ++i)
        for(state j = 0; j < other.size; ++j)
          transTable[i][j] = other.transTable[i][j];

      SCG.IEnumerator<input> cE = other.inputs.GetEnumerator();

      while(cE.MoveNext())
        inputs.Add(cE.Current);
    }

    public void ShiftStates(int shift)
    {
      int newSize = size + shift;

      if(shift < 1)
        return;

      input[][] newTransTable = new input[newSize][];

      for(int i = 0; i < newSize; ++i)
        newTransTable[i] = new input[newSize];

      for(state i = 0; i < size; ++i)
        for(state j = 0; j < size; ++j)
          newTransTable[i + shift][j + shift] = transTable[i][j];


      size = newSize;
      initial += shift;
      final += shift;
      transTable = newTransTable;
    }

    /// <summary>
    /// добавить состояния в нфа
    /// </summary>
    public void AppendEmptyState()
    {
      transTable = Resize(transTable, size + 1);

      size += 1;
    }

    private static input[][] Resize(input[][] transTable, int newSize)
    {
      input[][] newTransTable = new input[newSize][];

      for(int i = 0; i < newSize; ++i)
        newTransTable[i] = new input[newSize];

      for(int i = 0; i <= transTable.Length - 1; i++)
        for(int j = 0; j <= transTable[i].Length - 1; j++)
        {
          if(transTable[i][j] != '\0')
            newTransTable[i][j] = transTable[i][j];
        }

      return newTransTable;
    }
    public Set<state> Move(Set<state> states, input inp)
    {
      Set<state> result = new Set<state>();

      foreach(state state in states)
      {
        int i = 0;

        foreach(input input in transTable[state])
        {
          if(input == inp)
          {
            state u = Array.IndexOf(transTable[state], input, i);
            result.Add(u);
          }

          i = i + 1;
        }
      }

      return result;
    }

    public void Show()
    {
      Console.WriteLine("This NFA has {0} states: 0 - {1}", size, size - 1);
      Console.WriteLine("The initial state is {0}", initial);
      Console.WriteLine("The final state is {0}\n", final);

      for(state from = 0; from < size; ++from)
      {
        for(state to = 0; to < size; ++to)
        {
          input @in = transTable[from][to];

          if(@in != (char)Constants.None)
          {
            Console.Write("Transition from {0} to {1} on input ", from, to);

            if(@in == (char)Constants.Epsilon)
              Console.Write("Epsilon\n");
            else
              Console.Write("{0}\n", @in);
          }
        }
      }
      Console.Write("\n\n");
    }

    public static NFA TreeToNFA(ParseTree tree)
    {
        switch (tree.type)
        {
            case ParseTree.NodeType.Chr:
                return BuildNFABasic(tree.data.Value);
            case ParseTree.NodeType.Alter:
                return BuildNFAAlter(TreeToNFA(tree.left), TreeToNFA(tree.right));
            case ParseTree.NodeType.Concat:
                return BuildNFAConcat(TreeToNFA(tree.left), TreeToNFA(tree.right));
            case ParseTree.NodeType.Star:
                return BuildNFAStar(TreeToNFA(tree.left));
            case ParseTree.NodeType.Question:
                return BuildNFAAlter(TreeToNFA(tree.left), BuildNFABasic((char)Constants.Epsilon));
            default:
                return null;
        }
    }

    public static NFA BuildNFABasic(input @in)
    {
      NFA basic = new NFA(2, 0, 1);

      basic.AddTrans(0, 1, @in);

      return basic;
    }

    /// <summary>
    /// Альтернатива (nfa1|nfa2)
    /// </summary>
    public static NFA BuildNFAAlter(NFA nfa1, NFA nfa2)
    {
      // Формат: Новый нфа должен содержать оба нфа1 и нфа2 + инишиал и файнал состояния
      // Сначала определяем новое инишиал состояние, потом состояния нфа1, затем
      // затем состояние из нфа2, и финальное состояние

      // место для нового состояния стартового
      nfa1.ShiftStates(1);

      // место для нового состояния нфа1
      nfa2.ShiftStates(nfa1.size);

      // создаем новый нфа, добавля в нфа2 
      NFA newNFA = new NFA(nfa2);

      newNFA.FillStates(nfa1);

      // Устанавливаем стартовые состояния и переходы
      newNFA.AddTrans(0, nfa1.initial, (char)Constants.Epsilon);
      newNFA.AddTrans(0, nfa2.initial, (char)Constants.Epsilon);

      newNFA.initial = 0;

      // Место для нового финального состояния
      newNFA.AppendEmptyState();

      //  Устанавливаем новое финальное состояние
      newNFA.final = newNFA.size - 1;

      newNFA.AddTrans(nfa1.final, newNFA.final, (char)Constants.Epsilon);
      newNFA.AddTrans(nfa2.final, newNFA.final, (char)Constants.Epsilon);

      return newNFA;
    }

    /// <summary>
    /// Альтернатива (nfa1|nfa2)
    /// </summary>
    public static NFA BuildNFAConcat(NFA nfa1, NFA nfa2)
    {
      // финальные Состояния нфа2  заменяют финальные состояния нфа1
      nfa2.ShiftStates(nfa1.size - 1);

      // Создаем новый нфа размером с нфа2
      NFA newNFA = new NFA(nfa2);

      // нфа1 знимает свое место
      newNFA.FillStates(nfa1);

      // Инишиал 
      newNFA.initial = nfa1.initial;

      return newNFA;
    }

    /// <summary>
    /// Операция *
    ///  Сначала добавляем новый инишиал стейт, затем нфа1, затем новый файнал  стэйт
    /// </summary>
    public static NFA BuildNFAStar(NFA nfa)
    {
      nfa.ShiftStates(1);

      nfa.AppendEmptyState();

      // AДобавляем новые эпсилон переходы
      nfa.AddTrans(nfa.final, nfa.initial, (char)Constants.Epsilon);
      nfa.AddTrans(0, nfa.initial, (char)Constants.Epsilon);
      nfa.AddTrans(nfa.final, nfa.size - 1, (char)Constants.Epsilon);
      nfa.AddTrans(0, nfa.size - 1, (char)Constants.Epsilon);

      nfa.initial = 0;
      nfa.final = nfa.size - 1;

      return nfa;
    }
  }

}