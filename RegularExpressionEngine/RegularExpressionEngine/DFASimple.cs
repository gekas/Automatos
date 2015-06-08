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

    class DFASimple_info { 
        public state to;
        public input inp;

        public DFASimple_info(input inp) {
            this.inp = inp;
        }

        public DFASimple_info(input inp, state to)
        {
            this.inp = inp;
            this.to = to;
        }

        public DFASimple_info() { }
    }
    class DFASimple
    {
      //  public List<state> states = new List<state>();
        public state initial;
        public List<state> final = new List<state>();
        public state? halt = null;
        public int size;
        public SortedArray<input> inputs = new SortedArray<input>();
        public Dictionary<state, List<DFASimple_info>> transTable = new Dictionary<state, List<DFASimple_info>>();
        public Dictionary<C5.HashSet<state>, state> accordanceStates = new Dictionary<C5.HashSet<state>, state>();

        public DFASimple(DFA1 dfa) {
            for (int i = 0; i < dfa.states.Count; i++) {
                accordanceStates.Add(dfa.states[i], i);  // Заполняем список соответствий
            }
            size = accordanceStates.Count;
            foreach (C5.HashSet<state> st in accordanceStates.Keys)
                if (st.Count == 1 && st.Contains((int)dfa.halt))
                    halt = accordanceStates[st];
            inputs = dfa.inputs;
            changeInFinal(dfa);
            changeInInitial(dfa);
            changeInTransitions(dfa);
        }

        private void changeInTransitions(DFA1 dfa)
        {
            foreach(System.Collections.Generic.KeyValuePair<C5.HashSet<state>, List<DFA_info>> fullSate in dfa.transTable){
                
                List<DFASimple_info> lstS = new List<DFASimple_info>();
                foreach (DFA_info di in fullSate.Value) {
                    DFASimple_info tmpDS = new DFASimple_info();
                    tmpDS.inp = di.inp;

                    foreach (C5.HashSet<state> st in accordanceStates.Keys) 
                        if (st.ToString() == di.to.ToString())
                                tmpDS.to = accordanceStates[st];       
                    
                    lstS.Add(tmpDS);
                }
                foreach (C5.HashSet<state> st in accordanceStates.Keys)
                    if (st.ToString() == fullSate.Key.ToString())
                transTable.Add(accordanceStates[st], lstS);
            }
        }

        private void changeInFinal(DFA1 dfa) {
            foreach (C5.HashSet<state> fullState in dfa.final) final.Add(accordanceStates[fullState]);
        }

        private void changeInInitial(DFA1 dfa)
        {
            initial = accordanceStates[dfa.initial];
        }

    }
}
