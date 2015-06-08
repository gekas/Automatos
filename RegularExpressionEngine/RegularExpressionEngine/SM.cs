using System;
using SCG = System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using state = System.Int32;
using input = System.Char;
using C5;

namespace RegularExpressionEngine
{
    
    class SM
    {

        public DFA1 translateNfaToDfa(NFA nfa, DFA1 dfa) {

            initialTransition(nfa, dfa); // Check initial states including states by eps-transition

            for(int i = 0; i < dfa.states.Count; i++) {     // пока в таблице 2 есть записи
                HashSet<state> line = dfa.states[i];        // берем состояние(составное) 
                foreach (input inp in dfa.inputs)           // цикл по символам алфавита
                {
                    DFA_info dfa_inf = new DFA_info(inp);   // столбец перехода по символу (пока что пустая ячейка)
                    foreach (state st in line)              // Для каждого состояния в нашем Составном состоянии, то бишь в тайтле строки
                    {
                        inputStates(nfa, dfa, st, dfa_inf, inp);        // Ищем производные состояния
                    }
                    if (dfa_inf.to.Count != 0) checkStates(dfa, dfa_inf);
                    if(dfa_inf.to.Count != 0) dfa.transTable[line].Add(dfa_inf);
                }
            }

            setFinishStates(nfa, dfa);
            setHaltStates(nfa, dfa);

            return dfa;
        }

        public void setHaltStates(NFA nfa, DFA1 dfa){
                foreach(C5.HashSet<state> st in dfa.states) {
                    
                    SCG.List<input> stInputs = new SCG.List<input>();
                    foreach (DFA_info dfa_inf in dfa.transTable[st]) { stInputs.Add(dfa_inf.inp); }
                    foreach (input inp in dfa.inputs)
                    {
                        SCG.List<input> noTransition = new SCG.List<input>();
                        if(!stInputs.Contains(inp)) noTransition.Add(inp);
                        if (noTransition.Count != 0) {
                            if (dfa.halt == null) newHaltState(nfa, dfa);
                            HashSet<state> tmp = new HashSet<state>();
                            tmp.Add((int)dfa.halt);
                            dfa.transTable[st].Add(new DFA_info(inp, tmp));
                        }
                    }
                }
                if (dfa.halt != null)
                {
                    HashSet<state> haltHS = new HashSet<state>();
                    haltHS.Add((int)dfa.halt);
                    dfa.states.Add(haltHS);
                }
        }

        public void setFinishStates(NFA nfa, DFA1 dfa) {
            foreach (C5.HashSet<state> st in dfa.states) {
                if (st.Contains(nfa.final)) dfa.final.Add(st);
            }
        }

        public void initialTransition(NFA nfa, DFA1 dfa) {
            DFA_info dfa_inf = new DFA_info();
            
                epsStates(nfa, dfa, nfa.initial, dfa_inf);
                dfa.initial = dfa_inf.to;
                dfa.states.Add(dfa.initial);
                dfa.transTable.Add(dfa_inf.to, new SCG.List<DFA_info>());
            
            
        }

        private void newHaltState(NFA nfa, DFA1 dfa) {
            dfa.halt = nfa._size;
            HashSet<state> halt = new HashSet<state>();
            halt.Add((int)dfa.halt);

            SCG.List<DFA_info> lst = new SCG.List<DFA_info>();
            foreach (input inp in dfa.inputs)
            {
                DFA_info dfa_inf1 = new DFA_info();
                dfa_inf1.to = halt;
                dfa_inf1.inp = inp;
                lst.Add(dfa_inf1);
            }
            dfa.transTable.Add(halt, lst);
        }



        public void inputStates(NFA nfa, DFA1 dfa, state st, DFA_info dfa_inf, input inp)
        {
            SCG.List<state> lst = new SCG.List<state>();
            for (int i = 0; i < nfa._size; i++){        // По всем состояниям в нфа
                if (nfa.transTable[st][i] == inp){      // Если есть переход с нашего состояние в текущее состояние из НФА
                    dfa_inf.to.Add(i);
                    lst.Add(i);                         // Дополняем список состояний перехода
                    
                }
            }

            foreach(state st1 in lst){              // If move by input - check Epsilon transition from NEW input
                epsStates(nfa, dfa, st1, dfa_inf);
            }

            
            
        }

        public void epsStates(NFA nfa, DFA1 dfa, state st, DFA_info dfa_inf)
        {
            dfa_inf.to.Add(st);
            for (int i = 0; i < nfa._size; i++)
            {
                if (nfa.transTable[st][i] == (char)NFA.Constants.Epsilon)
                {
                    epsStates(nfa, dfa, i, dfa_inf);
                }
            }
        }

        private void checkStates(DFA1 dfa, DFA_info dfa_inf){   // Check an availibility of added states
            if ((!ListContainsHashState(dfa.states, dfa_inf.to)) && dfa_inf.to.Count != 0) { dfa.states.Add(dfa_inf.to); dfa.transTable.Add(dfa_inf.to, new SCG.List<DFA_info>());}
        }

        private bool ListContainsHashState(SCG.List<HashSet<state>> lst, HashSet<state> State)
        {
            foreach (HashSet<state> row in lst) {
                bool equal = true;
                if (row.Count != State.Count) continue;
                foreach (state st in State) {
                    if (!row.Contains(st)){ equal = false; break;}
                }
                if (equal) return true; // Равны
            }
            return false;  // Не равны
        }




    }

}
