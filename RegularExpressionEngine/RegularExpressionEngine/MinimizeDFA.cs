using System;
using System.Collections;
using C5;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using state = System.Int32;
using input = System.Char;

namespace RegularExpressionEngine
{
    class DFAMin_trans {
        public input inp;
        public SortedArray<state> to = new SortedArray<state>();

        public DFAMin_trans(input inp, SortedArray<state> to) {
            this.inp = inp;
            this.to = to;
        }
    }
    enum Markers
    {
        Unmarked,
        Check,
        Cross        
    }

    class MinimizeDFA
    {
        DFASimple dfa;
        int dfaSize;
        state[] finals;
        Markers[,] mrkTable;
        Dictionary<Tuple<state, state>, Dictionary<input, Tuple<state, state>>> STTable = new Dictionary<Tuple<state, state>, Dictionary<input, Tuple<state, state>>>();
        private List<Tuple<state, state>> checkList = new List<Tuple<state, state>>();

        public List<SortedArray<state>> minimizedStates = new List<SortedArray<state>>();
        public Dictionary<SortedArray<state>, List<DFAMin_trans>> transTable = new Dictionary<SortedArray<state>, List<DFAMin_trans>>();
        public SortedArray<state> initial = new SortedArray<state>();
        public List<SortedArray<state>> minFinals = new List<SortedArray<state>>();

        public bool Simulate(string str)
        {
            SortedArray<state> cur = initial;
            
            foreach (input symb in str) {
                if (!dfa.inputs.Contains(symb)) { Console.WriteLine("The wrong symbol in string --- Rejected"); return false; }
                foreach (DFAMin_trans dfaMin in transTable[cur]) { 
                        if (dfaMin.inp == symb) cur = dfaMin.to;
                }

            }
            bool accepted = false;
            foreach (SortedArray<state> fn in minFinals) {
                foreach (state st in cur) if (fn.Contains(st)) accepted = true;
            }

            return accepted;
        }

        public void Show() {
            Console.WriteLine("\nMinimized DFA has: " + minimizedStates.Count + " states");
            Console.Write("States: ");
            foreach (SortedArray<state> st in minimizedStates) Console.Write(st.ToString());
            Console.WriteLine("\nInitial state: " + initial.ToString());
            Console.Write("Finals: ");
            foreach (SortedArray<state> st in minFinals) Console.Write(st.ToString() + " ");
            Console.WriteLine("\n");
            foreach (SortedArray<state> st in transTable.Keys) {
                foreach (DFAMin_trans dfa_inf in transTable[st]) {
                    Console.WriteLine("Transition from " + st.ToString() + " to " + dfa_inf.to + " on input " + dfa_inf.inp);
                } 
            }

            if (Simulate(Options.str)) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine("Accepted"); }
            else { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine("Rejected"); }
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public MinimizeDFA(DFASimple dfa)
        {
            this.dfa = dfa;
            this.dfaSize = dfa.size;
            finals = dfa.final.ToArray<state>();
            fillMrkTable();  // Заполняем первую таблицу
            createSymbolTable();
            checkSymbolTable();
            getMinimizedStates();
            mergerMinimizedStates();
            setTransitions();
            setInFinal();
        }

        private void setInFinal() {
            foreach (SortedArray<state> minGroupSt in minimizedStates) {
                foreach (state finalDFAst in dfa.final) {
                    if (minGroupSt.Contains(finalDFAst)) {
                        bool flag = false;
                        foreach (SortedArray<state> st in minFinals) {
                            foreach (state st1 in minGroupSt) if (st.Contains(st1)) flag = true;
                        }
                        if(!flag) minFinals.Add(minGroupSt);
                    }
                    if (minGroupSt.Contains(dfa.initial)) {
                        initial = minGroupSt;
                    }
                }
            }

        //    initial.Add(dfa.initial);
        }

        // (c|d)*(c|(a|d))*a?  +
        // (aa*bb)*   +

        /// <summary>
        ///     Set relationships in minimized DFA
        /// </summary>
        private void setTransitions() {
            
            foreach (SortedArray<state> mergedState in minimizedStates) { // Для каждого состояния минимизированного DFA
                List<DFAMin_trans> cells = new List<DFAMin_trans>();
                foreach (input inp in dfa.inputs)                         // По каждому символу
                {
                    SortedArray<state> inpToStates = new SortedArray<state>();   // Заполняем список состояниями перехода
                    foreach (state stOriginal in mergedState)
                        foreach (DFASimple_info dfa_inf in dfa.transTable[stOriginal])
                            if(dfa_inf.inp == inp) inpToStates.Add(dfa_inf.to);
                      
                    // Далее надо проверить принадлежность группе
                    foreach (SortedArray<state> mergedState1 in minimizedStates) {
                            if (mergedState1.ContainsAll(inpToStates) ) {
                                cells.Add(new DFAMin_trans(inp, mergedState1));
                            }
                    }             
                }
                transTable.Add(mergedState, cells);  
            }
        }

        /// <summary>
        ///     Слияние вершин с имеющимися одинаковыми одиночными верщинами ДФА
        /// </summary>
        private void mergerMinimizedStates() {
            for (int i = 0; i < minimizedStates.Count; i++) {
                SortedArray<state> oState = minimizedStates[i];
                for (int j = 0; j < minimizedStates.Count; j++) { 
                    if(j==i) continue;                          
                    SortedArray<state> curState = minimizedStates[j];
                    for (int q = 0; q < curState.Count; q++) {
                        if (oState.Contains(curState[q])) {
                            for (int k = 0; k < curState.Count; k++) { // I know, it looks terrible... But I have to pass the labrotory tomorrow!..
                                oState.Add(curState[k]);
                            }
                            minimizedStates.Remove(curState);
                        }
                    }
                }
            }
        }


        /// <summary>
        ///    Формируем список финальных состояний до момента слияния. 
        ///    Формат выполнения: Проходим по всем состояниям ДФА. 
        ///    ЕСЛИ имеется отмеченная пара, включающая текущее состояние - добавляем эту пару, 
        ///    ИНАЧЕ добавляем одиночное состояние.
        /// </summary>
        private void getMinimizedStates() {
            foreach (state st in dfa.accordanceStates.Values) { // Для каждого состояния в dfa
                SortedArray<state> newState = new SortedArray<state>();
                foreach (Tuple<state, state> checkPair in checkList)  // Для каждой пары, отмеченной галочкой
                {
                    if (checkPair.Item1 == st || checkPair.Item2 == st) {   // Если данное состояние принадлежит паре, то в финальные состояния добавляем пару  и выходим                  
                        newState.Add(checkPair.Item1);
                        newState.Add(checkPair.Item2);
                        bool flag = false;
                        foreach (SortedArray<state> stateInMinimize in minimizedStates)
                            if (stateInMinimize.Contains(checkPair.Item1) && stateInMinimize.Contains(checkPair.Item2)) flag = true;
                            if(!flag) minimizedStates.Add(newState);
                      //  checkList.Remove(checkPair);
                        goto IfFound;
                    }
                }
                newState.Add(st); // Если не найдено, то добавляем простое состояние
                minimizedStates.Add(newState);
            IfFound: continue;
            }


        }

        private void fillMrkTable(){
            mrkTable = new Markers[dfaSize, dfaSize];
            for (state i = 0; i < dfaSize; i++) {      // Запоняем диагональ и нижнюю часть
                for (state j = 0; j < i; j++)
                    mrkTable[i, j] = Markers.Cross; // Заполняем нижнюю часть
                mrkTable[i, i] = Markers.Check;     // Заполняем диагональ

                for (state j = i+1; j < dfaSize; j++) {
                    if ((dfa.final.Contains(j) && !dfa.final.Contains(i)) || (!dfa.final.Contains(j) && dfa.final.Contains(i)))
                        mrkTable[i, j] = Markers.Cross;
                    else STTable.Add(new Tuple<state,state>(i,j), new Dictionary<input,Tuple<state,state>>());
                }
            }    
        }

        private void createSymbolTable() {
            foreach (Tuple<state, state> rowKey in STTable.Keys) { // Заполняем ключи таблицы STTable
                foreach (input inp in dfa.inputs) {
                    STTable[rowKey].Add(inp, getTuplePairBySymbol(rowKey, inp));
                }
            }
        }

        private Tuple<state, state> getTuplePairBySymbol(Tuple<state, state> rowKey, input inp) {
            state st1 = -1, st2 = -1; // Возвращаем состояния перехода по состояниям и символам в STTable
            foreach (DFASimple_info dfa_inf in dfa.transTable[rowKey.Item1]) 
                if (dfa_inf.inp == inp) { 
                    st1 = dfa_inf.to;
                    break;
                }
                
            foreach (DFASimple_info dfa_inf in dfa.transTable[rowKey.Item2]) 
                if (dfa_inf.inp == inp)
                {
                    st2 = dfa_inf.to;
                    break;
                }
            return new Tuple<state, state>(Math.Min(st1, st2), Math.Max(st1, st2)); 
        }

        private void checkSymbolTable() {
            foreach (Tuple<state, state> rowKey in STTable.Keys) { // Расставляем метки в пустых таблицах. Функция рекурсивна и выполняется до тех пор, пока таблица не будет пройдена полностью без установки метки
               
                List<Markers> rowMarkers = new List<Markers>();
                foreach (System.Collections.Generic.KeyValuePair<input, Tuple<state, state>> cell in STTable[rowKey]) {  // Заполнили список состояниями ячеек текущего ключа
                    rowMarkers.Add(mrkTable[cell.Value.Item1, cell.Value.Item2]);
                }

                // Далее надо поставить метку в зависимости от элементов списка И вызвать процедуру заново в случае установки метки. (Углубление в рекурсию)
                if (rowMarkers.Contains(Markers.Cross) && mrkTable[rowKey.Item1, rowKey.Item2] == Markers.Unmarked)
                {
                    mrkTable[rowKey.Item1, rowKey.Item2] = Markers.Cross;
                    checkSymbolTable();
                }
                else if (rowMarkers.All(o => o == Markers.Check) && mrkTable[rowKey.Item1, rowKey.Item2] == Markers.Unmarked)
                {
                    mrkTable[rowKey.Item1, rowKey.Item2] = Markers.Check;
                    checkList.Add(rowKey);
                    checkSymbolTable();
                }
 
            }
            // Далее ставим галочки во всех местах, которые остались пустыми
            for (int i = 0; i < dfaSize; i++)
                for (int j = i + 1; j < dfaSize; j++)
                    if (mrkTable[i, j] == Markers.Unmarked) mrkTable[i, j] = Markers.Check;

        }



    }
}
