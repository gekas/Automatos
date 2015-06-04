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
        public SM(NFA nfa) {
            DFA1 dfa = new DFA1(nfa);
            translateNfaToDfa(nfa, dfa);
          
        }

        public void translateNfaToDfa(NFA nfa, DFA1 dfa) {

            initialTransition(nfa, dfa); // Check initial states including states by eps-transition

            for(int i = 0; i < dfa.states.Count; i++) {
                HashSet<state> line = dfa.states[i];
                foreach (input inp in dfa.inputs)
                {
                    DFA_info dfa_inf = new DFA_info(line, inp);
                    foreach (state st in line)
                    {
                        inputStates(nfa, dfa, st, dfa_inf, inp);
                    }
                    if(dfa_inf.to.Count != 0) dfa.transTable[line].Add(dfa_inf);
                }

            }
                
            
        }

        public void initialTransition(NFA nfa, DFA1 dfa) {
            DFA_info dfa_inf = new DFA_info();
            epsStates(nfa, dfa, nfa.initial, dfa_inf);
            dfa.initial = dfa_inf.to;
            dfa.states.Add(dfa.initial);
            dfa.transTable.Add(dfa_inf.to, new SCG.List<DFA_info>());
        }

        public void epsStates(NFA nfa, DFA1 dfa, state st, DFA_info dfa_inf) {
            dfa_inf.to.Add(st);
            for (int i = 0; i < nfa._size; i++) {
                if (nfa.transTable[st][i] == (char)NFA.Constants.Epsilon) {
                    epsStates(nfa, dfa, i, dfa_inf);
                } 
            }
        }

        public void inputStates(NFA nfa, DFA1 dfa, state st, DFA_info dfa_inf, input inp)
        {
            SCG.List<state> lst = new SCG.List<state>();
            for (int i = 0; i < nfa._size; i++){
                if (nfa.transTable[st][i] == inp){
                    dfa_inf.to.Add(i);
                    lst.Add(i);
                    
                }
            }

            foreach(state st1 in lst){              // If move by input - check Epsilon transition from NEW input
                epsStates(nfa, dfa, st1, dfa_inf);
            }

            checkStates(dfa, dfa_inf);
            
        }

        private void checkStates(DFA1 dfa, DFA_info dfa_inf){   // Check an availibility of added states
           // if ((!ListContainsHashState(dfa.states, dfa_inf.from)) && dfa_inf.from.Count != 0) dfa.states.Add(dfa_inf.from);
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
                if (equal) return true;
            }
            return false;
        }




    }

}
